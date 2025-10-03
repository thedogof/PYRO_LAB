using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PyroLab.Fireworks.Editor
{
    public static class RecipeJsonUtility
    {
        private const string JsonFolderRelative = "Assets/RecipesJSON";
        private const string AssetFolderRelative = "Assets/Recipes";
        private const string DefaultExportName = "FireworkRecipe";

        public static string JsonFolderRelativePath => JsonFolderRelative;
        public static string AssetFolderRelativePath => AssetFolderRelative;

        public readonly struct RecipeValidationSummary
        {
            public readonly string RecipeName;
            public readonly int LayerCount;
            public readonly int TotalStarCount;
            public readonly int PeakParticleEstimate;
            public readonly int EstimatedDrawCalls;
            public readonly float TotalParticleLifetime;

            public RecipeValidationSummary(string recipeName, int layerCount, int totalStarCount, int peakParticleEstimate, int estimatedDrawCalls, float totalParticleLifetime)
            {
                RecipeName = recipeName;
                LayerCount = layerCount;
                TotalStarCount = totalStarCount;
                PeakParticleEstimate = peakParticleEstimate;
                EstimatedDrawCalls = estimatedDrawCalls;
                TotalParticleLifetime = totalParticleLifetime;
            }
        }

        public static bool Validate(string json)
        {
            return Validate(json, null, out _);
        }

        public static bool Validate(string json, string sourceLabel, out RecipeValidationSummary summary)
        {
            summary = default;

            string context = string.IsNullOrEmpty(sourceLabel) ? "Recipe JSON" : $"Recipe JSON ({sourceLabel})";
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError($"{context}: Content is empty.");
                return false;
            }

            Dictionary<string, object> root;
            try
            {
                root = SimpleJson.Parse(json) as Dictionary<string, object>;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{context}: Failed to parse JSON. {ex.Message}");
                return false;
            }

            if (root == null)
            {
                Debug.LogError($"{context}: Root JSON object is invalid.");
                return false;
            }

            var errors = new List<string>();
            var warnings = new List<string>();

            root.TryGetValue("recipeName", out var nameObj);
            string recipeName = nameObj as string;

            ValidateNumber(root, "size", context, errors, warnings, allowZero: false, allowNegative: false);
            ValidateNumber(root, "desiredBurstHeight", context, errors, warnings, allowZero: true, allowNegative: false);
            ValidateNumber(root, "fuseTime", context, errors, warnings, allowZero: false, allowNegative: false);

            int layerCount = 0;
            int totalStars = 0;
            int peakParticles = 0;
            int drawCalls = 0;
            float totalLifetime = 0f;

            if (!root.TryGetValue("layers", out var layersObj) || layersObj is not List<object> layersList)
            {
                errors.Add("Missing 'layers' array.");
            }
            else if (layersList.Count == 0)
            {
                warnings.Add("Recipe contains no layers.");
            }
            else
            {
                for (int i = 0; i < layersList.Count; i++)
                {
                    if (layersList[i] is not Dictionary<string, object> layerDict)
                    {
                        errors.Add($"Layer {i}: Invalid entry format.");
                        continue;
                    }

                    layerCount++;
                    drawCalls++;

                    int starCount = ReadInt(layerDict, "starCount", errors, context, i, minimum: 0);
                    totalStars += Mathf.Max(0, starCount);

                    double lifetimeValue = ReadNumber(layerDict, "lifetime", errors, context, i, allowZero: true, allowNegative: false);

                    ValidateNumber(layerDict, "speedMin", context, errors, warnings, allowZero: false, allowNegative: false, layerIndex: i);
                    ValidateNumber(layerDict, "speedMax", context, errors, warnings, allowZero: false, allowNegative: false, layerIndex: i);

                    int multiplier = 1;
                    bool hasTrails = false;

                    if (layerDict.TryGetValue("modifiers", out var modifiersObj) && modifiersObj is List<object> modifiersList)
                    {
                        foreach (var modifierObj in modifiersList)
                        {
                            if (modifierObj is not Dictionary<string, object> modifierDict)
                            {
                                warnings.Add($"Layer {i}: Modifier entry is malformed.");
                                continue;
                            }

                            string typeName = ReadString(modifierDict, "type");
                            if (string.IsNullOrEmpty(typeName))
                            {
                                warnings.Add($"Layer {i}: Modifier type missing.");
                                continue;
                            }

                            if (typeName.Contains("SplitModifier"))
                            {
                                int splitCount = ExtractSplitCount(modifierDict, context, errors, i);
                                multiplier = Mathf.Max(multiplier, splitCount);
                            }
                            else if (typeName.Contains("TrailModifier"))
                            {
                                hasTrails = true;
                            }
                        }
                    }

                    peakParticles += Mathf.Max(0, starCount) * Mathf.Max(1, multiplier);
                    totalLifetime += Mathf.Max(0, starCount) * Mathf.Max(1, multiplier) * Mathf.Max(0f, (float)lifetimeValue);

                    if (hasTrails)
                    {
                        drawCalls += 1;
                    }
                }
            }

            foreach (var error in errors)
            {
                Debug.LogError($"{context}: {error}");
            }

            foreach (var warning in warnings)
            {
                Debug.LogWarning($"{context}: {warning}");
            }

            bool isValid = errors.Count == 0;
            summary = new RecipeValidationSummary(recipeName, layerCount, totalStars, peakParticles, drawCalls, totalLifetime);
            return isValid;
        }

        public static string EnsureJsonFolder()
        {
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", JsonFolderRelative));
            if (!Directory.Exists(absolute))
            {
                Directory.CreateDirectory(absolute);
                AssetDatabase.Refresh();
            }

            return absolute;
        }

        public static string EnsureAssetFolder()
        {
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", AssetFolderRelative));
            if (!Directory.Exists(absolute))
            {
                Directory.CreateDirectory(absolute);
                AssetDatabase.Refresh();
            }

            return absolute;
        }

        public static string ExportWithDialog(FireworkRecipe recipe)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            string directory = EnsureJsonFolder();
            string defaultName = !string.IsNullOrWhiteSpace(recipe.name) ? recipe.name : DefaultExportName;
            string path = EditorUtility.SaveFilePanel("Export Firework Recipe", directory, defaultName, "json");
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            ExportToPath(recipe, path);
            return path;
        }

        public static FireworkRecipe ImportAsNewAssetWithDialog()
        {
            string directory = EnsureJsonFolder();
            string path = EditorUtility.OpenFilePanel("Import Firework Recipe", directory, "json");
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return ImportAsNewAsset(path);
        }

        public static void OverwriteFromJsonWithDialog(FireworkRecipe target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            string directory = EnsureJsonFolder();
            string path = EditorUtility.OpenFilePanel("Import Firework Recipe", directory, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            OverwriteFromJson(target, path);
        }

        public static void ExportToPath(FireworkRecipe recipe, string path)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            string json = recipe.ExportToJson();
            var summary = CreateSummaryFromRecipe(recipe);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            LogRecipeStatistics("Export", path, summary);
        }

        public static FireworkRecipe ImportAsNewAsset(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(jsonPath));
            }

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException("Recipe JSON not found.", jsonPath);
            }

            EnsureAssetFolder();
            string json = File.ReadAllText(jsonPath);
            if (!Validate(json, jsonPath, out var validationSummary))
            {
                return null;
            }

            var recipe = ScriptableObject.CreateInstance<FireworkRecipe>();
            recipe.hideFlags = HideFlags.None;
            recipe.ImportFromJson(json);

            string fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(jsonPath));
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DefaultExportName;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(AssetFolderRelative, fileName + ".asset"));
            AssetDatabase.CreateAsset(recipe, assetPath);
            EditorUtility.SetDirty(recipe);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var summary = CreateSummaryFromRecipe(recipe, validationSummary);
            LogRecipeStatistics("Import", jsonPath, summary);
            Debug.Log($"Imported recipe JSON as asset at {assetPath}");
            Selection.activeObject = recipe;
            return recipe;
        }

        public static void OverwriteFromJson(FireworkRecipe target, string jsonPath)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(jsonPath))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(jsonPath));
            }

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException("Recipe JSON not found.", jsonPath);
            }

            string json = File.ReadAllText(jsonPath);
            if (!Validate(json, jsonPath, out var validationSummary))
            {
                return;
            }

            Undo.RecordObject(target, "Import Firework Recipe");
            target.ImportFromJson(json);
            EditorUtility.SetDirty(target);
            var summary = CreateSummaryFromRecipe(target, validationSummary);
            LogRecipeStatistics("Import", jsonPath, summary);
            Debug.Log($"Imported recipe JSON into existing asset {target.name}");
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return DefaultExportName;
            }

            string invalid = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string pattern = $"[{invalid}]";
            return Regex.Replace(name, pattern, string.Empty).Trim();
        }

        private static RecipeValidationSummary CreateSummaryFromRecipe(FireworkRecipe recipe)
        {
            var estimate = RecipeCostUtility.Estimate(recipe);
            string recipeName = recipe != null ? recipe.name : string.Empty;
            return new RecipeValidationSummary(recipeName, estimate.LayerCount, estimate.TotalStarCount, estimate.EstimatedPeakParticles, estimate.EstimatedDrawCalls, estimate.TotalParticleLifetime);
        }

        private static RecipeValidationSummary CreateSummaryFromRecipe(FireworkRecipe recipe, RecipeValidationSummary validation)
        {
            var estimate = RecipeCostUtility.Estimate(recipe);
            string recipeName = !string.IsNullOrEmpty(validation.RecipeName) ? validation.RecipeName : recipe != null ? recipe.name : string.Empty;
            return new RecipeValidationSummary(recipeName, estimate.LayerCount, estimate.TotalStarCount, estimate.EstimatedPeakParticles, estimate.EstimatedDrawCalls, estimate.TotalParticleLifetime);
        }

        private static void LogRecipeStatistics(string action, string contextPath, RecipeValidationSummary summary)
        {
            string name = string.IsNullOrEmpty(summary.RecipeName) ? "(Unnamed Recipe)" : summary.RecipeName;
            string location = string.IsNullOrEmpty(contextPath) ? string.Empty : $" | Source: {contextPath}";
            Debug.Log($"[Recipe {action}] {name} | Layers: {summary.LayerCount} | Stars: {summary.TotalStarCount} | Peak Particles: {summary.PeakParticleEstimate} | Draw Calls: {summary.EstimatedDrawCalls} | Lifetime Sum: {summary.TotalParticleLifetime:F1}{location}");
        }

        private static void ValidateNumber(Dictionary<string, object> source, string key, string context, List<string> errors, List<string> warnings, bool allowZero, bool allowNegative, int layerIndex = -1)
        {
            double value = ReadNumber(source, key, errors, context, layerIndex, allowZero, allowNegative);
            if (double.IsNaN(value))
            {
                return;
            }

            if (allowZero && Math.Abs(value) < float.Epsilon)
            {
                string prefix = layerIndex >= 0 ? $"Layer {layerIndex}:" : string.Empty;
                warnings.Add($"{prefix} '{key}' is zero.");
            }
        }

        private static double ReadNumber(Dictionary<string, object> source, string key, List<string> errors, string context, int layerIndex, bool allowZero, bool allowNegative)
        {
            if (!source.TryGetValue(key, out var raw))
            {
                string prefix = layerIndex >= 0 ? $"Layer {layerIndex}:" : string.Empty;
                errors.Add($"{prefix} Missing '{key}'.");
                return double.NaN;
            }

            if (!TryConvertToDouble(raw, out double value))
            {
                string prefix = layerIndex >= 0 ? $"Layer {layerIndex}:" : string.Empty;
                errors.Add($"{prefix} '{key}' is not a number.");
                return double.NaN;
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                string prefix = layerIndex >= 0 ? $"Layer {layerIndex}:" : string.Empty;
                errors.Add($"{prefix} '{key}' contains an invalid numeric value.");
                return double.NaN;
            }

            if (!allowNegative && value < 0)
            {
                string prefix = layerIndex >= 0 ? $"Layer {layerIndex}:" : string.Empty;
                errors.Add($"{prefix} '{key}' cannot be negative.");
            }

            if (!allowZero && Math.Abs(value) < float.Epsilon)
            {
                string prefix = layerIndex >= 0 ? $"Layer {layerIndex}:" : string.Empty;
                errors.Add($"{prefix} '{key}' cannot be zero.");
            }

            return value;
        }

        private static int ReadInt(Dictionary<string, object> source, string key, List<string> errors, string context, int layerIndex, int minimum)
        {
            if (!source.TryGetValue(key, out var raw))
            {
                errors.Add($"Layer {layerIndex}: Missing '{key}'.");
                return 0;
            }

            if (!TryConvertToDouble(raw, out double value))
            {
                errors.Add($"Layer {layerIndex}: '{key}' is not numeric.");
                return 0;
            }

            int result = Mathf.RoundToInt((float)value);
            if (result < minimum)
            {
                errors.Add($"Layer {layerIndex}: '{key}' must be >= {minimum}.");
            }

            return result;
        }

        private static string ReadString(Dictionary<string, object> source, string key)
        {
            if (!source.TryGetValue(key, out var raw))
            {
                return null;
            }

            return raw as string;
        }

        private static int ExtractSplitCount(Dictionary<string, object> modifierDict, string context, List<string> errors, int layerIndex)
        {
            if (!modifierDict.TryGetValue("json", out var jsonObj) || jsonObj is not string json)
            {
                errors.Add($"Layer {layerIndex}: Split modifier missing payload.");
                return 1;
            }

            try
            {
                if (SimpleJson.Parse(json) is Dictionary<string, object> payload)
                {
                    if (payload.TryGetValue("splitCount", out var splitValue) && TryConvertToDouble(splitValue, out double raw))
                    {
                        return Mathf.Max(1, Mathf.RoundToInt((float)raw));
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Layer {layerIndex}: Failed to parse split modifier payload. {ex.Message}");
            }

            return 1;
        }

        private static bool TryConvertToDouble(object value, out double result)
        {
            switch (value)
            {
                case double d:
                    result = d;
                    return true;
                case float f:
                    result = f;
                    return true;
                case int i:
                    result = i;
                    return true;
                case long l:
                    result = l;
                    return true;
                case string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed):
                    result = parsed;
                    return true;
                default:
                    result = 0;
                    return false;
            }
        }

        private static class SimpleJson
        {
            public static object Parse(string json)
            {
                if (json == null)
                {
                    throw new ArgumentNullException(nameof(json));
                }

                var parser = new Parser(json);
                return parser.ParseValue();
            }

            private sealed class Parser
            {
                private readonly string json;
                private int index;

                public Parser(string json)
                {
                    this.json = json;
                    index = 0;
                }

                public object ParseValue()
                {
                    SkipWhitespace();
                    if (index >= json.Length)
                    {
                        throw new FormatException("Unexpected end of JSON.");
                    }

                    char c = json[index];
                    return c switch
                    {
                        '{' => ParseObject(),
                        '[' => ParseArray(),
                        '"' => ParseString(),
                        't' => ParseLiteral("true", true),
                        'f' => ParseLiteral("false", false),
                        'n' => ParseNull(),
                        '-' => ParseNumber(),
                        _ when char.IsDigit(c) => ParseNumber(),
                        _ => throw new FormatException($"Unexpected character '{c}' at position {index}.")
                    };
                }

                private Dictionary<string, object> ParseObject()
                {
                    var result = new Dictionary<string, object>();
                    index++; // skip '{'
                    SkipWhitespace();

                    if (index < json.Length && json[index] == '}')
                    {
                        index++;
                        return result;
                    }

                    while (true)
                    {
                        SkipWhitespace();
                        string key = ParseString();
                        SkipWhitespace();

                        if (index >= json.Length || json[index] != ':')
                        {
                            throw new FormatException("Expected ':' after object key.");
                        }

                        index++;
                        object value = ParseValue();
                        result[key] = value;

                        SkipWhitespace();
                        if (index >= json.Length)
                        {
                            throw new FormatException("Unterminated object.");
                        }

                        char c = json[index++];
                        if (c == '}')
                        {
                            break;
                        }

                        if (c != ',')
                        {
                            throw new FormatException($"Unexpected character '{c}' in object.");
                        }
                    }

                    return result;
                }

                private List<object> ParseArray()
                {
                    var result = new List<object>();
                    index++; // skip '['
                    SkipWhitespace();

                    if (index < json.Length && json[index] == ']')
                    {
                        index++;
                        return result;
                    }

                    while (true)
                    {
                        object value = ParseValue();
                        result.Add(value);
                        SkipWhitespace();
                        if (index >= json.Length)
                        {
                            throw new FormatException("Unterminated array.");
                        }

                        char c = json[index++];
                        if (c == ']')
                        {
                            break;
                        }

                        if (c != ',')
                        {
                            throw new FormatException($"Unexpected character '{c}' in array.");
                        }
                    }

                    return result;
                }

                private string ParseString()
                {
                    if (json[index] != '"')
                    {
                        throw new FormatException("Expected string opening quote.");
                    }

                    index++; // skip '"'
                    int start = index;
                    var result = new System.Text.StringBuilder();

                    while (index < json.Length)
                    {
                        char c = json[index++];
                        if (c == '"')
                        {
                            return result.ToString();
                        }

                        if (c == '\\')
                        {
                            if (index >= json.Length)
                            {
                                throw new FormatException("Unterminated escape sequence in string.");
                            }

                            char escape = json[index++];
                            switch (escape)
                            {
                                case '\"':
                                    result.Append('"');
                                    break;
                                case '\\':
                                    result.Append('\\');
                                    break;
                                case '/':
                                    result.Append('/');
                                    break;
                                case 'b':
                                    result.Append('\b');
                                    break;
                                case 'f':
                                    result.Append('\f');
                                    break;
                                case 'n':
                                    result.Append('\n');
                                    break;
                                case 'r':
                                    result.Append('\r');
                                    break;
                                case 't':
                                    result.Append('\t');
                                    break;
                                case 'u':
                                    if (index + 4 > json.Length)
                                    {
                                        throw new FormatException("Invalid unicode escape sequence.");
                                    }

                                    string hex = json.Substring(index, 4);
                                    if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort code))
                                    {
                                        throw new FormatException("Invalid unicode escape sequence.");
                                    }

                                    result.Append((char)code);
                                    index += 4;
                                    break;
                                default:
                                    throw new FormatException($"Invalid escape character '{escape}'.");
                            }
                        }
                        else
                        {
                            result.Append(c);
                        }
                    }

                    throw new FormatException("Unterminated string literal.");
                }

                private object ParseLiteral(string literal, object value)
                {
                    if (json.Length - index < literal.Length || json.Substring(index, literal.Length) != literal)
                    {
                        throw new FormatException($"Invalid literal starting at position {index}.");
                    }

                    index += literal.Length;
                    return value;
                }

                private object ParseNull()
                {
                    return ParseLiteral("null", null);
                }

                private object ParseNumber()
                {
                    int start = index;
                    if (json[index] == '-')
                    {
                        index++;
                    }

                    while (index < json.Length && char.IsDigit(json[index]))
                    {
                        index++;
                    }

                    if (index < json.Length && json[index] == '.')
                    {
                        index++;
                        while (index < json.Length && char.IsDigit(json[index]))
                        {
                            index++;
                        }
                    }

                    if (index < json.Length && (json[index] == 'e' || json[index] == 'E'))
                    {
                        index++;
                        if (index < json.Length && (json[index] == '+' || json[index] == '-'))
                        {
                            index++;
                        }
                        while (index < json.Length && char.IsDigit(json[index]))
                        {
                            index++;
                        }
                    }

                    string number = json.Substring(start, index - start);
                    if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        throw new FormatException($"Invalid number '{number}'.");
                    }

                    return value;
                }

                private void SkipWhitespace()
                {
                    while (index < json.Length && char.IsWhiteSpace(json[index]))
                    {
                        index++;
                    }
                }
            }
        }
    }
}

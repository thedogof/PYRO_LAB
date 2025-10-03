using System;
using System.IO;
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
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"Recipe exported to {path}");
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
            Undo.RecordObject(target, "Import Firework Recipe");
            target.ImportFromJson(json);
            EditorUtility.SetDirty(target);
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
    }
}

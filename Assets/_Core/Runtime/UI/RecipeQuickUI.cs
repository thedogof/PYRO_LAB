using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public class RecipeQuickUI : MonoBehaviour
    {
        [SerializeField] private FireworkSpawner spawner;
        [SerializeField] private string examplesFolderRelative = "RecipesJSON/Examples";
        [SerializeField] private float warningParticleThreshold = 300000f;
        [SerializeField] private bool autoApplyOnStart = true;

        private readonly List<ExampleEntry> examples = new();
        private string[] optionNames = Array.Empty<string>();
        private int selectedIndex;
        private Vector2 scrollPosition;
        private Rect windowRect = new Rect(0f, 0f, 340f, 320f);
        private bool windowPositionInitialized;
        private bool hasLoaded;

        private class ExampleEntry
        {
            public string DisplayName;
            public string FilePath;
            public string DefaultJson;
            public FireworkRecipe RecipeInstance;
            public RecipeCostEstimate Estimate;
        }

        private void Awake()
        {
            if (spawner == null)
            {
                spawner = FindObjectOfType<FireworkSpawner>();
            }

            LoadExamples();
        }

        private void OnEnable()
        {
            if (!hasLoaded)
            {
                LoadExamples();
            }
        }

        private void OnDestroy()
        {
            DisposeExamples();
        }

        private void LoadExamples()
        {
            DisposeExamples();
            examples.Clear();
            optionNames = Array.Empty<string>();
            hasLoaded = true;

            string basePath = Path.Combine(Application.dataPath, examplesFolderRelative);
            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"RecipeQuickUI: Examples folder not found at {basePath}.");
                return;
            }

            var files = Directory.GetFiles(basePath, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var recipe = ScriptableObject.CreateInstance<FireworkRecipe>();
                    recipe.hideFlags = HideFlags.DontSave;
                    recipe.ImportFromJson(json);

                    var entry = new ExampleEntry
                    {
                        DisplayName = Path.GetFileNameWithoutExtension(file),
                        FilePath = file,
                        DefaultJson = json,
                        RecipeInstance = recipe,
                        Estimate = RecipeCostUtility.Estimate(recipe)
                    };

                    examples.Add(entry);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"RecipeQuickUI: Failed to load example '{file}'. {ex.Message}");
                }
            }

            optionNames = new string[examples.Count];
            for (int i = 0; i < examples.Count; i++)
            {
                optionNames[i] = examples[i].DisplayName;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, examples.Count - 1));

            if (autoApplyOnStart && examples.Count > 0)
            {
                ApplySelectedRecipe();
            }
        }

        private void DisposeExamples()
        {
            foreach (var entry in examples)
            {
                if (entry?.RecipeInstance == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(entry.RecipeInstance);
                }
                else
                {
                    DestroyImmediate(entry.RecipeInstance);
                }
            }
        }

        private void OnGUI()
        {
            if (examples.Count == 0)
            {
                return;
            }

            if (!windowPositionInitialized)
            {
                windowRect.x = Screen.width - windowRect.width - 16f;
                windowRect.y = 16f;
                windowPositionInitialized = true;
            }

            windowRect = GUILayout.Window(GetInstanceID(), windowRect, DrawWindowContents, "Recipe Quick Switch");
        }

        private void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();

            int newIndex = examples.Count > 0 ? GUILayout.Popup(selectedIndex, optionNames) : selectedIndex;
            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                ApplySelectedRecipe();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload"))
            {
                LoadExamples();
                GUI.FocusControl(null);
            }

            GUI.enabled = examples.Count > 0;
            if (GUILayout.Button("Reset"))
            {
                ResetSelectedRecipe();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (examples.Count > 0)
            {
                var entry = examples[selectedIndex];
                GUILayout.Label($"Name: {entry.DisplayName}");
                GUILayout.Label($"Path: {entry.FilePath}");

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(140f));
                if (entry.RecipeInstance != null && entry.RecipeInstance.layers != null)
                {
                    for (int i = 0; i < entry.RecipeInstance.layers.Count; i++)
                    {
                        var layer = entry.RecipeInstance.layers[i];
                        if (layer == null)
                        {
                            continue;
                        }

                        string modifierSummary = "None";
                        if (layer.modifiers != null && layer.modifiers.Count > 0)
                        {
                            var modifierNames = new List<string>();
                            foreach (var modifier in layer.modifiers)
                            {
                                if (modifier == null)
                                {
                                    continue;
                                }

                                modifierNames.Add(modifier.GetType().Name.Replace("Modifier", string.Empty));
                            }

                            if (modifierNames.Count > 0)
                            {
                                modifierSummary = string.Join(", ", modifierNames);
                            }
                        }

                        GUILayout.Label($"Layer {i + 1}: {layer.pattern} | Stars: {layer.starCount} | Modifiers: {modifierSummary}");
                    }
                }
                GUILayout.EndScrollView();

                var estimate = entry.Estimate;
                GUILayout.Label($"Peak Particles: {estimate.EstimatedPeakParticles:N0}");
                GUILayout.Label($"Draw Calls: {estimate.EstimatedDrawCalls}");
                GUILayout.Label($"Lifetime Sum: {estimate.TotalParticleLifetime:F1}");

                if (estimate.EstimatedPeakParticles > warningParticleThreshold)
                {
                    GUILayout.Label($"⚠ High particle count (> {warningParticleThreshold:N0})");
                }
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000f, 20f));
        }

        private void ApplySelectedRecipe()
        {
            if (examples.Count == 0)
            {
                return;
            }

            if (spawner == null)
            {
                spawner = FindObjectOfType<FireworkSpawner>();
            }

            var entry = examples[selectedIndex];
            if (entry.RecipeInstance == null)
            {
                return;
            }

            entry.Estimate = spawner != null ? spawner.EstimateCost(entry.RecipeInstance) : RecipeCostUtility.Estimate(entry.RecipeInstance);
            if (spawner != null)
            {
                spawner.SetActiveRecipe(entry.RecipeInstance);
            }
        }

        private void ResetSelectedRecipe()
        {
            if (examples.Count == 0)
            {
                return;
            }

            var entry = examples[selectedIndex];
            if (entry.RecipeInstance == null || string.IsNullOrEmpty(entry.DefaultJson))
            {
                return;
            }

            try
            {
                entry.RecipeInstance.ImportFromJson(entry.DefaultJson);
                entry.Estimate = spawner != null ? spawner.EstimateCost(entry.RecipeInstance) : RecipeCostUtility.Estimate(entry.RecipeInstance);
                if (spawner != null)
                {
                    spawner.SetActiveRecipe(entry.RecipeInstance);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"RecipeQuickUI: Failed to reset recipe '{entry.DisplayName}'. {ex.Message}");
            }
        }
    }
}

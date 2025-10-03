using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PyroLab.Fireworks.Editor
{
    public class RecipeEditorWindow : EditorWindow
    {
        private FireworkRecipe currentRecipe;
        private SerializedObject serializedRecipe;
        private ReorderableList layerList;
        private Vector2 scroll;

        private FireworkLauncher previewLauncher;
        private ParticleSystem previewLift;
        private ParticleSystem previewBurst;
        private Light previewLight;

        private readonly Dictionary<int, bool> layerFoldouts = new();

        [MenuItem("PYRO/Recipe Editor", priority = 0)]
        public static void Open()
        {
            var window = GetWindow<RecipeEditorWindow>("Recipe Editor");
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            TryLoadSelection();
        }

        private void OnDisable()
        {
            if (previewLauncher != null)
            {
                DestroyImmediate(previewLauncher.gameObject);
            }
        }

        private void OnSelectionChange()
        {
            TryLoadSelection();
            Repaint();
        }

        private void TryLoadSelection()
        {
            if (Selection.activeObject is FireworkRecipe recipe)
            {
                LoadRecipe(recipe);
            }
        }

        private void LoadRecipe(FireworkRecipe recipe)
        {
            if (recipe == currentRecipe)
            {
                return;
            }

            currentRecipe = recipe;
            serializedRecipe = recipe != null ? new SerializedObject(recipe) : null;
            ConfigureLayerList();
        }

        private void ConfigureLayerList()
        {
            if (serializedRecipe == null)
            {
                layerList = null;
                return;
            }

            layerFoldouts.Clear();
            var layersProperty = serializedRecipe.FindProperty("layers");
            layerList = new ReorderableList(serializedRecipe, layersProperty, true, true, false, false);
            layerList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Layers");
            };

            layerList.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = layersProperty.GetArrayElementAtIndex(index);
                rect.height = EditorGUI.GetPropertyHeight(element, true);
                rect.y += 2f;
                rect.x += 4f;
                rect.width -= 8f;
                bool expanded = element.isExpanded;
                if (!layerFoldouts.TryGetValue(index, out expanded))
                {
                    expanded = element.isExpanded;
                }

                element.isExpanded = expanded;
                EditorGUI.PropertyField(rect, element, new GUIContent($"Layer {index + 1}"), true);
                layerFoldouts[index] = element.isExpanded;
            };

            layerList.elementHeightCallback = index =>
            {
                var element = layersProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6f;
            };

            layerList.onReorderCallback = list =>
            {
                if (currentRecipe == null)
                {
                    return;
                }

                Undo.RecordObject(currentRecipe, "Reorder Layers");
                serializedRecipe.ApplyModifiedProperties();
                EditorUtility.SetDirty(currentRecipe);
            };
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (currentRecipe == null)
            {
                EditorGUILayout.HelpBox("Select or create a FireworkRecipe asset to begin.", MessageType.Info);
                return;
            }

            if (serializedRecipe == null)
            {
                return;
            }

            serializedRecipe.Update();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.PropertyField(serializedRecipe.FindProperty("size"));
            EditorGUILayout.PropertyField(serializedRecipe.FindProperty("desiredBurstHeight"));
            EditorGUILayout.PropertyField(serializedRecipe.FindProperty("fuseTime"));
            EditorGUILayout.PropertyField(serializedRecipe.FindProperty("sizeOverLifetime"));
            EditorGUILayout.PropertyField(serializedRecipe.FindProperty("colorOverLifetime"));
            EditorGUILayout.PropertyField(serializedRecipe.FindProperty("hdrIntensity"));

            EditorGUILayout.Space(8f);
            if (layerList != null)
            {
                layerList.DoLayoutList();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Layer"))
            {
                AddLayer();
            }

            EditorGUI.BeginDisabledGroup(layerList == null || layerList.index < 0 || layerList.index >= currentRecipe.layers.Count);
            if (GUILayout.Button("Duplicate Layer"))
            {
                DuplicateLayer(layerList.index);
            }

            if (GUILayout.Button("Delete Layer"))
            {
                RemoveLayer(layerList.index);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8f);
            EditorGUILayout.PropertyField(serializedRecipe.FindProperty("timing"), true);
            EditorGUILayout.EndScrollView();

            if (serializedRecipe.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(currentRecipe);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var selected = (FireworkRecipe)EditorGUILayout.ObjectField(currentRecipe, typeof(FireworkRecipe), false, GUILayout.MinWidth(160f));
                if (selected != currentRecipe)
                {
                    LoadRecipe(selected);
                }

                if (GUILayout.Button("Select Asset", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                {
                    if (currentRecipe != null)
                    {
                        Selection.activeObject = currentRecipe;
                    }
                }

                if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    CreateNewRecipeAsset();
                }

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(currentRecipe == null);
                if (GUILayout.Button("Preview", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    PlayPreview();
                }

                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                {
                    try
                    {
                        RecipeJsonUtility.ExportWithDialog(currentRecipe);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                if (GUILayout.Button("Import JSON", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                {
                    try
                    {
                        RecipeJsonUtility.OverwriteFromJsonWithDialog(currentRecipe);
                        serializedRecipe.Update();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Import as New", EditorStyles.toolbarButton, GUILayout.Width(110f)))
                {
                    try
                    {
                        var imported = RecipeJsonUtility.ImportAsNewAssetWithDialog();
                        if (imported != null)
                        {
                            LoadRecipe(imported);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private void CreateNewRecipeAsset()
        {
            RecipeJsonUtility.EnsureAssetFolder();
            string path = EditorUtility.SaveFilePanelInProject("Create Firework Recipe", "NewFireworkRecipe", "asset", "Select a save location for the new recipe.", RecipeJsonUtility.AssetFolderRelativePath);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var recipe = ScriptableObject.CreateInstance<FireworkRecipe>();
            AssetDatabase.CreateAsset(recipe, path);
            EditorUtility.SetDirty(recipe);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = recipe;
            LoadRecipe(recipe);
        }

        private void AddLayer()
        {
            if (currentRecipe == null)
            {
                return;
            }

            Undo.RecordObject(currentRecipe, "Add Layer");
            currentRecipe.layers ??= new List<FireworkLayer>();
            currentRecipe.layers.Add(new FireworkLayer());
            serializedRecipe.Update();
            if (layerList != null)
            {
                layerList.index = currentRecipe.layers.Count - 1;
            }
            EditorUtility.SetDirty(currentRecipe);
        }

        private void DuplicateLayer(int index)
        {
            if (currentRecipe == null || index < 0 || index >= currentRecipe.layers.Count)
            {
                return;
            }

            Undo.RecordObject(currentRecipe, "Duplicate Layer");
            var clone = CloneLayer(currentRecipe.layers[index]);
            currentRecipe.layers.Insert(index + 1, clone);
            serializedRecipe.Update();
            if (layerList != null)
            {
                layerList.index = index + 1;
            }
            EditorUtility.SetDirty(currentRecipe);
        }

        private void RemoveLayer(int index)
        {
            if (currentRecipe == null || index < 0 || index >= currentRecipe.layers.Count)
            {
                return;
            }

            Undo.RecordObject(currentRecipe, "Remove Layer");
            currentRecipe.layers.RemoveAt(index);
            serializedRecipe.Update();
            if (layerList != null)
            {
                layerList.index = Mathf.Clamp(index, 0, currentRecipe.layers.Count - 1);
            }
            EditorUtility.SetDirty(currentRecipe);
        }

        private static FireworkLayer CloneLayer(FireworkLayer source)
        {
            var clone = new FireworkLayer
            {
                pattern = source.pattern,
                starCount = source.starCount,
                speedMin = source.speedMin,
                speedMax = source.speedMax,
                spread = source.spread,
                layerColor = source.layerColor,
                lifetime = source.lifetime,
                radius = source.radius,
                ringThickness = source.ringThickness,
                palmArcCount = source.palmArcCount,
                palmBend = source.palmBend,
                pistilRadius = source.pistilRadius,
                projectionMask = source.projectionMask,
                layeredShellRadii = source.layeredShellRadii != null ? new List<float>(source.layeredShellRadii) : new List<float>()
            };

            if (source.modifiers != null)
            {
                foreach (var modifier in source.modifiers)
                {
                    if (modifier == null)
                    {
                        continue;
                    }

                    var instance = Object.Instantiate(modifier);
                    instance.hideFlags = HideFlags.DontSave;
                    clone.modifiers.Add(instance);
                }
            }

            return clone;
        }

        private void PlayPreview()
        {
            if (currentRecipe == null)
            {
                return;
            }

            EnsurePreviewSystems();
            previewLauncher.Recipe = currentRecipe;
            previewLauncher.ResetAndLaunch();
        }

        private void EnsurePreviewSystems()
        {
            if (previewLauncher != null)
            {
                return;
            }

            var previewGo = new GameObject("FireworkRecipePreview")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            previewLauncher = previewGo.AddComponent<FireworkLauncher>();
            previewLauncher.hideFlags = HideFlags.HideAndDontSave;

            previewLift = previewGo.AddComponent<ParticleSystem>();
            previewLift.hideFlags = HideFlags.HideAndDontSave;

            previewBurst = previewGo.AddComponent<ParticleSystem>();
            previewBurst.hideFlags = HideFlags.HideAndDontSave;

            previewLight = previewGo.AddComponent<Light>();
            previewLight.type = LightType.Point;
            previewLight.range = 12f;
            previewLight.hideFlags = HideFlags.HideAndDontSave;

            var type = typeof(FireworkLauncher);
            type.GetField("liftSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(previewLauncher, previewLift);
            type.GetField("burstSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(previewLauncher, previewBurst);
            type.GetField("bloomLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(previewLauncher, previewLight);
        }
    }
}

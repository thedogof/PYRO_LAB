using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PyroLab.Fireworks.Editor
{
    [CustomEditor(typeof(FireworkRecipe))]
    public class FireworkRecipeEditor : UnityEditor.Editor
    {
        private readonly Dictionary<int, bool> layerFoldouts = new();
        private readonly Dictionary<int, bool> timingFoldouts = new();
        private Type[] modifierTypes;

        private FireworkLauncher previewLauncher;
        private ParticleSystem previewLift;
        private ParticleSystem previewBurst;
        private Light previewLight;

        private void OnEnable()
        {
            modifierTypes = GetModifierTypes();
        }

        public override void OnInspectorGUI()
        {
            var recipe = (FireworkRecipe)target;
            if (recipe == null)
            {
                return;
            }

            Undo.RecordObject(recipe, "Modify Firework Recipe");
            EditorGUI.BeginChangeCheck();
            DrawGlobalSettings(recipe);
            EditorGUILayout.Space(8f);
            DrawLayerComposer(recipe);
            EditorGUILayout.Space(8f);
            DrawTimingTrack(recipe);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(recipe);
            }

            EditorGUILayout.Space(12f);
            DrawComposerFooter(recipe);
        }

        private void DrawGlobalSettings(FireworkRecipe recipe)
        {
            EditorGUILayout.LabelField("Global", EditorStyles.boldLabel);
            recipe.size = EditorGUILayout.Slider("Size", recipe.size, 0.1f, 5f);
            recipe.desiredBurstHeight = EditorGUILayout.FloatField("Desired Burst Height", recipe.desiredBurstHeight);
            recipe.fuseTime = EditorGUILayout.FloatField("Fuse Time", recipe.fuseTime);
            recipe.sizeOverLifetime = EditorGUILayout.CurveField("Size Over Lifetime", recipe.sizeOverLifetime);
            recipe.colorOverLifetime = EditorGUILayout.GradientField("Color Over Lifetime", recipe.colorOverLifetime);
            recipe.hdrIntensity = EditorGUILayout.FloatField("HDR Intensity", recipe.hdrIntensity);
        }

        private void DrawLayerComposer(FireworkRecipe recipe)
        {
            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
            if (recipe.layers == null)
            {
                recipe.layers = new List<FireworkLayer>();
            }

            for (int i = 0; i < recipe.layers.Count; i++)
            {
                var layer = recipe.layers[i];
                if (!layerFoldouts.TryGetValue(i, out var foldout))
                {
                    foldout = true;
                }

                string header = $"Layer {i + 1}: {(layer != null ? layer.pattern.ToString() : "Undefined")}";
                foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, header);
                layerFoldouts[i] = foldout;
                if (foldout)
                {
                    if (layer == null)
                    {
                        recipe.layers[i] = new FireworkLayer();
                        layer = recipe.layers[i];
                    }

                    EditorGUI.indentLevel++;
                    layer.pattern = (BurstPatternType)EditorGUILayout.EnumPopup("Pattern", layer.pattern);
                    layer.starCount = EditorGUILayout.IntSlider("Star Count", layer.starCount, 4, 4096);
                    layer.speedMin = EditorGUILayout.FloatField("Speed Min", layer.speedMin);
                    layer.speedMax = EditorGUILayout.FloatField("Speed Max", layer.speedMax);
                    layer.spread = EditorGUILayout.Slider("Spread", layer.spread, 0f, 1f);
                    layer.layerColor = EditorGUILayout.GradientField("Layer Gradient", layer.layerColor);
                    layer.lifetime = EditorGUILayout.FloatField("Lifetime", layer.lifetime);
                    layer.radius = EditorGUILayout.FloatField("Radius", layer.radius);
                    if (layer.pattern == BurstPatternType.Ring || layer.pattern == BurstPatternType.PistilRing || layer.pattern == BurstPatternType.Projected2D)
                    {
                        layer.ringThickness = EditorGUILayout.Slider("Ring Thickness", layer.ringThickness, 0f, 1f);
                    }

                    if (layer.pattern == BurstPatternType.Palm)
                    {
                        layer.palmArcCount = EditorGUILayout.FloatField("Palm Arc Count", layer.palmArcCount);
                        layer.palmBend = EditorGUILayout.Slider("Palm Bend", layer.palmBend, 0f, 1f);
                    }

                    if (layer.pattern == BurstPatternType.PistilRing)
                    {
                        layer.pistilRadius = EditorGUILayout.Slider("Pistil Radius", layer.pistilRadius, 0f, 1f);
                    }

                    if (layer.pattern == BurstPatternType.Projected2D)
                    {
                        layer.projectionMask = (Texture2D)EditorGUILayout.ObjectField("Projection Mask", layer.projectionMask, typeof(Texture2D), false);
                    }

                    if (layer.pattern == BurstPatternType.LayeredShells)
                    {
                        DrawLayeredRadiusList(layer);
                    }

                    DrawModifierList(recipe, layer);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Duplicate", GUILayout.Width(90f)))
                {
                    Undo.RecordObject(recipe, "Duplicate Layer");
                    recipe.layers.Insert(i + 1, CloneLayer(layer));
                    EditorUtility.SetDirty(recipe);
                    return;
                }
                if (GUILayout.Button("Remove", GUILayout.Width(80f)))
                {
                    Undo.RecordObject(recipe, "Remove Layer");
                    recipe.layers.RemoveAt(i);
                    EditorUtility.SetDirty(recipe);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(6f);
            }

            if (GUILayout.Button("Add Layer"))
            {
                Undo.RecordObject(recipe, "Add Layer");
                recipe.layers.Add(new FireworkLayer());
                EditorUtility.SetDirty(recipe);
            }
        }

        private void DrawLayeredRadiusList(FireworkLayer layer)
        {
            EditorGUILayout.LabelField("Shell Radii", EditorStyles.miniBoldLabel);
            if (layer.layeredShellRadii == null)
            {
                layer.layeredShellRadii = new List<float>();
            }

            for (int r = 0; r < layer.layeredShellRadii.Count; r++)
            {
                EditorGUILayout.BeginHorizontal();
                layer.layeredShellRadii[r] = EditorGUILayout.Slider($"Shell {r + 1}", layer.layeredShellRadii[r], 0.05f, 2f);
                if (GUILayout.Button("✕", GUILayout.Width(24f)))
                {
                    layer.layeredShellRadii.RemoveAt(r);
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Radius"))
            {
                layer.layeredShellRadii.Add(1f);
            }
        }

        private void DrawModifierList(FireworkRecipe recipe, FireworkLayer layer)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Modifiers", EditorStyles.miniBoldLabel);
            if (layer.modifiers == null)
            {
                layer.modifiers = new List<VisualModifier>();
            }

            for (int i = 0; i < layer.modifiers.Count; i++)
            {
                var modifier = layer.modifiers[i];
                EditorGUILayout.BeginHorizontal();
                layer.modifiers[i] = (VisualModifier)EditorGUILayout.ObjectField(modifier, typeof(VisualModifier), false);
                if (modifier != null && GUILayout.Button("Select", GUILayout.Width(60f)))
                {
                    Selection.activeObject = modifier;
                }
                if (GUILayout.Button("▲", GUILayout.Width(24f)) && i > 0)
                {
                    (layer.modifiers[i - 1], layer.modifiers[i]) = (layer.modifiers[i], layer.modifiers[i - 1]);
                }
                if (GUILayout.Button("▼", GUILayout.Width(24f)) && i < layer.modifiers.Count - 1)
                {
                    (layer.modifiers[i + 1], layer.modifiers[i]) = (layer.modifiers[i], layer.modifiers[i + 1]);
                }
                if (GUILayout.Button("✕", GUILayout.Width(24f)))
                {
                    Undo.RecordObject(recipe, "Remove Modifier");
                    layer.modifiers.RemoveAt(i);
                    EditorUtility.SetDirty(recipe);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                if (layer.modifiers.Count > i && layer.modifiers[i] != null)
                {
                    EditorGUI.indentLevel++;
                    layer.modifiers[i].isEnabled = EditorGUILayout.Toggle("Enabled", layer.modifiers[i].isEnabled);
                    EditorGUI.indentLevel--;
                }
            }

            if (GUILayout.Button("Add Modifier"))
            {
                ShowModifierMenu(recipe, layer);
            }
        }

        private void ShowModifierMenu(FireworkRecipe recipe, FireworkLayer layer)
        {
            modifierTypes ??= GetModifierTypes();
            var menu = new GenericMenu();
            foreach (var type in modifierTypes)
            {
                string name = ObjectNames.NicifyVariableName(type.Name.Replace("Modifier", string.Empty));
                menu.AddItem(new GUIContent(name), false, () =>
                {
                    Undo.RecordObject(recipe, "Add Modifier");
                    var instance = CreateInstance(type) as VisualModifier;
                    if (instance != null)
                    {
                        instance.hideFlags = HideFlags.DontSave;
                        layer.modifiers.Add(instance);
                        EditorUtility.SetDirty(recipe);
                    }
                });
            }

            menu.ShowAsContext();
        }

        private Type[] GetModifierTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeGetTypes(a))
                .Where(t => typeof(VisualModifier).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }

        private void DrawTimingTrack(FireworkRecipe recipe)
        {
            EditorGUILayout.LabelField("Timing Track", EditorStyles.boldLabel);
            recipe.timing ??= new TimingTrack();
            recipe.timing.events ??= new List<TimingEvent>();

            for (int i = 0; i < recipe.timing.events.Count; i++)
            {
                var evt = recipe.timing.events[i];
                if (!timingFoldouts.TryGetValue(i, out var foldout))
                {
                    foldout = false;
                }

                string label = string.IsNullOrEmpty(evt.name) ? $"Event {i + 1}" : evt.name;
                foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
                timingFoldouts[i] = foldout;
                if (foldout)
                {
                    EditorGUI.indentLevel++;
                    evt.name = EditorGUILayout.TextField("Name", evt.name);
                    evt.time = EditorGUILayout.Slider("Normalized Time", evt.time, 0f, 1f);
                    evt.action = (TimingAction)EditorGUILayout.EnumPopup("Action", evt.action);
                    evt.layerIndex = EditorGUILayout.IntSlider("Layer", evt.layerIndex, 0, Mathf.Max(0, recipe.layers.Count - 1));
                    if (evt.action == TimingAction.ColorShift || evt.action == TimingAction.StrobeToggle)
                    {
                        int modifierCount = recipe.layers.Count == 0 ? 0 : recipe.layers[Mathf.Clamp(evt.layerIndex, 0, recipe.layers.Count - 1)].modifiers.Count;
                        evt.modifierIndex = EditorGUILayout.IntSlider("Modifier", evt.modifierIndex, 0, Mathf.Max(0, modifierCount - 1));
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(80f)))
                {
                    recipe.timing.events.RemoveAt(i);
                    EditorUtility.SetDirty(recipe);
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Event"))
            {
                recipe.timing.events.Add(new TimingEvent { name = "New Event", time = 0.5f });
                EditorUtility.SetDirty(recipe);
            }
        }

        private FireworkLayer CloneLayer(FireworkLayer source)
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

                    var instance = Instantiate(modifier);
                    instance.hideFlags = HideFlags.DontSave;
                    clone.modifiers.Add(instance);
                }
            }

            return clone;
        }

        private void DrawComposerFooter(FireworkRecipe recipe)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview"))
            {
                PlayPreview(recipe);
            }

            if (GUILayout.Button("Export JSON"))
            {
                ExportJson(recipe);
            }

            if (GUILayout.Button("Import JSON"))
            {
                ImportJson(recipe);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void PlayPreview(FireworkRecipe recipe)
        {
            EnsurePreviewSystems();
            previewLauncher.Recipe = recipe;
            previewLauncher.ResetAndLaunch();
        }

        private void ExportJson(FireworkRecipe recipe)
        {
            try
            {
                RecipeJsonUtility.ExportWithDialog(recipe);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ImportJson(FireworkRecipe recipe)
        {
            try
            {
                RecipeJsonUtility.OverwriteFromJsonWithDialog(recipe);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
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
            type.GetField("liftSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(previewLauncher, previewLift);
            type.GetField("burstSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(previewLauncher, previewBurst);
            type.GetField("bloomLight", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(previewLauncher, previewLight);
        }

        private void OnDisable()
        {
            if (previewLauncher != null)
            {
                DestroyImmediate(previewLauncher.gameObject);
            }
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

namespace PyroLab.Fireworks.Editor
{
    [CustomEditor(typeof(FireworkRecipe))]
    public class FireworkRecipeEditor : UnityEditor.Editor
    {
        private FireworkLauncher previewLauncher;
        private ParticleSystem previewLift;
        private ParticleSystem previewBurst;
        private const string DefaultExportFileName = "FireworkRecipe.json";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10f);
            if (GUILayout.Button("Preview Burst"))
            {
                PlayPreview();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export JSON"))
            {
                ExportJson();
            }

            if (GUILayout.Button("Import JSON"))
            {
                ImportJson();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void PlayPreview()
        {
            var recipe = (FireworkRecipe)target;
            EnsurePreviewSystems();
            previewLauncher.Recipe = recipe;
            previewLauncher.ResetAndLaunch();
        }

        private void EnsurePreviewSystems()
        {
            if (previewLauncher != null)
            {
                return;
            }

            var previewGo = new GameObject("FireworkRecipePreview");
            previewGo.hideFlags = HideFlags.HideAndDontSave;
            previewLauncher = previewGo.AddComponent<FireworkLauncher>();

            previewLift = previewGo.AddComponent<ParticleSystem>();
            previewBurst = previewGo.AddComponent<ParticleSystem>();
            var previewLight = previewGo.AddComponent<Light>();
            previewLight.type = LightType.Point;
            previewLight.range = 8f;

            previewLauncher.hideFlags = HideFlags.HideAndDontSave;
            previewLift.hideFlags = HideFlags.HideAndDontSave;
            previewBurst.hideFlags = HideFlags.HideAndDontSave;

            previewLauncher.GetType().GetField("liftSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(previewLauncher, previewLift);
            previewLauncher.GetType().GetField("burstSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(previewLauncher, previewBurst);
            previewLauncher.GetType().GetField("bloomLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(previewLauncher, previewLight);
        }

        private void ExportJson()
        {
            var recipe = (FireworkRecipe)target;
            string path = EditorUtility.SaveFilePanel("Export Firework Recipe", Application.dataPath, DefaultExportFileName, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllText(path, recipe.ExportToJson());
            AssetDatabase.Refresh();
        }

        private void ImportJson()
        {
            var recipe = (FireworkRecipe)target;
            string path = EditorUtility.OpenFilePanel("Import Firework Recipe", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string json = File.ReadAllText(path);
            recipe.ImportFromJson(json);
            EditorUtility.SetDirty(recipe);
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

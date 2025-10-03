using System.IO;
using UnityEditor;
using UnityEngine;

namespace PyroLab.Fireworks.Editor
{
    public static class JsonPresetExporter
    {
        public static void Export(FireworkRecipe recipe, string path)
        {
            if (recipe == null || string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Cannot export firework preset. Invalid recipe or path.");
                return;
            }

            string json = recipe.ExportToJson();
            File.WriteAllText(path, json);
            Debug.Log($"Firework preset exported to {path}");
            AssetDatabase.Refresh();
        }

        public static void Import(FireworkRecipe recipe, string path)
        {
            if (recipe == null || string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Cannot import firework preset. Invalid recipe or path.");
                return;
            }

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Preset file not found at {path}");
                return;
            }

            string json = File.ReadAllText(path);
            recipe.ImportFromJson(json);
            EditorUtility.SetDirty(recipe);
            Debug.Log($"Firework preset imported from {path}");
        }
    }
}

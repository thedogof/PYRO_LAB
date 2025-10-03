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

            RecipeJsonUtility.ExportToPath(recipe, path);
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

            RecipeJsonUtility.OverwriteFromJson(recipe, path);
        }
    }
}

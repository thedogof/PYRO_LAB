using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Recipe Catalog", fileName = "RecipeCatalog")]
    public class RecipeCatalog : ScriptableObject
    {
        [SerializeField] private List<FireworkRecipe> recipes = new();

        public IReadOnlyList<FireworkRecipe> Recipes => recipes;

        public FireworkRecipe GetRecipeByName(string recipeName)
        {
            if (string.IsNullOrWhiteSpace(recipeName))
            {
                return null;
            }

            return recipes.FirstOrDefault(r => r != null && r.name == recipeName);
        }

        public bool TryGetRecipe(int index, out FireworkRecipe recipe)
        {
            if (index < 0 || index >= recipes.Count)
            {
                recipe = null;
                return false;
            }

            recipe = recipes[index];
            return recipe != null;
        }

#if UNITY_EDITOR
        public void AddRecipe(FireworkRecipe recipe)
        {
            if (recipe == null || recipes.Contains(recipe))
            {
                return;
            }

            recipes.Add(recipe);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

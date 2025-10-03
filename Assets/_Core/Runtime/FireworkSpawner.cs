using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public class FireworkSpawner : MonoBehaviour
    {
        [SerializeField] private List<FireworkLauncher> launchers = new();
        [SerializeField] private List<FireworkRecipe> recipes = new();
        [SerializeField] private float interval = 6f;
        [SerializeField] private bool autoLoop = true;
        [SerializeField] private KeyCode triggerKey = KeyCode.Space;
        [SerializeField] private float sizeScale = 1f;
        [SerializeField] private float heightScale = 1f;

        private readonly Dictionary<FireworkRecipe, FireworkRecipe> scaledCache = new();
        private float timer;
        private int nextRecipeIndex;

        public IReadOnlyList<FireworkRecipe> Recipes => recipes;

        private void Update()
        {
            if (autoLoop && recipes.Count > 0)
            {
                timer += Time.deltaTime;
                if (timer >= interval)
                {
                    timer = 0f;
                    LaunchNext();
                }
            }

            if (Input.GetKeyDown(triggerKey))
            {
                LaunchNext();
            }
        }

        public void LaunchNext()
        {
            if (recipes.Count == 0 || launchers.Count == 0)
            {
                return;
            }

            var recipe = recipes[nextRecipeIndex];
            var scaledRecipe = GetScaledRecipe(recipe);
            foreach (var launcher in launchers)
            {
                if (launcher == null)
                {
                    continue;
                }

                launcher.Recipe = scaledRecipe;
                launcher.ResetAndLaunch();
            }

            nextRecipeIndex = (nextRecipeIndex + 1) % recipes.Count;
        }

        private FireworkRecipe GetScaledRecipe(FireworkRecipe original)
        {
            if (original == null)
            {
                return null;
            }

            if (!scaledCache.TryGetValue(original, out var clone) || clone == null)
            {
                clone = Instantiate(original);
                clone.hideFlags = HideFlags.DontSave;
                scaledCache[original] = clone;
            }

            clone.size = original.size * Mathf.Max(0.01f, sizeScale);
            clone.desiredBurstHeight = original.desiredBurstHeight * Mathf.Max(0.01f, heightScale);
            clone.fuseTime = original.fuseTime;
            clone.sizeOverLifetime = original.sizeOverLifetime;
            clone.colorOverLifetime = original.colorOverLifetime;
            clone.hdrIntensity = original.hdrIntensity;
            clone.layers.Clear();
            foreach (var layer in original.layers)
            {
                if (layer == null)
                {
                    continue;
                }

                var layerClone = new FireworkLayer
                {
                    pattern = layer.pattern,
                    starCount = layer.starCount,
                    speedMin = layer.speedMin,
                    speedMax = layer.speedMax,
                    spread = layer.spread,
                    layerColor = layer.layerColor,
                    lifetime = layer.lifetime,
                    radius = layer.radius,
                    ringThickness = layer.ringThickness,
                    palmArcCount = layer.palmArcCount,
                    palmBend = layer.palmBend,
                    pistilRadius = layer.pistilRadius,
                    projectionMask = layer.projectionMask,
                    layeredShellRadii = new List<float>(layer.layeredShellRadii)
                };

                foreach (var modifier in layer.modifiers)
                {
                    if (modifier == null)
                    {
                        continue;
                    }

                    var modifierClone = Instantiate(modifier);
                    modifierClone.hideFlags = HideFlags.DontSave;
                    layerClone.modifiers.Add(modifierClone);
                }

                clone.layers.Add(layerClone);
            }

            clone.timing = original.timing != null ? original.timing.Clone() : new TimingTrack();
            return clone;
        }

        public void SetActiveRecipe(FireworkRecipe recipe)
        {
            recipes.Clear();
            if (recipe != null)
            {
                recipes.Add(recipe);
            }

            nextRecipeIndex = 0;
            timer = 0f;
            scaledCache.Clear();
        }

        public void SetRecipes(IEnumerable<FireworkRecipe> newRecipes)
        {
            recipes.Clear();
            if (newRecipes != null)
            {
                foreach (var recipe in newRecipes)
                {
                    if (recipe != null)
                    {
                        recipes.Add(recipe);
                    }
                }
            }

            nextRecipeIndex = 0;
            timer = 0f;
            scaledCache.Clear();
        }

        public void LoadCatalog(RecipeCatalog catalog)
        {
            if (catalog == null)
            {
                return;
            }

            SetRecipes(catalog.Recipes);
        }

        public RecipeCostEstimate EstimateCost(FireworkRecipe recipe)
        {
            return RecipeCostUtility.Estimate(recipe);
        }
    }
}

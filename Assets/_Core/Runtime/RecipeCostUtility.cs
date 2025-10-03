using UnityEngine;

namespace PyroLab.Fireworks
{
    public readonly struct RecipeCostEstimate
    {
        public readonly int LayerCount;
        public readonly int TotalStarCount;
        public readonly int EstimatedPeakParticles;
        public readonly int EstimatedDrawCalls;
        public readonly float TotalParticleLifetime;

        public RecipeCostEstimate(int layerCount, int totalStarCount, int estimatedPeakParticles, int estimatedDrawCalls, float totalParticleLifetime)
        {
            LayerCount = layerCount;
            TotalStarCount = totalStarCount;
            EstimatedPeakParticles = estimatedPeakParticles;
            EstimatedDrawCalls = estimatedDrawCalls;
            TotalParticleLifetime = totalParticleLifetime;
        }
    }

    public static class RecipeCostUtility
    {
        private const int TrailAdditionalDrawCalls = 1;

        public static RecipeCostEstimate Estimate(FireworkRecipe recipe)
        {
            if (recipe == null)
            {
                return new RecipeCostEstimate(0, 0, 0, 0, 0f);
            }

            int layerCount = 0;
            int totalStars = 0;
            int peakParticles = 0;
            int drawCalls = 0;
            float totalLifetime = 0f;

            var layers = recipe.layers;
            if (layers != null)
            {
                foreach (var layer in layers)
                {
                    if (layer == null)
                    {
                        continue;
                    }

                    layerCount++;
                    drawCalls++;

                    int starCount = Mathf.Max(0, layer.starCount);
                    totalStars += starCount;

                    int multiplier = 1;
                    bool hasTrails = false;

                    if (layer.modifiers != null)
                    {
                        foreach (var modifier in layer.modifiers)
                        {
                            if (modifier == null || !modifier.isEnabled)
                            {
                                continue;
                            }

                            switch (modifier)
                            {
                                case SplitModifier split:
                                    multiplier = Mathf.Max(multiplier, Mathf.Max(1, split.splitCount));
                                    break;
                                case TrailModifier:
                                    hasTrails = true;
                                    break;
                            }
                        }
                    }

                    peakParticles += starCount * multiplier;
                    totalLifetime += starCount * multiplier * Mathf.Max(0f, layer.lifetime);

                    if (hasTrails)
                    {
                        drawCalls += TrailAdditionalDrawCalls;
                    }
                }
            }

            return new RecipeCostEstimate(layerCount, totalStars, peakParticles, drawCalls, totalLifetime);
        }
    }
}

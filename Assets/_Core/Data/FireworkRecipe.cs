using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Firework Recipe")]
    public class FireworkRecipe : ScriptableObject
    {
        [Header("Global")]
        [Min(0.1f)] public float size = 1f;
        [Min(0f)] public float desiredBurstHeight = 80f;
        [Min(0.1f)] public float fuseTime = 2f;
        public AnimationCurve sizeOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);
        public Gradient colorOverLifetime = FireworkLayer.DefaultLayerGradient();
        [Min(0f)] public float hdrIntensity = 2f;

        [Header("Composition")]
        public List<FireworkLayer> layers = new();
        public TimingTrack timing = new();

        public float GetScaledBurstHeight() => desiredBurstHeight * size;

        public float GetAverageSpeed(FireworkLayer layer)
        {
            if (layer == null)
            {
                return 0f;
            }

            return ((layer.speedMin + layer.speedMax) * 0.5f) * size;
        }

        public string ExportToJson()
        {
            var dto = FireworkRecipeDto.FromRecipe(this);
            return JsonUtility.ToJson(dto, true);
        }

        public void ImportFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("JSON content is null or empty.");
            }

            var dto = JsonUtility.FromJson<FireworkRecipeDto>(json);
            if (dto == null)
            {
                throw new ArgumentException("Invalid recipe JSON.");
            }

            dto.ApplyTo(this);
        }

        [Serializable]
        private class FireworkRecipeDto
        {
            public float size;
            public float desiredBurstHeight;
            public float fuseTime;
            public AnimationCurve sizeOverLifetime;
            public GradientWrapper colorOverLifetime;
            public float hdrIntensity;
            public List<FireworkLayerDto> layers;
            public TimingTrack timing;

            public static FireworkRecipeDto FromRecipe(FireworkRecipe recipe)
            {
                var dto = new FireworkRecipeDto
                {
                    size = recipe.size,
                    desiredBurstHeight = recipe.desiredBurstHeight,
                    fuseTime = recipe.fuseTime,
                    sizeOverLifetime = recipe.sizeOverLifetime,
                    colorOverLifetime = GradientWrapper.FromGradient(recipe.colorOverLifetime),
                    hdrIntensity = recipe.hdrIntensity,
                    layers = new List<FireworkLayerDto>(),
                    timing = recipe.timing != null ? recipe.timing.Clone() : new TimingTrack()
                };

                foreach (var layer in recipe.layers)
                {
                    dto.layers.Add(FireworkLayerDto.FromLayer(layer));
                }

                return dto;
            }

            public void ApplyTo(FireworkRecipe recipe)
            {
                recipe.size = size;
                recipe.desiredBurstHeight = desiredBurstHeight;
                recipe.fuseTime = fuseTime;
                recipe.sizeOverLifetime = sizeOverLifetime ?? AnimationCurve.Linear(0f, 1f, 1f, 0.2f);
                recipe.colorOverLifetime = colorOverLifetime?.ToGradient() ?? FireworkLayer.DefaultLayerGradient();
                recipe.hdrIntensity = hdrIntensity;

                recipe.layers ??= new List<FireworkLayer>();
                recipe.layers.Clear();
                if (layers != null)
                {
                    foreach (var dtoLayer in layers)
                    {
                        recipe.layers.Add(dtoLayer.ToLayer());
                    }
                }

                recipe.timing = timing != null ? timing.Clone() : new TimingTrack();
            }
        }

        [Serializable]
        private class FireworkLayerDto
        {
            public BurstPatternType pattern;
            public int starCount;
            public float speedMin;
            public float speedMax;
            public float spread;
            public GradientWrapper layerColor;
            public float lifetime;
            public float radius;
            public float ringThickness;
            public float palmArcCount;
            public float palmBend;
            public float pistilRadius;
            public Texture2D projectionMask;
            public List<float> layeredShellRadii;
            public List<ModifierDto> modifiers;

            public static FireworkLayerDto FromLayer(FireworkLayer layer)
            {
                var dto = new FireworkLayerDto
                {
                    pattern = layer.pattern,
                    starCount = layer.starCount,
                    speedMin = layer.speedMin,
                    speedMax = layer.speedMax,
                    spread = layer.spread,
                    layerColor = GradientWrapper.FromGradient(layer.layerColor),
                    lifetime = layer.lifetime,
                    radius = layer.radius,
                    ringThickness = layer.ringThickness,
                    palmArcCount = layer.palmArcCount,
                    palmBend = layer.palmBend,
                    pistilRadius = layer.pistilRadius,
                    projectionMask = layer.projectionMask,
                    layeredShellRadii = layer.layeredShellRadii != null ? new List<float>(layer.layeredShellRadii) : new List<float>(),
                    modifiers = new List<ModifierDto>()
                };

                foreach (var modifier in layer.modifiers)
                {
                    if (modifier == null)
                    {
                        continue;
                    }

                    dto.modifiers.Add(ModifierDto.FromModifier(modifier));
                }

                return dto;
            }

            public FireworkLayer ToLayer()
            {
                var layer = new FireworkLayer
                {
                    pattern = pattern,
                    starCount = starCount,
                    speedMin = speedMin,
                    speedMax = speedMax,
                    spread = spread,
                    layerColor = layerColor?.ToGradient() ?? FireworkLayer.DefaultLayerGradient(),
                    lifetime = lifetime,
                    radius = radius,
                    ringThickness = ringThickness,
                    palmArcCount = palmArcCount,
                    palmBend = palmBend,
                    pistilRadius = pistilRadius,
                    projectionMask = projectionMask
                };

                layer.layeredShellRadii.Clear();
                if (layeredShellRadii != null)
                {
                    layer.layeredShellRadii.AddRange(layeredShellRadii);
                }

                if (modifiers != null)
                {
                    foreach (var modifierDto in modifiers)
                    {
                        var modifier = modifierDto.ToModifier();
                        if (modifier != null)
                        {
                            layer.modifiers.Add(modifier);
                        }
                    }
                }

                return layer;
            }
        }

        [Serializable]
        private class ModifierDto
        {
            public string type;
            public string json;

            public static ModifierDto FromModifier(VisualModifier modifier)
            {
                return new ModifierDto
                {
                    type = modifier.GetType().AssemblyQualifiedName,
                    json = JsonUtility.ToJson(modifier, true)
                };
            }

            public VisualModifier ToModifier()
            {
                if (string.IsNullOrEmpty(type))
                {
                    return null;
                }

                var modifierType = Type.GetType(type);
                if (modifierType == null)
                {
                    Debug.LogWarning($"Modifier type {type} could not be resolved during import.");
                    return null;
                }

                var modifier = ScriptableObject.CreateInstance(modifierType) as VisualModifier;
                if (modifier == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(json))
                {
                    JsonUtility.FromJsonOverwrite(json, modifier);
                }

                modifier.hideFlags = HideFlags.DontSave;
                return modifier;
            }
        }
    }
}

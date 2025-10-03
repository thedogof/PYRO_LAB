using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public enum BurstPatternType
    {
        Peony,
        Willow,
        Ring,
        Palm,
        PistilRing,
        LayeredShells,
        Projected2D
    }

    [Serializable]
    public class FireworkLayer
    {
        public BurstPatternType pattern = BurstPatternType.Peony;
        [Range(4, 4096)] public int starCount = 300;
        [Min(0.1f)] public float speedMin = 8f;
        [Min(0.1f)] public float speedMax = 14f;
        [Range(0f, 1f)] public float spread = 0f;
        public Gradient layerColor = DefaultLayerGradient();
        [Min(0.1f)] public float lifetime = 3.5f;
        [Min(0f)] public float radius = 1f;
        [Range(0f, 1f)] public float ringThickness = 0.15f;
        [Min(1f)] public float palmArcCount = 5f;
        [Range(0f, 1f)] public float palmBend = 0.3f;
        [Range(0f, 1f)] public float pistilRadius = 0.35f;
        public Texture2D projectionMask;
        public List<float> layeredShellRadii = new() { 0.4f, 0.75f, 1f };
        public List<VisualModifier> modifiers = new();

        public static Gradient DefaultLayerGradient()
        {
            return new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            };
        }
    }
}

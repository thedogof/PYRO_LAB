using System;
using UnityEngine;

namespace PyroLab.Fireworks
{
    [Serializable]
    public class GradientWrapper
    {
        public GradientColorKey[] colorKeys;
        public GradientAlphaKey[] alphaKeys;

        public static GradientWrapper FromGradient(Gradient gradient)
        {
            if (gradient == null)
            {
                return new GradientWrapper
                {
                    colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                };
            }

            return new GradientWrapper
            {
                colorKeys = gradient.colorKeys,
                alphaKeys = gradient.alphaKeys
            };
        }

        public Gradient ToGradient()
        {
            return new Gradient
            {
                colorKeys = colorKeys ?? Array.Empty<GradientColorKey>(),
                alphaKeys = alphaKeys ?? Array.Empty<GradientAlphaKey>()
            };
        }
    }
}

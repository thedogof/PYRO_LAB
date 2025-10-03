using System;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public static class FireworkBurst
    {
        public static void GenerateLayerBurst(FireworkRecipe recipe, FireworkLayer layer, Vector3[] buffer, System.Random random)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length < layer.starCount)
            {
                throw new ArgumentException("Buffer too small for star count.");
            }

            switch (layer.pattern)
            {
                case BurstPatternType.Peony:
                    BurstPatterns.SampleSphereShell(buffer, layer.starCount, layer.spread, random);
                    break;
                case BurstPatternType.Willow:
                    GenerateWillow(buffer, layer.starCount, random);
                    break;
                case BurstPatternType.Ring:
                    BurstPatterns.SampleRing(buffer, layer.starCount, layer.radius, layer.ringThickness, random);
                    break;
                case BurstPatternType.Palm:
                    BurstPatterns.SamplePalm(buffer, layer.starCount, layer.palmArcCount, layer.palmBend, random);
                    break;
                case BurstPatternType.PistilRing:
                    GeneratePistilRing(buffer, layer, random);
                    break;
                case BurstPatternType.LayeredShells:
                    BurstPatterns.SampleLayeredShells(buffer, layer.starCount, layer.layeredShellRadii, random);
                    break;
                case BurstPatternType.Projected2D:
                    BurstPatterns.SampleProjected2D(buffer, layer.starCount, layer.projectionMask, layer.radius, random);
                    break;
                default:
                    BurstPatterns.SampleSphereShell(buffer, layer.starCount, layer.spread, random);
                    break;
            }

            float scale = recipe.size;
            for (int i = 0; i < layer.starCount; i++)
            {
                float speed = Mathf.Lerp(layer.speedMin, layer.speedMax, (float)random.NextDouble());
                buffer[i] = buffer[i].normalized * speed * scale;
            }
        }

        private static void GenerateWillow(Vector3[] buffer, int count, System.Random random)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Random.onUnitSphere;
                dir = Vector3.Slerp(dir, Vector3.down, 0.45f).normalized;
                buffer[i] = dir;
            }
        }

        private static void GeneratePistilRing(Vector3[] buffer, FireworkLayer layer, System.Random random)
        {
            int pistilCount = Mathf.Clamp(Mathf.CeilToInt(layer.starCount * 0.25f), 1, layer.starCount - 1);
            BurstPatterns.SamplePistil(buffer, pistilCount, layer.pistilRadius, random);
            for (int i = pistilCount; i < layer.starCount; i++)
            {
                buffer[i] = Vector3.zero;
            }

            var temp = new Vector3[layer.starCount - pistilCount];
            BurstPatterns.SampleRing(temp, temp.Length, layer.radius, layer.ringThickness, random);
            for (int i = 0; i < temp.Length; i++)
            {
                buffer[pistilCount + i] = temp[i];
            }
        }
    }
}

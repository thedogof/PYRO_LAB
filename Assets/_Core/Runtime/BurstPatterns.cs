using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public static class BurstPatterns
    {
        public static void SampleSphereShell(IList<Vector3> results, int count, float spread, System.Random random)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = RandomPointOnUnitSphere(random);
                if (spread > 0f)
                {
                    dir = Vector3.Slerp(dir, RandomPointOnUnitSphere(random), spread).normalized;
                }

                results[i] = dir;
            }
        }

        public static void SampleRing(IList<Vector3> results, int count, float radius, float thickness, System.Random random)
        {
            float angleStep = Mathf.PI * 2f / Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                float angle = angleStep * i;
                float radialOffset = Mathf.Lerp(-thickness, thickness, (float)random.NextDouble());
                Vector3 dir = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                results[i] = dir.normalized * (radius + radialOffset);
            }
        }

        public static void SampleLayeredShells(IList<Vector3> results, int count, IReadOnlyList<float> radii, System.Random random)
        {
            if (radii == null || radii.Count == 0)
            {
                SampleSphereShell(results, count, 0f, random);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int layerIndex = radii.Count == 1 ? 0 : random.Next(radii.Count);
                float radius = Mathf.Max(0.01f, radii[layerIndex]);
                Vector3 dir = RandomPointOnUnitSphere(random) * radius;
                results[i] = dir;
            }
        }

        public static void SamplePistil(IList<Vector3> results, int count, float innerRadius, System.Random random)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = RandomPointOnUnitSphere(random) * Mathf.Max(0.01f, innerRadius);
                results[i] = dir;
            }
        }

        public static void SamplePalm(IList<Vector3> results, int count, float arcCount, float bend, System.Random random)
        {
            float branchCount = Mathf.Max(1f, arcCount);
            for (int i = 0; i < count; i++)
            {
                float branch = Mathf.Floor(i / (float)count * branchCount);
                float t = (i % Mathf.CeilToInt(count / branchCount)) / Mathf.Max(1f, (count / branchCount));
                float angle = branch / branchCount * Mathf.PI * 2f;
                Vector3 dir = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                dir = Quaternion.AngleAxis(Mathf.Lerp(0f, 70f, bend) * t, Vector3.Cross(dir, Vector3.up)) * dir;
                results[i] = dir.normalized;
            }
        }

        public static void SampleProjected2D(IList<Vector3> results, int count, Texture2D mask, float radius, System.Random random)
        {
            if (mask == null)
            {
                SampleRing(results, count, radius, 0.1f, random);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector2 sample = SampleTexture(mask, random);
                Vector3 dir = new((sample.x - 0.5f) * 2f, (sample.y - 0.5f) * 2f, 0f);
                if (dir.sqrMagnitude > 1f)
                {
                    dir = dir.normalized;
                }
                results[i] = dir * radius;
            }
        }

        private static Vector3 RandomPointOnUnitSphere(System.Random random)
        {
            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();
            float theta = 2f * Mathf.PI * u;
            float phi = Mathf.Acos(2f * v - 1f);
            float sinPhi = Mathf.Sin(phi);
            return new Vector3(Mathf.Cos(theta) * sinPhi, Mathf.Cos(phi), Mathf.Sin(theta) * sinPhi);
        }

        private static Vector2 SampleTexture(Texture2D texture, System.Random random)
        {
            for (int attempt = 0; attempt < 32; attempt++)
            {
                float x = (float)random.NextDouble();
                float y = (float)random.NextDouble();
                Color color = texture.GetPixelBilinear(x, y);
                if (color.a >= random.NextDouble())
                {
                    return new Vector2(x, y);
                }
            }

            return new Vector2((float)random.NextDouble(), (float)random.NextDouble());
        }
    }
}

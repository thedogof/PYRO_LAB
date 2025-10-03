using System;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public static class FireworkBurst
    {
        public static void GenerateBurstVectors(FireworkRecipe recipe, Vector3[] buffer)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length < recipe.starCount)
            {
                throw new ArgumentException("Buffer is smaller than star count.");
            }

            switch (recipe.burstPattern)
            {
                case FireworkRecipe.BurstPattern.Peony:
                    GeneratePeony(recipe, buffer);
                    break;
                case FireworkRecipe.BurstPattern.Willow:
                    GenerateWillow(recipe, buffer);
                    break;
                case FireworkRecipe.BurstPattern.Ring:
                    GenerateRing(recipe, buffer);
                    break;
                case FireworkRecipe.BurstPattern.Custom:
                    GenerateCustom(recipe, buffer);
                    break;
                default:
                    GeneratePeony(recipe, buffer);
                    break;
            }
        }

        public static void GeneratePeony(FireworkRecipe recipe, Vector3[] buffer)
        {
            var random = UnityEngine.Random.state;
            UnityEngine.Random.InitState(Environment.TickCount);
            for (int i = 0; i < recipe.starCount; i++)
            {
                Vector3 direction = UnityEngine.Random.onUnitSphere;
                float speed = UnityEngine.Random.Range(recipe.burstSpeedMin, recipe.burstSpeedMax);
                buffer[i] = direction * speed;
            }
            UnityEngine.Random.state = random;
        }

        private static void GenerateWillow(FireworkRecipe recipe, Vector3[] buffer)
        {
            var random = UnityEngine.Random.state;
            UnityEngine.Random.InitState(Environment.TickCount + 13);
            for (int i = 0; i < recipe.starCount; i++)
            {
                Vector3 direction = UnityEngine.Random.onUnitSphere;
                direction = Vector3.Lerp(direction, Vector3.down, 0.35f).normalized;
                float speed = UnityEngine.Random.Range(recipe.burstSpeedMin * 0.6f, recipe.burstSpeedMax * 0.9f);
                buffer[i] = direction * speed;
            }
            UnityEngine.Random.state = random;
        }

        private static void GenerateRing(FireworkRecipe recipe, Vector3[] buffer)
        {
            var normal = recipe.ringNormal.normalized;
            if (normal == Vector3.zero)
            {
                normal = Vector3.up;
            }

            Vector3 tangent = Vector3.Cross(normal, Vector3.right);
            if (tangent.sqrMagnitude < 0.0001f)
            {
                tangent = Vector3.Cross(normal, Vector3.forward);
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

            var random = UnityEngine.Random.state;
            UnityEngine.Random.InitState(Environment.TickCount + 29);
            for (int i = 0; i < recipe.starCount; i++)
            {
                float angle = (float)i / recipe.starCount * Mathf.PI * 2f;
                float radius = 1f + UnityEngine.Random.Range(-recipe.ringThickness, recipe.ringThickness);
                Vector3 direction = (Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent).normalized * radius;
                float speed = UnityEngine.Random.Range(recipe.burstSpeedMin, recipe.burstSpeedMax);
                buffer[i] = direction.normalized * speed;
            }
            UnityEngine.Random.state = random;
        }

        private static void GenerateCustom(FireworkRecipe recipe, Vector3[] buffer)
        {
            if (recipe.patternTexture == null)
            {
                GeneratePeony(recipe, buffer);
                return;
            }

            Texture2D texture = recipe.patternTexture;
            var random = UnityEngine.Random.state;
            UnityEngine.Random.InitState(Environment.TickCount + 47);

            for (int i = 0; i < recipe.starCount; i++)
            {
                Vector2 sample = SampleTexture(texture);
                Vector3 dir = new(sample.x * 2f - 1f, sample.y * 2f - 1f, 0f);
                if (dir.sqrMagnitude > 1f)
                {
                    dir = dir.normalized;
                }
                Vector3 worldDir = dir.x * Vector3.right + dir.y * Vector3.up;
                float speed = UnityEngine.Random.Range(recipe.burstSpeedMin, recipe.burstSpeedMax);
                buffer[i] = worldDir.normalized * speed;
            }
            UnityEngine.Random.state = random;
        }

        private static Vector2 SampleTexture(Texture2D texture)
        {
            for (int attempts = 0; attempts < 32; attempts++)
            {
                float x = UnityEngine.Random.value;
                float y = UnityEngine.Random.value;
                Color color = texture.GetPixelBilinear(x, y);
                if (color.a >= UnityEngine.Random.value)
                {
                    return new Vector2(x, y);
                }
            }

            return new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
        }
    }
}

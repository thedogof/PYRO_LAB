using System;
using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PyroLab/Firework Recipe", fileName = "FireworkRecipe")]
    public class FireworkRecipe : ScriptableObject
    {
        public enum BurstPattern
        {
            Peony,
            Willow,
            Ring,
            Custom
        }

        [Header("Lift Phase")]
        [Min(0f)] public float liftSpeed = 10f;
        [Min(0f)] public float liftAcceleration = 0f;
        [Min(0f)] public float fuseTime = 2.5f;

        [Header("Burst Phase")]
        public BurstPattern burstPattern = BurstPattern.Peony;
        [Range(4, 2048)] public int starCount = 128;
        public float burstSpeedMin = 6f;
        public float burstSpeedMax = 9f;
        [Min(0f)] public float burstDrag = 0.1f;
        public float gravityFactor = -0.2f;
        public Gradient colorGradient;
        public AnimationCurve sizeOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [Min(0f)] public float hdrIntensity = 3f;
        public bool multiLayer;
        [Min(0f)] public float innerLayerRatio = 0.6f;
        [Min(0f)] public float innerLayerSpeedMultiplier = 0.5f;

        [Header("Ring Pattern")]
        [Range(0f, 1f)] public float ringThickness = 0.15f;
        public Vector3 ringNormal = Vector3.up;

        [Header("Custom Pattern")]
        [Tooltip("Optional texture for 2D pattern projection sampling. Alpha channel defines density.")]
        public Texture2D patternTexture;

        public string ExportToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public void ImportFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("JSON content is null or empty.");
            }

            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}

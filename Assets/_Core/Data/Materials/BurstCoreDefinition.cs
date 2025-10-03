using UnityEngine;
using PyroLab.Fireworks;

namespace PyroLab.Fireworks.Workshop
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Burst Core", fileName = "BurstCore")]
    public class BurstCoreDefinition : ScriptableObject
    {
        [Tooltip("Base pattern for the primary layer.")]
        public BurstPatternType pattern = BurstPatternType.Peony;

        [Tooltip("Visual star count for the layer (purely aesthetic).")]
        [Range(50, 1200)] public int starCount = 320;

        [Tooltip("Minimum/maximum speed range sampled for stars (visual only).")]
        public Vector2 speedMinMax = new(8f, 12f);

        [Tooltip("Minor angular spread jitter (visual only).")]
        [Range(0f, 0.3f)] public float spread = 0.05f;

        [Header("Secondary Burst")]
        [Tooltip("If enabled, a secondary visual burst will be triggered.")]
        public bool secondary = false;

        [Tooltip("Delay after the primary burst before triggering the secondary (normalized 0-1).")]
        [Range(0f, 1f)] public float secondaryDelay = 0.5f;

        [Tooltip("Pattern used for the secondary burst when enabled.")]
        public BurstPatternType secondaryPattern = BurstPatternType.PistilRing;

        [Tooltip("Workshop cost purely for economy balancing (visual only).")]
        [Min(0f)] public float cost = 20f;
    }
}

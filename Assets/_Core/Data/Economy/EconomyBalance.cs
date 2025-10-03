using System;
using System.Collections.Generic;
using PyroLab.Fireworks.Workshop;
using UnityEngine;

namespace PyroLab.Fireworks.Economy
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Economy Balance", fileName = "EconomyBalance")]
    public class EconomyBalance : ScriptableObject
    {
        [Tooltip("Score to currency multiplier used when settling workshop launches.")]
        [Min(0f)] public float scoreToCurrency = 3f;

        [Tooltip("Tier definitions controlling unlock requirements and available materials.")]
        public List<WorkshopTier> tiers = new();

        public WorkshopTier GetTier(string id)
        {
            foreach (var tier in tiers)
            {
                if (tier != null && tier.id == id)
                {
                    return tier;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class WorkshopTier
    {
        [Tooltip("Identifier used to look up this tier.")]
        public string id = "T1";

        [Tooltip("Display name for UI.")]
        public string displayName = "Tier 1";

        [Tooltip("Minimum visual score required to unlock this tier.")]
        [Min(0f)] public float requiredScore = 0f;

        [Tooltip("Minimum funds required to unlock this tier.")]
        [Min(0f)] public float requiredFunds = 0f;

        [Tooltip("Paper layer options unlocked at this tier.")]
        public List<PaperLayerDefinition> paperOptions = new();

        [Tooltip("Fuse options unlocked at this tier.")]
        public List<FuseDefinition> fuseOptions = new();

        [Tooltip("Shell options unlocked at this tier.")]
        public List<ShellDefinition> shellOptions = new();

        [Tooltip("Star compounds unlocked at this tier.")]
        public List<StarCompoundDefinition> starOptions = new();

        [Tooltip("Burst cores unlocked at this tier.")]
        public List<BurstCoreDefinition> burstOptions = new();

        [Tooltip("Timing tracks unlocked at this tier.")]
        public List<TimingTrackDefinition> timingOptions = new();

        public bool IsUnlocked(float score, float funds)
        {
            return score >= requiredScore || funds >= requiredFunds;
        }
    }
}

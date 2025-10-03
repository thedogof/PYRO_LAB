using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Modifiers/Twinkle")]
    public class TwinkleModifier : VisualModifier
    {
        [Range(0f, 1f)] public float twinkleProbability = 0.35f;
        [Min(0f)] public float intensityVariance = 0.4f;

        public override void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime)
        {
            float variance = 1f + Random.Range(-intensityVariance, intensityVariance);
            emitParams.startColor *= variance;
            if (Random.value < twinkleProbability)
            {
                emitParams.startColor *= 0.2f;
            }
        }
    }
}

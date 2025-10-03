using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Modifiers/Strobe")]
    public class StrobeModifier : VisualModifier
    {
        [Min(0.1f)] public float frequency = 8f;
        [Range(0f, 1f)] public float duty = 0.3f;
        public Color offColor = new(0f, 0f, 0f, 0.1f);

        public override void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime)
        {
            float phase = Mathf.Repeat(normalizedTime * frequency, 1f);
            if (phase >= duty)
            {
                emitParams.startColor = offColor;
            }
        }
    }
}

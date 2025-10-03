using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Modifiers/Fade")]
    public class FadeModifier : VisualModifier
    {
        public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        public override void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime)
        {
            Color color = emitParams.startColor;
            color.a *= Mathf.Clamp01(alphaCurve.Evaluate(normalizedTime));
            emitParams.startColor = color;
        }
    }
}

using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Modifiers/Color Shift")]
    public class ColorShiftModifier : VisualModifier
    {
        public Gradient colorGradient = FireworkLayer.DefaultLayerGradient();
        public AnimationCurve blendCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime)
        {
            float blend = Mathf.Clamp01(blendCurve.Evaluate(normalizedTime));
            Color color = colorGradient.Evaluate(normalizedIndex);
            emitParams.startColor = Color.Lerp(emitParams.startColor, color, blend);
        }
    }
}

using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Modifiers/Gravity Drag")]
    public class GravityDragModifier : VisualModifier
    {
        [Range(0f, 1f)] public float gravityFactor = 0.2f;
        public AnimationCurve dragCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 0.35f);

        public override void OnSetup(ParticleSystem ps, FireworkLayer layer)
        {
            if (ps == null)
            {
                return;
            }

            var main = ps.main;
            main.gravityModifier = gravityFactor;
            var limitVelocity = ps.limitVelocityOverLifetime;
            limitVelocity.enabled = true;
            limitVelocity.limit = new ParticleSystem.MinMaxCurve(dragCurve.Evaluate(0f), dragCurve);
        }

        public override void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime)
        {
        }
    }
}

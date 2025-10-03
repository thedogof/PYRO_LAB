using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Modifiers/Trail")]
    public class TrailModifier : VisualModifier
    {
        [Min(0f)] public float lengthScale = 3.5f;
        [Range(0f, 1f)] public float velocityStretch = 0.6f;

        public override void OnSetup(ParticleSystem ps, FireworkLayer layer)
        {
            if (ps == null)
            {
                return;
            }

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.lengthScale = lengthScale;
                renderer.velocityScale = velocityStretch;
            }

            var trails = ps.trails;
            trails.enabled = true;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.lifetime = 1f;
        }

        public override void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime)
        {
        }
    }
}

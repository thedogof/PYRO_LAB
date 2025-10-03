using UnityEngine;

namespace PyroLab.Fireworks
{
    public abstract class VisualModifier : ScriptableObject
    {
        public bool isEnabled = true;

        public virtual void OnSetup(ParticleSystem ps, FireworkLayer layer)
        {
        }

        public abstract void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime);

        public virtual void OnTimingEvent(ParticleSystem ps, FireworkLayer layer, TimingEvent evt)
        {
        }
    }
}

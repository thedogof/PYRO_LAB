using UnityEngine;
using PyroLab.Fireworks;

namespace PyroLab.Fireworks.Workshop
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Timing Track", fileName = "TimingTrack")]
    public class TimingTrackDefinition : ScriptableObject
    {
        [Tooltip("Normalized timing events for purely visual secondary bursts and modifiers.")]
        public TimingTrack track = new();

        [Tooltip("Workshop cost purely for economy balancing (visual only).")]
        [Min(0f)] public float cost = 0f;
    }
}

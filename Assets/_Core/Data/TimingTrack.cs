using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public enum TimingAction
    {
        SubBurst,
        Split,
        ColorShift,
        StrobeToggle
    }

    [Serializable]
    public class TimingEvent
    {
        [Tooltip("Label used by the workshop UI. Purely descriptive.")]
        public string name = "Event";

        [Tooltip("Normalized time within the burst (0 = launch, 1 = end).")]
        [Range(0f, 1f)] public float time = 0.5f;

        [Tooltip("Visual-only action performed when the event triggers.")]
        public TimingAction action = TimingAction.SubBurst;

        [Tooltip("Layer index the action targets (purely visual).")]
        public int layerIndex = 0;

        [Tooltip("Optional modifier index for toggle actions (visual only).")]
        public int modifierIndex = 0;
    }

    [Serializable]
    public class TimingTrack
    {
        [Tooltip("Ordered list of normalized timing events used by workshop recipes.")]
        public List<TimingEvent> events = new();

        public TimingTrack Clone()
        {
            var clone = new TimingTrack();
            foreach (var evt in events)
            {
                clone.events.Add(new TimingEvent
                {
                    name = evt.name,
                    time = evt.time,
                    action = evt.action,
                    layerIndex = evt.layerIndex,
                    modifierIndex = evt.modifierIndex
                });
            }

            return clone;
        }
    }
}

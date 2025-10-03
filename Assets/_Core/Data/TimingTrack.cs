using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public enum TimingAction
    {
        SubBurst,
        Split,
        TriggerModifier
    }

    [Serializable]
    public class TimingEvent
    {
        public string name = "Event";
        [Range(0f, 1f)] public float time = 0.5f;
        public TimingAction action = TimingAction.SubBurst;
        [Tooltip("Layer index that the action targets.")]
        public int layerIndex = 0;
        [Tooltip("Optional modifier index that will be toggled or triggered.")]
        public int modifierIndex = 0;
    }

    [Serializable]
    public class TimingTrack
    {
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

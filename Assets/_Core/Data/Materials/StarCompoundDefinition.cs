using UnityEngine;

namespace PyroLab.Fireworks.Workshop
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Star Compound", fileName = "StarCompound")]
    public class StarCompoundDefinition : ScriptableObject
    {
        [Tooltip("HDR gradient purely for star coloration in the visual sim.")]
        [GradientUsage(true)] public Gradient colorGradient = Fireworks.FireworkLayer.DefaultLayerGradient();

        [Tooltip("Visual HDR intensity multiplier for this star compound.")]
        [Range(1f, 6f)] public float hdrIntensity = 2f;

        [Tooltip("Lifetime of the star streak in seconds (visual only).")]
        [Range(0.8f, 3.5f)] public float lifeSeconds = 1.5f;

        [Tooltip("Strobe frequency in Hz for the visual strobe modifier.")]
        [Range(0f, 12f)] public float strobeFrequency = 0f;

        [Tooltip("Duty cycle (on fraction) of the strobe effect.")]
        [Range(0f, 1f)] public float strobeDuty = 0.25f;

        [Tooltip("Twinkle strength passed into the twinkle modifier (0 = off, 1 = max wobble).")]
        [Range(0f, 1f)] public float twinkleAmount = 0f;

        [Tooltip("Additional trail contribution purely visual.")]
        [Range(0f, 1f)] public float trail = 0.2f;

        [Tooltip("Workshop cost purely for economy balancing (visual only).")]
        [Min(0f)] public float cost = 10f;
    }
}

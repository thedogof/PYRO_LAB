using UnityEngine;

namespace PyroLab.Fireworks.Workshop
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Shell", fileName = "Shell")]
    public class ShellDefinition : ScriptableObject
    {
        [Tooltip("Perceived hardness purely affects trail length visuals.")]
        [Range(0f, 1f)] public float hardness = 0.3f;

        [Tooltip("Relative mass impression controlling gravity drag visuals.")]
        [Range(0f, 1f)] public float massFactor = 0.2f;

        [Tooltip("Burst tightness (1 = needle sharp). Drives angular spread visuals only.")]
        [Range(0f, 1f)] public float burstTightness = 0.4f;

        [Tooltip("Workshop cost purely for economy balancing (visual only).")]
        [Min(0f)] public float cost = 8f;
    }
}

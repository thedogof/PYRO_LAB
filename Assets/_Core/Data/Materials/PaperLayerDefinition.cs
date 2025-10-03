using UnityEngine;

namespace PyroLab.Fireworks.Workshop
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Paper Layer", fileName = "PaperLayer")]
    public class PaperLayerDefinition : ScriptableObject
    {
        [Tooltip("Relative wrap thickness purely for visual drag shaping (0 = none, 1 = very thick).")]
        [Range(0f, 1f)] public float thickness = 0.2f;

        [Tooltip("Number of decorative paper wraps. Only affects burst stability visuals.")]
        [Range(1, 20)] public int layerCount = 6;

        [Tooltip("How tightly the wrap is visually pulled. Drives burst symmetry only.")]
        [Range(0f, 1f)] public float wrapTension = 0.4f;

        [Tooltip("Workshop cost purely for economy balancing (visual only).")]
        [Min(0f)] public float cost = 5f;
    }
}

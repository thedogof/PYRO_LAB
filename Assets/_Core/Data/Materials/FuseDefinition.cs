using UnityEngine;

namespace PyroLab.Fireworks.Workshop
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Fuse", fileName = "Fuse")]
    public class FuseDefinition : ScriptableObject
    {
        [Tooltip("Purely visual fuse delay before burst (seconds).")]
        [Range(0.5f, 6f)] public float delaySeconds = 1.5f;

        [Tooltip("Launch stability visual variance control (0 = chaotic, 1 = straight).")]
        [Range(0f, 1f)] public float stability = 0.6f;

        [Tooltip("Workshop cost purely for economy balancing (visual only).")]
        [Min(0f)] public float cost = 4f;
    }
}

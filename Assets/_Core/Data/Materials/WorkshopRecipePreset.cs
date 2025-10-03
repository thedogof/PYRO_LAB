using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks.Workshop
{
    [CreateAssetMenu(menuName = "PYRO/Workshop/Recipe Preset", fileName = "WorkshopPreset")]
    public class WorkshopRecipePreset : ScriptableObject
    {
        [Tooltip("Display label shown in the workshop UI.")]
        public string displayName = "Preset";

        [Tooltip("Paper layers included in this preset (visual only).")]
        public List<PaperLayerDefinition> papers = new();

        [Tooltip("Fuse used for this preset.")]
        public FuseDefinition fuse;

        [Tooltip("Shell used for this preset.")]
        public ShellDefinition shell;

        [Tooltip("Star compounds layered in this preset.")]
        public List<StarCompoundDefinition> starCompounds = new();

        [Tooltip("Burst core definition applied.")]
        public BurstCoreDefinition burstCore;

        [Tooltip("Optional timing track applied to the preset.")]
        public TimingTrackDefinition timing;
    }
}

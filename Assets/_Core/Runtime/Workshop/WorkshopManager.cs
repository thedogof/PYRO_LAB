using System;
using System.Collections.Generic;
using PyroLab.Fireworks.Economy;
using PyroLab.Fireworks.Workshop;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public class WorkshopManager : MonoBehaviour
    {
        [Header("Assembly")]
        [SerializeField, Tooltip("Target recipe to write generated parameters into.")]
        private FireworkRecipe activeRecipe;

        [SerializeField, Tooltip("Default burst height before paper size scaling.")]
        private float baseBurstHeight = 90f;

        [SerializeField] private List<PaperLayerDefinition> selectedPapers = new();
        [SerializeField] private FuseDefinition selectedFuse;
        [SerializeField] private ShellDefinition selectedShell;
        [SerializeField] private List<StarCompoundDefinition> selectedStarCompounds = new();
        [SerializeField] private BurstCoreDefinition selectedBurstCore;
        [SerializeField] private TimingTrackDefinition selectedTiming;

        [Header("Presets")]
        [SerializeField] private List<WorkshopRecipePreset> presets = new();

        [Header("Systems")]
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private FireworkLauncher previewLauncher;

        public FireworkRecipe ActiveRecipe => activeRecipe;
        public IReadOnlyList<WorkshopRecipePreset> Presets => presets;

        private void Reset()
        {
            if (economyManager == null)
            {
                economyManager = GetComponent<EconomyManager>();
            }
        }

        private void OnEnable()
        {
            BuildRecipe();
        }

        public void ApplyPreset(WorkshopRecipePreset preset)
        {
            if (preset == null)
            {
                return;
            }

            SetSelections(preset.papers, preset.fuse, preset.shell, preset.starCompounds, preset.burstCore, preset.timing);
            BuildRecipe();
        }

        public void SetSelections(
            IEnumerable<PaperLayerDefinition> papers,
            FuseDefinition fuse,
            ShellDefinition shell,
            IEnumerable<StarCompoundDefinition> stars,
            BurstCoreDefinition burstCore,
            TimingTrackDefinition timing)
        {
            selectedPapers = new List<PaperLayerDefinition>(papers ?? Array.Empty<PaperLayerDefinition>());
            selectedFuse = fuse;
            selectedShell = shell;
            selectedStarCompounds = new List<StarCompoundDefinition>(stars ?? Array.Empty<StarCompoundDefinition>());
            selectedBurstCore = burstCore;
            selectedTiming = timing;
        }

        public FireworkRecipe BuildRecipe()
        {
            if (activeRecipe == null)
            {
                Debug.LogWarning("WorkshopManager missing active recipe reference.");
                return null;
            }

            return WorkshopRecipeBuilder.BuildRecipe(
                activeRecipe,
                selectedPapers,
                selectedFuse,
                selectedShell,
                selectedStarCompounds,
                selectedBurstCore,
                selectedTiming,
                baseBurstHeight);
        }

        public float CalculateCost()
        {
            return economyManager != null
                ? economyManager.CalculateCost(selectedPapers, selectedFuse, selectedShell, selectedStarCompounds, selectedBurstCore, selectedTiming)
                : InternalCost();
        }

        private float InternalCost()
        {
            float cost = 0f;
            foreach (var paper in selectedPapers)
            {
                if (paper != null) cost += paper.cost;
            }

            if (selectedFuse != null) cost += selectedFuse.cost;
            if (selectedShell != null) cost += selectedShell.cost;
            if (selectedBurstCore != null) cost += selectedBurstCore.cost;
            if (selectedTiming != null) cost += selectedTiming.cost;
            foreach (var star in selectedStarCompounds)
            {
                if (star != null) cost += star.cost;
            }

            return cost;
        }

        public float EstimateVisualScore()
        {
            float paperScore = 0f;
            float totalLayers = 0f;
            float totalWrap = 0f;
            float totalThickness = 0f;
            foreach (var paper in selectedPapers)
            {
                if (paper == null) continue;
                totalLayers += paper.layerCount;
                totalWrap += paper.wrapTension;
                totalThickness += paper.thickness;
            }

            if (selectedPapers.Count > 0)
            {
                paperScore = totalLayers * 0.6f + totalWrap * 12f + totalThickness * 8f;
            }

            float starScore = 0f;
            float intensitySum = 0f;
            foreach (var star in selectedStarCompounds)
            {
                if (star == null) continue;
                intensitySum += star.hdrIntensity;
                starScore += star.lifeSeconds * 4f;
                starScore += star.twinkleAmount * 6f;
                starScore += star.strobeFrequency * 0.8f;
                starScore += star.trail * 5f;
            }

            float burstScore = 0f;
            if (selectedBurstCore != null)
            {
                burstScore += selectedBurstCore.starCount * 0.05f;
                burstScore += selectedBurstCore.spread * 40f;
                if (selectedBurstCore.secondary) burstScore += 35f;
            }

            float shellScore = selectedShell != null ? (selectedShell.hardness + selectedShell.massFactor + selectedShell.burstTightness) * 20f : 15f;
            float fuseScore = selectedFuse != null ? selectedFuse.stability * 12f : 5f;

            float total = paperScore + starScore + intensitySum * 6f + burstScore + shellScore + fuseScore;
            return Mathf.Clamp(total, 10f, 500f);
        }

        public bool TryLaunchPreview()
        {
            if (previewLauncher == null)
            {
                Debug.LogWarning("WorkshopManager missing preview launcher.");
                return false;
            }

            BuildRecipe();
            float cost = CalculateCost();
            if (economyManager != null && !economyManager.CanAfford(cost))
            {
                Debug.LogWarning($"Insufficient funds for launch. Cost: {cost:0.0}");
                return false;
            }

            economyManager?.Spend(cost);
            previewLauncher.Recipe = activeRecipe;
            previewLauncher.ResetAndLaunch();

            float score = EstimateVisualScore();
            economyManager?.AwardFundsFromScore(score);
            return true;
        }
    }
}

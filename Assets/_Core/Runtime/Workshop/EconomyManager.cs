using System.Collections.Generic;
using PyroLab.Fireworks.Economy;
using PyroLab.Fireworks.Workshop;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public class EconomyManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Economy balance definitions used for unlock checks.")]
        private EconomyBalance balance;

        [SerializeField, Tooltip("Current workshop funds (visual currency).")]
        private float funds = 0f;

        [SerializeField, Tooltip("Best visual score reached by the player.")]
        private float bestScore = 0f;

        public EconomyBalance Balance => balance;
        public float Funds => funds;
        public float BestScore => bestScore;

        public void SetProgress(float newFunds, float newBestScore)
        {
            funds = Mathf.Max(0f, newFunds);
            bestScore = Mathf.Max(0f, newBestScore);
        }

        public bool IsTierUnlocked(WorkshopTier tier)
        {
            if (tier == null)
            {
                return false;
            }

            return tier.IsUnlocked(bestScore, funds);
        }

        public float CalculateCost(
            IEnumerable<PaperLayerDefinition> papers,
            FuseDefinition fuse,
            ShellDefinition shell,
            IEnumerable<StarCompoundDefinition> starCompounds,
            BurstCoreDefinition burstCore,
            TimingTrackDefinition timing)
        {
            float cost = 0f;
            if (papers != null)
            {
                foreach (var paper in papers)
                {
                    if (paper != null)
                    {
                        cost += paper.cost;
                    }
                }
            }

            if (fuse != null) cost += fuse.cost;
            if (shell != null) cost += shell.cost;
            if (burstCore != null) cost += burstCore.cost;
            if (timing != null) cost += timing.cost;

            if (starCompounds != null)
            {
                foreach (var star in starCompounds)
                {
                    if (star != null)
                    {
                        cost += star.cost;
                    }
                }
            }

            return cost;
        }

        public bool CanAfford(float cost)
        {
            return funds >= cost;
        }

        public void Spend(float cost)
        {
            funds = Mathf.Max(0f, funds - Mathf.Max(0f, cost));
        }

        public float AwardFundsFromScore(float visualScore)
        {
            float payout = Mathf.Max(0f, visualScore) * (balance != null ? balance.scoreToCurrency : 0f);
            funds += payout;
            bestScore = Mathf.Max(bestScore, visualScore);
            return payout;
        }
    }
}

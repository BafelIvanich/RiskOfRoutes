using RiskOfRoutes;
using RoR2;
using System;
using System.Reflection;
using UnityEngine;

namespace SamplePlugin.StageModifiers.CombatModifiers
{
    public class Tier2ElitesEarlierModifier : StageModifier
    {
        public CombatDirector.EliteTierDef modifiedTier;
        public Func<SpawnCard.EliteRules, bool> modifiedAvailability;
        public float modifiedCostMultiplier;

        public Tier2ElitesEarlierModifier(int cost)
        {
            activeText = "More powerful elites might appear earlier";
            reservedText = "More powerful elites might appear earlier";
            name = "Powerful elites";
            var ic = RiskOfRoutes.RiskOfRoutes.bundle.LoadAsset<Sprite>("tier2elites");
            if (ic != null)
            {
                icon = ic;
            }
            else
            {
                Log.Error("failed to load tier 2 elites modifier");
            }
            stackable = false;
            this.cost = cost;
        }

        public override void OnEnd()
        {
            Log.Info("Tier2 elites on end");
            Log.Info("Restoring elite tier costs");
            if (modifiedTier != null)
            {
                modifiedTier.isAvailable = modifiedAvailability;
                modifiedTier.costMultiplier = modifiedCostMultiplier;
            }
            else
            {
                Log.Error("Could not access elite tier that was modified");
            }
        }

        public override void OnStart()
        {
            Log.Info("Tier2 elites start");
            FieldInfo eliteTiersField = typeof(CombatDirector).GetField("eliteTiers", BindingFlags.NonPublic | BindingFlags.Static);
            if (eliteTiersField != null)
            {
                var eliteTiers = (CombatDirector.EliteTierDef[])eliteTiersField.GetValue(null);
                Log.Info($"Tier count{eliteTiers.Length}");
                for (int i = 0; i < eliteTiers.Length - 1; i++)
                {
                    var eliteTier = eliteTiers[i];
                    foreach (var elite in eliteTier.eliteTypes)
                    {
                        if (elite != null && elite.name.Contains("Poison"))
                        {
                            Log.Info("Malachhite");

                            modifiedTier = eliteTier;
                            modifiedAvailability = eliteTier.isAvailable;
                            modifiedCostMultiplier = eliteTier.costMultiplier;

                            eliteTier.isAvailable = (SpawnCard.EliteRules rules) => true;
                            eliteTier.costMultiplier = 6f;
                            return;
                        }
                    }

                }

            }
        }

    }
}

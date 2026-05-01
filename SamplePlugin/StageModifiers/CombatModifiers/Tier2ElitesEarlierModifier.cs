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

        public Tier2ElitesEarlierModifier()
        {
            //todo stack
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
            //todo they might remain permanent
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

                            modifiedTier = eliteTier;
                            modifiedAvailability = eliteTier.isAvailable;
                            modifiedCostMultiplier = eliteTier.costMultiplier;

                            eliteTier.isAvailable = (SpawnCard.EliteRules rules) => true;
                            //
                            //eliteTier.costMultiplier = 6f;
                            float coeff = Mathf.Max(0.1f, 1f - ((RiskOfRoutes.RiskOfRoutes.tier2ElitesStackPercent.Value * (stack - 1)) / 100f));
                            eliteTier.costMultiplier = Math.Max(6f * coeff, 1f);
                            return;
                        }
                    }

                }

            }
        }

    }
}

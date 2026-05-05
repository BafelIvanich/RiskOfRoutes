using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    public class MoreElitesModifier : StageModifier
    {
        public Dictionary<CombatDirector.EliteTierDef, float> tierAndInitialPrice = new Dictionary<CombatDirector.EliteTierDef, float>();
        public float defaultPercent = 0.33f;

        public MoreElitesModifier(int stack)
        {
            this.stack = stack;
        }
        public override void OnEnd()
        {
            Log.Info("MoreElites on end");
            Log.Info("Restoring elite tier costs");
            foreach (var tier in tierAndInitialPrice)
            {
                tier.Key.costMultiplier = tier.Value;
            }
        }

        public override void OnStart()
        {
            Log.Info("MoreElites on start");
            FieldInfo eliteTiersField = typeof(CombatDirector).GetField("eliteTiers", BindingFlags.NonPublic | BindingFlags.Static);
            if (eliteTiersField != null)
            {
                var eliteTiers = (CombatDirector.EliteTierDef[])eliteTiersField.GetValue(null);
                Log.Info($"Tier count{eliteTiers.Length}");
                for (int i = 0; i < eliteTiers.Length - 1; i++)
                {
                    var eliteTier = eliteTiers[i];
                    if (eliteTier != null)
                    {
                        if (eliteTier.costMultiplier == 1) continue;
                        float pct = 1 - defaultPercent - ((stack - 1) * (float)(RiskOfRoutes.RiskOfRoutes.elitesStackPercent.Value) / 100f);

                        Log.Info($"Cost for elite tier {eliteTier} lowered by {(stack - 1) * (float)(RiskOfRoutes.RiskOfRoutes.elitesStackPercent.Value) / 100f}: {eliteTier.costMultiplier}->{eliteTier.costMultiplier * pct}");
                        Log.Info($"Elite tier {eliteTier} consists of:");
                        foreach (var eliteType in eliteTier.eliteTypes)
                        {
                            Log.Info($"\tEliteDef:{eliteType.name}");
                        }
                        tierAndInitialPrice.Add(eliteTier, eliteTier.costMultiplier);
                        eliteTier.costMultiplier = Mathf.Max(eliteTier.costMultiplier * pct, 1);
                    }
                }

            }
        }

    }



}

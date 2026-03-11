using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    //не артефакт, элитки становятся дешевле для директора
    public class MoreElitesModifier : StageModifier
    {
        public Dictionary<CombatDirector.EliteTierDef, float> tierAndInitialPrice = new Dictionary<CombatDirector.EliteTierDef, float>();

        public MoreElitesModifier(int stack)
        {
            name = "More elites";
            activeText = "More <style=cDeath>elite</style> enemies will appear.";
            reservedText = "More <style=cDeath>elite</style> enemies will appear on the next stage.";
            iconName = "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveBossIcon.png";
            negative = true;
            this.stack = stack;
        }
        public override void OnEnd()
        {
            Log.Info("More elites on end");
            Log.Info("Restoring elite tier costs");
            foreach (var tier in tierAndInitialPrice)
            {
                tier.Key.costMultiplier = tier.Value;
            }
        }

        public override void OnStart()
        {
            Log.Info("More elites modifies start");
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
                        float pct = 1 - 0.15f - ((stack - 1) * 0.15f);

                        Log.Info($"Elite cost lowered by {(stack - 1) * 0.15f}: {eliteTier.costMultiplier}->{eliteTier.costMultiplier * pct}");
                        tierAndInitialPrice.Add(eliteTier, eliteTier.costMultiplier);
                        eliteTier.costMultiplier = Mathf.Max(eliteTier.costMultiplier * pct, 1);
                    }
                }

            }
        }

    }



}

using R2API;
using RoR2;
using SamplePlugin;
using UnityEngine;

namespace RiskOfRoutes.StageModifiers.SceneModifiers
{
    public class SoulCostController
    {
        public static void Initialize()
        {
            On.RoR2.CostTypeDef.BuildCostStringStyled += AppendSoulCost;
            RecalculateStatsAPI.GetStatCoefficients += AppendHPBarCurse;
        }

        private static void AppendSoulCost(On.RoR2.CostTypeDef.orig_BuildCostStringStyled orig, CostTypeDef self, int cost, System.Text.StringBuilder stringBuilder, bool forWorldDisplay, bool includeColor)
        {
            orig(self, cost, stringBuilder, forWorldDisplay, includeColor);
            if (StageModifierDirector.instance && StageModifierDirector.instance.IsModifierActive("SoulCost"))
            {
                int stack = StageModifierDirector.instance.soulCostStack;

                if (self == RoR2.CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money))
                    stringBuilder.AppendLine($"<style=cShrine> + {3 + (stack - 1)}% Soul</style>");
            }
        }

        private static void AppendHPBarCurse(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (StageModifierDirector.instance && StageModifierDirector.instance.IsModifierActive("SoulCost"))
            {
                if (sender.teamComponent && sender.teamComponent.teamIndex == TeamIndex.Player)
                {
                    float curse = StageModifierDirector.instance.syncedBaseCurse;
                    float curseMin = Mathf.Min(0.99f, curse);
                    float rem = 1.0f - curseMin;
                    float curseFinal = (1.0f / rem) - 1.0f;
                    args.baseCurseAdd += curseFinal;

                }

            }

        }
    }
}

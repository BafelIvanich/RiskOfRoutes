using R2API;
using RiskOfRoutes.Items;
using RoR2;
using SamplePlugin;
using UnityEngine;

namespace RiskOfRoutes
{
    public class SoulCostController
    {
        public static void Initialize()
        {
            On.RoR2.CostTypeDef.BuildCostStringStyled += AppendSoulPrice;
            RecalculateStatsAPI.GetStatCoefficients += AppendHPBarCurse;
        }

        private static void AppendSoulPrice(On.RoR2.CostTypeDef.orig_BuildCostStringStyled orig, CostTypeDef self, int cost, System.Text.StringBuilder stringBuilder, bool forWorldDisplay, bool includeColor)
        {
            orig(self, cost, stringBuilder, forWorldDisplay, includeColor);
            if (StageModifierDirector.instance && StageModifierDirector.instance.IsModifierActive("SoulCost"))
            {
                int stack = StageModifierDirector.instance.cursePerStack;

                if (self == CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money))
                    stringBuilder.AppendLine($"<style=cShrine> + {3 + (stack - 1)}% {RoR2.Language.GetString("SOUL")}</style>");
            }
        }

        private static void AppendHPBarCurse(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (StageModifierDirector.instance && StageModifierDirector.instance.IsModifierActive("SoulCost"))
            {
                if (sender.inventory)
                {
                    int itemStack = sender.inventory.GetItemCountPermanent(ModItemsManager.soulCostCurse);
                    if (itemStack > 0)
                    {
                        int stack = StageModifierDirector.instance.cursePerStack;
                        float curseAdd = 0.03f + (stack - 1) * Mathf.Max((float)(RiskOfRoutes.soulCostStackPercent.Value / 100), 0.25f);
                        float totalCurse = itemStack * curseAdd;

                        float curseMin = Mathf.Min(0.99f, totalCurse);
                        float rem = 1.0f - curseMin;
                        args.baseCurseAdd += (1.0f / rem - 1.0f);
                    }


                }

            }

        }
    }
}

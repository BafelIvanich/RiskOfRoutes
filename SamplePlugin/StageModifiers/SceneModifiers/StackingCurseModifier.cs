using R2API;
using RoR2;
using SamplePlugin.StageModifiers;
using System;
using UnityEngine;

namespace RiskOfRoutes.StageModifiers.SceneModifiers
{
    //Каждое открытие сундука стакает на всех игроках проклятье на макс хп.
    //todo networking UI
    public class StackingCurseModifier : StageModifier
    {
        public static float curse = 0.0f;
        public float curseAdd = 0;
        public StackingCurseModifier(int stack)
        {
            this.stack = stack;
            name = "SoulCost";
            reservedText = "Each purchase drains your max hp for an entire stage";
            activeText = reservedText;
            iconName = "RoR2/DLC2/texShrineColossusAccessSymbol.jpg";
        }

        public override void OnEnd()
        {
            curse = 0.0f;
            On.RoR2.CostTypeDef.BuildCostStringStyled -= CostTypeDef_BuildCostStringStyled;
            On.RoR2.PurchaseInteraction.Awake -= PurchaseInteraction_Awake;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }

        public override void OnStart()
        {
            curse = 0.0f;
            curseAdd = 0.03f + (stack - 1) * 0.01f;
            Log.Info("Stack" + stack);
            Log.Info("Curseadd" + curseAdd);
            On.RoR2.CostTypeDef.BuildCostStringStyled += CostTypeDef_BuildCostStringStyled;
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.teamComponent && sender.teamComponent.teamIndex == TeamIndex.Player)
            {
                float curseMin = Mathf.Min(0.99f, curse);
                float rem = 1.0f - curseMin;
                float curseFinal = (1.0f / rem) - 1.0f;
                args.baseCurseAdd += curseFinal;

            }
        }

        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);

            self.onPurchase.AddListener(PlayerPurchase);
        }

        private void PlayerPurchase(CostTypeDef.PayCostContext arg0, CostTypeDef.PayCostResults arg1)
        {
            throw new NotImplementedException();
        }

        private void PlayerPurchase(Interactor interactor)
        {
            curse += curseAdd;
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player.master && player.master.GetBody())
                {
                    player.master.GetBody().RecalculateStats();
                }
            }
        }

        private void CostTypeDef_BuildCostStringStyled(On.RoR2.CostTypeDef.orig_BuildCostStringStyled orig, CostTypeDef self, int cost, System.Text.StringBuilder stringBuilder, bool forWorldDisplay, bool includeColor)
        {
            orig(self, cost, stringBuilder, forWorldDisplay, includeColor);
            if (self == RoR2.CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money))
                stringBuilder.AppendLine($"<style=cShrine> + {3 + (stack - 1)}% Soul</style>");
        }
    }
}

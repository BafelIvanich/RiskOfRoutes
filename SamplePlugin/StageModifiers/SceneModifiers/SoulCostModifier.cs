using RoR2;
using SamplePlugin;
using SamplePlugin.StageModifiers;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskOfRoutes.StageModifiers.SceneModifiers
{
    //Каждое открытие сундука стакает на всех игроках проклятье на макс хп.
    //todo networking UI
    public class SoulCostModifier : StageModifier
    {
        public float curseAdd = 0;
        public SoulCostModifier(int stack)
        {
            this.stack = stack;
        }

        public override void OnEnd()
        {
            On.RoR2.PurchaseInteraction.Awake -= PurchaseInteraction_Awake;
        }

        public override void OnStart()
        {
            curseAdd = 0.03f + (stack - 1) * Mathf.Max((float)(RiskOfRoutes.soulCostStackPercent.Value / 100), 0.25f);

            if (NetworkServer.active && StageModifierDirector.instance)
            {
                StageModifierDirector.instance.soulCostStack = this.stack;
                StageModifierDirector.instance.syncedBaseCurse = 0f;
            }
            Log.Info("Stack" + stack);
            Log.Info("Curseadd" + curseAdd);
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
        }


        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (self.costType == CostTypeIndex.Money)
                self.onPurchase.AddListener(PlayerPurchase);
        }

        private void PlayerPurchase(Interactor interactor)
        {
            if (!NetworkServer.active) return;

            if (StageModifierDirector.instance)
            {
                StageModifierDirector.instance.syncedBaseCurse += curseAdd;
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    if (player.master && player.master.GetBody())
                    {
                        player.master.GetBody().RecalculateStats();
                    }
                }

            }

        }

    }
}

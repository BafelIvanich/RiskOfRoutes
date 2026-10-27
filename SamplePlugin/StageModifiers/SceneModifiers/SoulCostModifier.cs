using RiskOfRoutes.Items;
using RoR2;
using SamplePlugin;
using SamplePlugin.StageModifiers;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskOfRoutes.StageModifiers.SceneModifiers
{
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

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player.master && player.master.GetBody())
                {
                    player.master.inventory.RemoveItem(ModItemsManager.soulCostCurse, 1);
                }
            }
        }

        public override void OnStart()
        {
            curseAdd = 0.03f + (stack - 1) * Mathf.Max((float)(RiskOfRoutes.soulCostStackPercent.Value / 100), 0.25f);

            if (NetworkServer.active && StageModifierDirector.instance)
            {
                StageModifierDirector.instance.cursePerStack = this.stack;
                StageModifierDirector.instance.syncedBaseCurse = 0f;
            }
            Log.Info($"SoulCost stack - {stack}");
            Log.Info($"CurseAdd - {curseAdd}");
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
        }


        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (self.costType == CostTypeIndex.Money && self.cost > 0)
                self.onPurchase.AddListener(PlayerPurchase);
        }

        //todo should send to clients
        private void PlayerPurchase(Interactor interactor)
        {
            if (!NetworkServer.active) return;



            if (RiskOfRoutes.soulCostCurseShared.Value)
            {
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    if (player.master && player.master.GetBody())
                    {
                        player.master.inventory.GiveItem(ModItemsManager.soulCostCurse, 1);
                    }
                }

            }
            else
            {
                var body = interactor.GetComponent<CharacterBody>();
                if (body == null) return;
                body.inventory.GiveItem(ModItemsManager.soulCostCurse, 1);
            }

        }

    }
}

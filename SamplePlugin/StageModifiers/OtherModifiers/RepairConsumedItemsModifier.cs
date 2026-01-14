using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;

namespace SamplePlugin.StageModifiers.OtherModifiers
{
    public class RepairConsumedItemsModifier : StageModifier
    {


        public RepairConsumedItemsModifier()
        {
            reservedText = "Repairs all of your consumed items, available once per loop.";
            activeText = "Consumed items restored.";
            stackable = false;
        }

        public override void OnEnd()
        {
            Log.Info("On RepairConsumedItems end");
        }

        public override void OnStart()
        {
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (!cm.inventory) continue;
                var copy = new List<ItemIndex>(cm.inventory.itemAcquisitionOrder);

                foreach (ItemIndex item in copy)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    if (itemDef != null)
                    {
                        if (itemDef.isConsumed && itemDef != DLC1Content.Items.RegeneratingScrapConsumed && itemDef != DLC2Content.Items.LowerPricedChestsConsumed && itemDef != DLC2Content.Items.TeleportOnLowHealthConsumed)
                        {
                            var count = cm.inventory.GetItemCount(itemDef);

                            cm.inventory.RemoveItem(itemDef, count);
                            ItemDef repairedItem = new ItemDef();
                            if (itemDef == DLC1Content.Items.FragileDamageBonusConsumed) repairedItem = DLC1Content.Items.FragileDamageBonus;
                            else if (itemDef == DLC1Content.Items.HealingPotionConsumed) repairedItem = DLC1Content.Items.HealingPotion;
                            else if (itemDef == DLC1Content.Items.ExtraLifeVoidConsumed) repairedItem = DLC1Content.Items.ExtraLifeVoid;
                            else if (itemDef == RoR2Content.Items.ExtraLifeConsumed) repairedItem = RoR2Content.Items.ExtraLife;


                            Log.Debug($"For consumed item{itemDef} in count {count} found repaired {repairedItem}");

                            cm.inventory.GiveItem(repairedItem, count);


                            Log.Info($"Succesfully repaired {itemDef.name}");


                        }
                    }
                }

                //cm.inventory.itemAcquisitionOrder = copy.itemAcquisitionOrder;
            }
        }
    }
}

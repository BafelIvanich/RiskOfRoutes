using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;

namespace SamplePlugin.StageModifiers.OtherModifiers
{
    public class RepairConsumedItemsModifier : StageModifier
    {


        public RepairConsumedItemsModifier()
        {
        }

        public override void OnEnd()
        {
            Log.Info("Repair on end");
        }

        //todo can be made compatible with modded ones
        public override void OnStart()
        {
            Log.Info("Repair on start");

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
                            else
                            {
                                var repairedItemIndex = RoR2.ItemCatalog.FindItemIndex(itemDef.name.Replace("Consumed", ""));
                                if (repairedItemIndex != ItemIndex.None)
                                {
                                    var repairedDef = RoR2.ItemCatalog.GetItemDef(repairedItemIndex);
                                    if (repairedDef != null) repairedItem = repairedDef;
                                }
                            }


                            Log.Debug($"For consumed item{itemDef} in count {count} found repaired {repairedItem}");

                            cm.inventory.GiveItem(repairedItem, count);

                            Log.Info($"Succesfully repaired {itemDef.name}");
                        }
                    }
                }
            }
        }
    }
}

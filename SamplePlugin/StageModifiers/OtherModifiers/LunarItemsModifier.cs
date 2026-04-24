using RoR2;
using SamplePlugin.StageModifiers;
using SamplePlugin.StageModifiers.SceneModifiers;
using UnityEngine;

namespace RiskOfRoutes.StageModifiers.OtherModifiers
{
    public class LunarItemsModifier : StageModifier
    {
        ItemDef lunarItem;
        Sprite lunarItemSprite;
        string lunarItemDescToken;

        public LunarItemsModifier(string internalName)
        {
            lunarItem = PrinterItemModifier.stringToItemDef(internalName);
        }

        public LunarItemsModifier(ItemDef item)
        {
            lunarItem = item;
        }

        public override void OnEnd()
        {
            if (lunarItem == null || Run.instance == null || PlayerCharacterMasterController.instances == null || CharacterMaster.readOnlyInstancesList.Count == 0)
            {
                Log.Error("Lunar item is null");
                return;
            }

            On.RoR2.UI.ItemIcon.SetItemIndex_ItemIndex_int_float -= ItemIcon_SetItemIndex_ItemIndex_int_float;

            var instances = PlayerCharacterMasterController.instances;
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController != null && cm.inventory != null)
                {
                    var pearl = Run.instance.stageRng.nextNormalizedFloat > 0.2f ? RoR2Content.Items.Pearl : RoR2Content.Items.ShinyPearl;
                    if (cm.inventory.GetItemCountEffective(lunarItem) > 0)
                    {
                        cm.inventory.RemoveItemPermanent(lunarItem);
                        cm.inventory.GiveItemPermanent(pearl);
                        Log.Info($"LunarItemModifier Gave player {cm.playerCharacterMasterController.GetDisplayName()} a {pearl.name}");
                    }
                }
            }
        }

        public override void OnStart()
        {
            if (lunarItem == null)
            {
                Log.Error("Lunar item is null");
                return;
            }

            On.RoR2.UI.ItemIcon.SetItemIndex_ItemIndex_int_float += ItemIcon_SetItemIndex_ItemIndex_int_float;

            Log.Info("hook");

            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController != null && cm.inventory != null)
                {
                    cm.inventory.GiveItemPermanent(lunarItem);
                }
            }
        }

        private void ItemIcon_SetItemIndex_ItemIndex_int_float(On.RoR2.UI.ItemIcon.orig_SetItemIndex_ItemIndex_int_float orig, RoR2.UI.ItemIcon self, ItemIndex newItemIndex, int newItemCount, float newDurationPercent)
        {
            orig(self, newItemIndex, newItemCount, newDurationPercent);

            if (newItemIndex == lunarItem.itemIndex)
            {
                self.image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                self.tooltipProvider.bodyToken = "SMTH";
            }

            Log.Info($"Set item index intfloat{newItemIndex}");
        }
    }
}

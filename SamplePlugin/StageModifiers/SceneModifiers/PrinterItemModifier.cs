using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;


namespace SamplePlugin.StageModifiers.SceneModifiers
{
    //возможно весь модифаер будет опциональным в конфиге
    public class PrinterItemModifier : StageModifier
    {

        private List<ItemInfo> itemInfos =
        [
            new ItemInfo("SecondarySkillMagazine", 60, 1f),
            new ItemInfo("Crowbar", 60, 1f),
            new ItemInfo("NearbyDamageBonus", 60, 1f),
            new ItemInfo("Syringe", 100, 1f),
            new ItemInfo("IgniteOnKill", 60, 1f),
            new ItemInfo("Bear", 60, 1f),
            new ItemInfo("BleedOnHit", 60, 1f),
            new ItemInfo("FragileDamageBonus", 100, 1f),

            new ItemInfo("FireRing", 140, 1f),
            new ItemInfo("IceRing", 140, 1f),
            new ItemInfo("Missile", 140, 1f),
            new ItemInfo("ExecuteLowHealthElite", 100, 1f),
            new ItemInfo("Thorns", 100, 1f),
            new ItemInfo("ChainLightning", 100, 1f),
            new ItemInfo("ExplodeOnDeath", 100, 1f),
            new ItemInfo("StrengthenBurn", 60, 1f),
            new ItemInfo("KnockBackHitEnemies", 60, 1f),
            new ItemInfo("ShieldBooster", 60, 1f),
            new ItemInfo("EquipmentMagazine",100,1f),
            new ItemInfo("Seed",100,1f)
        ];

        public ItemInfo itemInfo;
        public ItemDef selectedItem;
        public static InteractableSpawnCard printerSpawnCard;

        public PrinterItemModifier()
        {
            negative = false;
            if (NetworkServer.active)
            {
                itemInfo = Run.instance.stageRng.NextElementUniform(itemInfos);
                selectedItem = stringToItemDef(itemInfo.internalName);
                this.cost = itemInfo.cost;

                string name = Language.GetString(selectedItem.nameToken);
                reservedText = $"Printer with <style=cIsUtility>{name}</style> will appear on the next stage";
                activeText = $"Printer with <style=cIsUtility>{name}</style> has appeared somewhere on the stage.";
                icon = selectedItem.pickupIconSprite;
                this.name = name + " printer";
                stackable = false;


            }
            CreatePrinterSpawnCard();
        }

        private ItemDef stringToItemDef(string selectedItemName)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(selectedItemName);
            if (index != ItemIndex.None)
            {
                return ItemCatalog.GetItemDef(index);
            }
            else
            {
                Log.Error("Error finding item");
                return null;
            }
        }

        public override void OnEnd()
        {
            Log.Info("PrinterItemModifier end");
        }

        public override void OnStart()
        {
            Log.Info("PrinterItemModifier start");
            SpawnPrinter();
        }

        private void SpawnPrinter()
        {

            GameObject printer = TryPlaceObjectOnScene(printerSpawnCard, $"Printer with {selectedItem.name}");
            if (printer)
            {
                Log.Info($"printer appeared at {printer.transform.position} on stage");

                ShopTerminalBehavior shop = printer.GetComponent<ShopTerminalBehavior>();
                shop.dropTable = null;
                shop.selfGeneratePickup = false;
                PickupIndex currentPickupIndex = PickupCatalog.FindPickupIndex(selectedItem.itemIndex);
                UniquePickup pickup = new UniquePickup { pickupIndex = currentPickupIndex };

                shop.SetPickup(pickup, false);

            }
        }

        private void CreatePrinterSpawnCard()
        {
            InteractableSpawnCard card = selectedItem.tier switch
            {
                ItemTier.Tier1 => Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion(),
                ItemTier.Tier2 => Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset").WaitForCompletion(),
                ItemTier.Tier3 => Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion(),
                ItemTier.Boss => Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion(),
                _ => Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion(),
            };

            if (card != null)
            {
                Log.Info("printer spawncard created succesfully");
                printerSpawnCard = card;
            }
            else
            {
                Log.Info("Asset path is invalid");
            }
        }
    }


    public struct ItemInfo
    {
        public string internalName;
        public int cost;
        public float weight;

        public ItemInfo(string internalName, int cost, float weight)
        {
            this.internalName = internalName;
            this.cost = cost;
            this.weight = weight;
        }
    }

}

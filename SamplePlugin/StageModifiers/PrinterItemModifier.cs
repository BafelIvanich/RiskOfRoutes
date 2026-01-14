using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;


namespace SamplePlugin.StageModifiers
{
    //возможно весь модифаер будет опциональным в конфиге
    public class PrinterItemModifier : StageModifier
    {
        private List<string> WhiteNames = new List<string>()
        {
                "SecondarySkillMagazine",
                "Crowbar",
                "NearbyDamageBonus",
                "Syringe",
                "IgniteOnKill",
                "Bear",
                "BleedOnHit",
                "FragileDamageBonus",
        };

        private List<string> GreenNames = new List<string>()
        {
                "FireRing",
                "IceRing",
                "Missile",
                "ExecuteLowHealthElite",
                "Thorns",
                "ChainLightning",
                "ExplodeOnDeath",
                "StrengthenBurn",
                "KnockBackHitEnemies",
                "ShieldBooster"
        };

        ItemDef selectedItem;
        public static InteractableSpawnCard printerSpawnCard;

        public PrinterItemModifier()
        {
            negative = false;
            if (NetworkServer.active)
            {
                List<string> mergedNames = new List<string>(WhiteNames.Concat(GreenNames));
                Util.ShuffleList(mergedNames);
                var selectedItemName = mergedNames[0];

                selectedItem = stringToItemDef(selectedItemName);
                string name = Language.GetString(selectedItem.nameToken);
                Log.Info($"Selected item:{name}, {mergedNames.Count}");
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
            Log.Info("Trying to place Printer");

            if (!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return;
            }

            if (printerSpawnCard == null)
            {
                Log.Error("Something wrong with card");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random
            };

            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(
                printerSpawnCard,
                placementRule,
                Run.instance.stageRng
            );

            GameObject printer = DirectorCore.instance.TrySpawnObject(spawnRequest);
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
            else
            {
                Log.Error($"Couldn't place yellow printer on scene");
            }
        }

        private void CreatePrinterSpawnCard()
        {
            InteractableSpawnCard card = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion();

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



}

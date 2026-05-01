using RiskOfRoutes;
using RoR2;
using UnityEngine;


namespace SamplePlugin.StageModifiers.SceneModifiers
{
    //возможно весь модифаер будет опциональным в конфиге
    public class PrinterItemModifier : StageModifier
    {
        public ItemDef selectedItem;
        public static InteractableSpawnCard printerSpawnCard;

        public PrinterItemModifier(string internalName)
        {
            selectedItem = stringToItemDef(internalName);
            if (selectedItem == null) return;

            CreatePrinterSpawnCard();
        }

        public static ItemDef stringToItemDef(string selectedItemName)
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
                shop.itemTier = selectedItem.tier;
                PickupIndex currentPickupIndex = PickupCatalog.FindPickupIndex(selectedItem.itemIndex);
                UniquePickup pickup = new UniquePickup { pickupIndex = currentPickupIndex };

                shop.SetPickup(pickup, false);
            }
        }

        private void CreatePrinterSpawnCard()
        {
            //todo вынести
            InteractableSpawnCard card = selectedItem.tier switch
            {
                ItemTier.Tier1 => AssetManager.tier1Printer,
                ItemTier.Tier2 => AssetManager.tier2Printer,
                ItemTier.Tier3 => AssetManager.tier3Printer,
                ItemTier.Boss => AssetManager.bossPrinter,
                _ => AssetManager.tier1Printer,
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




}

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

            stackable = false;
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
                ItemTier.Tier1 => RiskOfRoutes.RiskOfRoutes.tier1Printer,
                ItemTier.Tier2 => RiskOfRoutes.RiskOfRoutes.tier2Printer,
                ItemTier.Tier3 => RiskOfRoutes.RiskOfRoutes.tier3Printer,
                ItemTier.Boss => RiskOfRoutes.RiskOfRoutes.bossPrinter,
                _ => RiskOfRoutes.RiskOfRoutes.tier1Printer,
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

using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class RedPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard redPrinterSpawnCard;
        public ItemTag tag;

        public RedPrinterModifier(int stack, ItemTag tag = ItemTag.Any)
        {
            CreateRedPrinterSpawnCard();

            this.stack = stack;
            this.tag = tag;
        }
        public override void OnEnd()
        {
            Log.Info("RedPrinterModifier end");
        }

        public override void OnStart()
        {
            Log.Info("RedPrinterModifier start");
            SpawnRedPrinter();
        }

        private void SpawnRedPrinter()
        {
            List<PickupIndex> availableReds = Run.instance.availableTier3DropList;
            List<PickupIndex> filtered = new List<PickupIndex>();

            foreach (PickupIndex pickupIndex in availableReds)
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                if (pickupDef != null && pickupDef.itemIndex != ItemIndex.None)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);

                    if (itemDef != null && itemDef.tags.Contains(tag)) //todo проверка
                    {
                        filtered.Add(pickupIndex);
                    }
                }
            }

            if (filtered.Count == 0)
            {
                Log.Error($"No red items available for tag {tag}");
                filtered = availableReds;
            }

            for (int i = 0; i < stack; i++)
            {
                GameObject redPrinter = TryPlaceObjectOnScene(redPrinterSpawnCard, "Red item printer");

                if (redPrinter)
                {
                    //Log.Info($"printer appeared at {redPrinter.transform.position} on stage");

                    ShopTerminalBehavior shop = redPrinter.GetComponent<ShopTerminalBehavior>();

                    shop.dropTable = null;
                    shop.selfGeneratePickup = false;

                    PickupIndex currentPickupIndex = Run.instance.stageRng.NextElementUniform(filtered);
                    UniquePickup pickup = new UniquePickup { pickupIndex = currentPickupIndex };

                    shop.SetPickup(pickup, false);
                }
            }
        }

        private void CreateRedPrinterSpawnCard()
        {
            //TODO вынести
            InteractableSpawnCard card = AssetManager.tier3Printer;

            if (card != null)
            {
                //Log.Info("Red printer spawncard created succesfully");
                redPrinterSpawnCard = card;
            }
            else
            {
                //Log.Info("Asset path is invalid");
            }
        }


    }



}

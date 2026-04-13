using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.RemoteConfigSettingsHelper;



namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class YellowPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard yellowPrinterSpawnCard;
        public ItemTag tag;

        public YellowPrinterModifier(int stack, ItemTag tag = ItemTag.Any)
        {
            CreateYellowPrinterSpawnCard();
            this.stack = stack;
            this.tag = tag;
        }
        public override void OnEnd()
        {
            Log.Info("YellowPrinterModifier end");
        }

        public override void OnStart()
        {
            Log.Info("YellowPrinterModifier start");
            SpawnYellowPrinter();
        }

        private void SpawnYellowPrinter()
        {
            List<PickupIndex> availableYellows = Run.instance.availableBossDropList;
            List<PickupIndex> filtered = new List<PickupIndex>();

            foreach (PickupIndex pickupIndex in availableYellows)
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
                Log.Error($"No yellow items available for tag {tag}");
                filtered = availableYellows;
            }

            for (int i = 0; i < stack; i++)
            {
                GameObject redPrinter = TryPlaceObjectOnScene(yellowPrinterSpawnCard, "Yellow item printer");

                if (redPrinter)
                {
                    Log.Info($"printer appeared at {redPrinter.transform.position} on stage");

                    ShopTerminalBehavior shop = redPrinter.GetComponent<ShopTerminalBehavior>();

                    shop.dropTable = null;
                    shop.selfGeneratePickup = false;

                    PickupIndex currentPickupIndex = Run.instance.stageRng.NextElementUniform(filtered);
                    UniquePickup pickup = new UniquePickup { pickupIndex = currentPickupIndex };

                    shop.SetPickup(pickup, false);
                }
            }
        }

        private void CreateYellowPrinterSpawnCard()
        {
            InteractableSpawnCard card = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion();

            if (card != null)
            {
                Log.Info("Yellow printer spawncard created succesfully");
                yellowPrinterSpawnCard = card;
            }
            else
            {
                Log.Info("Asset path is invalid");
            }
        }


    }



}

using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SamplePlugin.StageModifiers
{
    public class RedPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard redPrinterSpawnCard;

        public RedPrinterModifier()
        {
            CreateRedPrinterSpawnCard();
            name = "Red item printer";
            reservedText = "<style=cDeath>Red printer</style> is guaranteed to appear somewhere on the next stage.";
            activeText = "<style=cDeath>Red printer</style> has appeared somewhere on the stage.";
            negative = false;
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
            Log.Info("Trying to place Red Printer");

            if (!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return;
            }

            if (redPrinterSpawnCard == null)
            {
                Log.Error("Something wrong with card");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random
            };

            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(
                redPrinterSpawnCard,
                placementRule,
                Run.instance.stageRng
            );

            GameObject redPrinter = DirectorCore.instance.TrySpawnObject(spawnRequest);
            if (redPrinter)
            {
                Log.Info($"Red printer appeared at {redPrinter.transform.position} on stage");
            }
            else
            {
                Log.Error($"Couldn't place red printer on scene");
            }
        }

        private void CreateRedPrinterSpawnCard()
        {
            InteractableSpawnCard card = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion();

            if (card != null)
            {
                Log.Info("Red printer spawncard created succesfully");
                redPrinterSpawnCard = card;
            }
            else
            {
                Log.Info("Asset path is invalid");
            }
        }


    }

    

}

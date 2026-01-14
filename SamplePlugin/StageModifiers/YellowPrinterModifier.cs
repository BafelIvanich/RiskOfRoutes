using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;



namespace SamplePlugin.StageModifiers
{
    public class YellowPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard yellowPrinterSpawnCard;

        public YellowPrinterModifier()
        {
            CreateYellowPrinterSpawnCard();
            name = "Yellow item printer";
            reservedText = "<style=cShrine>Yellow printer</style> is guaranteed to appear somewhere on the next stage.";
            activeText = "<style=cShrine>Yellow printer</style> has appeared somewhere on the stage.";
            negative = false;
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
            Log.Info("Trying to place Yellow Printer");

            if (!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return;
            }

            if (yellowPrinterSpawnCard == null)
            {
                Log.Error("Something wrong with card");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random
            };

            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(
                yellowPrinterSpawnCard,
                placementRule,
                Run.instance.stageRng
            );

            GameObject redPrinter = DirectorCore.instance.TrySpawnObject(spawnRequest);
            if (redPrinter)
            {
                Log.Info($"Yellow printer appeared at {redPrinter.transform.position} on stage");
            }
            else
            {
                Log.Error($"Couldn't place yellow printer on scene");
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

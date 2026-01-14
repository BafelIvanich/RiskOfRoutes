using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;



namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class YellowPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard yellowPrinterSpawnCard;

        public YellowPrinterModifier(int cost)
        {
            CreateYellowPrinterSpawnCard();
            name = "Yellow item printer";
            reservedText = "<style=cShrine>Yellow printer</style> is guaranteed to appear somewhere on the next stage.";
            activeText = "<style=cShrine>Yellow printer</style> has appeared somewhere on the stage.";
            negative = false;
            var ic = RiskOfRoutes.RiskOfRoutes.bundle.LoadAsset<Sprite>("yellowPrinter");
            if (ic != null)
            {
                icon = ic;
            }
            else
            {
                Log.Error("failed to load yellow printer modifier");
            }
            this.cost = cost;
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
            GameObject redPrinter = TryPlaceObjectOnScene(yellowPrinterSpawnCard, "Yellow item printer");
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

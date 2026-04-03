using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;



namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class YellowPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard yellowPrinterSpawnCard;

        public YellowPrinterModifier(int stack)
        {
            CreateYellowPrinterSpawnCard();
            this.stack = stack;
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

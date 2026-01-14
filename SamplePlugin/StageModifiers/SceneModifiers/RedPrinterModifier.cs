using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class RedPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard redPrinterSpawnCard;

        public RedPrinterModifier(int stack)
        {
            CreateRedPrinterSpawnCard();
            name = "Red item printer";
            reservedText = "<style=cDeath>Red printer</style> is guaranteed to appear somewhere on the next stage.";
            activeText = "<style=cDeath>Red printer</style> has appeared somewhere on the stage.";
            negative = false;
            var ic = RiskOfRoutes.RiskOfRoutes.bundle.LoadAsset<Sprite>("redPrinter");
            if (ic != null)
            {
                icon = ic;
            }
            else
            {
                Log.Error("failed to load red printer modifier");
            }
            this.stack = stack;
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
            GameObject redPrinter = TryPlaceObjectOnScene(redPrinterSpawnCard, "Red item printer");
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

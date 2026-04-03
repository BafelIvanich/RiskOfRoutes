using RiskOfRoutes;
using RoR2;
using UnityEngine;

namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class RedPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard redPrinterSpawnCard;

        public RedPrinterModifier(int stack)
        {
            CreateRedPrinterSpawnCard();

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
            //TODO вынести
            InteractableSpawnCard card = RiskOfRoutes.RiskOfRoutes.tier3Printer;

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

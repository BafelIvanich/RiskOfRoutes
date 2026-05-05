using RiskOfRoutes;
using RoR2;
using UnityEngine;

namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class WanderingChefModifier : StageModifier
    {

        public static bool spawnCardCreated = false;
        public static InteractableSpawnCard chefSpawnCard;

        public WanderingChefModifier()
        {
        }

        public override void OnEnd()
        {
            Log.Info("WanderingChef on end");
        }

        public override void OnStart()
        {
            Log.Info("WanderingChef on start");
            SpawnWanderingChef();
        }

        private void SpawnWanderingChef()
        {
            InteractableSpawnCard card = AssetManager.wanderingChefSpawnCard;

            GameObject wanderingChef = TryPlaceObjectOnScene(card, "Wandering Chef");
            if (wanderingChef)
            {
                wanderingChef.transform.position += wanderingChef.transform.up * 1.0f;
            }
        }


    }



}

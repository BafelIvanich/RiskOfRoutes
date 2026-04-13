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
            Log.Info("Gi");
        }

        public override void OnStart()
        {
            SpawnWanderingChef();
        }

        private void SpawnWanderingChef()
        {
            InteractableSpawnCard card = RiskOfRoutes.RiskOfRoutes.wanderingChefSpawnCard;

            GameObject wanderingChef = TryPlaceObjectOnScene(card, "Wandering Chef");
            if (wanderingChef)
            {
                Log.Info($"Wandering Chef appeared at {wanderingChef.transform.position} on stage");

                wanderingChef.transform.position += wanderingChef.transform.up * 1.0f;
            }
        }


    }



}

using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

//TODOD не уверен баг со спавном телепорта
namespace SamplePlugin.StageModifiers
{
    public class WanderingChefModifier : StageModifier
    {

        public static InteractableSpawnCard chefSpawnCard;

        public WanderingChefModifier()
        {
            CreateChefSpawnCard();
            name = "Wandering CHEF";
            activeText = "<style=cIsHealth>Wandering CHEF</style> has appeared somewhere on the stage";
            icon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC3/MealPrep/texMealPrepIcon.png").WaitForCompletion();
            reservedText = "<style=cIsHealth>Wandering CHEF</style> is guaranteed to appear on next stage.";
            stackable = false;
            negative = false;
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
            Log.Info("Trying to place CHEF");

            if (!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return;
            }

            InteractableSpawnCard card = chefSpawnCard;

            if (card == null)
            {
                Log.Error("Something wrong with card");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random
            };

            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(
                card,
                placementRule,
                Run.instance.stageRng
            );

            GameObject wanderingChef = DirectorCore.instance.TrySpawnObject(spawnRequest);
            if (wanderingChef)
            {
                Log.Info($"Wandering Chef appeared at {wanderingChef.transform.position} on stage");

                wanderingChef.transform.position += wanderingChef.transform.up * 1.0f;
            }
            else
            {
                Log.Error($"Couldn't place chef on scene");
            }



        }

        private void CreateChefSpawnCard()
        {
            Log.Info("Trying to create CHEF");
            InteractableSpawnCard card = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            card.prefab = RiskOfRoutes.RiskOfRoutes.chefPrefab;
            card.directorCreditCost = 0;
            card.sendOverNetwork = true;
            card.hullSize = HullClassification.Human;
            card.orientToFloor = true;
            card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            chefSpawnCard = card;
        }
    }

    

}

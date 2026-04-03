using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;


namespace SamplePlugin.StageModifiers
{
    [System.Serializable]
    public abstract class StageModifier
    {
        public int stack = 1;

        public bool stackable = true;

        public abstract void OnStart();

        public abstract void OnEnd();

        protected GameObject TryPlaceObjectOnScene(SpawnCard spawnCard, string name = "object")
        {
            if (!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return null;
            }

            if (spawnCard == null)
            {
                Log.Error($"Something went wrong with {name} spawncard");
                return null;
            }

            if (!Run.instance)
            {
                return null;
            }

            Log.Info($"Trying to place {name}");
            GameObject gameObject = null;
            for (int i = 0; i < 15; i++)
            {
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, Run.instance.stageRng));
                if (gameObject)
                {
                    Log.Info($"Succesfully placed {name} on {gameObject.transform.position}");
                    break;
                }
                else
                {
                    Log.Error($"Failed to place {name}, retrying");
                }
            }
            if (gameObject == null)
            {
                Log.Error($"It was impossible to place {name}");
            }
            return gameObject;
        }

    }



}


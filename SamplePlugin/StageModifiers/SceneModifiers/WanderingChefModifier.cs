using R2API;
using RiskOfRoutes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

//TODOD не уверен баг со спавном телепорта
namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class WanderingChefModifier : StageModifier
    {

        public static bool spawnCardCreated = false;
        public static InteractableSpawnCard chefSpawnCard;

        public WanderingChefModifier()
        {
            stackable = false;
            negative = false;
            if (!spawnCardCreated) CreateChefSpawnCard();
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
            InteractableSpawnCard card = chefSpawnCard;

            GameObject wanderingChef = TryPlaceObjectOnScene(card, "Wandering Chef");
            if (wanderingChef)
            {
                Log.Info($"Wandering Chef appeared at {wanderingChef.transform.position} on stage");

                wanderingChef.transform.position += wanderingChef.transform.up * 1.0f;
            }
        }

        private void CreateChefSpawnCard()
        {
            Log.Info("Trying to create CHEF");
            InteractableSpawnCard card = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            GameObject lunarTable = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/Bazaar_LunarTable.prefab").WaitForCompletion();

            var mealPrep = RiskOfRoutes.RiskOfRoutes.chefPrefab;
            //GameObject chefCopy = Object.Instantiate(RiskOfRoutes.RiskOfRoutes.chefPrefab);
            //chefTable.transform.SetParent(mealPrep.transform, false);
            //var chefTable = Addressables.LoadAsset<GameObject>("RoR2/DLC3/computationalexchange/mdlMealPrepTable.fbx").WaitForCompletion();

            //Объекты на столе
            var steak1 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FlatHealth/DisplaySteakFlat.prefab").WaitForCompletion().InstantiateClone("steak1");
            var steak2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FlatHealth/DisplaySteakFlat.prefab").WaitForCompletion().InstantiateClone("steak2");
            var steak3 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FlatHealth/DisplaySteakFlat.prefab").WaitForCompletion().InstantiateClone("steak3");
            var seed = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Seed/PickupSeed.prefab").WaitForCompletion().InstantiateClone("seed");
            var cleaver = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/meshChefCleaverGhost1.FBX").WaitForCompletion().InstantiateClone("cleaverGhost");
            var wok = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/lemuriantemple/Assets/LTWok.prefab").WaitForCompletion().InstantiateClone("wok");

            steak1.transform.SetParent(mealPrep.transform, false);
            steak2.transform.SetParent(mealPrep.transform, false);
            steak3.transform.SetParent(mealPrep.transform, false);
            seed.transform.SetParent(mealPrep.transform, false);
            cleaver.transform.SetParent(mealPrep.transform, false);
            wok.transform.SetParent(mealPrep.transform, false);
            lunarTable.transform.SetParent(mealPrep.transform, false);

            steak1.transform.localPosition = new Vector3();
            steak1.transform.rotation = Quaternion.Euler(new Vector3());
            steak1.transform.localScale = new Vector3();

            steak1.transform.localPosition = new Vector3(0.806f, -0.061f, 1.474f);
            steak1.transform.localRotation = Quaternion.Euler(272.0293f, 356.7472f, 66.6001f);
            steak1.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            steak2.transform.localPosition = new Vector3(-0.522f, -0.061f, 2.164f);
            steak2.transform.localRotation = Quaternion.Euler(272.0291f, 245.6929f, 66.6f);
            steak2.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            steak3.transform.localPosition = new Vector3(-0.492f, 0.088f, 2.209f);
            steak3.transform.localRotation = Quaternion.Euler(274.538f, 270.5f, 90.0001f);
            steak3.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            seed.transform.localPosition = new Vector3(0.366f, -0.162f, 1.872f);
            seed.transform.localRotation = Quaternion.Euler(48.0785f, 327.5262f, 319.4f);
            seed.transform.localScale = new Vector3(0.1043f, 0.1043f, 0.1043f);

            wok.transform.localPosition = new Vector3(-0.28f, 0.7f, -1.79f);
            wok.transform.localRotation = Quaternion.Euler(270f, 302.2861f, 0f);
            wok.transform.localScale = new Vector3(3.1129f, 3.1129f, 3.1129f);

            lunarTable.transform.localPosition = new Vector3(-0.289f, -1.256f, 0.01f);
            lunarTable.transform.localRotation = Quaternion.Euler(270f, 90f, 0f);
            lunarTable.transform.localScale = new Vector3(0.9041f, 0.9041f, 0.9041f);

            //для prefab другие но он не грузится
            cleaver.transform.localPosition = new Vector3(0.922f, -0.7f, -0.01f);
            cleaver.transform.localRotation = Quaternion.Euler(0f, 205.2114f, 127.0116f);
            cleaver.transform.localScale = new Vector3(1f, 1f, 1f);

            var cleaverMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Chef/matChef_Cleaver.mat").WaitForCompletion();
            var renderer = cleaver.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = cleaverMat;
            }

            //todo разные материалы
            //var tableMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/goolake/matGoolakeStoneStructs.mat").WaitForCompletion();
            //var rendererTable = lunarTable.GetComponentInChildren<Renderer>();
            //if(rendererTable != null)
            //{
            //    rendererTable.material = tableMat;
            //}

            card.prefab = RiskOfRoutes.RiskOfRoutes.chefPrefab;
            card.directorCreditCost = 0;
            card.sendOverNetwork = true;
            card.hullSize = HullClassification.Human;
            card.orientToFloor = true;
            card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            chefSpawnCard = card;
            spawnCardCreated = true;
        }
    }



}

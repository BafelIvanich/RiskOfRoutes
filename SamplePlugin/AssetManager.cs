using R2API;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskOfRoutes
{
    public class AssetManager
    {
        public static AssetBundle bundle;

        public static GameObject routePortalPrefab;
        public static GameObject iconBubblePrefab;
        public static GameObject chefPrefab;

        public static Texture2D droneSymbolTexture;
        public static Texture2D rewardSymbolTexture;
        public static Texture2D aurelioniteSymbolTexture;

        public static InteractableSpawnCard tier1Printer;
        public static InteractableSpawnCard tier2Printer;
        public static InteractableSpawnCard tier3Printer;
        public static InteractableSpawnCard bossPrinter;
        public static InteractableSpawnCard wanderingChefSpawnCard;

        public static TMP_FontAsset bomb;

        public static Sprite placeholderIcon;

        public static ObjectScaleCurve pingCurve;

        public static Animator progressBarAnimator;

        public static void Initialize(AssetBundle assetBundle)
        {
            bundle = assetBundle;

            routePortalPrefab = CreateRoutePortalPrefab();
            iconBubblePrefab = CreateIconBubblePrefab();
            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");

            droneSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterDrone");
            rewardSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterReward");
            aurelioniteSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterAurelionite");

            tier1Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion();
            tier2Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset").WaitForCompletion();
            tier3Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion();
            bossPrinter = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion();
            CreateChefSpawnCard();

            bomb = Addressables.LoadAssetAsync<TMP_FontAsset>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropShadow.asset").WaitForCompletion();

            placeholderIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();

            GameObject pingPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/PingIndicator.prefab").WaitForCompletion();
            PingIndicator pingIndicator = pingPrefab.GetComponent<PingIndicator>();
            pingCurve = pingIndicator.pingObjectScaleCurve;


            CheckAssetsLoaded();
        }

        private static GameObject CreateRoutePortalPrefab()
        {
            GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion(), "RoutePortal");
            Log.Info("Invoked");

            UnityEngine.Object.Destroy(routePortal.GetComponent<GenericInteraction>());
            UnityEngine.Object.Destroy(routePortal.GetComponent<SceneExitController>());

            var scaleCurve = routePortal.GetComponentInChildren<ObjectScaleCurve>();

            //у ваниллы 0.7 1.26
            float portalScaleX = 1f;
            float portalScaleY = 1f;
            routePortal.name = "RoutePortal";
            routePortal.transform.localScale = new Vector3(portalScaleX, portalScaleY, portalScaleX);

            var portalCenter = routePortal.transform.Find("PortalCenter");
            if (portalCenter != null)
            {
                var pointLight = portalCenter.Find("Point Light");
                if (pointLight != null)
                {
                    var light = pointLight.GetComponent<Light>();
                    if (light != null)
                    {
                        light.intensity = 2;

                    }
                    else
                    {
                        Log.Error("Not ifound lighthh");
                    }
                }
                else
                {
                    Log.Info("Not found poitn lignt");
                }
            }
            else
            {
                Log.Info("Not found center");
            }

            var renderers = routePortal.GetComponentsInChildren<Renderer>();

            RoutePortalManager mgr = routePortal.AddComponent<RoutePortalManager>();
            //Material mat = SceneCatalog.GetSceneDef((SceneIndex)mgr.destinationSceneIndex).portalMaterial;

            Texture2D edge = null;
            Texture2D center = null;

            if(RiskOfRoutes.usePortalTypeColors.Value)
            {
                Log.Info("Using black and white");
                edge = bundle.LoadAsset<Texture2D>("bwCenter");
                center = bundle.LoadAsset<Texture2D>("bwEdge");
            }
            else
            {
                edge = bundle.LoadAsset<Texture2D>("darkerBlueEdge");
                center = bundle.LoadAsset<Texture2D>("darkerBlueCenter");
            }

            foreach (var renderer in renderers)
            {
                if (renderer.material.name.Contains("Center"))
                {
                    if (RiskOfRoutes.usePortalMaterial.Value)
                    {
                        //renderer.material = mat;
                    }
                    else
                    {
                        renderer.material.SetTexture("_RemapTex", center);
                    }
                }
            }

            //создаем интеракцию
            GenericInteraction interaction = routePortal.AddComponent<GenericInteraction>();
            SceneExitController sceneExitController = routePortal.AddComponent<SceneExitController>();
            GenericDisplayNameProvider genericDisplayNameProvider = routePortal.AddComponent<GenericDisplayNameProvider>();
            GenericInspectInfoProvider genericInspectInfo = routePortal.AddComponent<GenericInspectInfoProvider>();

            //todo таб сюдаа
            mgr.genericInteraction = interaction;
            mgr.sceneExitController = sceneExitController;

            interaction.contextToken = "Enter Route Portal";

            //routePortal.GetComponent<Highlight>().targetRenderer = routePortal.GetComponentInChildren<SkinnedMeshRenderer>();
            //GameObject trigger = Instantiate(new GameObject("Trigger"), routePortal.transform);
            //trigger.AddComponent<BoxCollider>().isTrigger = true;
            //trigger.AddComponent<EntityLocator>().entity = routePortal;

            return routePortal;
        }

        public static GameObject CreateIconBubblePrefab()
        {
            var seerPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion(), "seer");

            Transform displayPivot = seerPrefab.transform.Find("Model").Find("DisplayPivot");
            Transform portalDisplay = displayPivot.Find("PortalActiveDisplay");

            if (portalDisplay)
            {
                GameObject parent = new GameObject("IconBubblePrefab");

                UnityEngine.Object.DontDestroyOnLoad(parent);
                parent.SetActive(false);
                portalDisplay.SetParent(parent.transform);
                portalDisplay.transform.localPosition = new Vector3(0, 0f, 0);
                portalDisplay.transform.localRotation = Quaternion.Euler(0f, 180f, 0);
                portalDisplay.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                var pointLight = portalDisplay.transform.Find("Point Light");
                var light = pointLight.GetComponent<Light>();
                light.intensity = 1;
                var mr = portalDisplay.GetComponentsInChildren<Renderer>();
                foreach (var rend in mr)
                {
                    if (rend.name == "Portal")
                    {
                        var mat = rend.materials[0];

                        mat.SetTexture("_Cloud1Tex", null);
                        mat.SetTexture("_Cloud2Tex", null);

                        mat.SetFloat("_Boost", 1.0f);
                        mat.SetFloat("_AlphaBoost", 1.0f);
                        mat.SetFloat("_DistortionStrength", 0f);

                        mat.SetTextureScale("_MainTex", new Vector2(1.75f, 1.75f));
                        mat.SetTextureOffset("_MainTex", new Vector2(-0.4f, -0.4f));
                        rend.materials = new Material[] { mat, mat };
                    }

                }

                GameObject.Destroy(seerPrefab);
                return parent;

            }
            return seerPrefab;
        }

        private static void CreateChefSpawnCard()
        {
            Log.Info("Trying to create CHEF");
            InteractableSpawnCard card = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            GameObject lunarTable = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/Bazaar_LunarTable.prefab").WaitForCompletion();

            var mealPrep = chefPrefab;

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

            //для prefab другие но он не грузится
            cleaver.transform.localPosition = new Vector3(0.922f, -0.7f, -0.01f);
            cleaver.transform.localRotation = Quaternion.Euler(0f, 205.2114f, 127.0116f);
            cleaver.transform.localScale = new Vector3(1f, 1f, 1f);

            wok.transform.localPosition = new Vector3(-0.28f, 0.7f, -1.79f);
            wok.transform.localRotation = Quaternion.Euler(270f, 302.2861f, 0f);
            wok.transform.localScale = new Vector3(3.1129f, 3.1129f, 3.1129f);

            lunarTable.transform.localPosition = new Vector3(-0.289f, -1.256f, 0.01f);
            lunarTable.transform.localRotation = Quaternion.Euler(270f, 90f, 0f);
            lunarTable.transform.localScale = new Vector3(0.9041f, 0.9041f, 0.9041f);

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

            card.prefab = chefPrefab;
            card.directorCreditCost = 0;
            card.sendOverNetwork = true;
            card.hullSize = HullClassification.Human;
            card.orientToFloor = true;
            card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            wanderingChefSpawnCard = card;
        }

        public static InteractableSpawnCard CreateRoutePortalSpawnCard(SceneDef destination)
        {
            var m = routePortalPrefab.GetComponent<RoutePortalManager>();
            m.destinationSceneIndex = (int)destination.sceneDefIndex;

            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            interactableSpawnCard.name = "iscRoutePortal";
            interactableSpawnCard.prefab = routePortalPrefab;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.hullSize = HullClassification.Human;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
            interactableSpawnCard.directorCreditCost = 0;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.orientToFloor = false;
            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            return interactableSpawnCard;
        }

        public static void CheckAssetsLoaded()
        {
            bool loadingFailed = false;

            if (bundle == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(bundle)}");
            }

            if (routePortalPrefab == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(routePortalPrefab)}");
            }

            if (iconBubblePrefab == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(iconBubblePrefab)}");
            }

            if (chefPrefab == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(chefPrefab)}");
            }

            if (droneSymbolTexture == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(droneSymbolTexture)}");
            }

            if (rewardSymbolTexture == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(rewardSymbolTexture)}");
            }

            if (aurelioniteSymbolTexture == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(aurelioniteSymbolTexture)}");
            }

            if (tier1Printer == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(tier1Printer)}");
            }

            if (tier2Printer == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(tier2Printer)}");
            }

            if (tier3Printer == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(tier3Printer)}");
            }

            if (bossPrinter == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(bossPrinter)}");
            }

            if (wanderingChefSpawnCard == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(wanderingChefSpawnCard)}");
            }

            if (bomb == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(bomb)}");
            }

            if (placeholderIcon == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(placeholderIcon)}");
            }

            if (pingCurve == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(pingCurve)}");
            }

            if (loadingFailed)
            {
                Log.Error($"{nameof(CheckAssetsLoaded)}: One or more assets failed to load");
            }
            else
            {
                Log.Info($"{nameof(CheckAssetsLoaded)}: All assets loaded");
            }
        }

    }
}

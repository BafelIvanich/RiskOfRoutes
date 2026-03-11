using BepInEx;
using BepInEx.Configuration;
using LeTai.Asset.TranslucentImage;
using R2API;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using RoR2.UI;
using SamplePlugin;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityHotReloadNS;
using Image = UnityEngine.UI.Image;

//TODO
//Core:
//Заменить seerStationController на базаре
//SyncList не работает как надо
//Особые модификаторы для стейджей
//(Подкорректировать)Добавить больше предметов в пулл зеленых и белых, добавить возможность для такого же с предметами любого тира через конфиг
//Сделать, чтобы всё работало в мультиплеере
//Артефакт инфляции
//(Возможно) фиксированные места поялвения шефа
//(Подкорректировать)Назначение exitController и purchase в RoutePortalManager нужно делать вручную, стоит сделать awake()
//Конфликт с артефакт модификат
//анти прок?
//реген меньше
//артефакт лимит модификаторов
//атрефакт стейджи в разном порядке
//(Возможно) более ровный спавн порталов
//!ROUTE PORTAL MANAGER хуки пересмотреть и почти все с обновлением текста
//почитать
//на улучшение дронов
//artifact of fallacy
//проверить появление шефа


//Визуал:
//Текстура локи в портале


//(Возможно) Скрытые модификаторы и предметы их раскрывающие
//(Возможно) Модификатор исключающий категорию предметов из пулла
//(Возможно) Покупка всего за хп
//(Возможно) Меньше сундуков, но больше магазинов
//(Возможно) Портал, очищающий лунные предметы
//(Возможно) категории порталов?
//Утилити урон хил
//Категории порталов
//Эклипсовые

//Пост релиз
//(Сложно) UI карты на манер шпиля
//(Возможно) Можно ли сделать так, чтобы любые мобы могли воровать предметы?
//(Возможно) Бродячий дрифтер или можно рюкзак
//(Возможно) рюзкак дрифтера позволяет поделиться предметами
//(Возможно) Void Stage
//(Возможно) Святилище, что рероллит все предметы, лежащие на земле
//(Возможно)стейдж где алтари удачи заменяют сундуки но другой дроп голды
//(Возможно)Модификаторы длящиеся несколько стейджей
//(Возможно)Если надоедать продавцу на базаре то гарантирует сложный портал


[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RiskOfRoutes
{
    // https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]

    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.0.0";

        //tweaks
        public static ConfigEntry<int> routeCount { get; set; }
        public static ConfigEntry<bool> usePortalMaterial { get; set; }
        public static ConfigEntry<bool> uniqueDestinations { get; set; }
        public static ConfigEntry<bool> showPortalDestination { get; set; }
        public static ConfigEntry<bool> teleporterUseRandomMods { get; set; }
        public static ConfigEntry<bool> modifiersCanBeBanned { get; set; }
        //artifacts
        public static ConfigEntry<bool> allowChaosArtifact;
        public static ConfigEntry<bool> allowCommandArtifact;
        public static ConfigEntry<bool> allowDeathArtifact;
        public static ConfigEntry<bool> allowDelusionArtifact;
        public static ConfigEntry<bool> allowDevotionArtifact;
        public static ConfigEntry<bool> allowDissonanceArtifact;
        public static ConfigEntry<bool> allowEnigmaArtifact;
        public static ConfigEntry<bool> allowEvolutionArtifact;
        public static ConfigEntry<bool> allowFrailtyArtifact;
        public static ConfigEntry<bool> allowGlassArtifact;
        public static ConfigEntry<bool> allowHonorArtifact;
        public static ConfigEntry<bool> allowKinArtifact;
        public static ConfigEntry<bool> allowMetamorphosisArtifact;
        public static ConfigEntry<bool> allowRebirthArtifact;
        public static ConfigEntry<bool> allowSacrificeArtifact;
        public static ConfigEntry<bool> allowSoulArtifact;
        public static ConfigEntry<bool> allowSpiteArtifact;
        public static ConfigEntry<bool> allowSwarmsArtifact;
        public static ConfigEntry<bool> allowVengeanceArtifact;
        //other modifiers
        public static ConfigEntry<int> droneStackPercent;


        public static GameObject routePortalPrefab;
        public static GameObject iconBubblePrefab;
        public static GameObject chefPrefab;
        public static GameObject routePortalPanelPrefab;
        public static GameObject routeMapPanelPrefab;

        public static GameObject routePortalPanel;
        public static GameObject routeMapPanel;

        private HUD hud = null;

        private InteractableSpawnCard portalSpawnCard;
        public static AssetBundle bundle;
        private TMP_FontAsset bomb;

        public static Sprite placeholderIcon;

        public static CombatDirector.EliteTierDef[] defaultEliteTiers;

        internal static GameObject CentralNetworkObject;
        private static GameObject _centralNetworkObjectSpawned;

        public void Awake()
        {
            Log.Init(Logger);
            //networking
            var tmpGo = new GameObject("tmpGo");
            tmpGo.AddComponent<NetworkIdentity>();
            CentralNetworkObject = PrefabAPI.InstantiateClone(tmpGo, "somethingUnique", false);
            GameObject.Destroy(tmpGo);
            CentralNetworkObject.AddComponent<StageModifierDirector>();
            CentralNetworkObject.AddComponent<MyNetworkComponent>();
            PrefabAPI.RegisterNetworkPrefab(CentralNetworkObject);
            RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;

            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
            //
            BindConfig();


            //для своих префабов
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "riskofroutes");
            bundle = AssetBundle.LoadFromFile(path);
            //префабы
            bomb = Addressables.LoadAssetAsync<TMP_FontAsset>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropShadow.asset").WaitForCompletion();
            placeholderIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");
            iconBubblePrefab = CreateIconBubblePrefab();
            routePortalPrefab = CreateRoutePortalPrefab();
            //routePortalPanelPrefab = CreateRoutePortalPanelPrefab();



            //для moreelitesmodifier
            FieldInfo eliteTiersField = typeof(CombatDirector).GetField("eliteTiers", BindingFlags.NonPublic | BindingFlags.Static);
            if (eliteTiersField != null)
            {
                defaultEliteTiers = (CombatDirector.EliteTierDef[])eliteTiersField.GetValue(null);
                Log.Info("Succesfully loaded elite tiers");
            }
            else
            {
                Log.Error("Could not load default elite tiers");
                defaultEliteTiers = new CombatDirector.EliteTierDef[] { };
            }

            //хуки

            //ui
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.Refresh += CurrentRunArtifactDisplayDataDriver_Refresh;

            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterEventSpawnRoutePortals;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += TeleporterInteraction_OnInteractionBegin;
            //On.RoR2.PortalSpawner.AttemptSpawnPortalServer += PortalSpawner_AttemptSpawnPortalServer;

            On.RoR2.DirectorCore.TrySpawnObject += OnVanillaPortalSpawn;

            //Портал выхода с базара
            //On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.BazaarController.Start += BazaarController_Start;
            On.RoR2.SeerStationController.SetRunNextStageToTarget += SeerStationController_SetRunNextStageToTarget;
            On.RoR2.SeerStationController.OnStartClient += GetModifiersForSeerStation;

            RoR2.RoR2Application.onLoad += () =>
            {
                IconCatalog.Init(bundle);
            };

            AddRouteManagersToPortalPrefabs();

        }

        private void BazaarController_Start(On.RoR2.BazaarController.orig_Start orig, BazaarController self)
        {
            orig(self);

            if (NetworkServer.active)
            {
                Log.Info("Replacing portal");

                GameObject shopPortal = null;
                RoR2.SceneExitController[] exits = FindObjectsOfType<RoR2.SceneExitController>(true);
                foreach (var exit in exits)
                {
                    if (exit.useRunNextStageScene || exit.gameObject.name.Contains("Shop"))
                    {
                        shopPortal = exit.gameObject;
                        Log.Info($"Shop portal with name {exit.gameObject.name}");
                        break;
                    }
                }

                if (shopPortal != null)
                {
                    Vector3 pos = shopPortal.transform.position;
                    Quaternion rot = shopPortal.transform.rotation;

                    NetworkServer.Destroy(shopPortal);

                    GameObject shopRoutePortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion();
                    GameObject shopRoutePortal = Object.Instantiate(shopRoutePortalPrefab, pos, rot);

                    var mg = shopRoutePortal.GetComponent<RoutePortalManager>();
                    if (mg != null)
                    {
                        mg.enabled = true;
                        mg.isManagerEnabled = true;

                        mg.destinationSceneIndex = (int)(RoR2.Run.instance.nextStageScene.sceneDefIndex);
                        mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(shopRoutePortal);
                        mg.isOval = true;
                        mg.UpdateText();
                        Log.Info("Portal was replaced");
                    }
                    else
                    {
                        Log.Error("Shop portal does not have manager");
                    }

                    NetworkServer.Spawn(shopRoutePortal);
                    Log.Info("Portal spawned on network");

                    SceneExitController sceneExit = shopRoutePortal.GetComponent<SceneExitController>();

                    if (sceneExit != null && self.seerStations != null)
                    {
                        List<SeerStationController> newSeerStations = new List<SeerStationController>();
                        foreach (var oldSeerStationController in self.seerStations)
                        {
                            if (oldSeerStationController != null)
                            {
                                var oldSeer = oldSeerStationController.gameObject;

                                Vector3 seerPos = oldSeer.transform.position;
                                Quaternion seerRot = oldSeer.transform.rotation;
                                int oldTargetSceneIndex = oldSeerStationController.targetSceneDefIndex;
                                
                                NetworkServer.Destroy(oldSeer);

                                GameObject seerStationPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion();
                                GameObject newSeer = Object.Instantiate(seerStationPrefab, seerPos, seerRot);

                                var newController = newSeer.GetComponent<SeerStationController>();
                                if (newController != null)
                                {
                                    newController.targetSceneDefIndex = oldTargetSceneIndex;
                                    newController.explicitTargetSceneExitController = sceneExit;
                                }
                                else
                                {
                                    Log.Error("Does not have controller by default");
                                }

                                var seerRouteManager = newSeer.GetComponent<RoutePortalManager>();
                                if(seerRouteManager != null)
                                {
                                    seerRouteManager.isSeerStation = true;
                                    seerRouteManager.destinationSceneIndex = oldTargetSceneIndex;
                                    seerRouteManager.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(newSeer);

                                    seerRouteManager.enabled = true;
                                    seerRouteManager.isManagerEnabled = true;
                                    seerRouteManager.UpdateText();
                                }

                                NetworkServer.Spawn(newSeer);
                                newSeerStations.Add(newController);
                                Log.Info("Seer was replaced");
                            }
                        }

                        self.seerStations = newSeerStations.ToArray();
                    }

                }
                else
                {
                    Log.Error("Shop portal was not found");
                }
            }
        }

        private void AddRouteManagersToPortalPrefabs()
        {
            var shopPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion();
            var goldshoresPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalGoldshores/PortalGoldshores.prefab").WaitForCompletion();
            var colossusPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/PortalColossus.prefab").WaitForCompletion();
            var hardwareProgPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/HardwareProgPortal.prefab").WaitForCompletion();
            var seerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion();

            List<GameObject> vanillaPortals = new List<GameObject>() { shopPortalPrefab, colossusPortalPrefab, goldshoresPortalPrefab, hardwareProgPortalPrefab };
            foreach (var portal in vanillaPortals)
            {
                Log.Info($"Adding route portal manager to portal {portal.name}");
                var mg = portal.GetComponent<RoutePortalManager>();
                if (mg == null)
                {
                    portal.AddComponent<RoutePortalManager>();
                }
                Log.Info($"Added route portal manager to {portal.name} prefab");
                mg = portal.GetComponent<RoutePortalManager>();
                if (mg != null)
                {
                    mg.enabled = false;
                    Log.Info($"Disabled manager for {portal}");
                }
                else
                {
                    Log.Info("Null for some reason");
                }
            }
            var seerMg = seerPrefab.GetComponent<RoutePortalManager>();
            if (seerMg == null)
            {
                seerPrefab.AddComponent<RoutePortalManager>();
                Log.Info($"Added route portal manager to seer prefab");
            }
            seerMg = seerPrefab.GetComponent<RoutePortalManager>();
            if (seerMg != null)
            {
                seerMg.enabled = true;
                seerMg.isManagerEnabled = true;
                seerMg.isSeerStation = true;
            }

        }

        private void TeleporterInteraction_OnInteractionBegin(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (teleporterUseRandomMods.Value)
                StageModifierDirector.instance.GetModifiersFromRandomPortal();
        }

        private GameObject CreateRoutePortalPrefab()
        {
            GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion(), "RoutePortal");
            Log.Info("Invoked");

            Destroy(routePortal.GetComponent<GenericInteraction>());
            Destroy(routePortal.GetComponent<SceneExitController>());

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

            //Material mat = SceneCatalog.GetSceneDef((SceneIndex)mg.destinationSceneIndex).portalMaterial;

            var edge = bundle.LoadAsset<Texture2D>("darkerBlueEdge");
            var center = bundle.LoadAsset<Texture2D>("darkerBlueCenter");
            foreach (var renderer in renderers)
            {
                if (renderer.material.name.Contains("Center"))
                {
                    if (usePortalMaterial.Value)
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
            RoutePortalManager mgr = routePortal.AddComponent<RoutePortalManager>();
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

        private InteractableSpawnCard CreateRoutePortalSpawnCard(SceneDef destination)
        {
            var m = routePortalPrefab.GetComponent<RoutePortalManager>();
            m.destinationSceneIndex = (int)destination.sceneDefIndex;

            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            interactableSpawnCard.name = "iscRoutePortal";
            //interactableSpawnCard.prefab = routePortalPrefab;
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

            portalSpawnCard = interactableSpawnCard;

            return interactableSpawnCard;
        }


        //todo объект спавнится в 0 0 0
        //todo убрать clamp текстуры
        public static GameObject CreateIconBubblePrefab()
        {
            var seerPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion(), "seer");

            Transform displayPivot = seerPrefab.transform.Find("Model").Find("DisplayPivot");
            Transform portalDisplay = displayPivot.Find("PortalActiveDisplay");

            if (portalDisplay)
            {
                GameObject parent = new GameObject("IconBubblePrefab");

                DontDestroyOnLoad(parent);
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

        //todo перестал грузиться
        private GameObject CreateRoutePortalPanelPrefab()
        {
            if (bundle == null)
            {
                Log.Error("Asset bundle is missing");
                return null;
            }

            GameObject prefab = bundle.LoadAsset<GameObject>("RoutePortalPortraitPanel");
            if (prefab == null)
            {
                Log.Error("What?");
                return null;
            }
            //блюр
            TranslucentImage blur = prefab.AddComponent<TranslucentImage>();
            if (blur == null)
            {
                Log.Error("Blur not loaded");
            }
            Shader blurShader = Shader.Find("UI/TranslucentImage");
            if (blurShader == null)
            {
                Log.Error("Shader not found");
            }
            blur.material = new Material(blurShader);
            blur.color = new Color(0, 0, 0, 0.7f);
            //текст //плейсхолдер
            var text = prefab.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null)
            {
                Log.Error("No text meshprougui");
            }
            if (text == null) Log.Error("Not found");
            text.text = "Placeholder";
            text.font = bomb;

            return prefab;
        }



        private void Run_onRunStartGlobal(Run obj)
        {
            if (NetworkServer.active)
            {
                if (!_centralNetworkObjectSpawned)
                {
                    _centralNetworkObjectSpawned = GameObject.Instantiate(CentralNetworkObject);
                    NetworkServer.Spawn(_centralNetworkObjectSpawned);
                    Log.Info("StageModifierDirector spawned");
                }
            }
        }

        private void BindConfig()
        {
            //tweaks
            routeCount = Config.Bind<int>(
                "Tweaks", "Route Count", 3, "How much route portals would appear after stage completion.");
            ModSettingsManager.AddOption(new IntSliderOption(routeCount, new IntSliderConfig() { min = 1, max = 10 }));

            usePortalMaterial = Config.Bind<bool>(
                "Tweaks", "Use Scene Material", false, "[WIP] Sets destination scene portrait as a portal texture");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalMaterial));

            showPortalDestination = Config.Bind<bool>(
                "Tweaks", "Show Portal Destination", true, "Show destination scene in hud");
            ModSettingsManager.AddOption(new CheckBoxOption(showPortalDestination));

            uniqueDestinations = Config.Bind<bool>(
                "Tweaks", "Unique Destination", true, "Makes each route portal have unique scene as destination, limits their count to amount of scenes available");
            ModSettingsManager.AddOption(new CheckBoxOption(uniqueDestinations));

            teleporterUseRandomMods = Config.Bind<bool>(
                "Tweaks", "Teleporter Use Random mods", true, "Makes teleporter use random modifiers of spawned route portals after interacting with it, set false if you want it not to use any stage modifiers");
            ModSettingsManager.AddOption(new CheckBoxOption(teleporterUseRandomMods));
            //todo
            modifiersCanBeBanned = Config.Bind<bool>(
                "Tweaks", "Modifiers can be banned after being used", true, "Prevents some of stage modifiers appearing on the next stage, if they were active on a current stage");
            ModSettingsManager.AddOption(new CheckBoxOption(modifiersCanBeBanned));

            //artifacts
            allowChaosArtifact = Config.Bind<bool>(
               "Artifacts", "Chaos", true, "Chaos artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowChaosArtifact));

            allowDeathArtifact = Config.Bind<bool>(
                "Artifacts", "Death", true, "Death artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDeathArtifact));

            allowDissonanceArtifact = Config.Bind<bool>(
                "Artifacts", "Dissonance", true, "Dissonance artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDissonanceArtifact));

            allowFrailtyArtifact = Config.Bind<bool>(
                "Artifacts", "Frailty", true, "Frailty artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowFrailtyArtifact));

            allowHonorArtifact = Config.Bind<bool>(
                "Artifacts", "Honor", true, "Honor artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowHonorArtifact));

            allowSacrificeArtifact = Config.Bind<bool>(
                "Artifacts", "Sacrifice", true, "Sacrifice artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSacrificeArtifact));

            allowSoulArtifact = Config.Bind<bool>(
                "Artifacts", "Soul", true, "Soul artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSoulArtifact));

            allowSpiteArtifact = Config.Bind<bool>(
                "Artifacts", "Spite", true, "Spite artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSpiteArtifact));

            allowSwarmsArtifact = Config.Bind<bool>(
                "Artifacts", "Swarms", true, "Swarms artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSwarmsArtifact));

            allowVengeanceArtifact = Config.Bind<bool>(
                "Artifacts", "Vengeance", true, "Vengeance artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowVengeanceArtifact));

            allowCommandArtifact = Config.Bind<bool>(
                "Artifacts", "Command", false, "Command artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowCommandArtifact));

            allowDelusionArtifact = Config.Bind<bool>(
                "Artifacts", "Delusion", false, "Delusion artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDelusionArtifact));

            allowDevotionArtifact = Config.Bind<bool>(
                "Artifacts", "Devotion", false, "Devotion artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDevotionArtifact));

            allowEnigmaArtifact = Config.Bind<bool>(
                "Artifacts", "Enigma", false, "Enigma artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowEnigmaArtifact));

            allowEvolutionArtifact = Config.Bind<bool>(
                "Artifacts", "Evolution", false, "Evolution artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowEvolutionArtifact));

            allowGlassArtifact = Config.Bind<bool>(
                "Artifacts", "Glass", false, "Glass artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowGlassArtifact));

            allowKinArtifact = Config.Bind<bool>(
                "Artifacts", "Kin", false, "Kin artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowKinArtifact));

            allowMetamorphosisArtifact = Config.Bind<bool>(
                "Artifacts", "Metamorphosis", false, "Metamorphosis artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMetamorphosisArtifact));

            allowRebirthArtifact = Config.Bind<bool>(
                "Artifacts", "Rebirth", false, "Rebirth artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowRebirthArtifact));



        }

        ////todo проверить несколько раз за забег
        private void GetModifiersForSeerStation(On.RoR2.SeerStationController.orig_OnStartClient orig, SeerStationController self)
        {
            orig(self);

            if (NetworkServer.active)
            {
                var mg = self.gameObject.GetComponent<RoutePortalManager>();
                if (mg != null)
                {
                    SceneIndex targetScene = (SceneIndex)self.targetSceneDefIndex;
                    if (targetScene != SceneIndex.Invalid)
                    {
                        mg.destinationSceneIndex = (int)targetScene;
                    }
                    else
                    {
                        Log.Error("Seer does not have scene");
                    }
                    Log.Info($"Seer destination:{mg.destinationSceneIndex}");
                    mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(self.gameObject);
                }
                else
                {
                    Log.Error("Seer doesnt have manaeger");
                }


            }

        }

        private void SeerStationController_SetRunNextStageToTarget(On.RoR2.SeerStationController.orig_SetRunNextStageToTarget orig, SeerStationController self)
        {
            orig(self);

            if (NetworkServer.active)
            {
                SceneIndex targetScene = (SceneIndex)self.targetSceneDefIndex;

                if (SceneManager.GetActiveScene().name == "bazaar")
                {
                    GameObject shopPortal = null;
                    SceneExitController exitController = null;
                    RoR2.SceneExitController[] exits = FindObjectsOfType<RoR2.SceneExitController>(true);
                    foreach (var exit in exits)
                    {
                        if (exit.useRunNextStageScene || exit.gameObject.name.Contains("Shop"))
                        {
                            shopPortal = exit.gameObject;
                            exitController = exit;
                            Log.Info($"Shop portal with name {exit.gameObject.name}");
                            break;
                        }
                    }

                    if (!shopPortal)
                    {
                        Log.Error("Could not find shopportal on bazaar scene");
                    }
                    if (!shopPortal.GetComponent<RoutePortalManager>())
                    {
                        //todo sync после смены destination
                        var mg = shopPortal.AddComponent<RoutePortalManager>();
                        mg.destinationSceneIndex = (int)targetScene;
                        mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(shopPortal);
                        Log.Info("Succesfully attached route portal manager to shop exit portal");
                    }
                    else
                    {
                        var seerRouteManager = self.gameObject.GetComponent<RoutePortalManager>();

                        var mg = shopPortal.GetComponent<RoutePortalManager>();
                        int previous = (int)mg.destinationSceneIndex;
                        mg.destinationSceneIndex = (int)targetScene;
                        if (seerRouteManager)
                            mg.stageModifiers = seerRouteManager.stageModifiers;
                        else
                            mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(shopPortal);
                        mg.UpdateText();
                        Log.Info($"Updated exit destination:{previous}->{mg.destinationSceneIndex}");
                    }

                }
                else
                {
                    Log.Debug("Not bazaar");
                }
            }
        }

        //private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        //{
        //    if (NetworkServer.active)
        //    {
        //        Log.Info($"Current scene: {SceneManager.GetActiveScene().name}");

        //        if (SceneManager.GetActiveScene().name == "bazaar")
        //        {
        //            GameObject shopPortal = GameObject.Find("PortalShop");
        //            if (!shopPortal)
        //            {
        //                Log.Error("Could not find shopportal");
        //            }
        //            var mg = shopPortal.GetComponent<RoutePortalManager>();
        //            if (mg == null)
        //            {
        //                Log.Info("Shop portal on bazaar doesnt have manager");
        //            }
        //            mg.enabled = true;
        //            mg.destinationSceneIndex = (int)(Run.instance.nextStageScene.sceneDefIndex);
        //            mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(shopPortal);
        //            mg.isOval = true;
        //            Log.Info("Succesfully attached");

        //        }
        //        else
        //        {
        //            Log.Debug("Not bazaar");
        //        }
        //    }
        //    orig(self);

        //}



        //todo переписать, добавить ванильным префабам mg сразу
        private GameObject OnVanillaPortalSpawn(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            //return orig(self, directorSpawnRequest);


            var obj = orig(self, directorSpawnRequest);

            if (!NetworkServer.active)
            {
                return obj;
            }

            if (obj)
            {
                if (directorSpawnRequest.spawnCard && directorSpawnRequest.spawnCard.name != null)
                {
                    var name = directorSpawnRequest.spawnCard.name;
                    if (name.Contains("Portal"))
                    {
                        Log.Info($"Director spawned {name} portal");
                        var mgr = obj.GetComponent<RoutePortalManager>();
                        if (!mgr)
                        {
                            Log.Error($"RoutePortalManager is missing for portal {name}");
                        }


                        if (name == "iscColossusPortal")
                        {

                            mgr.destinationSceneIndex = GetGreenPortalSceneIndex();
                            mgr.isOval = true;
                            mgr.isVanillaPortal = true;
                        }
                        else if (name == "iscGoldshoresPortal")
                        {
                            var exMgr = obj.GetComponent<SceneExitController>();
                            //if (exMgr && !exMgr.useRunNextStageScene)
                            //{
                            //    Log.Info("Goldshores Portal is not leading to the next stage, no routes");
                            //    return obj;
                            //}
                            if (SceneManager.GetActiveScene().name != "goldshores") return obj;
                            mgr.destinationSceneIndex = (int)Run.instance.nextStageScene.sceneDefIndex;
                            mgr.isOval = true;
                            mgr.isVanillaPortal = true;

                        }
                        else if (name == "iscShopPortal")
                        {
                            var exMgr = obj.GetComponent<SceneExitController>();
                            if (exMgr && !exMgr.useRunNextStageScene)
                            {
                                Log.Info("Portal is not leading to the next stage, no routes");
                                return obj;
                            }
                            mgr.destinationSceneIndex = (int)Run.instance.nextStageScene.sceneDefIndex;
                            mgr.isOval = true;
                            mgr.isVanillaPortal = true;

                        }
                        else if (name == "iscHardwareProgPortal")
                        {
                            if (Stage.instance.sceneDef.stageOrder != 3) return obj;
                            mgr.destinationSceneIndex = (int)SceneCatalog.FindSceneIndex("conduitcanyon");
                            mgr.isOval = false;
                            mgr.isVanillaPortal = true;
                        }
                        mgr.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(obj);
                        Log.Info($"Enabled manager for portal:{name}");
                        mgr.enabled = true;
                        mgr.isManagerEnabled = true;


                    }

                }
            }

            return obj;

        }


        private void CurrentRunArtifactDisplayDataDriver_Refresh(On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.orig_Refresh orig, CurrentRunArtifactDisplayDataDriver self)
        {
            if (RunArtifactManager.enabledArtifactsEnumerable.GetEnumerator().MoveNext())
            {
                orig(self);
            }
            if (self.artifactDisplayPanelController && self.artifactDisplayPanelController.iconContainer)
            {
                if (StageModifierDirector.instance && StageModifierDirector.instance.modifiersSynced.Count == 0)
                {
                    orig(self);
                    //Log.Info("There are no modifiers");
                    return;
                }
                self.dirty = false;

                AddCustomIcon(self.artifactDisplayPanelController);

                if (!self.artifactDisplayPanelController.panelObject.activeSelf)
                {
                    self.artifactDisplayPanelController.panelObject.SetActive(true);
                }
            }

        }

        private void AddCustomIcon(ArtifactDisplayPanelController controller)
        {
            //Log.Info($"For icons there are {StageModifierDirector.instance.activeModifiers.Count}");
            foreach (var mod in StageModifierDirector.instance.modifiersSynced)
            {
                if (mod.isArtifact) continue;

                Sprite icon = IconCatalog.ModifierIconFromDef(mod.name);
                if (icon == null)
                {
                    icon = placeholderIcon;
                    Log.Error($"Could not load icon, using placeholder for {mod.name}");
                }

                string iconName = mod.name;
                Transform container = controller.iconContainer;

                if (container.Find(iconName)) continue;

                GameObject iconObj = new GameObject(iconName);
                iconObj.transform.SetParent(container, false);

                Image img = iconObj.AddComponent<Image>();

                img.sprite = icon;
                //img.raycastTarget = true;

                LayoutElement layout = iconObj.AddComponent<LayoutElement>();
                layout.preferredHeight = 32f;
                layout.preferredWidth = 32f;
                layout.flexibleHeight = 0f;
                layout.flexibleWidth = 0f;

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(32f, 32f);

                AddStackTextToImage(mod.stack, img);

                RoR2.UI.TooltipProvider tooltip = iconObj.AddComponent<RoR2.UI.TooltipProvider>();
                tooltip.titleToken = mod.name;
                tooltip.bodyToken = mod.description;
                //tooltip.titleColor = mod.tooltipColor;
                tooltip.titleColor = new Color(246f / 255f, 90f / 255f, 58f / 255f);

                if (controller.panelObject && !controller.panelObject.activeSelf)
                {
                    controller.panelObject.SetActive(true);
                }
            }
        }

        private void AddStackTextToImage(int modStack, Image img)
        {
            GameObject stackTextObj = new GameObject("StackText");
            stackTextObj.transform.SetParent(img.transform, false);

            HGTextMeshProUGUI textComp = stackTextObj.AddComponent<HGTextMeshProUGUI>();
            RectTransform rect = textComp.GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            textComp.font = bomb;
            textComp.fontSize = 12f;

            textComp.alignment = TextAlignmentOptions.TopRight;
            if (modStack > 1) textComp.text = $"x{modStack}";
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);
            var hud = self;
            SetupRoutePortalPanel();
            SetupRouteMapPanel();

            if (routePortalPanel != null)
            {
                routePortalPanel.transform.SetParent(hud.mainContainer.transform, false);
            }
            else
                Log.Error("Route portal panel is null");

            if(routeMapPanel != null)
            {
                routeMapPanel.transform.SetParent(hud.mainContainer.transform,false);
                routeMapPanel.transform.SetParent(hud.scoreboardPanel.transform, true);
            }

            if (bundle == null)
            {
                Log.Error("Assets not loaded");
                return;
            }

            routePortalPanel.SetActive(false);
        }

        private void SetupRoutePortalPanel()
        {
            GameObject prefab = bundle.LoadAsset<GameObject>("RoutePortalPortraitPanel");
            if (prefab == null)
            {
                Log.Error("What?");
            }
            //блюр
            TranslucentImage blur = prefab.AddComponent<TranslucentImage>();
            Shader blurShader = Shader.Find("UI/TranslucentImage");
            blur.material = new Material(blurShader);
            blur.color = new Color(0, 0, 0, 0.7f);
            //текст //плейсхолдер
            var text = prefab.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null)
            {
                Log.Error("No text meshprougui");
            }
            if (text == null) Log.Error("Not found");
            text.text = "Placeholder";
            text.font = bomb;

            routePortalPanel = Object.Instantiate(prefab);
        }

        private void SetupRouteMapPanel()
        {
            GameObject prefab = bundle.LoadAsset<GameObject>("RouteMapPanel");
            if (prefab == null)
            {
                Log.Error("Couldnt load RouteMapPanel");
                return;
            }
            routeMapPanel = Object.Instantiate(prefab);

            //блюр
            TranslucentImage blur = prefab.AddComponent<TranslucentImage>();
            Shader blurShader = Shader.Find("UI/TranslucentImage");
            blur.material = new Material(blurShader);
            blur.color = new Color(0, 0, 0, 0.7f);

        }



        private void RoutePortalPanelUpdateText(GameObject routePortalPanel, RoutePortalManager mg)
        {
            //Log.Info("Trying to update text");
            var textComponents = routePortalPanel.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI panelText = null;
            foreach (var text in textComponents)
            {
                if (text.name == "BodyLabel")
                {
                    panelText = text;
                    break;
                }
            }
            if (panelText == null)
            {
                Log.Error("Panel text is null");
                return;
            }

            var imageComponents = routePortalPanel.GetComponentsInChildren<RawImage>();
            RawImage sceneImage = null;
            foreach (var image in imageComponents)
            {
                if (image.name == "ActualImage")
                {
                    sceneImage = image;
                    break;
                }
            }
            if (sceneImage == null)
            {
                Log.Error("SceneImage is null");
                return;
            }

            var tex = SceneCatalog.GetSceneDef((SceneIndex)mg.destinationSceneIndex).portalMaterial;
            sceneImage.texture = tex.mainTexture;
            panelText.text = mg.modifierTextSynced;
            var grid = routePortalPanel.GetComponentInChildren<GridLayoutGroup>();

            for (int i = grid.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(grid.transform.GetChild(i).gameObject);
            }

            routePortalPanel.gameObject.SetActive(true);

            foreach (var iconName in mg.iconNames)
            {
                GameObject obj = new GameObject("ModifierIcon");
                Image img = obj.AddComponent<Image>();
                if (iconName != "")
                {
                    img.sprite = IconCatalog.ModifierIconFromDef(iconName);
                }
                else
                {
                    img.sprite = placeholderIcon;
                }
                img.transform.SetParent(grid.transform, false);
                obj.AddComponent<LayoutElement>();
                var layout = obj.GetComponent<LayoutElement>();
                layout.minWidth = 16f;
                layout.minHeight = 16f;
                layout.preferredWidth = 64f;
                layout.preferredHeight = 64;

                //todo отображение стаков
                AddStackTextToImage(1, img);
            }
            //foreach (var mod in mg.modifiersSynced)
            //{
            //    if (!mod.negative)
            //    {
            //        GameObject obj = new GameObject("ModifierIcon");
            //        Image img = obj.AddComponent<Image>();
            //        if (mod.iconName != null)
            //        {
            //            img.sprite = IconCatalog.ModifierIconFromDef(mod.iconName);
            //        }
            //        else
            //        {
            //            img.sprite = placeholderIcon;
            //        }
            //        img.transform.SetParent(grid.transform, false);

            //        obj.AddComponent<LayoutElement>();
            //        var layout = obj.GetComponent<LayoutElement>();
            //        layout.minWidth = 16f;
            //        layout.minHeight = 16f;
            //        layout.preferredWidth = 64f;
            //        layout.preferredHeight = 64;

            //        AddStackTextToImage(mod.stack, img);
            //    }
            //}
        }

        private void TeleporterEventSpawnRoutePortals(TeleporterInteraction interaction)
        {
            spawnRoutePortals(interaction.transform.position);
        }

        private void spawnRoutePortals(Vector3 position)
        {
            List<SceneDef> scenes = GetNextStagesScenesList();

            if (scenes.Count == 0)
            {
                Log.Error("No stages available next");
                return;
            }

            Util.ShuffleList(scenes, Run.instance.stageRng);

            var count = routeCount.Value;
            //int lim = count > scenes.Count ? scenes.Count : count;
            //if (uniqueDestinations.Value)
            //{
            //    lim = count > scenes.Count ? scenes.Count : count;
            //}
            //else
            //{
            //    lim = count;
            //}
            var nodes = StageModifierDirector.instance.GetAvailableNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                SceneDef destination = new SceneDef();
                if (uniqueDestinations.Value)
                {
                    int index = scenes.Count - 1;
                    destination = scenes[index];
                    scenes.RemoveAt(index);
                }
                else
                {
                    destination = Run.instance.runRNG.NextElementUniform(scenes);
                }

                SpawnCard routePortal = CreateRoutePortalSpawnCard(destination);

                if (routePortal == null)
                {
                    Log.Error("PortalCard is null");
                    return;
                }

                DirectorSpawnRequest request = new DirectorSpawnRequest(routePortal, new DirectorPlacementRule
                {
                    minDistance = 10f,
                    maxDistance = 40f,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = position
                }, Run.instance.stageRng);



                GameObject portalObject = DirectorCore.instance.TrySpawnObject(request);

                if (portalObject)
                {
                    foreach (Transform child in portalObject.GetComponentsInChildren<Transform>())
                    {
                        if (child.name.Contains("bubble"))
                        {
                            Log.Info("1");
                        }
                    }



                    var mg = portalObject.GetComponent<RoutePortalManager>();

                    mg.enabled = true;
                    mg.isManagerEnabled = true;

                    if (node == null)
                    {
                        Log.Error("Node is null skipping");
                        continue;
                    }
                    mg.routeNode = node;
                    mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(portalObject);
                    mg.UpdateText();

                    Vector3 facingVector = position - portalObject.transform.position;
                    facingVector.y = 0;
                    if (facingVector != Vector3.zero)
                    {
                        portalObject.transform.rotation = Quaternion.LookRotation(facingVector);
                    }
                    Vector3 rdir = portalObject.transform.right;

                    StageModifierDirector.instance.AddRoutePortalInstance(portalObject);


                }

            }

        }




        public static List<SceneDef> GetNextStagesScenesList()
        {
            //TODO луп вариации проверить
            SceneDef nextStageScene = Run.instance.nextStageScene;
            List<SceneDef> list = new List<SceneDef>();
            if (nextStageScene != null)
            {
                int nextStageOrder = nextStageScene.stageOrder;
                var s = SceneCatalog.mostRecentSceneDef.destinationsGroup;

                foreach (RoR2.SceneCollection.SceneEntry se in s.sceneEntries)
                {
                    var scene = se.sceneDef;
                    if (scene.sceneType == SceneType.Stage &&
                        scene.mainTrack &&
                        scene.stageOrder == nextStageOrder &&
                        (scene.requiredExpansion == null || Run.instance.IsExpansionEnabled(scene.requiredExpansion)) &&
                        RoR2.Run.instance.CanPickStage(scene))
                    {
                        list.Add(scene);
                    }
                }
            }

            return list;

        }

        public static int GetGreenPortalSceneIndex()
        {
            SceneIndex sceneIndex = SceneIndex.Invalid;
            int stageOrder = Stage.instance.sceneDef.stageOrder;
            int num = Run.instance.nextStageScene.stageOrder;
            if (num != stageOrder + 1 && stageOrder <= 5)
            {
                num = stageOrder + 1;
            }
            Log.Info(stageOrder + " " + num);
            switch (num)
            {
                case 1:
                    sceneIndex = SceneCatalog.FindSceneIndex("lemuriantemple");
                    break;
                case 2:
                    sceneIndex = SceneCatalog.FindSceneIndex("lemurianTemple");
                    break;
                case 3:
                    sceneIndex = SceneCatalog.FindSceneIndex("habitat");
                    break;
                case 4:
                    sceneIndex = SceneCatalog.FindSceneIndex("meridian");
                    break;
                case 5:
                    sceneIndex = SceneCatalog.FindSceneIndex("lemuriantemple");

                    break;
            }
            Log.Info($"SELECTED SCENE: {sceneIndex}");
            return (int)sceneIndex;
        }

        static bool IsValidNextStage(SceneDef sceneDef)
        {
            SceneDef nextStageScene = RoR2.Run.instance.nextStageScene;
            if (nextStageScene != null && nextStageScene.baseSceneName == sceneDef.baseSceneName)
            {
                return false;
            }
            if (!sceneDef.hasAnyDestinations)
            {
                return false;
            }

            return sceneDef.validForRandomSelection;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                Log.Info($"Player pressed F2. Spawning route portals {transform.position}");
                spawnRoutePortals(transform.position);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                UnityHotReload.LoadNewAssemblyVersion(
                    typeof(RiskOfRoutes).Assembly, // The currently loaded assembly to replace.
                    "C:/Users/vit/AppData/Roaming/r2modmanPlus-local/RiskOfRain2/profiles/TEstling/BepInEx/plugins/RiskOfRoutes.dll"  // The path to the newly compiled DLL.
                );
                Log.Info("Done");
            }
            var localUser = LocalUserManager.GetFirstLocalUser();
            //TODO: Сделать смену при смене интерактбла, не чекать каждый кадр
            if (localUser != null && localUser.cachedBody != null)
            {
                var dr = localUser.cachedBody.GetComponent<InteractionDriver>();
                if (dr != null)
                {
                    if (dr.currentInteractable != null)
                    {
                        if (dr.currentInteractable.name.Contains("RoutePortal") || dr.currentInteractable.name.Contains("Portal") || dr.currentInteractable.name.Contains("SeerStation"))
                        {
                            var manager = dr.currentInteractable.GetComponentInChildren<RoutePortalManager>();

                            if (!manager) manager = dr.currentInteractable.GetComponentInParent<RoutePortalManager>();
                            if (manager != null && manager.isManagerEnabled)
                            {
                                //Log.Info("Looking at route portal");
                                //Log.Info(manager.destinationScene.cachedName);
                                //TODO: вроде можно переделать через TooltipProvider
                                if (manager.modifierTextSynced != null)
                                {
                                    RoutePortalPanelUpdateText(routePortalPanel, manager);
                                }
                                else
                                {
                                    Log.Error("Could not set text to panel");
                                }
                                routePortalPanel.SetActive(true);
                            }
                            else
                            {
                                if (manager == null)
                                    Log.Error("Does not have manager");
                                else
                                    Log.Error("Manager is disabled");
                            }
                        }
                    }
                    else
                    {

                        routePortalPanel.SetActive(false);

                    }
                }

            }

        }

        [ConCommand(commandName = "debuglog_on_all", flags = ConVarFlags.ExecuteOnServer, helpText = "Logs a network message to all connected people.")]
        private static void CCNetworkLog(ConCommandArgs args)
        {
            if (NetworkServer.active)
            {
                if (!_centralNetworkObjectSpawned)
                {
                    _centralNetworkObjectSpawned = Object.Instantiate(CentralNetworkObject);
                    NetworkServer.Spawn(_centralNetworkObjectSpawned);
                    Log.Info("instantiated network ob");
                }
                foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
                {
                    MyNetworkComponent.Invoke(user, string.Join("Pognali", args.userArgs));
                }
            }
        }


    }
}

using BepInEx;
using R2API;
using RoR2;
using RoR2.UI;
using SamplePlugin;
using SamplePlugin.StageModifiers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityHotReloadNS;

//TODO
//Core:
//Разный цвет у порталов
//Особые модификаторы для стейджей
//Логика стака модификаторов
//Добавить конфиг
//(Подкорректировать)Добавить больше предметов в пулл зеленых и белых, добавить возможность для такого же с предметами любого тира через конфиг
//(Подкорректировать)Поменять логику гаранта жёлтых предметов
//Разобраться, как добавлять мобов в пулл текущего стейджа (Void,moon модификаторы) 
//Сделать, чтобы всё работало в мультиплеере
//Вынести отдельно метод спавна объекта рандомно на сцене (для шефа, принтеров и другого)
//Артефакт инфляции
//TC дрон гарант
//(Возможно) модификатор на онли летающих врагов (AirNOdegrah)
//(Возможно) фиксированные места поялвения шефа
//(Подкорректировать)Иногда конструктор артефактов выкидывает исключение
//(Подкорректировать)Есть предположение, что модификаторы, расставляющие предметы на сцене могут привести к тому что не заспавнить телерорт
//(Подкорректировать)Назначение exitController и purchase в RoutePortalManager нужно делать вручную, стоит сделать awake()
//модификатор на тир2 элиток
//(Возможно)красный предмет с босса 
//(Возможно) Шанс на модификаторы повышается с получением подходящих предметов
//Конфликт с артефакт модификат


//Визуал:
//поменять UI при наведении
//Текстура локи в портале
//Шеф должен появлять со столом
//(Сложно) UI карты на манер шпиля


//(Возможно) Можно ли сделать так, чтобы любые мобы могли воровать предметы?
//(Возможно) Бродячий дрифтер или можно рюкзак
//(Возможно) рюзкак дрифтера позволяет поделиться предметами
//(Возможно) Void Stage
//(Возможно) Святилище, что рероллит все предметы, лежащие на земле
//(Возможно)стейдж где алтари удачи заменяют сундуки но другой дроп голды
//(Возможно)Модификаторы длящиеся несколько стейджей
//Модификатор ограничивающий макс хп
//(Возможно) Скрытые модификаторы и предметы их раскрывающие
//(Возможно) Модификатор исключающий категорию предметов из пулла
//(Возможно) Покупка всего за хп
//(Возможно) Меньше сундуков, но больше магазинов
//(Возможно) Портал, очищающий лунные предметы
//(Возможно) категории порталов?
//Двойник


[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RiskOfRoutes
{
    // https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]

    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.0.0";

        private GameObject portalPrefab;
        private GameObject ui;
        public static GameObject chefPrefab;

        private HUD hud = null;

        private InteractableSpawnCard portalSpawnCard;

        public const int RouteCount = 3;


        public static AssetBundle bundle;

        List<Texture2D> portalColors = new List<Texture2D>();

        private TMP_FontAsset bomb;

        public Sprite placeholderIcon;

        public bool useColoredPortals = false;

        public void Awake()
        {
            Log.Init(Logger);
            //префабы
            CreateRoutePortalPrefab();
            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");
            ui = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/Tooltip.prefab").WaitForCompletion(), "tooltipUI");
            //для своих префабов
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "riskofroutes");
            bundle = AssetBundle.LoadFromFile(path);
            LoadColors();
            //для панели , шрифт для стаков
            bomb = Addressables.LoadAssetAsync<TMP_FontAsset>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropShadow.asset").WaitForCompletion();
            placeholderIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            //директор
            gameObject.AddComponent<StageModifierDirector>();


            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterEventSpawnRoutePortals;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.Refresh += CurrentRunArtifactDisplayDataDriver_Refresh;
            //On.RoR2.PortalSpawner.AttemptSpawnPortalServer += PortalSpawner_AttemptSpawnPortalServer;

            On.RoR2.DirectorCore.TrySpawnObject += OnVanillaPortalSpawn;

            //Портал выхода с базара
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.SeerStationController.SetRunNextStageToTarget += SeerStationController_SetRunNextStageToTarget;
            On.RoR2.SeerStationController.OnStartClient += AddRouteManagerToSeerStation;

        }

        private void LoadColors()
        {
            portalColors = new List<Texture2D>();

            int variantCount = 28;

            for (int i = 0; i < variantCount; i++)
            {
                string edgeName = $"variant_{i}_1";
                string centerName = $"variant_{i}_2";

                Texture2D edgeTex = bundle.LoadAsset<Texture2D>(edgeName);
                Texture2D centerTex = bundle.LoadAsset<Texture2D>(centerName);

                if (edgeTex && centerTex)
                {
                    portalColors.Add(edgeTex);
                    portalColors.Add(centerTex);
                }
                else
                {
                    Log.Error($"Could not find portal textures: {edgeName} or {centerName}");

                }
            }
        }

        private void AddRouteManagerToSeerStation(On.RoR2.SeerStationController.orig_OnStartClient orig, SeerStationController self)
        {
            orig(self);
            if (!self.gameObject.GetComponent<RoutePortalManager>())
            {
                Log.Info("Adding route manager");
                var mg = self.gameObject.AddComponent<RoutePortalManager>();
                mg.isSeerStation = true;

                SceneDef targetScene = SceneCatalog.GetSceneDef((SceneIndex)self.targetSceneDefIndex);
                if (targetScene)
                {
                    mg.destinationScene = targetScene;
                }
                else
                {
                    Log.Error("Seer does not have scene");
                }
                Log.Info($"SEER:{mg.destinationScene}");
                mg.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(self.gameObject);

            }
        }

        private void SeerStationController_SetRunNextStageToTarget(On.RoR2.SeerStationController.orig_SetRunNextStageToTarget orig, SeerStationController self)
        {
            SceneDef targetScene = SceneCatalog.GetSceneDef((SceneIndex)self.targetSceneDefIndex);
            orig(self);

            if (SceneManager.GetActiveScene().name == "bazaar")
            {
                GameObject shopPortal = GameObject.Find("PortalShop");
                if (!shopPortal)
                {
                    Log.Error("Could not find");
                }
                if (!shopPortal.GetComponent<RoutePortalManager>())
                {
                    var mg = shopPortal.AddComponent<RoutePortalManager>();
                    mg.destinationScene = targetScene;
                    mg.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(shopPortal);
                    Log.Info("Succesfully attached");
                }
                else
                {
                    var seerRouteManager = self.gameObject.GetComponent<RoutePortalManager>();

                    var mng = shopPortal.GetComponent<RoutePortalManager>();
                    string previous = mng.destinationScene.cachedName;
                    mng.destinationScene = targetScene;
                    if (seerRouteManager)
                        mng.stageModifiers = seerRouteManager.stageModifiers;
                    else
                        mng.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(shopPortal);
                    mng.UpdateText();
                    Log.Info($"Updated destination:{previous}->{Run.instance.nextStageScene}");
                }

            }
            else
            {
                Log.Debug("Not bazaar");
            }
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            Log.Info("WOOOORKING");

            Log.Info($"Current scene: {SceneManager.GetActiveScene().name}");

            if (SceneManager.GetActiveScene().name == "bazaar")
            {
                GameObject shopPortal = GameObject.Find("PortalShop");
                if (!shopPortal)
                {
                    Log.Error("Could not find");
                }
                if (!shopPortal.GetComponent<RoutePortalManager>())
                {
                    var mg = shopPortal.AddComponent<RoutePortalManager>();
                    mg.destinationScene = Run.instance.nextStageScene;
                    mg.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(shopPortal);
                    Log.Info("Succesfully attached");
                }

            }
            else
            {
                Log.Debug("Not bazaar");
            }
            orig(self);

        }



        private GameObject OnVanillaPortalSpawn(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            var obj = orig(self, directorSpawnRequest);

            if (obj)
            {
                if (directorSpawnRequest.spawnCard)
                {
                    if (directorSpawnRequest.spawnCard.name == "iscColossusPortal")
                    {
                        Log.Info("Director spawned green portal");
                        var mgr = obj.GetComponent<RoutePortalManager>();

                        if (!mgr)
                        {
                            mgr = obj.AddComponent<RoutePortalManager>();
                            Log.Info("Added succesfully");
                        }
                        Log.Info($"Mgr:{mgr.name}");
                        mgr.destinationScene = GetGreenPortalScene();
                        mgr.isVanillaPortal = true;
                        Log.Info(mgr.destinationScene);

                        mgr.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(obj);
                        Log.Info($"Wjat: {mgr.stageModifiers.Count}");
                    }
                    else if (directorSpawnRequest.spawnCard.name == "iscGoldshoresPortal")
                    {
                        if (SceneManager.GetActiveScene().name == "goldshores")
                        {
                            Log.Info("Director spawned goldshores exit portal");
                            var mgr = obj.GetComponent<RoutePortalManager>();

                            if (!mgr)
                            {
                                mgr = obj.AddComponent<RoutePortalManager>();
                                Log.Info("Added succesfully");
                            }
                            Log.Info($"Mgr:{mgr.name}");
                            mgr.destinationScene = Run.instance.nextStageScene;
                            mgr.isVanillaPortal = true;
                            Log.Info(mgr.destinationScene);

                            mgr.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(obj);
                            Log.Info($"Wjat: {mgr.stageModifiers.Count}");
                        }
                        else
                        {
                            Log.Info($"Director spawned portal leading to goldshores");
                            return obj;
                        }
                    }
                    else if (directorSpawnRequest.spawnCard.name == "iscShopPortal")
                    {
                        Log.Info("Director spawned blue portal");
                        var exMgr = obj.GetComponent<SceneExitController>();
                        if (!exMgr.useRunNextStageScene)
                        {
                            Log.Info("Portal is not leading to the next stage.");
                            return obj;
                        }

                        var mgr = obj.GetComponent<RoutePortalManager>();

                        if (!mgr)
                        {
                            mgr = obj.AddComponent<RoutePortalManager>();
                            Log.Info("Added succesfully");
                        }
                        Log.Info($"Mgr:{mgr.name}");
                        mgr.destinationScene = GetGreenPortalScene();
                        mgr.isVanillaPortal = true;
                        Log.Info(mgr.destinationScene);

                        mgr.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(obj);
                        Log.Info($"Wjat: {mgr.stageModifiers.Count}");
                    }
                    else if (directorSpawnRequest.spawnCard.name == "iscHardwareProgPortal")
                    {
                        //если текущий стейдж 3 то есть manager
                        if (Stage.instance.sceneDef.stageOrder == 3)
                        {
                            Log.Info("Director spawned drone portal");
                            var mgr = obj.GetComponent<RoutePortalManager>();

                            if (!mgr)
                            {
                                mgr = obj.AddComponent<RoutePortalManager>();
                                Log.Info("Added succesfully");
                            }
                            Log.Info($"Mgr:{mgr.name}");
                            var scene = SceneCatalog.GetSceneDefFromSceneName("conduitcanyon");
                            if (scene == null) scene = Run.instance.nextStageScene;
                            mgr.destinationScene = scene;
                            mgr.isVanillaPortal = true;
                            Log.Info(mgr.destinationScene);

                            mgr.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(obj);
                            Log.Info($"Wjat: {mgr.stageModifiers.Count}");
                        }
                    }

                }
            }

            return obj;

        }


        private void CurrentRunArtifactDisplayDataDriver_Refresh(On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.orig_Refresh orig, CurrentRunArtifactDisplayDataDriver self)
        {
            //Игра не перестает лагать, если грузятся иконки модификаторов, и при этом не активен ни один из артефактов
            //Если артефакты есть, то лаг никогда не возникает

            if (RunArtifactManager.enabledArtifactsEnumerable.GetEnumerator().MoveNext())
            {
                orig(self);
            }
            if (self.artifactDisplayPanelController && self.artifactDisplayPanelController.iconContainer)
            {
                if (StageModifierDirector.Instance.activeModifiers.Count == 0)
                {
                    orig(self);
                    return;
                }
                self.dirty = false;

                //orig(self);
                //Log.Info("It does have icon container");
                AddCustomIcon(self.artifactDisplayPanelController);



                if (!self.artifactDisplayPanelController.panelObject.activeSelf)
                {
                    self.artifactDisplayPanelController.panelObject.SetActive(true);
                }
            }

        }

        private void AddCustomIcon(ArtifactDisplayPanelController controller)
        {
            //Log.Info($"For icons there are {StageModifierDirector.Instance.activeModifiers.Count}");
            foreach (var mod in StageModifierDirector.Instance.activeModifiers)
            {

                if (mod is ArtifactModifier) continue;
                string iconName = mod.name;
                Transform container = controller.iconContainer;

                if (container.Find(iconName))
                {
                    //Log.Error($"Found repeating name:{iconName}, could not load");
                    //return;
                    continue;

                }
                GameObject iconObj = new GameObject(iconName);
                iconObj.transform.SetParent(container, false);

                Image img = iconObj.AddComponent<Image>();
                var sprite = mod.icon;
                if (sprite != null)
                {
                    //Log.Info("Succesfully loaded icon");
                }
                else
                {
                    sprite = placeholderIcon;
                    Log.Error($"Could not load icon, using placeholder for {mod.name}");
                }

                img.sprite = sprite;
                //img.raycastTarget = true;

                LayoutElement layout = iconObj.AddComponent<LayoutElement>();
                layout.preferredHeight = 32f;
                layout.preferredWidth = 32f;
                layout.flexibleHeight = 0f;
                layout.flexibleWidth = 0f;

                //вроде похуй
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(32f, 32f);


                //текст стака как у предметов
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
                if (mod.stack > 1) textComp.text = $"x{mod.stack}";

                //TODO недочет если наводиться на панель то отображается текст последнего модификатора
                RoR2.UI.TooltipProvider tooltip = iconObj.AddComponent<RoR2.UI.TooltipProvider>();
                tooltip.titleToken = mod.name;
                tooltip.titleColor = mod.tooltipColor;
                tooltip.bodyToken = mod.activeText;

                if (controller.panelObject && !controller.panelObject.activeSelf)
                {
                    controller.panelObject.SetActive(true);
                }
            }

        }

        private GameObject DirectorCore_TrySpawnObject(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            return null;
        }

        //todo может стоит убрать destinationscene из routeportal manager и использовать sceneexitcontroller
        //Для синего, зеленого, желтого, фиолетового порталов из основной игры
        private bool PortalSpawner_AttemptSpawnPortalServer(On.RoR2.PortalSpawner.orig_AttemptSpawnPortalServer orig, PortalSpawner self)
        {
            if (self.PortalSpawnCard.name.Contains("Colossus"))
            {
                var portal = self.PortalSpawnCard.prefab;

                if (!portal.GetComponent<RoutePortalManager>())
                {
                    portal.AddComponent<RoutePortalManager>();
                }
                var mgr = portal.GetComponent<RoutePortalManager>();
                var exitMgr = portal.GetComponent<SceneExitController>();
                mgr.destinationScene = exitMgr.destinationScene;
                mgr.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(portal);
                Log.Info($"Success? {mgr.stageModifiers.Count()}");
                Log.Info($"Destination scene of green portal: {mgr.destinationScene}");

                //Log.Info("Green portal spawned");
                //foreach(var comp in portal.GetComponentsInChildren<Component>())
                //{
                //    Log.Info($"{comp.GetType().Name}");
                //}
            }

            return orig(self);
        }

        [ConCommand(commandName = "debug_scene_state", flags = ConVarFlags.ExecuteOnServer)]
        private static void DebugSceneState(ConCommandArgs args)
        {
            var sceneInfo = SceneInfo.instance;
            var classicInfo = ClassicStageInfo.instance;
            var sceneDir = sceneInfo?.GetComponent<SceneDirector>();

            Log.Info($"=== SCENE DEBUG ===");
            Log.Info($"SceneDirector: {sceneDir}");
            Log.Info($"TeleporterInteraction.instance: {TeleporterInteraction.instance}");
            Log.Info($"sceneInfo.sceneDef: {sceneInfo?.sceneDef?.baseSceneName}");
            Log.Info($"classicInfo.monsterCategories: {classicInfo?.monsterCategories}");
            Log.Info($"Director interactableCredit: {sceneDir?.interactableCredit}");
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);
            hud = self;

            GameObject newUi = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Tooltip"));
            newUi.transform.SetParent(hud.mainContainer.transform, false);
            Image imga = newUi.AddComponent<Image>();
            imga.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveLunarIcon.png").WaitForCompletion();

            RectTransform rectTransform = newUi.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.8f, 0.0f);
            rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;


            var images = newUi.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name == "TitleRect")
                {
                    img.color = new Color(0.0f, 0.8f, 0.9f, 1.0f);
                }
            }

            tooltipSetText(newUi, "Unknown");

            newUi.SetActive(false);

            ui = newUi;

        }

        private void tooltipSetText(GameObject tooltip, string bodyText, string labelText = "RoutePortal")
        {
            var tc = tooltip.GetComponent<TooltipController>();

            var label = tc.titleLabel;
            var body = tc.bodyLabel;

            label.text = labelText;
            body.text = bodyText;
        }



        private void CreateRoutePortalPrefab()
        {
            GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion(), "RoutePortal");
            //GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion(), "BeebleMemorialStatue");
            Log.Info("Invoked");

            Destroy(routePortal.GetComponent<GenericInteraction>());
            Destroy(routePortal.GetComponent<SceneExitController>());
            //Destroy(routePortal.GetComponent<PurchaseAvailabilityIndicator>());
            //Destroy(routePortal.GetComponent<PurchaseInteraction>());
            //Destroy(routePortal.GetComponent<SeerStationController>());

            //var renderers = routePortal.GetComponentsInChildren<MeshRenderer>();

            ////Material mat = SceneInfo.instance.sceneDef.portalMaterial;
            //foreach (var renderer in renderers)
            //{
            //    Log.Info($"Renderer:{renderer}");
            //    if (renderer.name == "Mesh")
            //    {
            //        UnityEngine.Object.Destroy(renderer.gameObject);
            //    }
            //}



            routePortal.name = "RoutePortal";
            routePortal.transform.localScale = new Vector3(0.7f, 1.26f, 0.7f);

            //создаем интеракцию
            RoutePortalManager mgr = routePortal.AddComponent<RoutePortalManager>();
            GenericInteraction interaction = routePortal.AddComponent<GenericInteraction>();
            SceneExitController sceneExitController = routePortal.AddComponent<SceneExitController>();
            interaction.contextToken = "Enter the portal";


            mgr.genericInteraction = interaction;
            mgr.sceneExitController = sceneExitController;
            mgr.modifierCredits = 1;


            //routePortal.GetComponent<Highlight>().targetRenderer = routePortal.GetComponentInChildren<SkinnedMeshRenderer>();
            //GameObject trigger = Instantiate(new GameObject("Trigger"), routePortal.transform);
            //trigger.AddComponent<BoxCollider>().isTrigger = true;
            //trigger.AddComponent<EntityLocator>().entity = routePortal;

            portalPrefab = routePortal;



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
                Log.Error("No stages availble next");
                return;
            }

            Util.ShuffleList(scenes, Run.instance.stageRng);

            int lim = RouteCount > scenes.Count ? scenes.Count : RouteCount;

            for (int i = 0; i < lim; i++)
            {
                int index = scenes.Count - 1;
                SceneDef destination = scenes[index];
                scenes.RemoveAt(index);

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
                    var m = portalObject.GetComponent<RoutePortalManager>();
                    m.stageModifiers = StageModifierDirector.Instance.GetNextStageModifiers(portalObject);
                    Log.Info($"Success? {m.stageModifiers.Count()}");
                    Log.Info($"{i + 1}th portal placed successfully");
                    Vector3 facingVector = position - portalObject.transform.position;
                    facingVector.y = 0;
                    if (facingVector != Vector3.zero)
                    {
                        portalObject.transform.rotation = Quaternion.LookRotation(facingVector);
                    }
                    Vector3 rdir = portalObject.transform.right;

                    var renderers = portalObject.GetComponentsInChildren<Renderer>();

                    Material mat = m.destinationScene.portalMaterial;

                    if (portalColors.Count >= 2)
                    {
                        int d = portalColors.Count / 2;
                        int selected = Run.instance.stageRng.RangeInt(0, d);
                        Log.Info("Selected:" + selected);

                        selected *= 2;

                        var edge = portalColors[selected];
                        var center = portalColors[selected + 1];


                        if (useColoredPortals)
                        {
                            foreach (var renderer in renderers)
                            {
                                if (renderer.material.name.Contains("Edge"))
                                {
                                    renderer.material.SetTexture("_RemapTex", edge);
                                }
                                else if (renderer.material.name.Contains("Center"))
                                {
                                    renderer.material.SetTexture("_RemapTex", center);
                                }
                            }
                        }


                    }
                }

            }

        }

        private InteractableSpawnCard CreateRoutePortalSpawnCard(SceneDef destination)
        {
            var m = portalPrefab.GetComponent<RoutePortalManager>();
            m.destinationScene = destination;

            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            interactableSpawnCard.name = "iscRoutePortal";
            interactableSpawnCard.prefab = portalPrefab;
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

        public static SceneDef GetGreenPortalScene()
        {
            SceneDef sceneDef = null;
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
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("lemuriantemple");
                    break;
                case 2:
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("lemurianTemple");
                    break;
                case 3:
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("habitat");
                    break;
                case 4:
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("meridian");
                    break;
                case 5:
                    sceneDef = SceneCatalog.GetSceneDefFromSceneName("lemuriantemple");

                    break;
            }
            Log.Info($"SELECTED SCENE: {sceneDef.cachedName}");
            return sceneDef;
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
                // Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // And then drop our defined item in front of the player.

                Log.Info($"Player pressed F2. Spawning route portals {transform.position}");
                spawnRoutePortals(transform.position);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                UnityHotReload.LoadNewAssemblyVersion(
                    typeof(RiskOfRoutes).Assembly, // The currently loaded assembly to replace.
                    "C:/Users/vit/AppData/Roaming/r2modmanPlus-local/RiskOfRain2/profiles/TEstling/BepInEx/plugins/SamplePlugin.dll"  // The path to the newly compiled DLL.
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
                            if (manager != null)
                            {
                                //Log.Info("Looking at route portal");
                                //Log.Info(manager.destinationScene.cachedName);
                                //TODO: вроде можно переделать через TooltipProvider
                                if (manager.modifierTextSynced != null)
                                {
                                    tooltipSetText(ui, manager.modifierTextSynced, manager.destinationScene.baseSceneName);
                                }
                                else
                                {
                                    Log.Error("Could not set text to Tooltip");
                                }
                                ui.SetActive(true);
                            }
                        }
                    }
                    else
                    {

                        ui.SetActive(false);

                    }
                }

            }

        }


    }
}

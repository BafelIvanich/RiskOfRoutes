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
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityHotReloadNS;

//TODO
//Core:
//Взаимодейтсвие с Seer'ами на базаре
//Модификаторы для других порталов
//Разный цвет у порталов
//Особые модификаторы для стейджей
//Переход на базар
//Разобраться с синими, зелёными, золотыми порталами из оригинальной игры
//Зеленый портал не переносит на свои локи
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

        private HUD hud = null;

        private InteractableSpawnCard portalSpawnCard;

        public static GameObject chefPrefab;

        public const int RouteCount = 3;

        private AssetBundle bundle;

        private TMP_FontAsset bomb;

        public Sprite placeholderIcon;

        public void Awake()
        {

            Log.Init(Logger);

            //шрифт для стаков
            bomb = Addressables.LoadAssetAsync<TMP_FontAsset>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropShadow.asset").WaitForCompletion();

            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");

            ui = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/Tooltip.prefab").WaitForCompletion(), "tooltipUI");

            //для своих префабов
            //string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "panel");
            //bundle = AssetBundle.LoadFromFile(path);

            placeholderIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();

            //statue
            CreateInteractable();

            TeleporterInteraction.onTeleporterChargedGlobal += teleporterSpawnHologram;
            gameObject.AddComponent<StageModifierManager>();
            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.Refresh += CurrentRunArtifactDisplayDataDriver_Refresh;

            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
            //On.RoR2.PortalSpawner.AttemptSpawnPortalServer += PortalSpawner_AttemptSpawnPortalServer;

            On.RoR2.DirectorCore.TrySpawnObject += OnVanillaPortalSpawn;

            On.RoR2.SceneDirector.PlaceTeleporter += SceneDirector_PlaceTeleporter;
        }

        private void SceneDirector_PlaceTeleporter(On.RoR2.SceneDirector.orig_PlaceTeleporter orig, SceneDirector self)
        {
            orig(self);

            Log.Info(self.teleporterInstance.transform);
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

                        mgr.stageModifiers = StageModifierManager.Instance.GetNextStageModifiers(obj);
                        Log.Info($"Wjat: {mgr.stageModifiers.Count}");
                    }
                    if (directorSpawnRequest.spawnCard.name == "iscGoldshoresPortal")
                    {
                        Log.Info("Director spawned goldshores portal");
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

                        mgr.stageModifiers = StageModifierManager.Instance.GetNextStageModifiers(obj);
                        Log.Info($"Wjat: {mgr.stageModifiers.Count}");

                    }
                    if (directorSpawnRequest.spawnCard.name == "iscShopPortal")
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

                        mgr.stageModifiers = StageModifierManager.Instance.GetNextStageModifiers(obj);
                        Log.Info($"Wjat: {mgr.stageModifiers.Count}");
                    }
                }
            }

            return obj;

        }


        private void CurrentRunArtifactDisplayDataDriver_Refresh(On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.orig_Refresh orig, CurrentRunArtifactDisplayDataDriver self)
        {
            if (self.artifactDisplayPanelController && self.artifactDisplayPanelController.iconContainer)
            {
                if (StageModifierManager.Instance.activeModifiers.Count == 0)
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
            //Log.Info($"For icons there are {StageModifierManager.Instance.activeModifiers.Count}");
            foreach (var mod in StageModifierManager.Instance.activeModifiers)
            {

                if (mod is ArtifactModifier) continue;
                string iconName = mod.name;
                Transform container = controller.iconContainer;

                if (container.Find(iconName))
                {
                    //Log.Error($"Found repeating name:{iconName}, could not load");
                    return;

                }
                GameObject iconObj = new GameObject(iconName);
                iconObj.transform.SetParent(container, false);
                Image img = iconObj.AddComponent<Image>();

                //текст стака как у предметов
                GameObject stackTextObj = new GameObject("StackText");
                stackTextObj.transform.SetParent(img.transform, false);
                HGTextMeshProUGUI textComp = stackTextObj.AddComponent<HGTextMeshProUGUI>();

                RectTransform rect = textComp.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.offsetMax = new Vector2(0, 0);

                textComp.font = bomb;
                textComp.fontSize = 12f;

                textComp.alignment = TextAlignmentOptions.TopRight;
                if(mod.stack>1) textComp.text = $"x{mod.stack}";

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

                LayoutElement layout = iconObj.AddComponent<LayoutElement>();
                layout.preferredHeight = 32f;
                layout.preferredWidth = 32f;

                //TODO недочет если наводиться на панель то отображается текст последнего модификатора
                RoR2.UI.TooltipProvider tooltip = iconObj.AddComponent<RoR2.UI.TooltipProvider>();
                tooltip.titleToken = mod.name;
                tooltip.titleColor = new Color(0.86f, 0.57f, 0.57f, 1.0f);

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
                mgr.stageModifiers = StageModifierManager.Instance.GetNextStageModifiers(portal);
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
            newUi.transform.SetParent(hud.mainContainer.transform,false);
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

        private void HUD_shouldHudDisplay(HUD hud, ref bool shouldDisplay)
        {
            var tc = ui.GetComponent<TooltipController>();
            var rc = ui.GetComponent<RectTransform>();
            tc.titleLabel.text = "frrrrrrrf";
            tc.bodyLabel.text = "gfefefef";
            ui.SetActive(true);
        }

        private void Stage_onServerStageBegin(Stage obj)
        {
            Log.Info($"Stage {SceneCatalog.mostRecentSceneDef.cachedName} started");
            return;
        }





        private void CreateInteractable()
        {
            GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion(), "BeebleMemorialStatue");
            Log.Info("Invoked");

            Destroy(routePortal.GetComponent<GenericInteraction>());
            Destroy(routePortal.GetComponent<SceneExitController>());

            routePortal.name = "RoutePortal";
            routePortal.transform.localScale = new Vector3(1f, 1f, 1f);

            //создаем интеракцию
            RoutePortalManager mgr = routePortal.AddComponent<RoutePortalManager>();
            GenericInteraction interaction = routePortal.AddComponent<GenericInteraction>();
            SceneExitController sceneExitController = routePortal.AddComponent<SceneExitController>();
            interaction.contextToken = "Enter dentfer the portal <sprite name=Utility>";


            mgr.genericInteraction = interaction;
            mgr.sceneExitController = sceneExitController;
            mgr.modifierCredits = 1;


            //routePortal.GetComponent<Highlight>().targetRenderer = routePortal.GetComponentInChildren<SkinnedMeshRenderer>();
            //GameObject trigger = Instantiate(new GameObject("Trigger"), routePortal.transform);
            //trigger.AddComponent<BoxCollider>().isTrigger = true;
            //trigger.AddComponent<EntityLocator>().entity = routePortal;

            portalPrefab = routePortal;



        }

        private void teleporterSpawnHologram(TeleporterInteraction interaction)
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
                    m.stageModifiers = StageModifierManager.Instance.GetNextStageModifiers(portalObject);
                    Log.Info($"Success? {m.stageModifiers.Count()}");
                    Log.Info($"{i + 1}th portal placed successfully");
                }
                Vector3 facingVector = position - portalObject.transform.position;
                facingVector.y = 0;
                if (facingVector != Vector3.zero)
                {
                    portalObject.transform.rotation = Quaternion.LookRotation(facingVector);
                }
                Vector3 rdir = portalObject.transform.right;

                var renderers = portalObject.GetComponentsInChildren<ParticleSystem>();

                Material mat = SceneInfo.instance.sceneDef.portalMaterial;

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

        private void ClosePortals(GameObject obj)
        {
            GameObject.Destroy(obj);
        }

        private static List<SceneDef> GetNextStagesScenesList()
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

        [ConCommand(commandName = "nextStages", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void GetNextStagesScenesCommand(ConCommandArgs args)
        {
            Log.Info("Next stages available:");
            foreach (var scene in GetNextStagesScenesList())
            {
                Log.Info(scene.cachedName);
            }
            Log.Info(Run.instance.nextStageScene.destinationsGroup);

        }

        [ConCommand(commandName = "green", flags = ConVarFlags.ExecuteOnServer, helpText = "Get nexeet stages")]
        private static void GetNextGreenStage(ConCommandArgs args)
        {
            var scene = GetGreenPortalScene();
            Log.Info($"{scene.cachedName}");
        }

        [ConCommand(commandName = "dccs", flags = ConVarFlags.ExecuteOnServer, helpText = "Get nexeet stages")]
        private static void GetDccs(ConCommandArgs args)
        {
            //var stageInfo = ClassicStageInfo.instance;
            //var categories = stageInfo.monsterCategories;
            //float temp = 0;
            //foreach (var c in categories.categories)
            //{
            //    //TODO stageInfo.interactableDccsPool. --- interactableCategories помечен как deprecated стоит посмотреть что за DCCS
            //    //еще есть             stageInfo.GetInteractableDccsPool
            //    Log.Info($"Category:{c.name}");
            //    foreach(var f in c.cards)
            //    {
            //        Log.Info($"Card:{f.spawnCard.directorCreditCost}");
            //    }
            //}

            CharacterMaster master = args.senderMaster;
            if (!master || !master.inventory)
            {
                master = PlayerCharacterMasterController.instances[0].master;
                if (!master || !master.inventory) return;
            }

            Log.Info($"Giving all broken items to {master.name}...");

            foreach (ItemDef itemDef in ItemCatalog.allItemDefs)
            {
                if (itemDef.isConsumed)
                {
                    if (itemDef == DLC1Content.Items.RegeneratingScrapConsumed) continue;
                    if (itemDef == DLC2Content.Items.LowerPricedChestsConsumed) continue;

                    master.inventory.GiveItem(itemDef, 1);

                }
            }


        }

        [ConCommand(commandName = "give_money", flags = ConVarFlags.ExecuteOnServer, helpText = "Gives 1000 gold to the executing player")]
        private static void CCGiveMoney(ConCommandArgs args)
        {
            if (args.senderMaster)
            {
                args.senderMaster.GiveMoney(1000);
                Debug.Log("Gave 1000 gold to " + args.senderMaster.name);
            }
            else
            {
                Debug.LogWarning("Command sender has no character master.");
            }
        }

        [ConCommand(commandName = "activeModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get currently active modifiers")]
        private static void GetActiveModifiersCommand(ConCommandArgs args)
        {
            if (StageModifierManager.Instance != null)
            {
                var mods = StageModifierManager.Instance.activeModifiers;
                if (mods != null)
                {
                    if (mods.Count > 0)
                    {
                        foreach (var modifier in mods)
                        {
                            Log.Info($"{modifier.activeText}:{modifier.stack}");
                        }
                    }
                    else
                    {
                        Log.Info("There are no modifiers");
                    }
                }
                else
                {
                    Log.Error("Modifiers list is null");
                }
            }

        }


        [ConCommand(commandName = "reservedModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers that would be active on next stage")]
        private static void GetReservedModifiersCommand(ConCommandArgs args)
        {
            if (StageModifierManager.Instance != null)
            {
                var mods = StageModifierManager.Instance.reservedModifiers;
                if (mods != null)
                {
                    if (mods.Count > 0)
                    {
                        foreach (var modifier in mods)
                        {
                            Log.Info(modifier.reservedText);
                        }
                    }
                    else
                    {
                        Log.Info("There are no modifiers");
                    }

                }
                else
                {
                    Log.Error("Modifiers list is null");
                }
            }

        }

        [ConCommand(commandName = "checkOneShot", flags = ConVarFlags.ExecuteOnServer, helpText = "Check if players have one shot prot (for one of modifiers)")]
        private static void CheckOneShotProtection(ConCommandArgs args)
        {
            var instances = PlayerCharacterMasterController.instances;
            foreach (var playerCharacterMaster in PlayerCharacterMasterController.instances)
            {
                Log.Info($"{playerCharacterMaster.body.hasOneShotProtection}:{playerCharacterMaster.name}");
                playerCharacterMaster.body.hasOneShotProtection = false;
            }
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

        //телпорт сразу
        [ConCommand(commandName = "charge", flags = ConVarFlags.ExecuteOnServer, helpText = "Charges TP and kills boss")]
        static void ChargeTeleporter(ConCommandArgs args)
        {
            if (!TeleporterInteraction.instance) return;

            var tp = TeleporterInteraction.instance;

            tp.holdoutZoneController.charge = 0.99f;

            if (tp.bossGroup && tp.bossGroup.combatSquad)
            {

                List<CharacterMaster> bossMasters = new List<CharacterMaster>(tp.bossGroup.combatSquad.readOnlyMembersList);

                foreach (CharacterMaster master in bossMasters)
                {
                    CharacterBody body = master.GetBody();

                    if (body && body.healthComponent && body.healthComponent.alive)
                    {
                        body.healthComponent.Suicide(null, null, DamageType.Generic);
                    }
                }
            }

            Log.Info("Teleporter charged and bosses eliminated.");
        }

        [ConCommand(commandName = "tp", flags = ConVarFlags.ExecuteOnServer, helpText = "tp <x> <y> <z> - teleport local player to coordinates.")]
        private static void CC_Teleport(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            if (args.Count != 3)
            {
                Debug.Log("Usage: tp <x> <y> <z>");
                return;
            }

            float x = (float)args.TryGetArgFloat(0);
            float y = (float)args.TryGetArgFloat(1);
            float z = (float)args.TryGetArgFloat(2);

            var body = args.GetSenderBody();
            if (!body) return;

            var newPos = new Vector3(x, y, z);
            body.characterMotor?.Motor.SetPositionAndRotation(newPos, body.transform.rotation);
            body.transform.position = newPos;
        }

        [ConCommand(commandName = "greenportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void SpawnGreenPortal(ConCommandArgs args)
        {

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscColossusPortal");

            if (!spawnCard)
            {
                Debug.LogError("Could not load iscPortalColossus. Trying fallback name...");
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                Debug.LogError("Failed to load Green Portal SpawnCard.");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

            if (spawnedObject)
            {
                Debug.Log("Successfully spawned Green Portal at " + position);
            }
            else
            {
                Debug.LogError("DirectorCore failed to spawn the portal.");
            }
        }



        [ConCommand(commandName = "shopportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void SpawnShopPortal(ConCommandArgs args)
        {

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShopPortal");

            if (!spawnCard)
            {
                Debug.LogError("Could not load iscPortalColossus. Trying fallback name...");
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                Debug.LogError("Failed to load Shop Portal SpawnCard.");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

            if (spawnedObject)
            {
                Debug.Log("Successfully spawned Shop Portal at " + position);
            }
            else
            {
                Debug.LogError("DirectorCore failed to spawn the portal.");
            }
        }

        [ConCommand(commandName = "goldportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void SpawnGoldPortal(ConCommandArgs args)
        {

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward); 
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscGoldshoresPortal");

            if (!spawnCard)
            {
                Debug.LogError("Could not load iscPortalColossus. Trying fallback name...");
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                Debug.LogError("Failed to load GoldShores Portal SpawnCard.");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

            if (spawnedObject)
            {
                Debug.Log("Successfully spawned GoldShores Portal at " + position);
            }
            else
            {
                Debug.LogError("DirectorCore failed to spawn the portal.");
            }
        }

        [ConCommand(commandName = "fing", flags = ConVarFlags.ExecuteOnServer)]
        private static void FindShopPortal(ConCommandArgs args)
        {
            GameObject obj = GameObject.Find("Portal");
            if (obj == null)
            {
                Log.Error("Could not find it");
            }
            else
            {
                Log.Info("Found it");
                foreach (var c in obj.GetComponentsInChildren<Component>())
                {
                    Log.Info(c.GetType());
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

                //mgr.stageModifiers = StageModifierManager.Instance.GetNextStageModifiers(obj);
                mgr.stageModifiers = new List<StageModifier>() { new MoreDronesModifier() { stack = 10} };
                Log.Info($"Wjat: {mgr.stageModifiers.Count}");

            }
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // And then drop our defined item in front of the player.

                Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
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
                        if (dr.currentInteractable.name.Contains("RoutePortal") || dr.currentInteractable.name.Contains("Portal"))
                        {
                            var manager = dr.currentInteractable.GetComponentInChildren<RoutePortalManager>();

                            if (!manager) manager = dr.currentInteractable.GetComponentInParent<RoutePortalManager>();
                            //foreach (var c in dr.currentInteractable.GetComponentsInChildren<Component>())
                            //{
                            //    Log.Info($"Component:{c.GetType()}");
                            //    if (c is RoutePortalManager)
                            //    {
                            //        Log.Info("GERER");
                            //    }
                            //}
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
                                    Log.Info("hiii");
                                }
                                ui.SetActive(true);
                            }
                            else
                            {

                                Log.Error("No manager");

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

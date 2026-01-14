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
using SamplePlugin.StageModifiers;
using SamplePlugin.StageModifiers.OtherModifiers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityHotReloadNS;

//TODO
//Core:
//Особые модификаторы для стейджей
//(Подкорректировать)Добавить больше предметов в пулл зеленых и белых, добавить возможность для такого же с предметами любого тира через конфиг
//Сделать, чтобы всё работало в мультиплеере
//Артефакт инфляции
//(Возможно) фиксированные места поялвения шефа
//(Подкорректировать)Назначение exitController и purchase в RoutePortalManager нужно делать вручную, стоит сделать awake()
//(Возможно) Шанс на модификаторы повышается с получением подходящих предметов
//Конфликт с артефакт модификат
//анти прок?
//реген меньше
//артефакт лимит модификаторов
//атрефакт стейджи в разном порядке
//(Возможно) более ровный спавн порталов


//Визуал:
//Текстура локи в портале


//Модификатор ограничивающий макс хп
//(Возможно) Скрытые модификаторы и предметы их раскрывающие
//(Возможно) Модификатор исключающий категорию предметов из пулла
//(Возможно) Покупка всего за хп
//(Возможно) Меньше сундуков, но больше магазинов
//(Возможно) Портал, очищающий лунные предметы
//(Возможно) категории порталов?
//Доппельгангер
//Утилити урон хил
//Награда зел дрон
//Категории порталов
//Двойник каждыенесколько минут
//Эклипсовые
//модификатор позволяющий унести с собой испытание горы

//Пост релиз
//(Сложно) UI карты на манер шпиля
//(Возможно) Можно ли сделать так, чтобы любые мобы могли воровать предметы?
//(Возможно) Бродячий дрифтер или можно рюкзак
//(Возможно) рюзкак дрифтера позволяет поделиться предметами
//(Возможно) Void Stage
//(Возможно) Святилище, что рероллит все предметы, лежащие на земле
//(Возможно)стейдж где алтари удачи заменяют сундуки но другой дроп голды
//(Возможно)Модификаторы длящиеся несколько стейджей
//


[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RiskOfRoutes
{
    // https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]

    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.0.0";

        public static ConfigEntry<int> routeCount { get; set; }
        public static ConfigEntry<bool> usePortalMaterial { get; set; }
        public static ConfigEntry<bool> uniqueDestinations { get; set; }
        public static ConfigEntry<bool> showPortalDestination { get; set; }
        public static ConfigEntry<bool> teleporterUseRandomMods { get; set; }
        public static ConfigEntry<bool> modifiersCanBeBanned { get; set; }

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

        public static ConfigEntry<int> droneStackPercent;


        public static GameObject routePortalPrefab;
        public static GameObject iconBubblePrefab;
        public static GameObject routePortalPanel;
        public static GameObject chefPrefab;

        private HUD hud = null;

        private InteractableSpawnCard portalSpawnCard;
        public static AssetBundle bundle;
        private TMP_FontAsset bomb;

        public static Sprite placeholderIcon;

        public static CombatDirector.EliteTierDef[] defaultEliteTiers;

        public void Awake()
        {
            BindConfig();
            Log.Init(Logger);
            placeholderIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            bomb = Addressables.LoadAssetAsync<TMP_FontAsset>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropShadow.asset").WaitForCompletion();
            //префабы
            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");
            iconBubblePrefab = CreateIconBubblePrefab();
            routePortalPrefab = CreateRoutePortalPrefab();
            //ui = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/Tooltip.prefab").WaitForCompletion(), "tooltipUI");
            //для своих префабов
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "riskofroutes");
            bundle = AssetBundle.LoadFromFile(path);
            //для панели , шрифт для стаков
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
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };




        }


        private bool CombatDirector_AttemptSpawnOnTarget(On.RoR2.CombatDirector.orig_AttemptSpawnOnTarget orig, CombatDirector self, Transform spawnTarget, DirectorPlacementRule.PlacementMode placementMode)
        {
            throw new System.NotImplementedException();
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

        [Command]
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
                mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(self.gameObject);

            }
        }

        [Command]
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
                    mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(shopPortal);
                    Log.Info("Succesfully attached route portal manager to seer station");
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
                        mng.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(shopPortal);
                    mng.UpdateText();
                    Log.Info($"Updated destination:{previous}->{mng.destinationScene.cachedName}");
                }

            }
            else
            {
                Log.Debug("Not bazaar");
            }
        }

        [Command]
        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            Log.Info("Working scene director");

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
                    mg.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(shopPortal);
                    mg.isOval = true;
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
                        mgr.isOval = true;
                        Log.Info(mgr.destinationScene);

                        mgr.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(obj);
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
                            mgr.isOval = true;
                            Log.Info(mgr.destinationScene);

                            mgr.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(obj);
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
                        mgr.isOval = true;
                        Log.Info(mgr.destinationScene);

                        mgr.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(obj);
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
                            mgr.isOval = false;
                            Log.Info(mgr.destinationScene);

                            mgr.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(obj);
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
                if (StageModifierDirector.instance.activeModifiers.Count == 0)
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
            //Log.Info($"For icons there are {StageModifierDirector.instance.activeModifiers.Count}");
            foreach (var mod in StageModifierDirector.instance.activeModifiers)
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

                SetupStackText(mod, img);

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

        private void SetupStackText(StageModifier mod, Image img)
        {

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
                mgr.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(portal);
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


        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);
            var hud = self;
            SetupRoutePortalPanel();

            if (routePortalPanel != null)
                routePortalPanel.transform.SetParent(hud.mainContainer.transform, false);
            else
                Log.Error("Route portal panel is null");
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


        private void RoutePortalPanelUpdateText(GameObject routePortalPanel, RoutePortalManager mg)
        {
            var withText = routePortalPanel.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI text = null;
            foreach (var t in withText)
            {
                if (t.name == "BodyLabel")
                {
                    text = t;
                    break;
                }
            }
            var withImage = routePortalPanel.GetComponentsInChildren<RawImage>();
            RawImage sceneImage = null;
            foreach (var t in withImage)
            {
                if (t.name == "ActualImage")
                {
                    sceneImage = t;
                    break;
                }
            }
            if (text == null) return;
            if (sceneImage == null) return;
            var tex = mg.destinationScene.previewTexture;
            sceneImage.texture = tex;
            text.text = mg.modifierTextSynced;
            var grid = routePortalPanel.GetComponentInChildren<GridLayoutGroup>();
            for (int i = grid.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(grid.transform.GetChild(i).gameObject);
            }
            foreach (var mod in mg.stageModifiers)
            {
                if (!mod.negative)
                {
                    GameObject obj = new GameObject("ModifierIcon");
                    Image img = obj.AddComponent<Image>();
                    if (mod.icon != null)
                    {
                        img.sprite = mod.icon;
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

                    SetupStackText(mod, img);

                }
            }
            foreach (var mod in mg.stageModifiers)
            {
                if (mod.negative)
                {
                    GameObject obj = new GameObject("ModifierIcon");
                    Image img = obj.AddComponent<Image>();
                    if (mod.icon != null)
                    {
                        img.sprite = mod.icon;
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

                    SetupStackText(mod, img);
                }
            }
        }



        private GameObject CreateRoutePortalPrefab()
        {
            GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion(), "RoutePortal");
            Log.Info("Invoked");

            Destroy(routePortal.GetComponent<GenericInteraction>());
            Destroy(routePortal.GetComponent<SceneExitController>());

            var scaleCurve = routePortal.GetComponentInChildren<ObjectScaleCurve>();
            if (routePortal.transform.Find("PortalCenter") != null)
            {
                Log.Info("ScaleCurveFound");
            }

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

            //создаем интеракцию
            RoutePortalManager mgr = routePortal.AddComponent<RoutePortalManager>();
            GenericInteraction interaction = routePortal.AddComponent<GenericInteraction>();
            SceneExitController sceneExitController = routePortal.AddComponent<SceneExitController>();
            GenericDisplayNameProvider genericDisplayNameProvider = routePortal.AddComponent<GenericDisplayNameProvider>();
            GenericInspectInfoProvider genericInspectInfo = routePortal.AddComponent<GenericInspectInfoProvider>();

            //todo таб сюдаа


            mgr.genericInteraction = interaction;
            mgr.sceneExitController = sceneExitController;
            mgr.modifierCredits = 1;

            interaction.contextToken = "Enter Route Portal";



            //routePortal.GetComponent<Highlight>().targetRenderer = routePortal.GetComponentInChildren<SkinnedMeshRenderer>();
            //GameObject trigger = Instantiate(new GameObject("Trigger"), routePortal.transform);
            //trigger.AddComponent<BoxCollider>().isTrigger = true;
            //trigger.AddComponent<EntityLocator>().entity = routePortal;

            return routePortal;




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

            for (int i = 0;i < nodes.Count; i++)
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
                    //TODO ОТДЕЛЬНЫЙ МЕТОД И ОПТИМИЗАЦИЯ
                    foreach (Transform child in portalObject.GetComponentsInChildren<Transform>())
                    {
                        if (child.name.Contains("bubble"))
                        {
                            Log.Info("1");
                        }
                    }


                    var m = portalObject.GetComponent<RoutePortalManager>();


                    m.routeNode = node;
                    m.stageModifiers = StageModifierDirector.instance.GetNextStageModifiers(portalObject);

                    Vector3 facingVector = position - portalObject.transform.position;
                    facingVector.y = 0;
                    if (facingVector != Vector3.zero)
                    {
                        portalObject.transform.rotation = Quaternion.LookRotation(facingVector);
                    }
                    Vector3 rdir = portalObject.transform.right;

                    var renderers = portalObject.GetComponentsInChildren<Renderer>();

                    Material mat = m.destinationScene.portalMaterial;

                    var edge = bundle.LoadAsset<Texture2D>("darkerBlueEdge");
                    var center = bundle.LoadAsset<Texture2D>("darkerBlueCenter");
                    foreach (var renderer in renderers)
                    {
                        if (renderer.material.name.Contains("Center"))
                        {
                            if (usePortalMaterial.Value)
                            {
                                renderer.material = mat;
                            }
                            else
                            {
                                renderer.material.SetTexture("_RemapTex", center);
                            }
                        }
                    }

                }

            }

        }

        private InteractableSpawnCard CreateRoutePortalSpawnCard(SceneDef destination)
        {
            var m = routePortalPrefab.GetComponent<RoutePortalManager>();
            m.destinationScene = destination;

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
                            if (manager != null)
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
                                    Log.Error("Could not set text to Tooltip");
                                }
                                routePortalPanel.SetActive(true);
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


    }
}

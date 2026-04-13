using BepInEx;
using BepInEx.Configuration;
using LeTai.Asset.TranslucentImage;
using R2API;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfRoutes.StageModifiers;
using RiskOfRoutes.StageModifiers.SceneModifiers;
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
//!Утечка памяти
//Пофиксить синхронизация токенов name desc
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
//more drones с плюсиком


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
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]

    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.0.0";

        //tweaks
        public static ConfigEntry<bool> usePortalMaterial { get; set; }
        public static ConfigEntry<bool> usePositiveNegativeColors { get; set; }
        public static ConfigEntry<bool> uniqueDestinations { get; set; }
        public static ConfigEntry<bool> showPortalDestination { get; set; }
        public static ConfigEntry<bool> teleporterUseRandomMods { get; set; }
        public static ConfigEntry<bool> modifiersCanBeBanned { get; set; }
        //artifacts
        public static ConfigEntry<bool> allowMysteryArtifact;
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
        public static ConfigEntry<int> doppelStackTime;
        public static ConfigEntry<int> elitesStackPercent;
        public static ConfigEntry<int> lunarStackPercent;
        public static ConfigEntry<int> voidStackPercent;
        public static ConfigEntry<int> soulCostStackPercent;

        public static ConfigEntry<int> routeMapWidth;


        public static GameObject routePortalPrefab;
        public static GameObject iconBubblePrefab;
        public static GameObject chefPrefab;
        public static GameObject routePortalPanelPrefab;
        public static GameObject routeMapPanelPrefab;

        public static Texture2D droneSymbolTexture;
        public static Texture2D rewardSymbolTexture;
        public static Texture2D aurelioniteSymbolTexture;

        public static InteractableSpawnCard tier1Printer;
        public static InteractableSpawnCard tier2Printer;
        public static InteractableSpawnCard tier3Printer;
        public static InteractableSpawnCard bossPrinter;
        public static InteractableSpawnCard wanderingChefSpawnCard;

        private InteractableSpawnCard portalSpawnCard;
        public static AssetBundle bundle;
        public static TMP_FontAsset bomb;

        public static Sprite placeholderIcon;

        public static ObjectScaleCurve pingCurve;

        public static CombatDirector.EliteTierDef[] defaultEliteTiers;

        public static GameObject networkedInventoryPrefab;

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
            //

            // для теста мультиплеера
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
            //

            BindConfig();

            //для своих префабов

            //для модифаера на предметы мобов
            networkedInventoryPrefab = new GameObject("NetworkedEnemyInventory", typeof(SetDontDestroyOnLoad));
            networkedInventoryPrefab.AddComponent<Inventory>();
            networkedInventoryPrefab.AddComponent<TeamFilter>();
            networkedInventoryPrefab.AddComponent<EnemyInfoPanelInventoryProvider>();

            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "riskofroutes");
            bundle = AssetBundle.LoadFromFile(path);
            //префабы
            bomb = Addressables.LoadAssetAsync<TMP_FontAsset>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropShadow.asset").WaitForCompletion();
            placeholderIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");
            iconBubblePrefab = CreateIconBubblePrefab();
            routePortalPrefab = CreateRoutePortalPrefab();
            droneSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterDrone");
            rewardSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterReward");
            aurelioniteSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterAurelionite");

            GameObject pingPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/PingIndicator.prefab").WaitForCompletion();
            PingIndicator pingIndicator = pingPrefab.GetComponent<PingIndicator>();
            pingCurve = pingIndicator.pingObjectScaleCurve;

            tier1Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion();
            tier2Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset").WaitForCompletion();
            tier3Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion();
            bossPrinter = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion();
            CreateChefSpawnCard();
            //

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
            //

            //хуки

            //ui
            ModUIManager.Initialize();

            //TODO портал в магазин может не успеть взять модификаторы
            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterEventSpawnRoutePortals;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += TeleporterInteraction_OnInteractionBegin;
            //On.RoR2.PortalSpawner.AttemptSpawnPortalServer += PortalSpawner_AttemptSpawnPortalServer;

            On.RoR2.DirectorCore.TrySpawnObject += OnVanillaPortalSpawn;

            //Портал выхода с базара
            //On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.BazaarController.Start += BazaarController_Start;
            On.RoR2.SeerStationController.SetRunNextStageToTarget += SeerStationController_SetRunNextStageToTarget;

            RoR2.RoR2Application.onLoad += () =>
            {
                StageModifierCatalog.Init(bundle);
            };
            AddRouteManagersToPortalPrefabs();

            MysteryArtifact.InitializeArtifact();
            SoulCostController.Initialize();

            AddHologramControllerToTeleporter();
            //
            networkedInventoryPrefab = new GameObject("NetworkedInventory", typeof(SetDontDestroyOnLoad));
            networkedInventoryPrefab.AddComponent<Inventory>();
            networkedInventoryPrefab.AddComponent<TeamFilter>();
            networkedInventoryPrefab.AddComponent<EnemyInfoPanelInventoryProvider>();


        }

        private void CreateChefSpawnCard()
        {
            Log.Info("Trying to create CHEF");
            InteractableSpawnCard card = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            GameObject lunarTable = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/Bazaar_LunarTable.prefab").WaitForCompletion();

            var mealPrep = chefPrefab;
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

            card.prefab = chefPrefab;
            card.directorCreditCost = 0;
            card.sendOverNetwork = true;
            card.hullSize = HullClassification.Human;
            card.orientToFloor = true;
            card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            wanderingChefSpawnCard = card;
        }


        private void BindConfig()
        {

            //VISUAL
            usePortalMaterial = Config.Bind<bool>(
                "Visual", "Use Scene Material", false, "[WIP] Sets destination scene portrait as a portal texture");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalMaterial));

            usePositiveNegativeColors = Config.Bind<bool>(
                "Visual", "Use Positive/Negative colors", false, "Makes bubbles on portals red and green, otherwise they are just blue");
            ModSettingsManager.AddOption(new CheckBoxOption(usePositiveNegativeColors));

            showPortalDestination = Config.Bind<bool>(
                "Visual", "Show Portal Destination", true, "Show destination scene in hud");
            ModSettingsManager.AddOption(new CheckBoxOption(showPortalDestination));


            //GENERAL
            uniqueDestinations = Config.Bind<bool>(
                "General", "Unique Destination", true, "Makes each route portal have unique scene as destination, otherwise they might repeat");
            ModSettingsManager.AddOption(new CheckBoxOption(uniqueDestinations));

            teleporterUseRandomMods = Config.Bind<bool>(
                "General", "Teleporter Use Random mods", true, "Makes teleporter use random modifiers of spawned route portals after interacting with it, set false if you want it not to use any stage modifiers");
            ModSettingsManager.AddOption(new CheckBoxOption(teleporterUseRandomMods));

            //todo
            modifiersCanBeBanned = Config.Bind<bool>(
                "General", "Modifiers can be banned after being used", true, "Prevents some of stage modifiers appearing on the next stage, if they were active on the current stage");
            ModSettingsManager.AddOption(new CheckBoxOption(modifiersCanBeBanned));

            routeMapWidth = Config.Bind<int>(
                "General", "Route map width", 3, "Width of the map created at the start of each loop");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapWidth, new IntSliderConfig() { min = 3, max = 5 }));

            //ARTIFACT MODIFIER
            allowMysteryArtifact = Config.Bind<bool>(
                "Artifacts", "Mystery", true, "Mystery artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMysteryArtifact));

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

            //more drones modifier
            droneStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(droneStackPercent));

            //more elites modifier
            elitesStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Elite increase percent", 33, "Percent per stack of modifier");
            ModSettingsManager.AddOption(new IntFieldOption(elitesStackPercent));

            //doppelganger modifier

            doppelStackTime = Config.Bind<int>(
                "Stage modifier tweaks", "Doppelganger cooldown reduced by stack", 30, "Cooldown reducement with one stack in seconds");
            ModSettingsManager.AddOption(new IntFieldOption(doppelStackTime));

            //lunar enemies
            lunarStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Lunar Enemies percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(lunarStackPercent));

            //void enemies
            voidStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Void Enemies percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(voidStackPercent));

            //soul cost
            soulCostStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Soul cost percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(soulCostStackPercent));
        }

        private void AddHologramControllerToTeleporter()
        {
            var teleporterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").WaitForCompletion();

            if (teleporterPrefab != null)
            {
                var hologramController = teleporterPrefab.AddComponent<ModifierHologramController>();

                hologramController.droneTex = droneSymbolTexture;
                hologramController.rewardTex = rewardSymbolTexture;
                hologramController.aurelioniteTex = aurelioniteSymbolTexture;
            }
        }

        private void BazaarController_Start(On.RoR2.BazaarController.orig_Start orig, BazaarController self)
        {
            orig(self);

            if (!NetworkServer.active) return;
            Log.Info("BazaarController_Start: Replacing portal");

            GameObject shopPortalObject = null;
            RoR2.SceneExitController[] exits = FindObjectsOfType<RoR2.SceneExitController>(true);
            foreach (var exit in exits)
            {
                if (exit.useRunNextStageScene || exit.gameObject.name.Contains("Shop"))
                {
                    shopPortalObject = exit.gameObject;
                    Log.Info($"Shop portal with name {exit.gameObject.name}");
                    break;
                }
            }

            if (shopPortalObject != null)
            {
                Vector3 pos = shopPortalObject.transform.position;
                Quaternion rot = shopPortalObject.transform.rotation;

                NetworkServer.Destroy(shopPortalObject);

                GameObject shopRoutePortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion();
                GameObject shopRoutePortal = Object.Instantiate(shopRoutePortalPrefab, pos, rot);

                var shopRouteManager = shopRoutePortal.GetComponent<RoutePortalManager>();
                if (shopRouteManager != null)
                {
                    shopRouteManager.enabled = true;
                    //mg.isManagerEnabled = true;
                    shopRouteManager.destinationSceneIndex = (int)(RoR2.Run.instance.nextStageScene.sceneDefIndex);
                    shopRouteManager.isOval = true;
                    shopRouteManager.isUsingModifiers = true;
                    shopRouteManager.isUsingSceneExit = true;
                    shopRouteManager.routeNode = StageModifierDirector.instance.currentNode;
                    shopRouteManager.nodeSync = new Vector2(shopRouteManager.routeNode.x, shopRouteManager.routeNode.y);
                    StageModifierDirector.instance.GetNextStageModifiers(shopRoutePortal);
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
                            if (seerRouteManager != null)
                            {
                                seerRouteManager.isSeerStation = true;
                                seerRouteManager.destinationSceneIndex = oldTargetSceneIndex;
                                if (IsSceneColossus((SceneIndex)oldTargetSceneIndex))
                                {
                                    seerRouteManager.routeNode = StageModifierDirector.instance.routeMap.CreateColossusNode(StageModifierDirector.instance.currentNode, currentLayer: true);
                                    seerRouteManager.nodeSync = new Vector2(seerRouteManager.routeNode.x, seerRouteManager.routeNode.y);
                                    StageModifierDirector.instance.UpdateSyncedMap();
                                }
                                else
                                {
                                    seerRouteManager.routeNode = StageModifierDirector.instance.currentNode;
                                    seerRouteManager.nodeSync = new Vector2(seerRouteManager.routeNode.x, seerRouteManager.routeNode.y);
                                }
                                if (!IsSceneIntermission((SceneIndex)oldTargetSceneIndex))
                                {
                                    seerRouteManager.isUsingModifiers = true;
                                    StageModifierDirector.instance.GetNextStageModifiers(newSeer);
                                }
                                else
                                {
                                    seerRouteManager.isUsingModifiers = false;
                                }

                                seerRouteManager.enabled = true;
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

        private void AddRouteManagersToPortalPrefabs()
        {
            var shopPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion();
            var goldshoresPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalGoldshores/PortalGoldshores.prefab").WaitForCompletion();
            var colossusPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/PortalColossus.prefab").WaitForCompletion();
            var hardwareProgPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/HardwareProgPortal.prefab").WaitForCompletion();
            var hardwareProgHauntPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/HardwareProgPortal_Haunt.prefab").WaitForCompletion();
            var destinationPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/PM DestinationPortal.prefab").WaitForCompletion();
            var seerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion();

            List<GameObject> vanillaPortals = new List<GameObject>() { shopPortalPrefab, colossusPortalPrefab, goldshoresPortalPrefab, hardwareProgPortalPrefab, hardwareProgHauntPortalPrefab, destinationPortalPrefab };
            //порталы
            foreach (var portal in vanillaPortals)
            {
                Log.Info($"Adding route portal manager to portal {portal.name}");
                var mg = portal.GetComponent<RoutePortalManager>();
                if (mg == null)
                {
                    portal.AddComponent<RoutePortalManager>();
                }
                Log.Info($"Added route portal manager to {portal.name} prefab");
            }
            //сиеры
            var seerRouteMg = seerPrefab.GetComponent<RoutePortalManager>();
            if (seerRouteMg == null)
            {
                seerPrefab.AddComponent<RoutePortalManager>();
                Log.Info($"Added route portal manager to seer prefab");
            }

        }

        private void TeleporterInteraction_OnInteractionBegin(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (!self.isCharged) return;
            if (Stage.instance.sceneDef.baseSceneName == "conduitcanyon")
            {
                Log.Info("Conduit canyon");
                var hardwareNode = StageModifierDirector.instance.routeMap.CreateHardwareNode(StageModifierDirector.instance.currentNode);
                StageModifierDirector.instance.currentNode = hardwareNode;
                StageModifierDirector.instance.currentNodeSync = new Vector2(hardwareNode.x, hardwareNode.y);
                StageModifierDirector.instance.UpdateSyncedMap();
            }
            else
            {

                StageModifierDirector.instance.GetRouteNodeFromRandomPortal();
                if (teleporterUseRandomMods.Value)
                    StageModifierDirector.instance.GetModifiersFromRandomPortal(); //todo пересмотреть
            }
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

        private void SeerStationController_SetRunNextStageToTarget(On.RoR2.SeerStationController.orig_SetRunNextStageToTarget orig, SeerStationController self)
        {
            orig(self);

            if (!NetworkServer.active || SceneManager.GetActiveScene().name != "bazaar")
            {
                return;
            }

            //сцена самого сиера
            SceneIndex targetScene = (SceneIndex)self.targetSceneDefIndex;

            GameObject shopPortal = null;
            SceneExitController exitController = null;
            RoR2.SceneExitController[] exits = FindObjectsOfType<RoR2.SceneExitController>(true);
            foreach (var exit in exits)
            {
                if (exit.useRunNextStageScene || exit.gameObject.name.Contains("Shop"))
                {
                    shopPortal = exit.gameObject;
                    exitController = exit;
                    exitController.useRunNextStageScene = true;
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
                //TODO удалить после теста
                Log.Error("Shop portal doesn't have manager");
            }
            else
            {
                var seerRouteManager = self.gameObject.GetComponent<RoutePortalManager>();

                var mg = shopPortal.GetComponent<RoutePortalManager>();
                int previous = (int)mg.destinationSceneIndex;
                mg.destinationSceneIndex = (int)targetScene;
                if (seerRouteManager)
                {
                    mg.stageModifiers.Clear();
                    foreach (var mod in seerRouteManager.stageModifiers)
                    {
                        mg.stageModifiers.Add(mod);
                    }

                }
                else
                    StageModifierDirector.instance.GetNextStageModifiers(shopPortal);

                SceneDef targetSceneDef = SceneCatalog.GetSceneDef((SceneIndex)targetScene);

                if (IsSceneColossus((SceneIndex)targetScene))
                {
                    mg.routeNode = seerRouteManager.routeNode;
                    mg.nodeSync = new Vector2(mg.routeNode.x, mg.routeNode.y);
                }
                else if (targetSceneDef.sceneType == SceneType.Intermission)
                {
                    mg.isUsingModifiers = false;

                }

                Log.Info($"Updated exit destination:{previous}->{mg.destinationSceneIndex}");
            }


        }



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
                    SceneDef currentScene = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
                    if (name.Contains("Portal"))
                    {
                        Log.Info("Some vanilla portal spawned");
                        Log.Info($"Director spawned {name} portal");
                        var mgr = obj.GetComponent<RoutePortalManager>();
                        if (!mgr)
                        {
                            Log.Error($"RoutePortalManager is missing for portal {name}");
                            return obj;
                        }

                        if (name == "iscRoutePortal")
                        {
                            return obj;
                        }

                        if (name == "iscColossusPortal")
                        {
                            //useRunNext у него false
                            mgr.destinationSceneIndex = GetGreenPortalSceneIndex();
                            mgr.isOval = true;
                            mgr.isUsingModifiers = Stage.instance.sceneDef.stageOrder != 3;
                            //todo здесь обновление карты захардкодено
                            if (currentScene != null && currentScene.sceneType == SceneType.Intermission)
                            {
                                StageModifierDirector.instance.currentNode.visited = false;
                                var colossusNode = StageModifierDirector.instance.routeMap.CreateColossusNode(StageModifierDirector.instance.currentNode, currentLayer: true);
                                StageModifierDirector.instance.UpdateSyncedMap();
                                mgr.routeNode = colossusNode;
                                mgr.nodeSync = new Vector2(colossusNode.x, colossusNode.y);

                            }
                            else
                            {
                                var colossusNode = StageModifierDirector.instance.routeMap.CreateColossusNode(StageModifierDirector.instance.currentNode);
                                StageModifierDirector.instance.UpdateSyncedMap();
                                mgr.routeNode = colossusNode;
                                mgr.nodeSync = new Vector2(colossusNode.x, colossusNode.y);
                            }
                            Log.Info("Succesfuly set node to colossus");
                        }
                        else if (name == "iscGoldshoresPortal")
                        {
                            mgr.isOval = true;
                            if (SceneManager.GetActiveScene().name != "goldshores")
                            {
                                mgr.isUsingModifiers = false;
                                mgr.routeNode = StageModifierDirector.instance.GetAvailableNodes()[0];
                            }
                            else
                            {
                                //TODO
                                mgr.destinationSceneIndex = (int)Run.instance.nextStageScene.sceneDefIndex;
                                mgr.isUsingModifiers = true;
                                mgr.isUsingSceneExit = false;
                                mgr.routeNode = StageModifierDirector.instance.currentNode;
                                mgr.nodeSync = new Vector2(mgr.routeNode.x, mgr.routeNode.y);
                            }

                        }
                        else if (name == "iscShopPortal")
                        {
                            mgr.isOval = true;
                            var exMgr = obj.GetComponent<SceneExitController>();
                            if (exMgr && !exMgr.useRunNextStageScene)
                            {
                                Log.Info("Portal is leading to shop");

                                var routes = StageModifierDirector.instance.GetAvailableNodes();
                                if (routes == null || routes.Count == 0)
                                {
                                    Log.Info("node zero");
                                    mgr.routeNode = StageModifierDirector.instance.routeMap.root;
                                    mgr.nodeSync = new Vector2(0, 0);
                                }
                                else
                                {
                                    mgr.routeNode = StageModifierDirector.instance.GetAvailableNodes()[0]; //todo рандом метод
                                    mgr.nodeSync = new Vector2(mgr.routeNode.x, mgr.routeNode.y);
                                }
                                mgr.isUsingSceneExit = false;
                            }
                            //else обрабатывается в методе BazaarOnStart 

                        }
                        else if (name == "iscHardwareProgPortal" || name == "iscHardwareProgPortal_Haunt")
                        {
                            //
                            mgr.isOval = false;
                            if (Stage.instance.sceneDef.stageOrder != 3)
                            {
                                mgr.isUsingModifiers = false;
                            }
                            else
                            {
                                mgr.destinationSceneIndex = (int)SceneCatalog.FindSceneIndex("conduitcanyon");
                                mgr.isUsingModifiers = true;

                            }
                            var hardwareNode = StageModifierDirector.instance.routeMap.CreateHardwareNode(StageModifierDirector.instance.currentNode);
                            StageModifierDirector.instance.UpdateSyncedMap();
                            mgr.routeNode = hardwareNode;
                            mgr.nodeSync = new Vector2(hardwareNode.x, hardwareNode.y);
                        }
                        else if (name == "iscDestinationPortal")
                        {
                            mgr.isOval = false;
                            mgr.isUsingModifiers = false;
                            mgr.routeNode = StageModifierDirector.instance.GetAvailableNodes()[0]; //TODO
                        }

                        if (mgr.isUsingModifiers)
                            StageModifierDirector.instance.GetNextStageModifiers(obj);
                        Log.Info($"Enabled manager for portal:{name}");
                        mgr.enabled = true;
                        //mgr.isManagerEnabled = true;
                        mgr.nodeSync = new Vector2(mgr.routeNode.x, mgr.routeNode.y);

                    }

                }
            }

            return obj;

        }




        public static string GetModifierDescription(ModifierSync mod, ModifierDef def)
        {
            string stackText = mod.stack > 1 ? $" - x{mod.stack}" : "";

            if (def.name.Contains("_Printer") || def.name.Contains("_EnemyItem") || def.name.Contains("_LunarItem"))
            {
                string itemName = def.name.Split('_')[0];
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(itemName);
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);

                string localizedName = itemName;

                if (itemDef != null)
                {
                    string defaultName = RoR2.Language.GetString(itemDef.nameToken);
                    string colorHex = "#FFFFFF";

                    switch (itemDef.tier)
                    {
                        case ItemTier.Tier1:
                            colorHex = "#FFFFFF";
                            break;
                        case ItemTier.Tier2:
                            colorHex = "#72ff13";
                            break;
                        case ItemTier.Tier3:
                            colorHex = "#e65541";
                            break;
                        case ItemTier.Boss:
                            colorHex = "#E4C415";
                            break;
                        case ItemTier.Lunar:
                            colorHex = "#3168c0";
                            break;
                    }

                    localizedName = $"<color={colorHex}>{defaultName}</color>";
                }

                if (def.name.Contains("_Printer"))
                {
                    return RoR2.Language.GetStringFormatted("PRINTER_ITEMS_MODIFIER_DESCRIPTION", localizedName);
                }
                else if (def.name.Contains("_EnemyItem"))
                {
                    return RoR2.Language.GetStringFormatted("ENEMY_ITEMS_MODIFIER_DESCRIPTION", localizedName, mod.stack);
                }
                else if (def.name.Contains("_LunarItem"))
                {
                    return RoR2.Language.GetStringFormatted("LUNAR_ITEMS_MODIFIER_DESCRIPTION", localizedName);
                }
            }
            else if (def.isArtifact)
            {
                return $"<style=cWorldEvent>{RoR2.Language.GetStringFormatted(def.descriptionToken)}</style>";
            }

            return RoR2.Language.GetString(def.descriptionToken) + stackText;
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

            var nodes = StageModifierDirector.instance.GetAvailableNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                Log.Info($"SpawnRouteportals:{nodes.Count}:{i}");
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
                    Log.Error("RoutePortal spawncard is null");
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
                    var mg = portalObject.GetComponent<RoutePortalManager>();

                    mg.enabled = true;

                    if (node == null)
                    {
                        Log.Error("RouteNode is null, skipping");
                        continue;
                    }
                    mg.routeNode = node;
                    mg.nodeSync = new Vector2(node.x, node.y);
                    Log.Info($"MG route node:{mg.routeNode.portalType}:{mg.routeNode.x},{mg.routeNode.y}");
                    mg.isUsingModifiers = true;
                    mg.isUsingSceneExit = true;
                    StageModifierDirector.instance.GetNextStageModifiers(portalObject);

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

        public bool IsSceneColossus(SceneIndex index)
        {
            return ((index == SceneCatalog.FindSceneIndex("lemuriantemple")) || (index == SceneCatalog.FindSceneIndex("habitat")) || (index == SceneCatalog.FindSceneIndex("meridian")));
        }

        public bool IsSceneIntermission(SceneIndex index)
        {
            var scene = SceneCatalog.GetSceneDef(index);
            if (scene == null) return false;
            return scene.sceneType == SceneType.Intermission;
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


        //Можно не перерисовывать карту если игрок просто нажал таб
        private void Update()
        {
            //if (Time.timeScale == 0f) return;

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
            /////

          


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

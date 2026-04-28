using BepInEx;
using BepInEx.Configuration;
using R2API;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfRoutes.StageModifiers;
using RiskOfRoutes.StageModifiers.SceneModifiers;
using RoR2;
using SamplePlugin;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityHotReloadNS;

//TODO
//Особые модификаторы для стейджей
//(Подкорректировать)Добавить больше предметов в пулл зеленых и белых, добавить возможность для такого же с предметами любого тира через конфиг
//Сделать, чтобы всё работало в мультиплеере
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
        public static ConfigEntry<bool> usePortalTypeColors { get; set; }
        public static ConfigEntry<bool> useUniqueDestinations { get; set; }
        public static ConfigEntry<bool> showPortalDestination { get; set; }
        public static ConfigEntry<bool> teleporterUseRandomMods { get; set; }
        public static ConfigEntry<bool> modifiersCanBeBanned { get; set; }
        public static ConfigEntry<bool> negativeStackPunishment { get; set; }
        public static ConfigEntry<int> punishmentTimeMinutes { get; set; }

        public static ConfigEntry<int> routeMapSize { get; set; }
        public static ConfigEntry<int> routeMapIconSize { get; set; }
        public static ConfigEntry<bool> routePanelShowsOnlyNames { get; set; }
        public static ConfigEntry<MapOrientation> punishmentBarOrientation { get; set; }
        public static ConfigEntry<Color> punishmentBarColor { get; set; }
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
        //stage modifiers
        public static ConfigEntry<bool> allowAurelioniteModifier;
        public static ConfigEntry<bool> allowBossRedItemModifier;
        public static ConfigEntry<bool> allowBossYellowItemModifier;
        public static ConfigEntry<bool> allowDroneBossGreenModifier;
        public static ConfigEntry<bool> allowDroneBossRedModifier;
        public static ConfigEntry<bool> allowMoreElitesModifier;
        public static ConfigEntry<bool> allowMoreDronesModifier;
        public static ConfigEntry<bool> allowUpgradeDronesModifier;
        public static ConfigEntry<bool> allowLunarEnemiesModifier;
        public static ConfigEntry<bool> allowVoidEnemiesModifier;
        public static ConfigEntry<bool> allowYellowPrinterModifier;
        public static ConfigEntry<bool> allowRedPrinterModifier;
        public static ConfigEntry<bool> allowOnlyFlyingModifier;
        public static ConfigEntry<bool> allowPowerfulElitesModifier;
        public static ConfigEntry<bool> allowRepairModifier;
        public static ConfigEntry<bool> allowWanderingChefModifier;
        public static ConfigEntry<bool> allowMountainModifier;
        public static ConfigEntry<bool> allowSoulCostModifier;
        public static ConfigEntry<bool> allowDoppelgangerModifier;
        //modifiers options
        public static ConfigEntry<int> droneStackPercent;
        public static ConfigEntry<int> doppelStackTime;
        public static ConfigEntry<int> elitesStackPercent;
        public static ConfigEntry<int> lunarStackPercent;
        public static ConfigEntry<int> voidStackPercent;
        public static ConfigEntry<int> soulCostStackPercent;
        //printer item
        public static ConfigEntry<bool> useDefaultPrinterItems;
        public static ConfigEntry<string> tier1PrinterItems;
        public static ConfigEntry<string> tier2PrinterItems;
        public static ConfigEntry<string> tier3PrinterItems;
        //enemy item
        public static ConfigEntry<bool> useDefaultEnemyItems;
        public static ConfigEntry<string> tier1EnemyItems;
        public static ConfigEntry<string> tier2EnemyItems;
        public static ConfigEntry<string> tier3EnemyItems;
        //lunar item
        public static ConfigEntry<bool> useDefaultLunarItems;
        public static ConfigEntry<string> tier1LunarItems;
        public static ConfigEntry<string> tier2LunarItems;
        public static ConfigEntry<string> tier3LunarItems;
        public static ConfigEntry<bool> lunarTurnToPearl;

        public static ConfigEntry<int> routeMapWidth;


        public static CombatDirector.EliteTierDef[] defaultEliteTiers;

        public static GameObject networkedInventoryPrefab;

        internal static GameObject CentralNetworkObject;
        private static GameObject _centralNetworkObjectSpawned;

        public void Awake()
        {
            Log.Init(Logger);
            BindConfig();

            var temp = new GameObject("tmpGo");
            temp.AddComponent<NetworkIdentity>();

            CentralNetworkObject = PrefabAPI.InstantiateClone(temp, "somethingUnique", false);

            CentralNetworkObject.AddComponent<StageModifierDirector>();
            CentralNetworkObject.AddComponent<MyNetworkComponent>();
            PrefabAPI.RegisterNetworkPrefab(CentralNetworkObject);

            networkedInventoryPrefab = PrefabAPI.InstantiateClone(temp, "monsterInventory", false);
            networkedInventoryPrefab.AddComponent<Inventory>();
            networkedInventoryPrefab.AddComponent<TeamFilter>();
            networkedInventoryPrefab.AddComponent<EnemyInfoPanelInventoryProvider>();
            PrefabAPI.RegisterNetworkPrefab(networkedInventoryPrefab);

            GameObject.Destroy(temp);



            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };

            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "riskofroutes");
            var bundle = AssetBundle.LoadFromFile(path);
            AssetManager.Initialize(bundle);

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

            ModUIManager.Initialize();

            RoR2.RoR2Application.onLoad += () =>
            {
                StageModifierCatalog.Init(bundle);
            };

            MysteryArtifact.InitializeArtifact();
            RoutesExpansion.InitializeExpansion(CentralNetworkObject);
            SoulCostController.Initialize();
        }

        //private void CreateNetworkObj(Run obj)
        //{
        //    if (NetworkServer.active)
        //    {
        //        if (!_centralNetworkObjectSpawned)
        //        {
        //            _centralNetworkObjectSpawned = GameObject.Instantiate(CentralNetworkObject);
        //            NetworkServer.Spawn(_centralNetworkObjectSpawned);
        //            Log.Info("StageModifierDirector spawned");
        //        }
        //    }
        //}

        private void BindConfig()
        {

            //GENERAL //////////////////////////////////////////////////////
            useUniqueDestinations = Config.Bind<bool>(
                "General", "Unique Destinations", true, "Makes each route portal have unique scene as destination, otherwise they might repeat");
            ModSettingsManager.AddOption(new CheckBoxOption(useUniqueDestinations));

            teleporterUseRandomMods = Config.Bind<bool>(
                "General", "Teleporter Use Random mods", true, "Makes teleporter use random modifiers of spawned route portals after interacting with it, set false if you want it to not have any stage modifiers");
            ModSettingsManager.AddOption(new CheckBoxOption(teleporterUseRandomMods));

            //todo
            modifiersCanBeBanned = Config.Bind<bool>(
                "General", "Modifiers Can Be Banned", true, "Prevents some of the stage modifiers appearing on the next stage, if they were active on the current stage");
            ModSettingsManager.AddOption(new CheckBoxOption(modifiersCanBeBanned));

            routeMapWidth = Config.Bind<int>(
                "General", "Route Map Width", 3, "Width of the map created at the start of each loop");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapWidth, new IntSliderConfig() { min = 3, max = 5 }));

            negativeStackPunishment = Config.Bind<bool>(
                "General", "Time Spent Punishment", true, "Adds one negative stack per punishment, punishment is given by spending too much time on current stage, by default it's 8 minutes");
            ModSettingsManager.AddOption(new CheckBoxOption(negativeStackPunishment));

            punishmentTimeMinutes = Config.Bind<int>(
                "General", "Punishment Time", 8, "Adds one negative stack per punishment, punishment is given by spending too much time on current stage, by default it's 8 minutes");
            ModSettingsManager.AddOption(new IntSliderOption(punishmentTimeMinutes, new IntSliderConfig() { min = 1, max = 20 }));

            //VISUAL ///////////////////////////////////////////////////////
            usePortalMaterial = Config.Bind<bool>(
                "Visual", "WIP Use Scene Material", false, "[WIP] Sets destination scene portrait as a portal texture");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalMaterial));

            usePositiveNegativeColors = Config.Bind<bool>(
                "Visual", "Use Positive/Negative colors", false, "Makes bubbles on portals red and green, otherwise they are just blue");
            ModSettingsManager.AddOption(new CheckBoxOption(usePositiveNegativeColors));

            showPortalDestination = Config.Bind<bool>(
                "Visual", "Show Portal Destination", true, "Show destination scene in hud");
            ModSettingsManager.AddOption(new CheckBoxOption(showPortalDestination));

            routeMapSize = Config.Bind<int>(
                "Visual", "Route Map Panel Size", 90, "Size of Route Map panel in percents");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapSize));

            routeMapIconSize = Config.Bind<int>(
                "Visual", "Route Map Icons Size", 100, "Size of icons on Route Map panel");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapIconSize));

            routePanelShowsOnlyNames = Config.Bind<bool>(
                "Visual", "Route Panel Shows Only Names", false, "Makes route portal panel show only names of modifiers, use if descriptions seem too cluttered");
            ModSettingsManager.AddOption(new CheckBoxOption(routePanelShowsOnlyNames));

            punishmentBarOrientation = Config.Bind<MapOrientation>(
                "Visual", "Punishment Bar Orientation", MapOrientation.Vertical, "Orientation of punishment bar");
            ModSettingsManager.AddOption(new ChoiceOption(punishmentBarOrientation));

            punishmentBarColor = Config.Bind<Color>(
                "Visual", "Punishment Bar Color", Color.white, "Color of progress bar");
            ModSettingsManager.AddOption(new ColorOption(punishmentBarColor));

            usePortalTypeColors = Config.Bind<bool>(
                "Visual", "Portal Color Type", false, "Portal color is changed according to it's node type, otherwise they are just blue");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalTypeColors));

            //more drones modifier /////////////////////////////////////////////////////////////////
            droneStackPercent = Config.Bind<int>(
                "Modifier Options", "Drone Increase", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(droneStackPercent));
            ////////////////////////////////////////////////////////////////////////////////////////

            //more elites modifier /////////////////////////////////////////////////////////////////
            elitesStackPercent = Config.Bind<int>(
                "Modifier Options", "Elite Increase", 33, "Percent of elite discount for director");
            ModSettingsManager.AddOption(new IntFieldOption(elitesStackPercent));
            ////////////////////////////////////////////////////////////////////////////////////////

            //doppelganger modifier

            doppelStackTime = Config.Bind<int>(
                "Modifier Options", "Doppelganger Cooldown Decrease", 30, "Cooldown reducement with one stack in seconds");
            ModSettingsManager.AddOption(new IntFieldOption(doppelStackTime));
            //////////////////////////////////////////////////////////////////////////////////////

            //lunar enemies
            lunarStackPercent = Config.Bind<int>(
                "Modifier Options", "Lunar Enemies Discount", 15, "Discount on lunar enemies in percents for more than one stack");
            ModSettingsManager.AddOption(new IntFieldOption(lunarStackPercent));
            /////////////////////////////////////////////////////////////////////////////////////

            //void enemies
            voidStackPercent = Config.Bind<int>(
                "Modifier Options", "Void Enemies Discount", 15, "Discount on lunar enemies in percents for more than one stack");
            ModSettingsManager.AddOption(new IntFieldOption(voidStackPercent));
            /////////////////////////////////////////////////////////////////////////////////////

            //soul cost
            soulCostStackPercent = Config.Bind<int>(
                "Modifier Options", "Soul Cost Increase", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(soulCostStackPercent));
            //////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////

            // PRINTER ITEM MODIFIER/////////////////////////////////////////////////////////////////

            useDefaultPrinterItems = Config.Bind<bool>(
                "Printer Items", "Use Default Items", true, "Use default configuration of what items printer modifier can have by modifier tier, use fields below to add other items from vanilla or mods. Disable if you want it to only use defs from fields below. ");
            ModSettingsManager.AddOption(new CheckBoxOption(useDefaultPrinterItems));

            tier1PrinterItems = Config.Bind<string>(
                "Printer Items", "Tier 1 Items", "", "Adds item defs to a printer modifier as tier1");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier1PrinterItems));

            tier2PrinterItems = Config.Bind<string>(
                "Printer Items", "Tier 2 Items", "", "Adds item defs to a printer modifier as tier2");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier2PrinterItems));

            tier3PrinterItems = Config.Bind<string>(
                "Printer Items", "Tier 3 Items", "", "Adds item defs to a printer modifier as tier3");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier3PrinterItems));
            // ENEMY ITEM MODIFIER/////////////////////////////////////////////////////////////////

            useDefaultEnemyItems = Config.Bind<bool>(
                "Enemy Items", "Use Default Items", true, "Use default configuration of what items enemy item modifier can have by modifier tier, use fields below to add other items from vanilla or mods. Disable if you want it to only use defs from fields below.");
            ModSettingsManager.AddOption(new CheckBoxOption(useDefaultEnemyItems));

            tier1EnemyItems = Config.Bind<string>(
                "Enemy Items", "Tier 1 Items", "", "Adds item defs to a enemy item modifier as tier1");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier1EnemyItems));

            tier2EnemyItems = Config.Bind<string>(
                "Enemy Items", "Tier 2 Items", "", "Adds item defs to a enemy item modifier as tier2");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier2EnemyItems));

            tier3EnemyItems = Config.Bind<string>(
                "Enemy Items", "Tier 3 Items", "", "Adds item defs to a enemy item modifier as tier3");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier3EnemyItems));

            // LUNAR ITEM MODIFIER/////////////////////////////////////////////////////////////////

            useDefaultLunarItems = Config.Bind<bool>(
                "Lunar Items", "Use Default Items", true, "Use default configuration of what items lunar item modifier can have by modifier tier, use fields below to add other items from vanilla or mods. Disable if you want it to only use defs from fields below. Accepts only lunar itemDefs");
            ModSettingsManager.AddOption(new CheckBoxOption(useDefaultLunarItems));

            tier1LunarItems = Config.Bind<string>(
                "Lunar Items", "Tier 1 Items", "", "Adds item defs to a lunar item modifier as tier1");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier1LunarItems));

            tier2LunarItems = Config.Bind<string>(
                "Lunar Items", "Tier 2 Items", "", "Adds item defs to a lunar item modifier as tier2");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier2LunarItems));

            tier3LunarItems = Config.Bind<string>(
                "Lunar Items", "Tier 3 Items", "", "Adds item defs to a lunar item modifier as tier3");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier3LunarItems));

            lunarTurnToPearl = Config.Bind<bool>(
                "Lunar Items", "Purify At The End", true, "By default the lunar item given by modifier is turned into a pearl at the end of the stage");
            ModSettingsManager.AddOption(new CheckBoxOption(lunarTurnToPearl));

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
                "Artifacts", "Vengeance", false, "Vengeance artifact available as artifact stage modifier. Not intended to be available");
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

            // STAGE MODIFIER TOGGLES /////////////////////////////////////////////////////////
            allowAurelioniteModifier = Config.Bind<bool>(
                "Stage Modifiers", "Aurelionite", true, "Aurelionite reward available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowAurelioniteModifier));

            allowBossRedItemModifier = Config.Bind<bool>(
                "Stage Modifiers", "Boss Red Item", true, "Boss Red Item available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowBossRedItemModifier));

            allowBossYellowItemModifier = Config.Bind<bool>(
                "Stage Modifiers", "Boss Yellow Item", true, "Boss Yellow Item available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowBossYellowItemModifier));

            allowDroneBossGreenModifier = Config.Bind<bool>(
                "Stage Modifiers", "Drone Boss Green", true, "Drone Boss Green available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDroneBossGreenModifier));

            allowDroneBossRedModifier = Config.Bind<bool>(
                "Stage Modifiers", "Drone Boss Red", true, "Drone Boss Red available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDroneBossRedModifier));

            allowMoreElitesModifier = Config.Bind<bool>(
                "Stage Modifiers", "More Elites", true, "More Elites available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMoreElitesModifier));

            allowMoreDronesModifier = Config.Bind<bool>(
                "Stage Modifiers", "More Drones", true, "More Drones available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMoreDronesModifier));

            allowUpgradeDronesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Upgrade Drones", true, "Upgrade Drones available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowUpgradeDronesModifier));

            allowLunarEnemiesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Lunar Enemies", true, "Lunar Enemies available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowLunarEnemiesModifier));

            allowVoidEnemiesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Void Enemies", true, "Void Enemies available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowVoidEnemiesModifier));

            allowYellowPrinterModifier = Config.Bind<bool>(
                "Stage Modifiers", "Yellow Printers", true, "Yellow Printer available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowYellowPrinterModifier));

            allowRedPrinterModifier = Config.Bind<bool>(
                "Stage Modifiers", "Red Printers", true, "Red Printer available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowRedPrinterModifier));

            allowOnlyFlyingModifier = Config.Bind<bool>(
                "Stage Modifiers", "Only Flying", true, "Only Flying enemies available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowOnlyFlyingModifier));

            allowPowerfulElitesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Powerful Elites", true, "Earlier Tier2 Elites available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowPowerfulElitesModifier));

            allowRepairModifier = Config.Bind<bool>(
                "Stage Modifiers", "Repair Items", true, "Repair Items available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowRepairModifier));

            allowWanderingChefModifier = Config.Bind<bool>(
                "Stage Modifiers", "Wandering Chef", true, "Wandering Chef available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowWanderingChefModifier));

            allowMountainModifier = Config.Bind<bool>(
                "Stage Modifiers", "Mountain Shrine", true, "Mountain Shrine available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMountainModifier));

            allowSoulCostModifier = Config.Bind<bool>(
                "Stage Modifiers", "Soul Cost", true, "Soul Cost available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSoulCostModifier));

            allowDoppelgangerModifier = Config.Bind<bool>(
                "Stage Modifiers", "Doppelganger", true, "Doppelganger available as a stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDoppelgangerModifier));
            ////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////
        }

        private void Update()
        {
            //if (Time.timeScale == 0f) return;

            if (Input.GetKeyDown(KeyCode.F2))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                Log.Info($"Player pressed F2. Spawning route portals {transform.position}");
                RoutesExpansion.SpawnRoutePortals(transform.position);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                UnityHotReload.LoadNewAssemblyVersion(
                    typeof(RiskOfRoutes).Assembly, // The currently loaded assembly to replace.
                    "C:/Users/vit/AppData/Roaming/r2modmanPlus-local/RiskOfRain2/profiles/TEstling/BepInEx/plugins/RiskOfRoutes.dll"  // The path to the newly compiled DLL.
                );
                Log.Info("Done");
            }
            ////
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

        public enum MapOrientation { Vertical, Horizontal }


    }
}

using BepInEx;
using BepInEx.Configuration;
using R2API;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfRoutes.Items;
using RiskOfRoutes.ModSupport;
using RiskOfRoutes.StageModifiers;
using RiskOfRoutes.UI;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using SamplePlugin;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
//using UnityHotReloadNS;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RiskOfRoutes
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(ItemAPI.PluginGUID)] //RiskofThunder-R2API_Items-1.0.8
    [BepInDependency(RecalculateStatsAPI.PluginGUID)] //RiskofThunder-R2API_RecalculateStats-1.6.6
    [BepInDependency("com.rune580.riskofoptions")] //Rune580-Risk_Of_Options-2.8.5
    [BepInDependency(R2API.LanguageAPI.PluginGUID)] //RiskofThunder-R2API_Language-1.1.0
    [BepInDependency(R2API.PrefabAPI.PluginGUID)] //RiskofThunder-R2API_Prefab-1.1.1
    [BepInDependency("LordVGames-CleanestHud-2.0.2", BepInDependency.DependencyFlags.SoftDependency)]

    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.1.1";

        public enum MapOrientation { Vertical, Horizontal }
        public enum ItemPoolOptions { Default, VanillaEntire, Entire, None }
        public enum CleanestHUDOptions { BottomCenter, BottomRight }

        //tweaks
        public static ConfigEntry<bool> useUniqueDestinations { get; set; }
        public static ConfigEntry<bool> teleporterUseRandomMods { get; set; }
        public static ConfigEntry<bool> modifiersCanBeBanned { get; set; }
        public static ConfigEntry<int> routeMapWidth;
        public static ConfigEntry<int> punishmentTimeMinutes { get; set; }
        public static ConfigEntry<bool> negativeStackPunishment { get; set; }
        //visual
        public static ConfigEntry<bool> cleanestHudCompat { get; set; }
        public static ConfigEntry<bool> usePortalMaterial { get; set; }
        public static ConfigEntry<bool> usePositiveNegativeColors { get; set; }
        public static ConfigEntry<bool> usePortalTypeColors { get; set; }
        public static ConfigEntry<bool> showPortalDestination { get; set; }
        public static ConfigEntry<bool> showIntermissionNodes;
        public static ConfigEntry<int> routeMapSize { get; set; }
        public static ConfigEntry<int> routeMapIconSize { get; set; }
        public static ConfigEntry<bool> routePanelShowsOnlyNames { get; set; }
        public static ConfigEntry<MapOrientation> punishmentBarOrientation { get; set; }
        public static ConfigEntry<CleanestHUDOptions> cleanestHudOption { get; set; }
        public static ConfigEntry<bool> hologramsAreColored { get; set; }
        public static ConfigEntry<Color> punishmentBarColor { get; set; }

        //artifacts
        public static ConfigEntry<string> tier1Artifacts;
        public static ConfigEntry<string> tier2Artifacts;
        public static ConfigEntry<string> tier3Artifacts;

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
        //modifiers options
        public static ConfigEntry<bool> soulCostCurseShared;
        public static ConfigEntry<int> droneStackPercent;
        public static ConfigEntry<int> doppelStackTime;
        public static ConfigEntry<int> elitesStackPercent;
        public static ConfigEntry<int> lunarStackPercent;
        public static ConfigEntry<int> voidStackPercent;
        public static ConfigEntry<int> soulCostStackPercent;
        public static ConfigEntry<int> tier2ElitesStackPercent;
        //printer item
        public static ConfigEntry<ItemPoolOptions> printerItemPool;
        //enemy item
        public static ConfigEntry<ItemPoolOptions> enemyItemPool;
        public static ConfigEntry<bool> canAppearWithAIBlacklisted;
        public static ConfigEntry<bool> enemyItemsModifierCanStack;
        public static ConfigEntry<bool> enemyItemsAllowBoss;
        public static ConfigEntry<bool> enemyItemsAllowVoid;
        public static ConfigEntry<int> commonDefaultStack;
        public static ConfigEntry<int> uncommonDefaultStack;
        public static ConfigEntry<int> legendaryDefaultStack;
        public static ConfigEntry<int> bossDefaultStack;
        public static ConfigEntry<int> commonExtraRange;
        public static ConfigEntry<int> uncommonExtraRange;
        public static ConfigEntry<int> legendaryExtraRange;
        public static ConfigEntry<int> bossExtraRange;
        public static ConfigEntry<int> chanceForExtra;
        //lunar item
        public static ConfigEntry<ItemPoolOptions> lunarItemPool;
        public static ConfigEntry<string> tier1LunarItems;
        public static ConfigEntry<string> tier2LunarItems;
        public static ConfigEntry<string> tier3LunarItems;
        public static ConfigEntry<bool> lunarTurnToPearl;

        public static ConfigEntry<bool> allowDebug;
        public static ConfigEntry<bool> enableFalseSonReward;

        public static GameObject networkedInventoryPrefab;

        internal static GameObject CentralNetworkObject;
        private static GameObject _centralNetworkObjectSpawned;

        public static HashSet<string> bannedModifierDefs = new HashSet<string>();
        public static HashSet<string> bannedArtifactDefs = new HashSet<string>();
        public static HashSet<string> bannedPrinterItems = new HashSet<string>();
        public static HashSet<string> bannedEnemyItems = new HashSet<string>();
        public static HashSet<string> bannedLunarDefs = new HashSet<string>();
        //
        public static HashSet<DefInfo> moddedArtifactsInfo = new HashSet<DefInfo>();
        public static HashSet<DefInfo> moddedLunarsInfo = new HashSet<DefInfo>();

        public static ConfigEntry<string> bannedModifierConfig;
        public static ConfigEntry<string> bannedArtifactConfig;
        public static ConfigEntry<string> bannedPrinterConfig;
        public static ConfigEntry<string> bannedEnemyConfig;
        public static ConfigEntry<string> bannedLunarConfig;
        //
        public static ConfigEntry<string> moddedArtifactsConfig;
        public static ConfigEntry<string> moddedLunarsConfig;


        public void Awake()
        {
            BindConfig();

            Log.Init(Logger);

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

            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };

            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "riskofroutes");
            var bundle = AssetBundle.LoadFromFile(path);
            AssetManager.Initialize(bundle);

            ModUIManager.Initialize();

            RoR2.RoR2Application.onLoad += () =>
            {
                if (ModSupport.CleanestHudCompat.enabled)
                {
                    Log.Info("Cleanest hud enabled");
                    CleanestHudCompat.Hooks();
                }
                else
                {
                    Log.Info("NOt enabled");
                }

                StageModifierCatalog.Init(bundle);
            };

            RoR2.RoR2Application.onLoad += PopulateLists;

            MysteryArtifact.InitializeArtifact();
            RoutesExpansion.InitializeExpansion(CentralNetworkObject);
            SoulCostController.Initialize();

            Sprite icon = AssetManager.modIcon;
            ModSettingsManager.SetModIcon(icon);
            ModSettingsManager.SetModDescription("Risk Of Routes. Mod that adds routing system and run map to the game.");

            ModItemsManager.InitializeItems();
        }

        private void PopulateLists()
        {
            var bannedModifiersList = bannedModifierConfig.Value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            foreach (var a in bannedModifiersList)
            {
                Log.Info($"\t{a}");
                if (StageModifierCatalog.FindModifier(a) != null)
                    bannedModifierDefs.Add(a);
            }

            var bannedArtifactsList = bannedArtifactConfig.Value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            foreach (var a in bannedArtifactsList)
            {
                Log.Info($"\t{a}");
                if (ArtifactCatalog.FindArtifactDef(a) != null)
                    bannedArtifactDefs.Add(a);
            }
            var bannedPrinterList = bannedPrinterConfig.Value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            foreach (var a in bannedPrinterList)
            {
                Log.Info($"\t{a}");
                if (ItemCatalog.FindItemIndex(a) != ItemIndex.None)
                {
                    bannedPrinterItems.Add(a);
                }
                else
                {
                    Log.Warning($"Couldnt find the def by {a}");
                }
            }
            var bannedEnemyList = bannedEnemyConfig.Value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            foreach (var a in bannedEnemyList)
            {
                if (ItemCatalog.FindItemIndex(a) != ItemIndex.None)
                    bannedEnemyItems.Add(a);
            }
            var bannedLunarList = bannedLunarConfig.Value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            foreach (var a in bannedLunarList)
            {
                if (ItemCatalog.FindItemIndex(a) != ItemIndex.None)
                    bannedLunarDefs.Add(a);
            }

            Log.Info("Content manager");
            foreach (var cont in ContentManager.allLoadedContentPacks)
            {
                Log.Info($"\t{cont.identifier}");
            }
            //
            var moddedArtsList = moddedArtifactsConfig.Value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            foreach (var a in moddedArtsList)
            {
                var split = a.Split('=');
                if (split.Length >= 3)
                    moddedArtifactsInfo.Add(new DefInfo
                    {
                        name = split[0],
                        isNegative = split[1] == "-",
                        tier = (ModifierTier)int.Parse(split[2])
                    });
            }

            var moddedLunarsList = moddedLunarsConfig.Value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            foreach (var a in moddedLunarsList)
            {
                var split = a.Split('=');
                if (split.Length >= 3)
                    moddedLunarsInfo.Add(new DefInfo
                    {
                        name = split[0],
                        isNegative = split[1] == "-",
                        tier = (ModifierTier)int.Parse(split[2])
                    });
            }
        }



        private void BindConfig()
        {
            //GENERAL
            useUniqueDestinations = Config.Bind<bool>(
                "General", "Unique Destinations", true, "Makes each route portal have unique scene as destination, otherwise they might repeat.");
            ModSettingsManager.AddOption(new CheckBoxOption(useUniqueDestinations));

            teleporterUseRandomMods = Config.Bind<bool>(
                "General", "Teleporter Use Random mods", true, "Makes the teleporter use random modifiers of spawned route portals after interacting with it, set false if you want it to not have any stage modifiers.");
            ModSettingsManager.AddOption(new CheckBoxOption(teleporterUseRandomMods));

            modifiersCanBeBanned = Config.Bind<bool>(
                "General", "Modifiers Can Be Banned", true, "Prevents some of the stage modifiers appearing on the next stage, if they were active on the current stage.");
            ModSettingsManager.AddOption(new CheckBoxOption(modifiersCanBeBanned));

            routeMapWidth = Config.Bind<int>(
                "General", "Route Map Width", 4, "Width of the map created at the start of each loop.");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapWidth, new IntSliderConfig() { min = 3, max = 5 }));

            negativeStackPunishment = Config.Bind<bool>(
                "General", "Time Spent Punishment", true, "Adds one negative stack per punishment. Punishment is given by spending too much time on current stage, by default it's 8 minutes.");
            ModSettingsManager.AddOption(new CheckBoxOption(negativeStackPunishment));

            punishmentTimeMinutes = Config.Bind<int>(
                "General", "Punishment Time", 8, "Adds one negative stack per punishment, punishment is given by spending too much time on current stage, by default it's 8 minutes.");
            ModSettingsManager.AddOption(new IntSliderOption(punishmentTimeMinutes, new IntSliderConfig() { min = 1, max = 20 }));

            //VISUAL
            usePositiveNegativeColors = Config.Bind<bool>(
                "Visual", "Use Positive/Negative colors", true, "Makes bubbles on portals red and green, otherwise they are just blue.");
            ModSettingsManager.AddOption(new CheckBoxOption(usePositiveNegativeColors));

            usePortalTypeColors = Config.Bind<bool>(
                "Visual", "Portal Color Type", true, "Portal color is changed according to its node type, otherwise they are just blue.");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalTypeColors));

            //showPortalDestination = Config.Bind<bool>(
            //    "Visual", "Show Portal Destination", true, "Show destination scene on panel.");
            //ModSettingsManager.AddOption(new CheckBoxOption(showPortalDestination));

            routeMapSize = Config.Bind<int>(
                "Visual", "Route Map Panel Size", 95, "Size of Route Map panel in percents.");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapSize));

            routeMapIconSize = Config.Bind<int>(
                "Visual", "Route Map Icons Size", 100, "Size of icons on Route Map panel.");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapIconSize));

            showIntermissionNodes = Config.Bind<bool>(
                "Visual", "Show Intermission Nodes", true, "Shows nodes for intermission stages.");
            ModSettingsManager.AddOption(new CheckBoxOption(showIntermissionNodes));

            routePanelShowsOnlyNames = Config.Bind<bool>(
                "Visual", "Route Panel Shows Only Names", false, "Makes route portal panel show only names of modifiers. Use if descriptions seem too cluttered.");
            ModSettingsManager.AddOption(new CheckBoxOption(routePanelShowsOnlyNames));

            punishmentBarOrientation = Config.Bind<MapOrientation>(
                "Visual", "Punishment Bar Orientation", MapOrientation.Vertical, "Orientation of punishment bar.");
            ModSettingsManager.AddOption(new ChoiceOption(punishmentBarOrientation));

            punishmentBarColor = Config.Bind<Color>(
                "Visual", "Punishment Bar Color", Color.red, "Color of punishment bar.");
            ModSettingsManager.AddOption(new ColorOption(punishmentBarColor));

            hologramsAreColored = Config.Bind<bool>(
                "Visual", "Holograms Are Colored", true, "Makes teleporter holograms colored according to active modifiers, otherwise they are just blue.");
            ModSettingsManager.AddOption(new CheckBoxOption(hologramsAreColored));

            cleanestHudCompat = Config.Bind<bool>(
                "Visual", "Cleanest HUD Compat", true, "Disable if you encounter any bugs with CleanestHUD mod");
            ModSettingsManager.AddOption(new CheckBoxOption(cleanestHudCompat));

            cleanestHudOption = Config.Bind<CleanestHUDOptions>(
                "Visual", "CleanestHUD Position", CleanestHUDOptions.BottomCenter, "Position of route map with CleanestHUD mod.");
            ModSettingsManager.AddOption(new ChoiceOption(cleanestHudOption));

            usePortalMaterial = Config.Bind<bool>(
                "Visual", "WIP Use Scene Material", false, "[WIP] Sets destination scene portrait as a portal texture.");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalMaterial));

            //MODIFIER OPTIONS 
            droneStackPercent = Config.Bind<int>(
                "Modifier Options", "Drone Increase", 33, "Percent per stack of initial scene director budget, used only to spawn additional drones. By default it's 33%.");
            ModSettingsManager.AddOption(new IntFieldOption(droneStackPercent));

            elitesStackPercent = Config.Bind<int>(
                "Modifier Options", "Elite Increase", 33, "Percent of elite discount for director. By default it's 33%.");
            ModSettingsManager.AddOption(new IntFieldOption(elitesStackPercent));

            doppelStackTime = Config.Bind<int>(
                "Modifier Options", "Doppelganger Decrease", 30, "Cooldown reduction with one stack in seconds. By default it's 30s.");
            ModSettingsManager.AddOption(new IntFieldOption(doppelStackTime));

            lunarStackPercent = Config.Bind<int>(
                "Modifier Options", "Lunar Enemies Discount", 15, "Discount on lunar enemies in percents for more than one stack. By default it's 15%.");
            ModSettingsManager.AddOption(new IntFieldOption(lunarStackPercent));

            voidStackPercent = Config.Bind<int>(
                "Modifier Options", "Void Enemies Discount", 15, "Discount on void enemies in percents for more than one stack. By default it's 15%.");
            ModSettingsManager.AddOption(new IntFieldOption(voidStackPercent));

            tier2ElitesStackPercent = Config.Bind<int>(
                "Modifier Options", "Post-Loop Elites Increase", 15, "Discount on tier2 elites in percents for more than one stack. By default it's 15%.");
            ModSettingsManager.AddOption(new IntFieldOption(tier2ElitesStackPercent));

            soulCostStackPercent = Config.Bind<int>(
                "Modifier Options", "Soul Cost Increase", 2, "Percents added to hp curse for stack. By default it's 2%.");
            ModSettingsManager.AddOption(new IntFieldOption(soulCostStackPercent));

            soulCostCurseShared = Config.Bind<bool>(
                "Modifier Options", "Soul Cost Curse Shared", false, "Soul Cost curse is shared between all players.");
            ModSettingsManager.AddOption(new CheckBoxOption(soulCostCurseShared));

            //banned printer items
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle White Printer Items",
                "Printer Items",
                "Use it if you want to ban items from appearing in printer item modifier, by default all registered items are used",
                "Open Menu", () => OpenPopoutMenu("whitePrinterItems", "Printer Items", "Toggle White Items")));

            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle Green Printer Items",
                "Printer Items",
                "Use it if you want to ban items from appearing in printer item modifier, by default all registered items are used",
                "Open Menu", () => OpenPopoutMenu("greenPrinterItems", "Printer Items", "Toggle Green Items")));

            //toggle stageModifiers
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle Stage Modifiers",
                "StageModifier Toggle",
                "Use it if you want to ban certain stage modifiers from appearing",
                "Open Menu", () => OpenPopoutMenu("stageModifiersGeneral", "Stage Modifiers", "Toggle Stage Modifiers")));

            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle Artifact Modifiers",
                "Artifact Toggle",
                "Configure what vanilla artifacts are available as ArtifactModifier",
                "Open Menu", () => OpenPopoutMenu("vanillaArtifacts", "Vanilla Artifacts", "Toggle Artifacts")));

            ModSettingsManager.AddOption(new GenericButtonOption(
                "Configure Modded Artifacts",
                "Artifact Toggle",
                "Configure what modded artifacts are available as ArtifactModifier. You can also choose their tier and if they are positive or negative. Positive artifact modifier appears on the \'Rare\' node",
                "Open Menu",
                () => OpenPopoutMenu("moddedArtifacts", "Modded artifacts", "Configure their type, tier and pos/negative")));


            // ENEMY ITEM MODIFIER/////////////////////////////////////////////////////////////////

            //banned enemy items
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle White Enemy Items",
                "Enemy Items",
                "Configure stuff in here",
                "Open menu",
                () => OpenPopoutMenu("enemyWhiteItems", "Enemy White Items", "Toggle enemy white items")));
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle Green Enemy Items",
                "Enemy Items",
                "Configure stuff in here",
                "Open menu", () => OpenPopoutMenu("enemyGreenItems", "Enemy Green Items", "Toggle enemy green items")));
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle Red Enemy Items",
                "Enemy Items",
                "Configure stuff in here",
                "Open menu", () => OpenPopoutMenu("enemyRedItems", "Enemy Red Items", "Toggle enemy red items")));


            ModSettingsManager.AddOption(new GenericButtonOption("Toggle Yellow Items", "Enemy Items", "Configure stuff in here", "Open Menu", () => OpenPopoutMenu("enemyYellowItems", "Enemy Yellow Items", "Toggle enemy yellow items")));

            ModSettingsManager.AddOption(new GenericButtonOption("Toggle Void Items", "Enemy Items", "Configure stuff in here", "Open Menu", () => OpenPopoutMenu("enemyVoidItems", "Enemy Void Items", "Toggle enemy void items")));

            ModSettingsManager.AddOption(new GenericButtonOption("Toggle AIBlacklisted Items", "Enemy Items", "Configure stuff in here", "Open Menu", () => OpenPopoutMenu("enemyBlacklistedItems", "Enemy AIBlacklisted", "Toggle AIBlacklisted")));

            commonDefaultStack = Config.Bind<int>(
                "Enemy Items", "Common Default Stack", 5, "Default amount per stack.");
            ModSettingsManager.AddOption(new IntFieldOption(commonDefaultStack));

            uncommonDefaultStack = Config.Bind<int>(
                "Enemy Items", "Uncommon Default Stack", 3, "Default amount per stack.");
            ModSettingsManager.AddOption(new IntFieldOption(uncommonDefaultStack));

            legendaryDefaultStack = Config.Bind<int>(
                "Enemy Items", "Legendary Default Stack", 1, "Default amount per stack.");
            ModSettingsManager.AddOption(new IntFieldOption(legendaryDefaultStack));

            bossDefaultStack = Config.Bind<int>(
                "Enemy Items", "Boss Default Stack", 1, "Default amount per stack.");
            ModSettingsManager.AddOption(new IntFieldOption(bossDefaultStack));

            chanceForExtra = Config.Bind<int>(
                "Enemy Items", "Chance For Extra", 0, "Chance for enemy items to have extra stacks.");
            ModSettingsManager.AddOption(new IntSliderOption(chanceForExtra));

            commonExtraRange = Config.Bind<int>(
                "Enemy Items", "Common Extra Range", 3, "Amount of item stacks added as extra.");
            ModSettingsManager.AddOption(new IntFieldOption(commonExtraRange));

            uncommonExtraRange = Config.Bind<int>(
                "Enemy Items", "Uncommon Extra Range", 2, "Amount of item stacks added as extra.");
            ModSettingsManager.AddOption(new IntFieldOption(uncommonExtraRange));

            legendaryExtraRange = Config.Bind<int>(
                "Enemy Items", "Legendary Extra Range", 1, "Amount of item stacks added as extra.");
            ModSettingsManager.AddOption(new IntFieldOption(legendaryExtraRange));

            bossExtraRange = Config.Bind<int>(
                "Enemy Items", "Boss Extra Range", 1, "Amount of item stacks added as extra.");
            ModSettingsManager.AddOption(new IntFieldOption(bossExtraRange));

            // LUNAR ITEM MODIFIER/////////////////////////////////////////////////////////////////

            lunarTurnToPearl = Config.Bind<bool>(
                "Lunar Items", "Purify At The End", true, "By default the lunar item given by the modifier is turned into a pearl at the end of the stage.");
            ModSettingsManager.AddOption(new CheckBoxOption(lunarTurnToPearl));

            ModSettingsManager.AddOption(new GenericButtonOption(
                "Toggle Lunar Items",
                "Lunar Items",
                "Configure what vanilla lunar items are available as modifier",
                "Open Menu", () => OpenPopoutMenu("vanillaLunar", "Vanilla Artifacts", "Use to ban")));

            ModSettingsManager.AddOption(new GenericButtonOption(
                "Configure Modded Lunars",
                "Lunar Items",
                "Configure what modded lunar items are available as ArtifactModifier. You can also choose their tier.",
                "Open Menu",
                () => OpenPopoutMenu("moddedLunar", "Modded artifacts", "Configure their type, tier and pos/negative")));


            //debug

            allowDebug = Config.Bind<bool>(
                "Debugging", "Enable debugging", false, "For debugging purposes.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDebug));

            enableFalseSonReward = Config.Bind<bool>(
                "Debugging", "Enable reward", false, "WIP False Son now drops new item. False Son's Heart applies Aurelinite Modifier on every stage.");
            ModSettingsManager.AddOption(new CheckBoxOption(enableFalseSonReward));

            bannedModifierConfig = Config.Bind<string>(
                "Debugging", "BannedModifierConfig", "", "Banned modifier defs separated by comma.");

            bannedArtifactConfig = Config.Bind<string>(
                "Debugging",
                "BannedArtifactConfig",
                "Command, Delusion, Devotion, MixEnemy, Enigma, MonsterTeamGainsItems, Glass, SingleMonsterType, RandomSurvivorOnRespawn, Rebirth, ShadowClone, Prestige",
                "Banned modifier defs separated by comma.");

            bannedPrinterConfig = Config.Bind<string>(
    "Debugging", "BannedPrinterConfig", "", "Banned modifier defs separated by comma.");

            bannedEnemyConfig = Config.Bind<string>(
    "Debugging",
    "BannedEnemyConfig",
    "BearVoid, BleedOnHitVoid, ChainLightningVoid, CloverVoid, CritGlassesVoid, ElementalRingVoid, EquipmentMagazineVoid, ExplodeOnDeathVoid, ExtraLifeVoid, MissileVoid, MushroomVoid, SlowOnHitVoid, VoidMegaCrabItem, ArtifactKey, BeetleGland, BleedOnHitAndExplode, FireballsOnHit, Knurl, LightningStrikeOnHit, MinorConstructOnKill, NovaOnLowHealth, ParentEgg, Pearl, PowerPyramid, RoboBallBuddy, ShinyPearl, ShockDamageAura, SiphonOnLowHealth, SprintWisp, TitanGoldDuringTP, AlienHead, ArmorReductionOnHit, BarrierOnOverHeal, Behemoth, BoostAllStats, BounceNearby, CaptainDefenseMatrix, Clover, CritDamage, ExtraLife, GhostOnKill, Icicle, ImmuneToDebuff, IncreaseHealing, LaserTurbine, MeteorAttackOnHighDamage, MoreMissile, NovaOnHeal, PermanentDebuffOnHit, Plant, RandomEquipmentTrigger, SharedSuffering, StunAndPierce, UtilitySkillMagazine",
    "Banned modifier defs separated by comma."
    );

            bannedLunarConfig = Config.Bind<string>(
    "Debugging",
    "BannedLunarConfig",
    "LunarTrinket, LunarSun, LunarSpecialReplacement, RandomlyLunar, AutoCastEquipment, LunarSecondaryReplacement, OnLevelUpFreeUnlock, RandomDamageZone, TransferDebuffOnHit, LunarPrimaryReplacement",
    "Banned modifier defs separated by comma.");

            moddedArtifactsConfig = Config.Bind<string>(
    "Debugging", "ModdedArtifactConfing", "", "Banned modifier defs separated by comma.");

            moddedLunarsConfig = Config.Bind<string>(
    "Debugging", "ModdedLunarConfig", "", "Banned modifier defs separated by comma.");

            ModSettingsManager.AddOption(new StringInputFieldOption(bannedModifierConfig));
            ModSettingsManager.AddOption(new StringInputFieldOption(bannedArtifactConfig));
            ModSettingsManager.AddOption(new StringInputFieldOption(bannedPrinterConfig));
            ModSettingsManager.AddOption(new StringInputFieldOption(bannedEnemyConfig));
            ModSettingsManager.AddOption(new StringInputFieldOption(bannedLunarConfig));
            //
            ModSettingsManager.AddOption(new StringInputFieldOption(moddedArtifactsConfig));
            ModSettingsManager.AddOption(new StringInputFieldOption(moddedLunarsConfig));


        }

        private void OpenPopoutMenu(string type, string title, string desc, bool checkboxShowInfo = false)
        {
            var globalCanvas = ModUIManager.globalCanvas;
            if (globalCanvas == null)
            {
                Log.Warning("Global canvas is null, cant attach panel");
                return;
            }

            GameObject popout = null;
            var popoutTransform = globalCanvas.transform.Find(type);
            if (popoutTransform == null)
            {
                Log.Info($"Creatring for the first time - {type}");
                popout = Object.Instantiate(AssetManager.CreatePopoutPrefab(), globalCanvas.transform, false); //todo replace m
                //popout.name = type;
                popout.SetActive(true);
            }
            else
            {
                popout = popoutTransform.gameObject;
            }

            RoutesPopoutController popoutController = popout.GetComponent<RoutesPopoutController>();
            if (popoutController == null)
            {
                Log.Error("HGPopout panel is nuull");
                return;
            }

            popoutController.title.token = title;
            popoutController.subtitle.token = desc;
            popoutController.checkboxShowInfo = checkboxShowInfo;


            var container = popoutController.container;

            HashSet<string> usedBanList = new HashSet<string>();
            ConfigEntry<string> usedBanConfig = bannedPrinterConfig;
            HashSet<DefInfo> usedModList = new HashSet<DefInfo>();
            ConfigEntry<string> usedModConfig = bannedPrinterConfig;

            HashSet<DefInfo> usedDefs = new HashSet<DefInfo>();
            if (type == "whitePrinterItems")
            {
                foreach (var def in StageModifierCatalog.printerRegisteredDefs.Where(def => def.tier == ItemTier.Tier1))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedPrinterItems;
                usedBanConfig = bannedPrinterConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }
            if (type == "greenPrinterItems")
            {
                foreach (var def in StageModifierCatalog.printerRegisteredDefs.Where(def => def.tier == ItemTier.Tier2))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                usedBanList = bannedPrinterItems;
                usedBanConfig = bannedPrinterConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "enemyWhiteItems")
            {
                foreach (var def in StageModifierCatalog.enemyRegisteredDefs.Where(def => def.tier == ItemTier.Tier1 && def.DoesNotContainTag(ItemTag.AIBlacklist)))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedEnemyItems;
                usedBanConfig = bannedEnemyConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "enemyGreenItems")
            {
                foreach (var def in StageModifierCatalog.enemyRegisteredDefs.Where(def => def.tier == ItemTier.Tier2 && def.DoesNotContainTag(ItemTag.AIBlacklist)))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedEnemyItems;
                usedBanConfig = bannedEnemyConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "enemyRedItems")
            {
                foreach (var def in StageModifierCatalog.enemyRegisteredDefs.Where(def => def.tier == ItemTier.Tier3 && def.DoesNotContainTag(ItemTag.AIBlacklist)))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedEnemyItems;
                usedBanConfig = bannedEnemyConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "enemyYellowItems")
            {
                foreach (var def in StageModifierCatalog.enemyRegisteredDefs.Where(def => def.tier == ItemTier.Boss && def.DoesNotContainTag(ItemTag.AIBlacklist)))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedEnemyItems;
                usedBanConfig = bannedEnemyConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "enemyVoidItems")
            {
                foreach (var def in StageModifierCatalog.enemyRegisteredDefs.Where(def =>
                (def.tier == ItemTier.VoidTier1 ||
                def.tier == ItemTier.VoidTier2 ||
                def.tier == ItemTier.VoidTier3 ||
                def.tier == ItemTier.VoidBoss) && def.DoesNotContainTag(ItemTag.AIBlacklist)
                ))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedEnemyItems;
                usedBanConfig = bannedEnemyConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "enemyBlacklistedItems")
            {
                foreach (var def in StageModifierCatalog.enemyRegisteredDefs.Where(def => def.ContainsTag(ItemTag.AIBlacklist)))
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedEnemyItems;
                usedBanConfig = bannedEnemyConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "stageModifiersGeneral")
            {
                foreach (var def in StageModifierCatalog.AllGeneralDefs)
                {
                    usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedModifierDefs;
                usedBanConfig = bannedModifierConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
            }

            if (type == "vanillaArtifacts")
            {
                foreach (var def in StageModifierCatalog.artifactTiers.Keys)
                {
                    var artInfo = new DefInfo(ArtifactCatalog.FindArtifactDef(def));
                    var defInfo = StageModifierCatalog.artifactTiers[def];

                    artInfo.tier = (ModifierTier)defInfo.tier;
                    artInfo.isNegative = defInfo.isNegative;

                    usedDefs.Add(artInfo);
                }
                ;
                usedBanList = bannedArtifactDefs;
                usedBanConfig = bannedArtifactConfig;
                popoutController.cycleOption = PopoutCycleOption.Default;
                popoutController.checkboxShowInfo = true;
                popoutController.defaultBanList = StageModifierCatalog.artifactDefaultBanList;
            }

            if (type == "moddedArtifacts")
            {
                foreach (var def in ArtifactCatalog.artifactDefs)
                {
                    if (!StageModifierCatalog.artifactTiers.Keys.Contains(def.cachedName))
                        usedDefs.Add(new DefInfo(def));
                }
                ;
                usedBanList = bannedArtifactDefs;
                usedBanConfig = bannedArtifactConfig;
                popoutController.cycleOption = PopoutCycleOption.All;
                usedModList = moddedArtifactsInfo;
                usedModConfig = moddedArtifactsConfig;
            }

            if (type == "vanillaLunar")
            {
                foreach (var def in StageModifierCatalog.lunarItemTiers.Keys)
                {
                    var itemDef = new DefInfo(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(def)));
                    var lunarInfo = StageModifierCatalog.lunarItemTiers[def];

                    itemDef.tier = (ModifierTier)lunarInfo.tier;
                    itemDef.isNegative = lunarInfo.isNegative;

                    usedDefs.Add(itemDef);
                }
                usedBanList = bannedLunarDefs;
                usedBanConfig = bannedLunarConfig;
                popoutController.moddedList = moddedLunarsInfo;
                popoutController.cycleOption = PopoutCycleOption.Default;
                popoutController.checkboxShowInfo = true;
                popoutController.defaultBanList = StageModifierCatalog.lunarDefaultBanList;
            }


            if (type == "moddedLunar")
            {
                foreach (var def in StageModifierCatalog.lunarRegisteredDefs)
                {
                    if (!StageModifierCatalog.lunarItemTiers.Keys.Contains(def.name))
                        usedDefs.Add(new DefInfo(def));
                }
                usedBanList = bannedLunarDefs;
                usedBanConfig = bannedLunarConfig;
                popoutController.moddedList = moddedLunarsInfo;
                popoutController.cycleOption = PopoutCycleOption.Negative;
                usedModList = moddedLunarsInfo;
                usedModConfig = moddedLunarsConfig;
            }

            popoutController.defList = usedDefs;
            popoutController.banList = usedBanList;
            popoutController.configString = usedBanConfig;
            popoutController.moddedList = usedModList;
            popoutController.moddedConfig = usedModConfig;

            popoutController.Populate();

        }

        //
        private static void AddToStats(On.RoR2.UI.GameEndReportPanelController.orig_SetDisplayData orig, GameEndReportPanelController self, GameEndReportPanelController.DisplayData newDisplayData)
        {
            orig(self, newDisplayData);

            var obj = self.gameObject; //GameEndReportPanel
            Transform contentTransform = self.transform.Find("SafeArea (JUICED)/BodyArea/StatsAndChatArea/StatsContainer/Stats Body/ScrollView/Viewport/Content");
            if (contentTransform != null) Log.Info("Found it");

            ModUIManager.routeMapPanel.transform.SetParent(contentTransform, false);
            Log.Info("Found it set");
        }


        private void Update()
        {
            if (!allowDebug.Value || !NetworkServer.active) return;

            if (Input.GetKeyDown(KeyCode.F2))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                RoutesExpansion.SpawnRoutePortals(transform.position);
            }
            //if (Input.GetKeyDown(KeyCode.F3))
            //{
            //    UnityHotReload.LoadNewAssemblyVersion(
            //        typeof(RiskOfRoutes).Assembly,
            //        "C:/Users/vit/AppData/Roaming/r2modmanPlus-local/RiskOfRain2/profiles/TEstling/BepInEx/plugins/RiskOfRoutes.dll"
            //    );
            //    Log.Info("Done");
            //}

        }
    }
}



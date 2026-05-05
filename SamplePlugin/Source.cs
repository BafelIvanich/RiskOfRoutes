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
using UnityEngine;
using UnityEngine.Networking;
using UnityHotReloadNS;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RiskOfRoutes
{


    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(ItemAPI.PluginGUID)] //RiskofThunder-R2API_Items-1.0.8
    [BepInDependency(RecalculateStatsAPI.PluginGUID)] //RiskofThunder-R2API_RecalculateStats-1.6.6
    [BepInDependency("com.rune580.riskofoptions")] //Rune580-Risk_Of_Options-2.8.5
    [BepInDependency(R2API.LanguageAPI.PluginGUID)] //RiskofThunder-R2API_Language-1.1.0
    [BepInDependency(R2API.PrefabAPI.PluginGUID)] //RiskofThunder-R2API_Prefab-1.1.1




    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.1.0";

        public enum MapOrientation { Vertical, Horizontal }
        public enum ItemPoolOptions { Default, VanillaEntire, Entire, None }
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
        public static ConfigEntry<int> routeMapWidth;

        public static ConfigEntry<bool> showIntermissionNodes;
        public static ConfigEntry<int> routeMapSize { get; set; }
        public static ConfigEntry<int> routeMapIconSize { get; set; }
        public static ConfigEntry<bool> routePanelShowsOnlyNames { get; set; }
        public static ConfigEntry<MapOrientation> punishmentBarOrientation { get; set; }
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
        public static ConfigEntry<int> tier2ElitesStackPercent;
        //printer item
        public static ConfigEntry<ItemPoolOptions> printerItemPool;
        public static ConfigEntry<string> tier1PrinterItems;
        public static ConfigEntry<string> tier2PrinterItems;
        public static ConfigEntry<string> tier3PrinterItems;
        //enemy item
        public static ConfigEntry<ItemPoolOptions> enemyItemPool;
        public static ConfigEntry<bool> canAppearWithAIBlacklisted;
        public static ConfigEntry<string> tier1EnemyItems;
        public static ConfigEntry<string> tier2EnemyItems;
        public static ConfigEntry<string> tier3EnemyItems;
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

            ModUIManager.Initialize();

            //hooks
            RoR2.RoR2Application.onLoad += () =>
            {
                StageModifierCatalog.Init(bundle);
            };

            //

            MysteryArtifact.InitializeArtifact();
            RoutesExpansion.InitializeExpansion(CentralNetworkObject);
            SoulCostController.Initialize();

            Sprite icon = AssetManager.modIcon;
            ModSettingsManager.SetModIcon(icon);
            ModSettingsManager.SetModDescription("Risk Of Routes. Mod that adds routing system and run map to the game.");
        }

        private void BindConfig()
        {

            //GENERAL //////////////////////////////////////////////////////
            useUniqueDestinations = Config.Bind<bool>(
                "General", "Unique Destinations", true, "Makes each route portal have unique scene as destination, otherwise they might repeat.");
            ModSettingsManager.AddOption(new CheckBoxOption(useUniqueDestinations));

            teleporterUseRandomMods = Config.Bind<bool>(
                "General", "Teleporter Use Random mods", true, "Makes the teleporter use random modifiers of spawned route portals after interacting with it, set false if you want it to not have any stage modifiers.");
            ModSettingsManager.AddOption(new CheckBoxOption(teleporterUseRandomMods));
            //todo
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

            //VISUAL ///////////////////////////////////////////////////////
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
                "Visual", "Route Map Panel Size", 90, "Size of Route Map panel in percents.");
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

            usePortalMaterial = Config.Bind<bool>(
                "Visual", "WIP Use Scene Material", false, "[WIP] Sets destination scene portrait as a portal texture.");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalMaterial));

            //MODIFIER OPTIONS /////////////////////////////////////////////////////////////////
            droneStackPercent = Config.Bind<int>(
                "Modifier Options", "Drone Increase", 33, "Percent per stack of initial scene director budget, used only to spawn additional drones. By default it's 33%.");
            ModSettingsManager.AddOption(new IntFieldOption(droneStackPercent));

            elitesStackPercent = Config.Bind<int>(
                "Modifier Options", "Elite Increase", 33, "Percent of elite discount for director. By default it's 33%.");
            ModSettingsManager.AddOption(new IntFieldOption(elitesStackPercent));

            doppelStackTime = Config.Bind<int>(
                "Modifier Options", "Doppelganger Cooldown Decrease", 30, "Cooldown reduction with one stack in seconds. By default it's 30s.");
            ModSettingsManager.AddOption(new IntFieldOption(doppelStackTime));

            lunarStackPercent = Config.Bind<int>(
                "Modifier Options", "Lunar Enemies Discount", 15, "Discount on lunar enemies in percents for more than one stack. By default it's 15%.");
            ModSettingsManager.AddOption(new IntFieldOption(lunarStackPercent));

            voidStackPercent = Config.Bind<int>(
                "Modifier Options", "Void Enemies Discount", 15, "Discount on lunar enemies in percents for more than one stack. By default it's 15%.");
            ModSettingsManager.AddOption(new IntFieldOption(voidStackPercent));

            soulCostStackPercent = Config.Bind<int>(
                "Modifier Options", "Soul Cost Increase", 2, "Percents added to hp curse for stack. By default it's 2%.");
            ModSettingsManager.AddOption(new IntFieldOption(soulCostStackPercent));

            tier2ElitesStackPercent = Config.Bind<int>(
                "Modifier Options", "Post-Loop Elites Increase", 15, "Discount on tier2 elites in percents for more than one stack. By default it's 15%.");
            ModSettingsManager.AddOption(new IntFieldOption(tier2ElitesStackPercent));

            // PRINTER ITEM MODIFIER/////////////////////////////////////////////////////////////////
            printerItemPool = Config.Bind<ItemPoolOptions>(
                "Printer Items", "Printer Item Pool", ItemPoolOptions.Default, "The configuration of what items printer modifier can have by modifier tier, use fields below to add other items from vanilla or mods to configuration.");
            ModSettingsManager.AddOption(new ChoiceOption(printerItemPool));

            tier1PrinterItems = Config.Bind<string>(
                "Printer Items", "Add Tier 1 Items", "", "Adds item defs separated by comma to a printer modifier as Tier1. ");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier1PrinterItems));

            tier2PrinterItems = Config.Bind<string>(
                "Printer Items", "Add Tier 2 Items", "", "");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier2PrinterItems));

            tier3PrinterItems = Config.Bind<string>(
                "Printer Items", "Add Tier 3 Items", "", "");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier3PrinterItems));

            // ENEMY ITEM MODIFIER/////////////////////////////////////////////////////////////////
            enemyItemPool = Config.Bind<ItemPoolOptions>(
                "Enemy Items", "Enemy Item Pool", ItemPoolOptions.Default, "");
            ModSettingsManager.AddOption(new ChoiceOption(enemyItemPool));

            canAppearWithAIBlacklisted = Config.Bind<bool>(
                "Enemy Items", "Allow AI Blacklisted", false, ".");
            ModSettingsManager.AddOption(new CheckBoxOption(canAppearWithAIBlacklisted));

            enemyItemsAllowBoss = Config.Bind<bool>(
    "Enemy Items", "Allow Boss Tier", false, "Adds items with boss tier to enemy item pool.");
            ModSettingsManager.AddOption(new CheckBoxOption(enemyItemsAllowBoss));

            enemyItemsAllowVoid = Config.Bind<bool>(
                "Enemy Items", "Allow Void Tiers", false, "Adds items with void tiers to enemy item pool.");
            ModSettingsManager.AddOption(new CheckBoxOption(enemyItemsAllowVoid));

            tier1EnemyItems = Config.Bind<string>(
                "Enemy Items", "Add Tier 1 Items", "", "Adds item defs separated by comma to an enemy item modifier as Tier1. \nItems from default configuration for Tier1 are:\nSlowOnHit - Chronobauble\nFlatHealth - Bison Steak\nBarrierOnCooldown - Eclipse Lite\nNearbyDamageBonus - Focus Crystal\nOutOfCombatArmor - Oddly-shaped Opal\nPersonalShield - Personal Shield Generator\nHoof - Paul's Goat Hoof\nShieldBooster - Kinetic Dampener");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier1EnemyItems));

            tier2EnemyItems = Config.Bind<string>(
                "Enemy Items", "Add Tier 2 Items", "", "Adds item defs separated by comma to an enemy item modifier as Tier2. \nItems from default configuration for Tier2 are:\nSprintOutOfCombat - Red Whip\nFragileDamageBonus - Delicate Watch\nCritGlasses - Lens-Maker's Glasses\nSyringe - Soldier's Syringe\nArmorPlate - Repulsion Armor Plate");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier2EnemyItems));

            tier3EnemyItems = Config.Bind<string>(
                "Enemy Items", "Add Tier 3 Items", "", "Adds item defs separated by comma to an enemy item modifier as Tier3. \nItems from default configuration for Tier3 are:\nBehemoth - Brilliant Behemoth\nImmuneToDebuff - Ben's Raincoat\nExtraLife - Dio's Best Friend\nArmorReductionOnHit - Shattering Justice\nBear - Tougher Times");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier3EnemyItems));

            enemyItemsModifierCanStack = Config.Bind<bool>(
                "Enemy Items", "Modifier can stack", false, "Allows enemy item modifier to be stacked, so enemies can have more than one default stack for item.\nDefault amount for stack is:\nCommon - x5,\nUncommon - x3,\nLegendary - x1.");
            ModSettingsManager.AddOption(new CheckBoxOption(enemyItemsModifierCanStack));

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
                "Enemy Items", "Boss Default Stack", 1, "Default amount per stack. Note that the default configuration doesn't include any boss items by default.");
            ModSettingsManager.AddOption(new IntFieldOption(bossDefaultStack));

            chanceForExtra = Config.Bind<int>(
                "Enemy Items", "Chance For Extra", 0, "Chance for enemy items to have extra stack.");
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
                "Enemy Items", "Boss Extra Range", 1, "Default amount per stack. Note that the default configuration doesn't include any boss items by default.");
            ModSettingsManager.AddOption(new IntFieldOption(bossExtraRange));

            // LUNAR ITEM MODIFIER/////////////////////////////////////////////////////////////////
            lunarItemPool = Config.Bind<ItemPoolOptions>(
                "Lunar Items", "Lunar Item Pool", ItemPoolOptions.Default, "The configuration of what items lunar item modifier can have by modifier tier, use fields below to add other items from vanilla or mods. Disable if you want it to only use defs from fields below. Accepts only lunar itemDefs");
            ModSettingsManager.AddOption(new ChoiceOption(lunarItemPool));

            tier1LunarItems = Config.Bind<string>(
                "Lunar Items", "Add Tier 1 Items", "", "Adds item defs separated by comma to a lunar item modifier as Tier1. \nItems from default configuration for Tier1 are:\nGoldOnHit - Brittle Crown\nMonstersOnShrineUse - Defiant Gouge\nFocusConvergence - Focused Convergence");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier1LunarItems));

            tier2LunarItems = Config.Bind<string>(
                "Lunar Items", "Add Tier 2 Items", "", "Adds item defs separated by comma to a lunar item modifier as Tier2. \nItems from default configuration for Tier2 are:\nRepeatHeal - Corpsebloom\nHalfAttackSpeedHalfCooldowns - Light Flux Pauldron\nShieldOnly - Transcendence");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier2LunarItems));

            tier3LunarItems = Config.Bind<string>(
                "Lunar Items", "Add Tier 3 Items", "", "Adds item defs separated by comma to a lunar item modifier as Tier3. \nItems from default configuration for Tier3 are:\nLunarBadLuck - Purity\nHalfSpeedDoubleHealth - Stone Flux Pauldron");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier3LunarItems));

            lunarTurnToPearl = Config.Bind<bool>(
                "Lunar Items", "Purify At The End", true, "By default the lunar item given by the modifier is turned into a pearl at the end of the stage.");
            ModSettingsManager.AddOption(new CheckBoxOption(lunarTurnToPearl));

            //ARTIFACT MODIFIER
            tier1Artifacts = Config.Bind<string>(
                "Artifacts", "Add Tier 1 Artifacts", "", "Adds artifactDefs separated by comma to the Artifact Modifier pool, use it to add artifacts from other mods.");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier1Artifacts));

            tier2Artifacts = Config.Bind<string>(
                "Artifacts", "Add Tier 2 Artifacts", "", "Adds artifactDefs separated by comma to the Artifact Modifier pool, use it to add artifacts from other mods.");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier2Artifacts));

            tier3Artifacts = Config.Bind<string>(
                "Artifacts", "Add Tier 3 Artifacts", "", "Adds artifactDefs separated by comma to the Artifact Modifier pool, use it to add artifacts from other mods.");
            ModSettingsManager.AddOption(new StringInputFieldOption(tier3Artifacts));

            allowMysteryArtifact = Config.Bind<bool>(
                "Artifacts", "Mystery", true, "Mystery artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMysteryArtifact));

            allowChaosArtifact = Config.Bind<bool>(
               "Artifacts", "Chaos", true, "Chaos artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowChaosArtifact));

            allowDeathArtifact = Config.Bind<bool>(
                "Artifacts", "Death", true, "Death artifact available as artifact stage modifier. Appears if run is launched in multiplayer.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDeathArtifact));

            allowDissonanceArtifact = Config.Bind<bool>(
                "Artifacts", "Dissonance", true, "Dissonance artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDissonanceArtifact));

            allowFrailtyArtifact = Config.Bind<bool>(
                "Artifacts", "Frailty", true, "Frailty artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowFrailtyArtifact));

            allowHonorArtifact = Config.Bind<bool>(
                "Artifacts", "Honor", true, "Honor artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowHonorArtifact));

            allowSacrificeArtifact = Config.Bind<bool>(
                "Artifacts", "Sacrifice", true, "Sacrifice artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSacrificeArtifact));

            allowSoulArtifact = Config.Bind<bool>(
                "Artifacts", "Soul", true, "Soul artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSoulArtifact));

            allowSpiteArtifact = Config.Bind<bool>(
                "Artifacts", "Spite", true, "Spite artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSpiteArtifact));

            allowSwarmsArtifact = Config.Bind<bool>(
                "Artifacts", "Swarms", true, "Swarms artifact available as artifact stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSwarmsArtifact));

            allowVengeanceArtifact = Config.Bind<bool>(
                "Artifacts", "Vengeance", false, "Vengeance artifact available as artifact stage modifier. Not intended to be available, replaced by Doppelganger modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowVengeanceArtifact));

            allowCommandArtifact = Config.Bind<bool>(
                "Artifacts", "Command", false, "Command artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowCommandArtifact));

            allowDelusionArtifact = Config.Bind<bool>(
                "Artifacts", "Delusion", false, "Delusion artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDelusionArtifact));

            allowDevotionArtifact = Config.Bind<bool>(
                "Artifacts", "Devotion", false, "Devotion artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDevotionArtifact));

            allowEnigmaArtifact = Config.Bind<bool>(
                "Artifacts", "Enigma", false, "Enigma artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowEnigmaArtifact));

            allowEvolutionArtifact = Config.Bind<bool>(
                "Artifacts", "Evolution", false, "Evolution artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowEvolutionArtifact));

            allowGlassArtifact = Config.Bind<bool>(
                "Artifacts", "Glass", false, "Glass artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowGlassArtifact));

            allowKinArtifact = Config.Bind<bool>(
                "Artifacts", "Kin", false, "Kin artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowKinArtifact));

            allowMetamorphosisArtifact = Config.Bind<bool>(
                "Artifacts", "Metamorphosis", false, "Metamorphosis artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMetamorphosisArtifact));

            allowRebirthArtifact = Config.Bind<bool>(
                "Artifacts", "Rebirth", false, "Rebirth artifact available as artifact stage modifier. Not intended to be available.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowRebirthArtifact));


            // STAGE MODIFIER TOGGLES /////////////////////////////////////////////////////////
            allowAurelioniteModifier = Config.Bind<bool>(
                "Stage Modifiers", "Aurelionite", true, "Aurelionite reward available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowAurelioniteModifier));

            allowBossRedItemModifier = Config.Bind<bool>(
                "Stage Modifiers", "Boss Red Item", true, "Boss Red Item available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowBossRedItemModifier));

            allowBossYellowItemModifier = Config.Bind<bool>(
                "Stage Modifiers", "Boss Yellow Item", true, "Boss Yellow Item available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowBossYellowItemModifier));

            allowDroneBossGreenModifier = Config.Bind<bool>(
                "Stage Modifiers", "Drone Boss Green", true, "Drone Boss Green available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDroneBossGreenModifier));

            allowDroneBossRedModifier = Config.Bind<bool>(
                "Stage Modifiers", "Drone Boss Red", true, "Drone Boss Red available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDroneBossRedModifier));

            allowMoreElitesModifier = Config.Bind<bool>(
                "Stage Modifiers", "More Elites", true, "More Elites available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMoreElitesModifier));

            allowMoreDronesModifier = Config.Bind<bool>(
                "Stage Modifiers", "More Drones", true, "More Drones available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMoreDronesModifier));

            allowUpgradeDronesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Upgrade Drones", true, "Upgrade Drones available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowUpgradeDronesModifier));

            allowLunarEnemiesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Lunar Enemies", true, "Lunar Enemies available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowLunarEnemiesModifier));

            allowVoidEnemiesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Void Enemies", true, "Void Enemies available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowVoidEnemiesModifier));

            allowYellowPrinterModifier = Config.Bind<bool>(
                "Stage Modifiers", "Yellow Printers", true, "Yellow Printer available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowYellowPrinterModifier));

            allowRedPrinterModifier = Config.Bind<bool>(
                "Stage Modifiers", "Red Printers", true, "Red Printer available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowRedPrinterModifier));

            allowOnlyFlyingModifier = Config.Bind<bool>(
                "Stage Modifiers", "Only Flying", true, "Only Flying enemies available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowOnlyFlyingModifier));

            allowPowerfulElitesModifier = Config.Bind<bool>(
                "Stage Modifiers", "Post-Loop Elites", true, "Earlier Tier2 Elites available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowPowerfulElitesModifier));

            allowRepairModifier = Config.Bind<bool>(
                "Stage Modifiers", "Repair Items", true, "Repair Items available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowRepairModifier));

            allowWanderingChefModifier = Config.Bind<bool>(
                "Stage Modifiers", "Wandering Chef", true, "Wandering Chef available as a stage modifier. why would you even disable it???");
            ModSettingsManager.AddOption(new CheckBoxOption(allowWanderingChefModifier));

            allowMountainModifier = Config.Bind<bool>(
                "Stage Modifiers", "Mountain Shrine", true, "Mountain Shrine available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMountainModifier));

            allowSoulCostModifier = Config.Bind<bool>(
                "Stage Modifiers", "Soul Cost", true, "Soul Cost available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSoulCostModifier));

            allowDoppelgangerModifier = Config.Bind<bool>(
                "Stage Modifiers", "Doppelganger", true, "Doppelganger available as a stage modifier.");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDoppelgangerModifier));
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

            ////
        }


    }
}



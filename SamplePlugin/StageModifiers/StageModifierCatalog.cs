using RiskOfRoutes.StageModifiers.CombatModifiers;
using RiskOfRoutes.StageModifiers.OtherModifiers;
using RiskOfRoutes.StageModifiers.SceneModifiers;
using RoR2;
using SamplePlugin;
using SamplePlugin.StageModifiers;
using SamplePlugin.StageModifiers.BossRewardModifiers;
using SamplePlugin.StageModifiers.CombatModifiers;
using SamplePlugin.StageModifiers.OtherModifiers;
using SamplePlugin.StageModifiers.SceneModifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace RiskOfRoutes.StageModifiers
{
    public class ArtifactInfo
    {
        public string name;
        public int tier;
        public bool isNegative;

        public ArtifactInfo(string name, int tier, bool isNegative)
        {
            this.name = name;
            this.tier = tier;
            this.isNegative = isNegative;
        }
    }

    public class LunarItemInfo
    {
        public string name;
        public int tier;
        public bool isDefault;

        public LunarItemInfo(string name, int tier, bool isDefault)
        {
            this.name = name;
            this.tier = tier;
            this.isDefault = isDefault;
        }
    }

    public class ItemInfo
    {
        public int printerTier;
        public bool isPrinterDefault;
        public int enemyTier;
        public bool isEnemyDefault;

        public ItemInfo(int printerTier, bool isPrinterDefault, int enemyTier, bool isEnemyDefault)
        {
            this.printerTier = printerTier;
            this.isPrinterDefault = isPrinterDefault;
            this.enemyTier = enemyTier;
            this.isEnemyDefault = isEnemyDefault;
        }
    }

    public static class StageModifierCatalog
    {
        private static AssetBundle assetBundle;

        public static Dictionary<string, ModifierDef> AllDefs = new Dictionary<string, ModifierDef>();

        public static List<string> generalPositivePool = new List<string>();
        public static List<string> generalNegativePool = new List<string>();
        public static List<string> droneTypePool = new List<string>();
        public static List<string> rarePool = new List<string>();
        public static List<string> chefPool = new List<string>();

        public static List<string> combatPool = new List<string>();
        public static List<string> healPool = new List<string>();
        public static List<string> utilityPool = new List<string>();


        public static List<string> itemTypePool = new List<string>();
        public static List<string> enemyItemPool = new List<string>();
        public static List<string> lunarItemPool = new List<string>();

        public static List<string> additionalDefs = new List<string>();

        public static readonly List<string> AllDefNames = new List<string>
        {
        "Aurelionite",
        "BossRedItem",
        "BossYellowItem",
        "DroneBossGreen",
        "DroneBossRed",
        "MoreElites",
        "MoreDrones",
        "UpgradeDrones",
        "LunarEnemies",
        "VoidEnemies",
        "YellowPrinter",
        "RedPrinter",
        "OnlyFlying",
        "PowerfulElites",
        "Repair",
        "WanderingChef",
        "Mountain",
        "SoulCost",
        "Doppelganger",

        "FriendlyFire",
        "Command",
        "Delusion",
        "Devotion",
        "MixEnemy",
        "Enigma",
        "MonsterTeamGainsItems",
        "WeakAssKnees",
        "Glass",
        "EliteOnly",
        "SingleMonsterType",
        "RandomSurvivorOnRespawn",
        "Rebirth",
        "Sacrifice",
        "WispOnDeath",
        "Bomb",
        "Swarms",
        "ShadowClone",
        "TeamDeath",
        "Mystery",
        };

        public static readonly Dictionary<string, ItemInfo> vanillaItemsInfo = new Dictionary<string, ItemInfo>()
        {
            //tiers based on how much they affect the run, default includes in default configuration
            //whites
            {"BossDamageBonus", new ItemInfo(2, true, 1, false)},
            {"SecondarySkillMagazine", new ItemInfo(1, true, 1, false)},
            {"FlatHealth", new ItemInfo(1, true, 1, true)},
            {"AttackSpeedPerNearbyAllyOrEnemy", new ItemInfo(2, true, 2, true)},
            {"Firework", new ItemInfo(1, true, 1, false)},
            {"Mushroom", new ItemInfo(1, true, 1, false)},
            {"HealWhileSafe", new ItemInfo(1, false, 1, false)},
            {"IncreaseDamageOnMultiKill", new ItemInfo(2, true, 1, false)},
            {"Crowbar", new ItemInfo(2, true, 2, true)},
            {"FragileDamageBonus", new ItemInfo(2, true, 3, true)},
            {"BarrierOnCooldown", new ItemInfo(2, true, 2, true)},
            {"SpeedBoostPickup", new ItemInfo(1, true, 1, false)},
            {"SprintBonus", new ItemInfo(2, true, 2, true)},
            {"IgniteOnKill", new ItemInfo(1, true, 1, false)},
            {"CritAtLowerElevation", new ItemInfo(2, true, 2, true)},
            {"CritGlasses", new ItemInfo(3, true, 3, true)},
            {"Medkit", new ItemInfo(1, false, 1, true)},
            {"AttackSpeedAndMoveSpeed", new ItemInfo(2, true, 2, true)},
            {"Tooth", new ItemInfo(1, true, 1, false)},
            {"OutOfCombatArmor", new ItemInfo(1, false, 1, true)},
            {"Hoof", new ItemInfo(1, true, 2, true)},
            {"PersonalShield", new ItemInfo(1, true, 1, true)},
            {"HealingPotion", new ItemInfo(1, false, 2, true)},
            {"ArmorPlate", new ItemInfo(1, true, 2, true)},
            {"GoldOnHurt", new ItemInfo(1, false, 1, false)},
            {"TreasureCache", new ItemInfo(1, false, 1, false)},
            {"Syringe", new ItemInfo(3, true, 3, true)},
            {"StickyBomb", new ItemInfo(3, true, 3, true)},
            {"StunChanceOnHit", new ItemInfo(1, true, 1, false)},
            {"BarrierOnKill", new ItemInfo(2, true, 1, false)},
            {"Bear", new ItemInfo(3, true, 3, true)},
            {"BleedOnHit", new ItemInfo(3, true, 3, true)},
            {"WardOnLevel", new ItemInfo(1, true, 2, true)},
            {"DelayedDamage", new ItemInfo(1, true, 2, true)},
            //greens
            {"Missile", new ItemInfo(3, true, 3, true)},
            {"Bandolier", new ItemInfo(1, true, 1, false)},
            {"WarCryOnMultiKill", new ItemInfo(1, true, 1, false)},
            {"DronesDropDynamite", new ItemInfo(1, true, 1, false)},
            {"KnockBackHitEnemies", new ItemInfo(1, true, 1, false)},
            {"ExtraShrineItem", new ItemInfo(1, false, 1, false)},
            {"SlowOnHit", new ItemInfo(1, false, 2, true)},
            {"SpeedOnPickup", new ItemInfo(1, false, 1, false)},
            {"DeathMark", new ItemInfo(1, false, 1, false)},
            {"JumpDamageStrike", new ItemInfo(2, true, 2, true)},
            {"EquipmentMagazine", new ItemInfo(2, true, 1, false)},
            {"BonusGoldPackOnKill", new ItemInfo(1, false, 1, false)},
            {"HealOnCrit", new ItemInfo(1, false, 1, true)},
            {"Feather", new ItemInfo(2, true, 1, true)},
            {"MoveSpeedOnKill", new ItemInfo(2, true, 1, false)},
            {"StrengthenBurn", new ItemInfo(1, true, 1, false)},
            {"Infusion", new ItemInfo(2, true, 1, false)},
            {"ShieldBooster", new ItemInfo(1, true, 1, true)},
            {"FireRing", new ItemInfo(3, true, 3, true)},
            {"Seed", new ItemInfo(2, true, 2, true)},
            {"TPHealingNova", new ItemInfo(1, false, 1, false)},
            {"IncreasePrimaryDamage", new ItemInfo(1, false, 1, true)},
            {"TriggerEnemyDebuffs", new ItemInfo(1, false, 1, true)},
            {"ExecuteLowHealthElite", new ItemInfo(2, true, 2, true)},
            {"Phasing", new ItemInfo(1, false, 1, true)},
            {"ExtraStatsOnLevelUp", new ItemInfo(1, false, 1, false)},
            {"AttackSpeedOnCrit", new ItemInfo(2, true, 1, true)},
            {"Thorns", new ItemInfo(1, true, 3, false)},
            {"SprintOutOfCombat", new ItemInfo(2, true, 2, true)},
            {"RegeneratingScrap", new ItemInfo(1, false, 1, false)},
            {"SprintArmor", new ItemInfo(1, false, 1, true)},
            {"IceRing", new ItemInfo(3, true, 3, true)},
            {"LowerPricedChests", new ItemInfo(3, false, 1, false)},
            {"FreeChest", new ItemInfo(2, false, 1, false)},
            {"PrimarySkillShuriken", new ItemInfo(2, true, 2, true)},
            {"Squid", new ItemInfo(1, false, 1, false)},
            {"ChainLightning", new ItemInfo(2, true, 1, true)},
            {"TeleportOnLowHealth", new ItemInfo(1, false, 3, false)},
            {"EnergizedOnEquipmentUse", new ItemInfo(2, true, 1, false)},
            {"JumpBoost", new ItemInfo(2, true, 1, false)},
            {"ExplodeOnDeath", new ItemInfo(2, true, 1, false)},
            //reds
            {"Clover", new ItemInfo(3, false, 1, false)},
            {"BarrierOnOverHeal", new ItemInfo(1, false, 1, false)},
            {"AlienHead", new ItemInfo(2, false, 3, true)},
            {"ImmuneToDebuff", new ItemInfo(1, false, 3, true)},
            {"RandomEquipmentTrigger", new ItemInfo(1, false, 1, false)},
            {"KillEliteFrenzy", new ItemInfo(1, false, 1, false)},
            {"Behemoth", new ItemInfo(1, false, 3, true)},
            {"Dagger", new ItemInfo(1, false, 1, false)},
            {"CaptainDefenseMatrix", new ItemInfo(1, false, 3, false)},
            {"ExtraLife", new ItemInfo(1, false, 3, true)},
            {"StunAndPierce", new ItemInfo(1, false, 1, true)},
            {"Icicle", new ItemInfo(1, false, 1, false)},
            {"BoostAllStats", new ItemInfo(1, false, 1, false)},
            {"FallBoots", new ItemInfo(1, false, 1, false)},
            {"GhostOnKill", new ItemInfo(1, false, 1, false)},
            {"UtilitySkillMagazine", new ItemInfo(1, false, 1, false)},
            {"Plant", new ItemInfo(1, false, 1, false)},
            {"ScrapRed", new ItemInfo(1, false, 1, false)},
            {"CritDamage", new ItemInfo(1, false, 1, false)},
            {"NovaOnHeal", new ItemInfo(1, false, 1, false)},
            {"SharedSuffering", new ItemInfo(1, false, 3, true)},
            {"PhysicsProjectile", new ItemInfo(1, false, 3, false)},
            {"MoreMissile", new ItemInfo(1, false, 1, false)},
            {"IncreaseHealing", new ItemInfo(1, false, 2, true)},
            {"LaserTurbine", new ItemInfo(1, false, 1, false)},
            {"MeteorAttackOnHighDamage", new ItemInfo(1, false, 1, false)},
            {"BounceNearby", new ItemInfo(1, false, 1, true)},
            {"ArmorReductionOnHit", new ItemInfo(1, false, 3, true)},
            {"ItemDropChanceOnKill", new ItemInfo(1, false, 1, false)},
            {"Talisman", new ItemInfo(1, false, 1, false)},
            {"DroneWeapons", new ItemInfo(1, false, 3, false)},
            {"Duplicator", new ItemInfo(1, false, 1, false)},
            {"PermanentDebuffOnHit", new ItemInfo(1, false, 3, false)},
            {"ShockNearby", new ItemInfo(1, false, 3, false)},
            {"HeadHunter", new ItemInfo(1, false, 1, false)},
            {"BarrageOnBoss", new ItemInfo(1, false, 1, false)},
            //yellows
            {"ArtifactKey", new ItemInfo(1, false, 1, false)},
            {"LightningStrikeOnHit", new ItemInfo(1, false, 3, true)},
            {"MinorConstructOnKill", new ItemInfo(1, false, 1, false)},
            {"RoboBallBuddy", new ItemInfo(1, false, 3, false)},
            {"MasterBattery", new ItemInfo(1, false, 1, false)},
            {"MasterCore", new ItemInfo(1, false, 1, false)},
            {"ShockDamageAura", new ItemInfo(1, false, 3, true)},
            {"ExtraEquipment", new ItemInfo(1, false, 1, false)},
            {"NovaOnLowHealth", new ItemInfo(1, false, 3, true)},
            {"TitanGoldDuringTP", new ItemInfo(1, false, 1, false)},
            {"ShinyPearl", new ItemInfo(1, false, 1, true)},
            {"ScrapYellow", new ItemInfo(1, false, 1, true)},
            {"SprintWisp", new ItemInfo(1, false, 3, true)},
            {"SiphonOnLowHealth", new ItemInfo(1, false, 3, false)},
            {"FireballsOnHit", new ItemInfo(1, false, 3, true)},
            {"Pearl", new ItemInfo(1, false, 1, true)},
            {"ParentEgg", new ItemInfo(1, false, 3, true)},
            {"PowerCube", new ItemInfo(1, false, 3, false)},
            {"BeetleGland", new ItemInfo(1, false, 1, false)},
            {"PowerPyramid", new ItemInfo(1, false, 1, false)},
            {"BleedOnHitAndExplode", new ItemInfo(1, false, 1, false)},
            {"Knurl", new ItemInfo(1, false, 1, true)}
        };

        public static readonly Dictionary<string, LunarItemInfo> lunarItemTiers = new Dictionary<string, LunarItemInfo>()
        {
            {"LunarTrinket", new LunarItemInfo("LunarTrinket", 1, false)},
            {"GoldOnHit", new LunarItemInfo("GoldOnHit", 2, true)},
            {"RepeatHeal", new LunarItemInfo("RepeatHeal", 1, true)},
            {"MonstersOnShrineUse", new LunarItemInfo("MonstersOnShrineUse", 1, true)},
            {"LunarSun", new LunarItemInfo("LunarSun", 3, false)},
            {"LunarSpecialReplacement", new LunarItemInfo("LunarSpecialReplacement", 3, false)},
            {"RandomlyLunar", new LunarItemInfo("RandomlyLunar", 1, false)},
            {"FocusConvergence", new LunarItemInfo("FocusConvergence", 1, true)},
            {"AutoCastEquipment", new LunarItemInfo("AutoCastEquipment", 1, false)},
            {"LunarSecondaryReplacement", new LunarItemInfo("LunarSecondaryReplacement", 3, false)},
            {"HalfAttackSpeedHalfCooldowns", new LunarItemInfo("HalfAttackSpeedHalfCooldowns", 3, true)},
            {"OnLevelUpFreeUnlock", new LunarItemInfo("OnLevelUpFreeUnlock", 3, false)},
            {"RandomDamageZone", new LunarItemInfo("RandomDamageZone", 1, false)},
            {"TransferDebuffOnHit", new LunarItemInfo("TransferDebuffOnHit", 1, false)},
            {"LunarBadLuck", new LunarItemInfo("LunarBadLuck", 3, true)},
            {"LunarDagger", new LunarItemInfo("LunarDagger", 3, true)},
            {"HalfSpeedDoubleHealth", new LunarItemInfo("HalfSpeedDoubleHealth", 3, true)},
            {"LunarUtilityReplacement", new LunarItemInfo("LunarUtilityReplacement", 3, true)},
            {"ShieldOnly", new LunarItemInfo("ShieldOnly", 3, true)},
            {"LunarPrimaryReplacement", new LunarItemInfo("LunarPrimaryReplacement", 3, false)}
        };

        public static readonly Dictionary<string, ArtifactInfo> artifactTiers = new Dictionary<string, ArtifactInfo>
        {
            { "Mystery", new ArtifactInfo("Mystery", 2, isNegative: true) },
            { "FriendlyFire", new ArtifactInfo("FriendlyFire", 1, isNegative: true) },
            { "Command", new ArtifactInfo("Command", 3, isNegative: false) },
            { "Delusion", new ArtifactInfo("Delusion", 3, isNegative: true) },
            { "Devotion", new ArtifactInfo("Devotion", 2, isNegative: true) },
            { "MixEnemy", new ArtifactInfo("MixEnemy", 2, isNegative: true) },
            { "Enigma", new ArtifactInfo("Enigma", 3, isNegative: true) },
            { "MonsterTeamGainsItems", new ArtifactInfo("MonsterTeamGainsItems", 1, isNegative: true) },
            { "WeakAssKnees", new ArtifactInfo("WeakAssKnees", 1, isNegative: true) },
            { "Glass", new ArtifactInfo("Glass", 3, isNegative: true) },
            { "EliteOnly", new ArtifactInfo("EliteOnly", 3, isNegative: true) },
            { "SingleMonsterType", new ArtifactInfo("SingleMonsterType", 2, isNegative: true) },
            { "RandomSurvivorOnRespawn", new ArtifactInfo("RandomSurvivorOnRespawn", 3, isNegative: true) },
            { "Rebirth", new ArtifactInfo("Rebirth", 1, isNegative: false) },
            { "Sacrifice", new ArtifactInfo("Sacrifice", 3, isNegative: true) },
            { "WispOnDeath", new ArtifactInfo("WispOnDeath", 1, isNegative: true) },
            { "Bomb", new ArtifactInfo("Bomb", 1, isNegative: true) },
            { "Swarms", new ArtifactInfo("Swarms", 1, isNegative: true) },
            { "ShadowClone", new ArtifactInfo("ShadowClone", 1, isNegative: true) },
            { "TeamDeath", new ArtifactInfo("TeamDeath", 2, isNegative: true) }
         };


        public static void Init(AssetBundle bundle)
        {
            assetBundle = bundle;

            AllDefs.Clear();
            generalPositivePool.Clear();
            generalNegativePool.Clear();
            itemTypePool.Clear();
            enemyItemPool.Clear();
            lunarItemPool.Clear();
            rarePool.Clear();
            droneTypePool.Clear();
            chefPool.Clear();
            combatPool.Clear();
            healPool.Clear();
            utilityPool.Clear();
            additionalDefs.Clear();

            generalPositivePool.AddRange(new string[] { });
            generalNegativePool.AddRange(new string[] { "Doppelganger", "SoulCost", "VoidEnemies", "MoreElites", "LunarEnemies", "PowerfulElites", "OnlyFlying" });

            itemTypePool.AddRange(new string[] { "YellowPrinter", "RedPrinter" });

            combatPool.AddRange(new string[] { "YellowPrinter_Damage", "RedPrinter_Damage" });
            healPool.AddRange(new string[] { "YellowPrinter_Healing", "RedPrinter_Healing" });
            utilityPool.AddRange(new string[] { "YellowPrinter_Utility", "RedPrinter_Utility" });

            droneTypePool.AddRange(new string[] { "DroneBossGreen", "DroneBossRed", "UpgradeDrones", "MoreDrones" });
            rarePool.AddRange(new string[] { "BossRedItem", "BossYellowItem" });
            chefPool.AddRange(new string[] { "RedPrinter_FoodRelated", "YellowPrinter_FoodRelated" });

            //artifacts
            List<string> moddedArtifacts = new List<string>();

            foreach (var artifact in RoR2.ArtifactCatalog.artifactDefs)
            {
                ArtifactInfo value = null;
                if (artifactTiers.TryGetValue(artifact.cachedName, out value))
                {
                    TryRegArtifact(value);
                }
                else
                {
                    var defaultInfo = new ArtifactInfo(artifact.cachedName, 1, true);
                    moddedArtifacts.Add(artifact.cachedName);
                }
            }

            Log.Warning($"{nameof(Init)}: Those modded artifacts can are not available, you can add them in config");
            foreach (var moddedArt in moddedArtifacts)
            {
                Log.Info($"\t{moddedArt}");
            }

            //items
            foreach (var itemDef in RoR2.ItemCatalog.itemDefs)
            {
                if (itemDef.tier == ItemTier.Lunar)
                {
                    if (lunarItemTiers.TryGetValue(itemDef.name, out LunarItemInfo info))
                    {
                        TryRegLunar(itemDef.name, (ModifierTier)info.tier);
                    }
                    else
                    {
                        TryRegLunar(itemDef.name, ModifierTier.Tier3);
                        Log.Info($"{nameof(Init)}: trying to register lunar {itemDef} as Tier3");
                    }
                }
                else
                {
                    if (vanillaItemsInfo.TryGetValue(itemDef.name, out ItemInfo info))
                    {
                        TryRegPrinter(itemDef.name, (ModifierTier)info.printerTier);
                        TryRegEnemy(itemDef.name, (ModifierTier)info.enemyTier);
                    }
                    else
                    {
                        TryRegPrinter(itemDef.name, ModifierTier.Tier1);
                        Log.Info($"{nameof(Init)}: trying to register printer {itemDef} as Tier1");
                        TryRegEnemy(itemDef.name, ModifierTier.Tier1);
                        Log.Info($"{nameof(Init)}: trying to register enemy item {itemDef} as Tier1");
                    }
                }
            }

            Register(new ModifierDef("Aurelionite", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false), "texBuffAurelioniteBlessingIcon");
            Register(new ModifierDef("BossRedItem", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false, new string[] { "BossYellowItem", "DroneBossGreen" }), "redBossItem");
            Register(new ModifierDef("BossYellowItem", ModifierTier.Tier2, 1f, isStackable: false, isNegative: false, new string[] { "BossRedItem", "DroneBossGreen" }), "yellowBossItem");
            Register(new ModifierDef("DroneBossGreen", ModifierTier.Tier1, 1f, isStackable: false, isNegative: false, new string[] { "DroneBossRed" }), "uncommonDroneReward");
            Register(new ModifierDef("DroneBossRed", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false, new string[] { "DroneBossGreen" }), "legendaryDroneReward");

            Register(new ModifierDef("MoreElites", ModifierTier.Tier1, 1f, isStackable: true, isNegative: true), "moreElitesIcon");
            Register(new ModifierDef("MoreDrones", ModifierTier.Tier1, 1f, isStackable: true, isNegative: false), "texMoreDronesModifier");
            Register(new ModifierDef("UpgradeDrones", ModifierTier.Tier3, 1f, isStackable: true, isNegative: false), "RoR2/DLC3/UI/texUIdroneupgradeIcon.png");
            Register(new ModifierDef("LunarEnemies", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveLunarIcon.png");
            Register(new ModifierDef("VoidEnemies", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveVoidIcon.png");

            Register(new ModifierDef("YellowPrinter", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), "yellowPrinter");
            Register(new ModifierDef("RedPrinter", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), "redPrinter");

            Register(new ModifierDef("YellowPrinter_Damage", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), "yellowPrinterDamage");
            Register(new ModifierDef("YellowPrinter_Healing", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), "yellowPrinterHeal");
            Register(new ModifierDef("YellowPrinter_Utility", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), "yellowPrinterUtility");
            Register(new ModifierDef("YellowPrinter_FoodRelated", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), "yellowPrinterFood");
            Register(new ModifierDef("RedPrinter_Damage", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), "redPrinterDamage");
            Register(new ModifierDef("RedPrinter_Healing", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), "redPrinterHealing");
            Register(new ModifierDef("RedPrinter_Utility", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), "redPrinterUtility");
            Register(new ModifierDef("RedPrinter_FoodRelated", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), "redPrinterFood");


            Register(new ModifierDef("OnlyFlying", ModifierTier.Tier2, 1f, isStackable: false, isNegative: true), "onlyFlying");
            Register(new ModifierDef("PowerfulElites", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), "tier2ElitesIcon");
            Register(new ModifierDef("Repair", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckPlayersHaveConsumedItems() ?? false, isStackable: false, isNegative: false), "repair");
            Register(new ModifierDef("WanderingChef", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false), "RoR2/DLC3/MealPrep/texMealPrepIcon.png");

            Register(new ModifierDef("Mountain", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckMountainShrine() ?? false, isStackable: false, isNegative: false), "mountIcon");
            Register(new ModifierDef("SoulCost", ModifierTier.Tier2, 1f, isStackable: true, isNegative: true), "RoR2/DLC2/texBuffSoulCostIcon.png");
            Register(new ModifierDef("Doppelganger", ModifierTier.Tier2, 1f, isStackable: true, isNegative: true), "texAurelioniteIcon");
        }

        public static void CreateRunPools(RiskOfRoutes.ItemPoolOptions printer, RiskOfRoutes.ItemPoolOptions lunar, RiskOfRoutes.ItemPoolOptions enemy, bool allowAIBlacklisted, bool allowBoss, bool allowVoid, HashSet<string> bannedRun)
        {
            generalNegativePool.Clear();
            itemTypePool.Clear();
            enemyItemPool.Clear();
            lunarItemPool.Clear();
            rarePool.Clear();
            droneTypePool.Clear();
            chefPool.Clear();
            combatPool.Clear();
            healPool.Clear();
            utilityPool.Clear();
            additionalDefs.Clear();

            generalPositivePool.AddRange(new string[] { });
            generalNegativePool.AddRange(new string[] { "Doppelganger", "SoulCost", "VoidEnemies", "MoreElites", "LunarEnemies", "PowerfulElites", "OnlyFlying" });

            itemTypePool.AddRange(new string[] { "YellowPrinter", "RedPrinter" });

            combatPool.AddRange(new string[] { "YellowPrinter_Damage", "RedPrinter_Damage" });
            healPool.AddRange(new string[] { "YellowPrinter_Healing", "RedPrinter_Healing" });
            utilityPool.AddRange(new string[] { "YellowPrinter_Utility", "RedPrinter_Utility" });

            droneTypePool.AddRange(new string[] { "DroneBossGreen", "DroneBossRed", "UpgradeDrones", "MoreDrones" });
            rarePool.AddRange(new string[] { "BossRedItem", "BossYellowItem" });
            chefPool.AddRange(new string[] { "RedPrinter_FoodRelated", "YellowPrinter_FoodRelated" });


            //artifact
            foreach (var artDef in AllDefs.Values.Where(def => def.isArtifact))
            {
                if (!artifactTiers.ContainsKey(artDef.name)) continue;

                if (bannedRun.Contains(artDef.name)) continue;
                if (artDef.isNegative) generalNegativePool.Add(artDef.name);
                else rarePool.Add(artDef.name);
            }

            //printer
            if (printer != RiskOfRoutes.ItemPoolOptions.None)
            {
                var filtered = RoR2.ItemCatalog.allItemDefs.Where(def =>
                (def.tier == ItemTier.Tier1) ||
                (def.tier == ItemTier.Tier2) &&
                def.DoesNotContainTag(ItemTag.CannotDuplicate)).ToList(); //entire

                if (printer == RiskOfRoutes.ItemPoolOptions.VanillaEntire) filtered = filtered.Where(def => vanillaItemsInfo.ContainsKey(def.name)).ToList();
                else if (printer == RiskOfRoutes.ItemPoolOptions.Default)
                {
                    filtered = filtered.Where(def => vanillaItemsInfo.TryGetValue(def.name, out ItemInfo info) && info.isPrinterDefault).ToList(); //todo
                }

                foreach (var def in filtered)
                {
                    string modName = def.name + "_Printer";
                    if (def.tags.Contains(ItemTag.Damage) && !combatPool.Contains(modName))
                        combatPool.Add(modName);
                    else if (def.tags.Contains(ItemTag.Healing) && !healPool.Contains(modName))
                        healPool.Add(modName);
                    else if (def.tags.Contains(ItemTag.Utility) && !utilityPool.Contains(modName))
                        utilityPool.Add(modName);

                    if (def.tags.Contains(ItemTag.FoodRelated) && !chefPool.Contains(modName))
                        chefPool.Add(modName);

                    if (!itemTypePool.Contains(modName)) itemTypePool.Add(modName);
                }
            }
            else Log.Warning("Printer pool is set to none");

            //enemy
            if (enemy != RiskOfRoutes.ItemPoolOptions.None)
            {
                //todo shorten
                var filtered = RoR2.ItemCatalog.allItemDefs.Where(def =>
                (def.tier == ItemTier.Tier1) ||
                (def.tier == ItemTier.Tier2) ||
                (def.tier == ItemTier.Tier3) ||
                (allowBoss && def.tier == ItemTier.Boss) ||
                (allowVoid && def.tier == ItemTier.VoidTier1) ||
                (allowVoid && def.tier == ItemTier.VoidTier2) ||
                (allowVoid && def.tier == ItemTier.VoidTier3)).ToList(); //entire

                if (!allowAIBlacklisted) filtered = filtered.Where(def => def.DoesNotContainTag(ItemTag.AIBlacklist)).ToList();

                if (enemy == RiskOfRoutes.ItemPoolOptions.VanillaEntire) filtered = filtered.Where(def => vanillaItemsInfo.ContainsKey(def.name)).ToList();
                else if (enemy == RiskOfRoutes.ItemPoolOptions.Default)
                {
                    filtered = filtered.Where(def => vanillaItemsInfo.TryGetValue(def.name, out ItemInfo info) && info.isEnemyDefault).ToList();
                }

                bool isStackable = StageModifierDirector.instance.enemyItemsModifierCanStack;

                foreach (var def in filtered)
                {
                    string modName = def.name + "_EnemyItem";

                    if (AllDefs.TryGetValue(modName, out ModifierDef enemyDef)) enemyDef.isStackable = isStackable;

                    if (!generalNegativePool.Contains(modName)) generalNegativePool.Add(modName);
                }
            }
            else Log.Warning("Enemy pool is set to none");
            //lunar
            if (lunar != RiskOfRoutes.ItemPoolOptions.None)
            {
                var filtered = RoR2.ItemCatalog.allItemDefs.Where(def => (def.tier is ItemTier.Lunar)).ToList(); //entire

                if (lunar == RiskOfRoutes.ItemPoolOptions.VanillaEntire) filtered = filtered.Where(def => lunarItemTiers.ContainsKey(def.name)).ToList();
                else if (lunar == RiskOfRoutes.ItemPoolOptions.Default)
                {
                    filtered = filtered.Where(def => lunarItemTiers.TryGetValue(def.name, out LunarItemInfo info) && info.isDefault).ToList();
                }

                foreach (var def in filtered)
                {
                    string modName = def.name + "_LunarItem";
                    if (!generalNegativePool.Contains(modName)) generalNegativePool.Add(modName);
                }
            }
            else Log.Warning("Lunar pool is set to none");
        }



        public static void Register(ModifierDef def, string iconAddress, bool isArtifact = false, bool isItem = false, bool isAdditional = false)
        {
            if (AllDefs.ContainsKey(def.name))
            {
                if (AllDefs[def.name].tier == def.tier)
                {
                    Log.Warning($"Trying to register modifier {def.name} that already exists and has the same tier, check config");
                    return;
                }
                else
                {
                    Log.Warning($"Trying to register modifier {def.name} that already exists with the different tier");
                    AllDefs.Remove(def.name);
                }
            }

            def.icon = LoadSprite(iconAddress, isArtifact, isItem);
            def.isArtifact = isArtifact;
            AllDefs.Add(def.name, def);

            string nameToken = "";
            string descriptionToken = "";

            if (isArtifact)
            {
                ArtifactDef art = ArtifactCatalog.FindArtifactDef(def.name);
                if (art != null)
                {
                    nameToken = art.nameToken;
                    descriptionToken = (art == RoR2Content.Artifacts.TeamDeath) ? "ARTIFACT_DEATH_DESCRIPTION_SHORT" : art.descriptionToken;
                    def.nameToken = nameToken;
                    def.descriptionToken = descriptionToken;
                }
                else
                {
                    Log.Error($"{nameof(StageModifierCatalog)}: Init - Something went wrong for artifact:{def.name}");

                    nameToken = "MODIFIER_" + def.name.ToUpper() + "_NAME";
                    descriptionToken = "MODIFIER_" + def.name.ToUpper() + "_DESCRIPTION";

                    def.nameToken = nameToken;
                    def.descriptionToken = descriptionToken;
                }
            }
            else if (isItem)
            {
                string itemName = def.name.Split('_')[0];
                ItemDef item = PrinterItemModifier.stringToItemDef(itemName);
                if (item != null)
                {
                    nameToken = item.nameToken;
                    descriptionToken = item.nameToken;

                    def.nameToken = nameToken;
                    def.descriptionToken = descriptionToken;
                }
                else
                {
                    Log.Error($"{nameof(StageModifierCatalog)}: Init - Something went wrong for item:{def.name}");
                    //todo обработка
                }
            }
            else
            {
                nameToken = "MODIFIER_" + def.name.ToUpper() + "_NAME";
                descriptionToken = "MODIFIER_" + def.name.ToUpper() + "_DESCRIPTION";

                def.nameToken = nameToken;
                def.descriptionToken = descriptionToken;
            }

            //Log.Info($"Added tokens for modifier {def.name}:NAME - {nameToken}, DESCRIPTION - {descriptionToken}");
        }
        public static void TryRegArtifact(ArtifactInfo artifactInfo, bool isAdditional = false)
        {
            //if (!isAllowed) return;
            string name = artifactInfo.name;
            ModifierTier tier = (ModifierTier)artifactInfo.tier;
            bool isNegative = artifactInfo.isNegative;

            Func<bool> isAvailable = (name == "TeamDeath")
                ? () => StageModifierDirector.instance?.CheckArtifactOfDeathAvailable() ?? false
                : () => StageModifierDirector.instance?.CheckArtifactAvailable(name) ?? false;

            Register(new ModifierDef(name, tier, 1f, isAvailable, isStackable: false, isNegative: isNegative), name, isArtifact: true);

            if (isAdditional)
            {
                if (isNegative)
                {
                    generalNegativePool.Add(name);
                    Log.Info("ad");
                }
                else
                {
                    rarePool.Add(name);
                }
            }
        }

        public static void TryRegEnemy(string itemName, ModifierTier tier, bool isAdditional = false)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(itemName);
            ItemDef def = ItemCatalog.GetItemDef(index);
            if (def == null)
            {
                Log.Warning($"{nameof(TryRegEnemy)}: Tried registering enemy item {itemName} but def is null, check config");
                return;
            }

            string modName = itemName + "_EnemyItem";

            bool isStackable = StageModifierDirector.instance?.enemyItemsModifierCanStack ?? RiskOfRoutes.enemyItemsModifierCanStack.Value;

            if (AllDefs.TryGetValue(modName, out ModifierDef enemyDef)) enemyDef.isStackable = isStackable;

            Register(new ModifierDef(modName, tier, 1f, isStackable: StageModifierDirector.instance?.enemyItemsModifierCanStack ?? RiskOfRoutes.enemyItemsModifierCanStack.Value, true), itemName, false, true);

            if (isAdditional)
            {
                if (!generalNegativePool.Contains(modName)) generalNegativePool.Add(modName);
                //if (!enemyItemPool.Contains(modName)) enemyItemPool.Add(modName);
            }
        }

        public static void TryRegPrinter(string itemName, ModifierTier tier, bool isAdditional = false)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(itemName);
            ItemDef def = ItemCatalog.GetItemDef(index);
            if (def == null)
            {
                Log.Warning($"{nameof(TryRegPrinter)}: Tried registering printer item {itemName} but def is null, check config");
                return;
            }

            string modName = itemName + "_Printer";
            Register(new ModifierDef(modName, tier, 1f, false, false), itemName, false, true);

            if (!itemTypePool.Contains(modName)) itemTypePool.Add(modName);

            if (isAdditional)
            {
                if (def.tags.Contains(ItemTag.Damage) && !combatPool.Contains(modName))
                    combatPool.Add(modName);
                else if (def.tags.Contains(ItemTag.Healing) && !healPool.Contains(modName))
                    healPool.Add(modName);
                else if (def.tags.Contains(ItemTag.Utility) && !utilityPool.Contains(modName))
                    utilityPool.Add(modName);

                if (def.tags.Contains(ItemTag.FoodRelated) && !chefPool.Contains(modName))
                    chefPool.Add(modName);

                if (!itemTypePool.Contains(modName)) itemTypePool.Add(modName);

            }
        }

        public static void TryRegLunar(string itemName, ModifierTier tier, bool isAdditional = false)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(itemName);
            ItemDef def = ItemCatalog.GetItemDef(index);
            if (def == null)
            {
                Log.Warning($"{nameof(TryRegLunar)}: Tried registering lunar item {itemName} but def is null, check config");
                return;
            }

            if (def.tier != ItemTier.Lunar)
            {
                Log.Warning($"{nameof(TryRegLunar)}: Tried registering non-lunar item {itemName} as LunarItemModifier, check config");
                return;
            }

            string modName = itemName + "_LunarItem";
            Register(new ModifierDef(modName, tier, 1f, false, true), itemName, isArtifact: false, isItem: true);

            if (isAdditional)
            {
                //if (!lunarItemPool.Contains(modName)) lunarItemPool.Add(modName);
                if (!generalNegativePool.Contains(modName)) generalNegativePool.Add(modName);

            }
        }

        public static ModifierDef FindModifier(string name)
        {
            if (AllDefs.TryGetValue(name, out ModifierDef def))
            {
                return def;
            }
            else
            {
                //todo might be changed if client needs to know modifier tier for the ui
                Log.Warning($"{nameof(FindModifier)}: Missing the host modifier with name {name}");
                return null;

            }
        }

        public static StageModifier StageModifierFromDef(string def, int stack = 1)
        {
            switch (def)
            {
                case "Aurelionite": return new AurelioniteModifier();
                case "BossRedItem": return new BossRedItemModifier();
                case "BossYellowItem": return new BossYellowItemModifier();
                case "DroneBossGreen": return new DroneBossModifier(dropLegendary: false);
                case "DroneBossRed": return new DroneBossModifier(dropLegendary: true);
                case "MoreElites": return new MoreElitesModifier(stack);
                case "MoreDrones": return new MoreDronesModifier(stack);
                case "LunarEnemies": return new LunarEnemiesModifier(stack);
                case "VoidEnemies": return new VoidEnemiesModifier(stack);

                case "YellowPrinter": return new YellowPrinterModifier(stack);
                case "YellowPrinter_Damage": return new YellowPrinterModifier(stack, ItemTag.Damage);
                case "YellowPrinter_Healing": return new YellowPrinterModifier(stack, ItemTag.Healing);
                case "YellowPrinter_Utility": return new YellowPrinterModifier(stack, ItemTag.Utility);
                case "YellowPrinter_FoodRelated": return new YellowPrinterModifier(stack, ItemTag.FoodRelated);

                case "RedPrinter": return new RedPrinterModifier(stack);
                case "RedPrinter_Damage": return new RedPrinterModifier(stack, ItemTag.Damage);
                case "RedPrinter_Healing": return new RedPrinterModifier(stack, ItemTag.Healing);
                case "RedPrinter_Utility": return new RedPrinterModifier(stack, ItemTag.Utility);
                case "RedPrinter_FoodRelated": return new RedPrinterModifier(stack, ItemTag.FoodRelated);

                case "OnlyFlying": return new OnlyFlyingEnemiesModifier();
                case "PowerfulElites": return new Tier2ElitesEarlierModifier(stack);
                case "Repair": return new RepairConsumedItemsModifier();
                case "WanderingChef": return new WanderingChefModifier();
                case "Mountain": return new SaveMountainEffectModifier(stack);
                case "SoulCost": return new SoulCostModifier(stack);
                case "Doppelganger": return new DoppelgangerModifier(stack);
                case "UpgradeDrones": return new UpgradeDronesModifier(stack);

                //arts
                case "FriendlyFire": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.FriendlyFire);
                case "Command": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Command);
                case "Delusion": return new ArtifactModifier(RoR2.CU8Content.Artifacts.Delusion);
                case "Devotion": return new ArtifactModifier(RoR2.CU8Content.Artifacts.Devotion);
                case "MixEnemy": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.MixEnemy);
                case "Enigma": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Enigma);
                case "MonsterTeamGainsItems": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.MonsterTeamGainsItems);
                case "WeakAssKnees": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.WeakAssKnees);
                case "Glass": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Glass);
                case "EliteOnly": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.EliteOnly);
                case "SingleMonsterType": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.SingleMonsterType);
                case "RandomSurvivorOnRespawn": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.RandomSurvivorOnRespawn);
                case "Rebirth": return new ArtifactModifier(RoR2.DLC2Content.Artifacts.Rebirth);
                case "Sacrifice": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Sacrifice);
                case "WispOnDeath": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.WispOnDeath);
                case "Bomb": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Bomb);
                case "Swarms": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Swarms);
                case "ShadowClone": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.ShadowClone);
                case "TeamDeath": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.TeamDeath);
                //
                case "Mystery": return new ArtifactModifier(MysteryArtifact.Mystery);



                default:
                    {
                        if (def.Contains("_Printer"))
                        {
                            string itemName = def.Split('_')[0];
                            ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(itemName);
                            if (itemIndex == ItemIndex.None)
                            {
                                Log.Error($"Could not find item by the name {itemName}");
                                return null;
                            }
                            PrinterItemModifier mod = new PrinterItemModifier(itemName);
                            return mod;
                        }
                        else if (def.Contains("_EnemyItem"))
                        {
                            string itemName = def.Split('_')[0];
                            ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(itemName);
                            if (itemIndex == ItemIndex.None)
                            {
                                Log.Error($"Could not find item by the name {itemName}");
                                return null;
                            }
                            EnemyItemsModifier mod = new EnemyItemsModifier(itemName, stack);
                            return mod;
                        }
                        else if (def.Contains("_LunarItem"))
                        {
                            string itemName = def.Split('_')[0];
                            ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(itemName);
                            if (itemIndex == ItemIndex.None)
                            {
                                Log.Error($"Could not find lunar item by the name {itemName}");
                                return null;
                            }
                            LunarItemsModifier mod = new LunarItemsModifier(itemName);
                            return mod;
                        }
                        else
                        {
                            var artifact = RoR2.ArtifactCatalog.FindArtifactDef(def);
                            if (artifact != null)
                            {
                                return new ArtifactModifier(artifact);
                            }
                            else
                            {
                                Log.Error($"Could not find artifact by the name {def}");
                                return null;
                            }
                        }
                    }
            }
        }

        public static List<string> GetPool(RoutePortalType routePortalType, bool isNegative = false)
        {
            if (isNegative) return generalNegativePool;

            switch (routePortalType)
            {
                case RoutePortalType.ItemType: return itemTypePool;
                case RoutePortalType.DroneType: return droneTypePool;
                case RoutePortalType.Rare: return rarePool;
                case RoutePortalType.Combat: return combatPool;
                case RoutePortalType.Heal: return healPool;
                case RoutePortalType.Utility: return utilityPool;
                case RoutePortalType.ChefType: return chefPool;
                case RoutePortalType.Colossus: return GetRandomPool();
                default: return itemTypePool; //placeholder
            }
        }

        public static List<string> GetRandomPool()
        {
            var pools = new List<string>[]
            {
                droneTypePool,
                rarePool,
                combatPool,
                healPool,
                utilityPool
            };
            var res = Run.instance.stageRng.NextElementUniform(pools);
            Log.Info($"{nameof(GetRandomPool)}: For collosus got random pool with {res[0]}");
            return res;
        }

        public static ModifierDef SelectModifier(HashSet<string> banned, RoutePortalType portalType, ModifierTier tier, List<string> typePool)
        {
            var pool = typePool;
            if (pool.Count == 0)
            {
                Log.Warning($"{nameof(SelectModifier)}: Couldnt get pool for portal type - {portalType}");
            }
            else
            {
                Log.Info($"{nameof(SelectModifier)}: {portalType} - Got pool with {pool.Count} modifiers");
            }
            WeightedSelection<ModifierDef> selection = new WeightedSelection<ModifierDef>();

            foreach (var modName in pool)
            {
                if (banned.Contains(modName)) continue;

                var currentDef = FindModifier(modName);
                if (currentDef == null)
                {
                    Log.Error($"{nameof(SelectModifier)}: Returning null for name {modName}");
                    continue;
                }

                if (!currentDef.isAvailable()) continue;

                if (currentDef.tier == tier || (currentDef.tier < tier && currentDef.isStackable))
                {
                    //if (currentDef.tier != tier)
                    //Log.Info($"{nameof(SelectModifier)}: Modifier {currentDef.name} with default tier {currentDef.tier} needs to be stacked for tier {tier}");
                    selection.AddChoice(currentDef, 1f);
                }
                else
                {
                    //Log.Error($"Was searching for modifier with tier {tier} but got modifier {currentDef.name} with tier {currentDef.tier}");
                }
            }

            if (selection.Count > 0)
            {
                var selected = selection.Evaluate(Run.instance.stageRng.nextNormalizedFloat);

                return selected;
            }
            else
            {
                Log.Warning($"{nameof(SelectModifier)}: Selection is empty for Type: {portalType}, Tier: {tier}, banlist:");
                foreach (var modName in banned)
                {
                    Log.Warning($"\t{modName}");
                }
                return null;
            }
        }

        private static Sprite LoadSprite(string address, bool isArtifact, bool isItem)
        {
            if (address.Contains("RoR2/"))
            {
                //Log.Info($"Searching for ingame sprite for{address}");
                return Addressables.LoadAssetAsync<Sprite>(address).WaitForCompletion();
            }

            if (isArtifact)
            {
                //Log.Info($"Searching for artifact sprite for{address}");
                ArtifactDef art = ArtifactCatalog.FindArtifactDef(address);
                return art != null ? art.smallIconSelectedSprite : AssetManager.placeholderIcon;
            }

            if (isItem)
            {
                //Log.Info($"Searching for def sprite for {address}");
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(address);
                if (itemIndex != ItemIndex.None)
                    return ItemCatalog.GetItemDef(itemIndex).pickupIconSprite;
            }

            if (assetBundle != null && assetBundle.Contains(address))
            {
                //Log.Info($"Searching for assetBundle sprite for{address}");
                return assetBundle.LoadAsset<Sprite>(address);
            }

            Log.Warning($"Not found sprite for {address}");
            return AssetManager.placeholderIcon;
        }

        //todo
        public static bool isPortalTypeAvailable(RoutePortalType type)
        {
            return true;
        }

        public static string GetModifierNameFormatted(ModifierSync mod, ModifierDef def)
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
                            colorHex = "#4AE2F7";
                            break;
                        case ItemTier.VoidTier1:
                            colorHex = "#e95fae";
                            break;
                        case ItemTier.VoidTier2:
                            colorHex = "#e95fae";
                            break;
                        case ItemTier.VoidTier3:
                            colorHex = "#e95fae";
                            break;
                        case ItemTier.VoidBoss:
                            colorHex = "#e95fae";
                            break;
                        default:
                            colorHex = "#FFFFFF";
                            break;
                    }


                    localizedName = $"<color={colorHex}>{defaultName}</color>";
                }

                if (def.name.Contains("_Printer"))
                {
                    return RoR2.Language.GetStringFormatted("PRINTER_ITEMS_MODIFIER_NAME", localizedName);
                }
                else if (def.name.Contains("_EnemyItem"))
                {
                    if (itemDef != null)
                    {
                        int enemyItemStack = EnemyItemsModifier.GetItemStack(itemDef, mod.stack);
                        return RoR2.Language.GetStringFormatted("ENEMY_ITEMS_MODIFIER_NAME", localizedName, enemyItemStack);
                    }

                    return RoR2.Language.GetStringFormatted("ENEMY_ITEMS_MODIFIER_NAME", localizedName, mod.stack);
                }
                else if (def.name.Contains("_LunarItem"))
                {
                    return RoR2.Language.GetStringFormatted("LUNAR_ITEMS_MODIFIER_NAME", localizedName);
                }
            }

            return RoR2.Language.GetString(def.nameToken) + stackText;
        }

        public static string GetModifierDescriptionFormatted(ModifierSync mod, ModifierDef def)
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
                            colorHex = "#4AE2F7";
                            break;
                        case ItemTier.VoidTier1:
                            colorHex = "#e95fae";
                            break;
                        case ItemTier.VoidTier2:
                            colorHex = "#e95fae";
                            break;
                        case ItemTier.VoidTier3:
                            colorHex = "#e95fae";
                            break;
                        case ItemTier.VoidBoss:
                            colorHex = "#e95fae";
                            break;
                        default:
                            colorHex = "#FFFFFF";
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
                    if (itemDef != null)
                    {
                        int enemyItemStack = EnemyItemsModifier.GetItemStack(itemDef, mod.stack);
                        return RoR2.Language.GetStringFormatted("ENEMY_ITEMS_MODIFIER_DESCRIPTION", localizedName, enemyItemStack);
                    }

                    return RoR2.Language.GetStringFormatted("ENEMY_ITEMS_MODIFIER_DESCRIPTION", localizedName, mod.stack);
                }
                else if (def.name.Contains("_LunarItem"))
                {
                    if (StageModifierDirector.instance.purifyLunarItemModifier)
                    {
                        return RoR2.Language.GetStringFormatted("LUNAR_ITEMS_MODIFIER_DESCRIPTION_PURIFY", localizedName);
                    }
                    else
                    {
                        return RoR2.Language.GetStringFormatted("LUNAR_ITEMS_MODIFIER_DESCRIPTION_DEFAULT", localizedName);
                    }
                }

            }
            else if (def.isArtifact)
            {
                return $"<style=cWorldEvent>{RoR2.Language.GetStringFormatted(def.descriptionToken)}</style>";
            }
            else if (def.name == "Doppelganger")
            {
                string time = DoppelgangerModifier.GetCooldownString(mod.stack);

                return RoR2.Language.GetStringFormatted("MODIFIER_DOPPELGANGER_DESCRIPTION", time);
            }

            return RoR2.Language.GetString(def.descriptionToken) + stackText;
        }

    }
}

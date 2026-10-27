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
    public enum ModifierTags
    {

    }

    public class ModifierInfo
    {
        public string name;
        public int tier;
        public bool isNegative;
        public bool isDefault;

        public ModifierInfo(string name, int tier, bool isNegative = true, bool isDefault = false)
        {
            this.name = name;
            this.tier = tier;
            this.isNegative = isNegative;
            this.isDefault = isDefault;
        }
    }

    public static class StageModifierCatalog
    {
        private static AssetBundle assetBundle;

        public static Dictionary<string, ModifierDef> AllDefs = new Dictionary<string, ModifierDef>();

        public static readonly HashSet<ModifierDef> AllGeneralDefs = new HashSet<ModifierDef>();

        public static List<string> generalPositivePool = new List<string>();
        public static List<string> generalNegativePool = new List<string>();
        public static List<string> droneTypePool = new List<string>();
        public static List<string> rarePool = new List<string>();
        public static List<string> chefPool = new List<string>();
        public static List<string> bossPool = new List<string>();

        public static List<string> combatPool = new List<string>();
        public static List<string> healPool = new List<string>();
        public static List<string> utilityPool = new List<string>();

        public static List<string> itemTypePool = new List<string>();
        public static List<string> enemyItemPool = new List<string>();
        public static List<string> lunarItemPool = new List<string>();

        public static List<string> utilityItems = new List<string>();
        public static List<string> healingItems = new List<string>();
        public static List<string> damageItems = new List<string>();
        public static List<string> foodRelatedItems = new List<string>();

        public static HashSet<ItemDef> printerRegisteredDefs = new HashSet<ItemDef>();
        public static HashSet<ItemDef> enemyRegisteredDefs = new HashSet<ItemDef>();
        public static HashSet<ItemDef> lunarRegisteredDefs = new HashSet<ItemDef>();

        public static HashSet<string> printerDefaultBanList = new HashSet<string>();
        public static HashSet<string> enemyDefaultBanList = new HashSet<string>();
        public static HashSet<string> lunarDefaultBanList = new HashSet<string>();
        public static HashSet<string> artifactDefaultBanList = new HashSet<string>();

        //
        public static Dictionary<string, ModifierDef> RunDefs = new Dictionary<string, ModifierDef>();
        public static HashSet<string> additionalDefs = new HashSet<string>();


        public static readonly Dictionary<string, ModifierInfo> lunarItemTiers = new Dictionary<string, ModifierInfo>()
        {
            {"LunarTrinket", new ModifierInfo("LunarTrinket", 1, isNegative: true, isDefault: false)},
            {"GoldOnHit", new ModifierInfo("GoldOnHit", 2,isNegative:true, isDefault: true)},
            {"RepeatHeal", new ModifierInfo("RepeatHeal", 1, isNegative : true, isDefault : true)},
            {"MonstersOnShrineUse", new ModifierInfo("MonstersOnShrineUse", 1, isNegative : true, isDefault : true)},
            {"LunarSun", new ModifierInfo("LunarSun", 3, isNegative : true, isDefault : false)},
            {"LunarSpecialReplacement", new ModifierInfo("LunarSpecialReplacement", 3, isNegative : true, isDefault : false)},
            {"RandomlyLunar", new ModifierInfo("RandomlyLunar", 1, isNegative : true, isDefault : false)},
            {"FocusConvergence", new ModifierInfo("FocusConvergence", 1, isNegative : true, isDefault : true)},
            {"AutoCastEquipment", new ModifierInfo("AutoCastEquipment", 1, isNegative : true, isDefault : false)},
            {"LunarSecondaryReplacement", new ModifierInfo("LunarSecondaryReplacement", 3, isNegative : true, isDefault : false)},
            {"HalfAttackSpeedHalfCooldowns", new ModifierInfo("HalfAttackSpeedHalfCooldowns", 3, isNegative : true, isDefault : true)},
            {"OnLevelUpFreeUnlock", new ModifierInfo("OnLevelUpFreeUnlock", 3, isNegative : true, isDefault : false)},
            {"RandomDamageZone", new ModifierInfo("RandomDamageZone", 1, isNegative : true, isDefault : false)},
            {"TransferDebuffOnHit", new ModifierInfo("TransferDebuffOnHit", 1, isNegative : true, isDefault : false)},
            {"LunarBadLuck", new ModifierInfo("LunarBadLuck", 3, isNegative : true, isDefault : true)},
            {"LunarDagger", new ModifierInfo("LunarDagger", 3, isNegative : true, isDefault : true)},
            {"HalfSpeedDoubleHealth", new ModifierInfo("HalfSpeedDoubleHealth", 3, isNegative : true, isDefault : true)},
            {"LunarUtilityReplacement", new ModifierInfo("LunarUtilityReplacement", 3, isNegative : true, isDefault : true)},
            {"ShieldOnly", new ModifierInfo("ShieldOnly", 3, isNegative : true, isDefault : true)},
            {"LunarPrimaryReplacement", new ModifierInfo("LunarPrimaryReplacement", 3, isNegative : true, isDefault : false)}
        };

        public static readonly Dictionary<string, ModifierInfo> artifactTiers = new Dictionary<string, ModifierInfo>
        {
            { "Mystery", new ModifierInfo("Mystery", 2, isNegative: true, isDefault:true) },
            { "FriendlyFire", new ModifierInfo("FriendlyFire", 1, isNegative: true, isDefault : true) },
            { "Command", new ModifierInfo("Command", 3, isNegative: false, isDefault : false) },
            { "Delusion", new ModifierInfo("Delusion", 3, isNegative: false, isDefault : false) },
            { "Devotion", new ModifierInfo("Devotion", 2, isNegative: true, isDefault : false) },
            { "MixEnemy", new ModifierInfo("MixEnemy", 2, isNegative: true, isDefault : false) },
            { "Enigma", new ModifierInfo("Enigma", 3, isNegative: true, isDefault : false) },
            { "MonsterTeamGainsItems", new ModifierInfo("MonsterTeamGainsItems", 1, isNegative: true, isDefault : false) },
            { "WeakAssKnees", new ModifierInfo("WeakAssKnees", 1, isNegative: true, isDefault : true) },
            { "Glass", new ModifierInfo("Glass", 3, isNegative: true, isDefault : false) },
            { "EliteOnly", new ModifierInfo("EliteOnly", 3, isNegative: true, isDefault : true) },
            { "SingleMonsterType", new ModifierInfo("SingleMonsterType", 2, isNegative: true, isDefault : false) },
            { "RandomSurvivorOnRespawn", new ModifierInfo("RandomSurvivorOnRespawn", 3, isNegative: true, isDefault : false) },
            { "Rebirth", new ModifierInfo("Rebirth", 1, isNegative: false, isDefault : false) },
            { "Sacrifice", new ModifierInfo("Sacrifice", 3, isNegative: true, isDefault : true) },
            { "WispOnDeath", new ModifierInfo("WispOnDeath", 1, isNegative: true, isDefault : true) },
            { "Bomb", new ModifierInfo("Bomb", 1, isNegative: true, isDefault : true) },
            { "Swarms", new ModifierInfo("Swarms", 1, isNegative: true, isDefault : true) },
            { "ShadowClone", new ModifierInfo("ShadowClone", 1, isNegative: true, isDefault : false) },
            { "TeamDeath", new ModifierInfo("TeamDeath", 2, isNegative: true, isDefault : true) },
            { "Prestige", new ModifierInfo("Prestige", 1, isNegative:true, isDefault: false) }

            //todo some of the modded ones

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

            generalPositivePool.AddRange(new string[] { });
            generalNegativePool.AddRange(new string[] { "Doppelganger", "SoulCost", "VoidEnemies", "MoreElites", "LunarEnemies", "PowerfulElites", "OnlyFlying", "EnemyItemModifier" });

            itemTypePool.AddRange(new string[] { "YellowPrinter", "RedPrinter" });

            combatPool.AddRange(new string[] { "YellowPrinter_Damage", "RedPrinter_Damage", "DamagePrinterModifier" });
            healPool.AddRange(new string[] { "YellowPrinter_Healing", "RedPrinter_Healing", "HealingPrinterModifier" });
            utilityPool.AddRange(new string[] { "YellowPrinter_Utility", "RedPrinter_Utility", "UtilityPrinterModifier" });

            droneTypePool.AddRange(new string[] { "DroneBossGreen", "DroneBossRed", "UpgradeDrones", "MoreDrones" });
            rarePool.AddRange(new string[] { "BossRedItem", "BossYellowItem" });
            chefPool.AddRange(new string[] { "RedPrinter_FoodRelated", "YellowPrinter_FoodRelated", "FoodRelatedPrinterModifier" });

            Register(new ModifierDef("EnemyItemModifier", ModifierTier.Tier1, 1f, isStackable: true, isNegative: true), iconAddress: "enemyItemModifierIcon", isGeneral: true);
            Register(new ModifierDef("DamagePrinterModifier", ModifierTier.Tier1, 1f, isStackable: true, isNegative: false), iconAddress: "printerItemModifierIcon", isGeneral: true);
            Register(new ModifierDef("HealingPrinterModifier", ModifierTier.Tier1, 1f, isStackable: true, isNegative: false), iconAddress: "printerItemModifierIcon", isGeneral: true);
            Register(new ModifierDef("UtilityPrinterModifier", ModifierTier.Tier1, 1f, isStackable: true, isNegative: false), iconAddress: "printerItemModifierIcon", isGeneral: true);
            Register(new ModifierDef("FoodRelatedPrinterModifier", ModifierTier.Tier1, 1f, isStackable: true, isNegative: false), iconAddress: "printerItemModifierIcon", isGeneral: true);

            Register(new ModifierDef("Aurelionite", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false), iconAddress: "texBuffAurelioniteBlessingIcon", isGeneral: true);
            Register(new ModifierDef("BossRedItem", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false, conflicts: new string[] { "BossYellowItem", "DroneBossGreen" }), iconAddress: "redBossItem", isGeneral: true);
            Register(new ModifierDef("BossYellowItem", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false, conflicts: new string[] { "BossRedItem", "DroneBossGreen" }), iconAddress: "yellowBossItem", isGeneral: true);
            Register(new ModifierDef("DroneBossGreen", ModifierTier.Tier1, 1f, isStackable: false, isNegative: false, conflicts: new string[] { "DroneBossRed" }), iconAddress: "uncommonDroneReward", isGeneral: true);
            Register(new ModifierDef("DroneBossRed", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false, conflicts: new string[] { "DroneBossGreen" }), iconAddress: "legendaryDroneReward", isGeneral: true);

            Register(new ModifierDef("MoreElites", ModifierTier.Tier1, 1f, isStackable: true, isNegative: true), iconAddress: "moreElitesIcon", isGeneral: true);
            Register(new ModifierDef("MoreDrones", ModifierTier.Tier1, 1f, isStackable: true, isNegative: false), iconAddress: "texMoreDronesModifier", isGeneral: true);
            Register(new ModifierDef("UpgradeDrones", ModifierTier.Tier3, 1f, isStackable: true, isNegative: false), iconAddress: "RoR2/DLC3/UI/texUIdroneupgradeIcon.png", isGeneral: true);
            Register(new ModifierDef("LunarEnemies", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), iconAddress: "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveLunarIcon.png", isGeneral: true);
            Register(new ModifierDef("VoidEnemies", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), iconAddress: "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveVoidIcon.png", isGeneral: true);

            Register(new ModifierDef("YellowPrinter", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), iconAddress: "yellowPrinter", isGeneral: true);
            Register(new ModifierDef("RedPrinter", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), iconAddress: "redPrinter", isGeneral: true);

            Register(new ModifierDef("YellowPrinter_Damage", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), iconAddress: "yellowPrinterDamage", isGeneral: true);
            Register(new ModifierDef("YellowPrinter_Healing", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), iconAddress: "yellowPrinterHeal", isGeneral: true);
            Register(new ModifierDef("YellowPrinter_Utility", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), iconAddress: "yellowPrinterUtility", isGeneral: true);
            Register(new ModifierDef("YellowPrinter_FoodRelated", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, isStackable: true, isNegative: false), iconAddress: "yellowPrinterFood", isGeneral: true);
            Register(new ModifierDef("RedPrinter_Damage", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), iconAddress: "redPrinterDamage", isGeneral: true);
            Register(new ModifierDef("RedPrinter_Healing", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), iconAddress: "redPrinterHealing", isGeneral: true);
            Register(new ModifierDef("RedPrinter_Utility", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), iconAddress: "redPrinterUtility", isGeneral: true);
            Register(new ModifierDef("RedPrinter_FoodRelated", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, isStackable: true, isNegative: false), iconAddress: "redPrinterFood", isGeneral: true);

            Register(new ModifierDef("OnlyFlying", ModifierTier.Tier2, 1f, isStackable: false, isNegative: true), iconAddress: "onlyFlying", isGeneral: true);
            Register(new ModifierDef("PowerfulElites", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), iconAddress: "tier2ElitesIcon", isGeneral: true);
            Register(new ModifierDef("Repair", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckRepairAvailable() ?? false, isStackable: false, isNegative: false), iconAddress: "repair", isGeneral: true);
            Register(new ModifierDef("WanderingChef", ModifierTier.Tier3, 1f, isStackable: false, isNegative: false), iconAddress: "RoR2/DLC3/MealPrep/texMealPrepIcon.png", isGeneral: true);

            Register(new ModifierDef("Mountain", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckMountainShrine() ?? false, isStackable: false, isNegative: false), iconAddress: "mountIcon", isGeneral: true);
            Register(new ModifierDef("SoulCost", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), iconAddress: "RoR2/DLC2/texBuffSoulCostIcon.png", isGeneral: true);
            Register(new ModifierDef("Doppelganger", ModifierTier.Tier2, 1f, isStackable: true, isNegative: true), iconAddress: "texAurelioniteIcon", isGeneral: true);

            Register(new ModifierDef("BossAspectModifier", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), iconAddress: "repair", isGeneral: true);
            Register(new ModifierDef("BossSwarmModifier", ModifierTier.Tier3, 1f, isStackable: true, isNegative: true), iconAddress: "repair", isGeneral: true);

            foreach (var item in RoR2.ItemCatalog.itemDefs)
            {
                if (item.hidden || item.tier == ItemTier.NoTier || item.ContainsTag(ItemTag.Scrap)) continue;

                if ((item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2) && item.DoesNotContainTag(ItemTag.CannotDuplicate))
                {
                    TryRegPrinter(item);
                }

                if (item.tier == ItemTier.Tier1 ||
                    item.tier == ItemTier.Tier2 ||
                    item.tier == ItemTier.Tier3 ||
                    item.tier == ItemTier.Boss ||
                    item.tier == ItemTier.VoidTier1 ||
                    item.tier == ItemTier.VoidTier2 ||
                    item.tier == ItemTier.VoidTier3 ||
                    item.tier == ItemTier.VoidBoss)
                {
                    TryRegEnemy(item);
                }

                if (item.tier == ItemTier.Lunar)
                {
                    TryRegLunar(item);
                }
            }

            foreach (var art in RoR2.ArtifactCatalog.artifactDefs)
            {
                TryRegArtifact(art);
            }


            CreateDefaultBanLists();
        }

        public static void CreateDefaultBanLists()
        {
            string[] enemyBannedDefs = new string[] { };

            foreach (var def in lunarItemTiers)
            {
                if (!def.Value.isDefault) lunarDefaultBanList.Add(def.Key);
            }

            foreach (var def in artifactTiers)
            {
                if (!def.Value.isDefault) artifactDefaultBanList.Add(def.Key);
            }
        }

        public static void CreateRunPools()
        {
            List<DefInfo> lunarsInfo = new List<DefInfo>();
            List<DefInfo> artInfo = new List<DefInfo>();

            var lunars = RiskOfRoutes.moddedLunarsConfig.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var lunar in lunars)
            {
                var split = lunar.Split("=");
                if (split.Length < 3) continue;

                string name = split[0];
                bool isNegative = split[1] == "-";
                int tier = int.Parse(split[2]);

                lunarsInfo.Add(new DefInfo() { name = name, isNegative = isNegative, tier = (ModifierTier)tier });
            }

            var artifacts = RiskOfRoutes.moddedArtifactsConfig.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var art in artifacts)
            {
                var split = art.Split("=");
                if (split.Length < 3) continue;

                string name = split[0];
                bool isNegative = split[1] == "-";
                int tier = int.Parse(split[2]);

                artInfo.Add(new DefInfo() { name = name, isNegative = isNegative, tier = (ModifierTier)tier });
            }

            RunDefs.Clear();

            generalNegativePool.Clear();
            generalNegativePool.AddRange(new string[] { "Doppelganger", "SoulCost", "VoidEnemies", "MoreElites", "LunarEnemies", "PowerfulElites", "OnlyFlying", "EnemyItemModifier" });

            rarePool.Clear();
            rarePool.AddRange(new string[] { "BossRedItem", "BossYellowItem" });


            foreach (var defPair in AllDefs)
            {
                string defName = defPair.Key;
                ModifierDef def = defPair.Value;

                var foundArtifact = artInfo.FirstOrDefault(defInfo => defInfo.name == defName);
                var foundLunar = lunarsInfo.FirstOrDefault(defInfo => defInfo.name + "_LunarItem" == defName);

                ModifierDef foundDef = def;

                if (foundArtifact != null)
                {
                    foundDef = new ModifierDef(defName, foundArtifact.tier, def.weight, false, foundArtifact.isNegative)
                    {
                        spriteIcon = def.spriteIcon,
                        isArtifact = def.isArtifact,
                        nameToken = def.nameToken,
                        descriptionToken = def.descriptionToken,
                        conflictList = def.conflictList
                    };
                }
                else if (foundLunar != null)
                {
                    foundDef = new ModifierDef(defName, foundLunar.tier, def.weight, false, foundLunar.isNegative)
                    {
                        spriteIcon = def.spriteIcon,
                        isArtifact = def.isArtifact,
                        nameToken = def.nameToken,
                        descriptionToken = def.descriptionToken,
                        conflictList = def.conflictList
                    };
                }

                RunDefs.Add(defName, foundDef);

                bool isLunar = defName.EndsWith("_LunarItem");
                bool isArtifact = foundDef.isArtifact;

                if (isArtifact || isLunar)
                {
                    if (foundDef.isNegative)
                    {
                        if (!generalNegativePool.Contains(defName))
                            generalNegativePool.Add(defName);
                    }
                    else
                    {
                        if (!rarePool.Contains(defName))
                            rarePool.Add(defName);
                    }
                }
            }

        }

        public static void Register(ModifierDef def, string iconAddress, bool isArtifact = false, bool isItem = false, bool isGeneral = false)
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

            def.spriteIcon = LoadSprite(iconAddress, isArtifact, isItem);
            def.isArtifact = isArtifact;
            AllDefs.Add(def.name, def);

            if (isGeneral) AllGeneralDefs.Add(def);

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
            if (!isItem && !isArtifact)
            {
                nameToken = "MODIFIER_" + def.name.ToUpper() + "_NAME";
                descriptionToken = "MODIFIER_" + def.name.ToUpper() + "_DESCRIPTION";

                def.nameToken = nameToken;
                def.descriptionToken = descriptionToken;
            }

            //Log.Info($"Added tokens for modifier {def.name}:NAME - {nameToken}, DESCRIPTION - {descriptionToken}");
        }


        public static void TryRegPrinter(ItemDef itemDef)
        {
            if (itemDef == null)
            {
                Log.Warning($"{nameof(TryRegPrinter)}: Tried registering printer item but def is null, check config");
                return;
            }
            string modName = itemDef.name + "_Printer";

            ModifierDef modifierDef = new ModifierDef(modName, ModifierTier.Tier1, 1f, false, false);

            modifierDef.nameToken = itemDef.nameToken;
            modifierDef.descriptionToken = itemDef.descriptionToken;

            Register(modifierDef, iconAddress: itemDef.name, isArtifact: false, isItem: true); 
            printerRegisteredDefs.Add(itemDef);

            if (!itemTypePool.Contains(modName)) itemTypePool.Add(modName);

            if (itemDef.tags.Contains(ItemTag.Damage) && !damageItems.Contains(modName))
                damageItems.Add(modName);
            else if (itemDef.tags.Contains(ItemTag.Healing) && !healingItems.Contains(modName))
                healingItems.Add(modName);
            else if (itemDef.tags.Contains(ItemTag.Utility) && !utilityItems.Contains(modName))
                utilityItems.Add(modName);
            if (itemDef.tags.Contains(ItemTag.FoodRelated) && !foodRelatedItems.Contains(modName))
                foodRelatedItems.Add(modName);

            if (!itemTypePool.Contains(modName)) itemTypePool.Add(modName);
        }

        public static void TryRegEnemy(ItemDef itemDef)
        {
            if (itemDef == null)
            {
                Log.Warning($"{nameof(TryRegEnemy)}: Tried registering enemy item but def is null, check config");
                return;
            }
            string modName = itemDef.name + "_EnemyItem";

            ModifierDef modifierDef = new ModifierDef(modName, ModifierTier.Tier1, 1f, true, true);

            modifierDef.nameToken = itemDef.nameToken;
            modifierDef.descriptionToken = itemDef.descriptionToken;

            if (AllDefs.TryGetValue(modName, out ModifierDef enemyDef)) enemyDef.isStackable = true;

            Register(modifierDef, iconAddress: itemDef.name, false, true);
            enemyRegisteredDefs.Add(itemDef);

            if (!enemyItemPool.Contains(modName)) enemyItemPool.Add(modName);
        }

        public static void TryRegLunar(ItemDef itemDef)
        {
            bool isAdditional = false;

            if (itemDef == null)
            {
                Log.Warning($"{nameof(TryRegLunar)}: Tried registering lunar item but def is null, check config");
                return;
            }

            if (itemDef.tier != ItemTier.Lunar)
            {
                Log.Warning($"{nameof(TryRegLunar)}: Tried registering non-lunar item {itemDef.name} as LunarItemModifier, check config");
                return;
            }

            string modName = itemDef.name + "_LunarItem";

            ModifierDef modifierDef = null;
            if (lunarItemTiers.TryGetValue(itemDef.name, out ModifierInfo info))
                modifierDef = new ModifierDef(modName, (ModifierTier)info.tier, 1f, false, true);
            else
            {
                modifierDef = new ModifierDef(modName, ModifierTier.Tier1, 1f, false, true);
                isAdditional = true;
            }

            modifierDef.nameToken = itemDef.nameToken;
            modifierDef.descriptionToken = itemDef.descriptionToken;

            Register(modifierDef, iconAddress: itemDef.name, isArtifact: false, isItem: true);
            lunarRegisteredDefs.Add(itemDef);

            if (isAdditional)
            {
                additionalDefs.Add(itemDef.name);
            }
        }

        public static void TryRegArtifact(ArtifactDef artDef)
        {
            bool isAdditional = false;
            string name = artDef.cachedName;

            ModifierDef modifierDef = null;

            Func<bool> isAvailable = (name == "TeamDeath")
                ? () => StageModifierDirector.instance?.CheckArtifactOfDeathAvailable() ?? false
                : () => StageModifierDirector.instance?.CheckArtifactAvailable(name) ?? false;

            if (artifactTiers.TryGetValue(artDef.cachedName, out ModifierInfo artInfo))
                modifierDef = new ModifierDef(artDef.cachedName, (ModifierTier)artInfo.tier, 1f, isAvailable, false, artInfo.isNegative);
            else
            {
                modifierDef = new ModifierDef(artDef.cachedName, ModifierTier.Tier3, 1f, isAvailable, false, true);
                isAdditional = true;
            }
            modifierDef.nameToken = artDef.nameToken;
            modifierDef.descriptionToken = artDef.descriptionToken;
            modifierDef.isArtifact = true;

            Register(modifierDef, name, isArtifact: true);

            if (isAdditional)
            {
                additionalDefs.Add(artDef.cachedName);
            }
        }

        public static ModifierDef FindModifier(string name)
        {
            if (RunDefs.Keys.Count == 0)
            {
                Log.Error("Run defs are empty");
                return null;
            }

            if (RunDefs.TryGetValue(name, out ModifierDef def))
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
                case "BossAspectModifier": return new BossAspectModifier();
                case "BossSwarmModifier": return new BossSwarmModifier();

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
                            int last = def.LastIndexOf('_');
                            string itemName = def.Substring(0, last);

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
                            int last = def.LastIndexOf('_');
                            string itemName = def.Substring(0, last); ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(itemName);

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
                            int last = def.LastIndexOf('_');
                            string itemName = def.Substring(0, last);

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


        private static Sprite LoadSprite(string address, bool isArtifact, bool isItem)
        {
            if (address.Contains("RoR2/"))
            {
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
                int last = def.name.LastIndexOf('_');
                string itemName = def.name.Substring(0, last);
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(itemName);
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);

                string localizedName = itemName;

                if (itemDef != null)
                {
                    string defaultName = RoR2.Language.GetString(itemDef.nameToken);
                    string colorHex = "FFFFFF";

                    switch (itemDef.tier)
                    {
                        case ItemTier.Tier1:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier1Item);
                            break;
                        case ItemTier.Tier2:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier2Item);
                            break;
                        case ItemTier.Tier3:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier3Item);
                            break;
                        case ItemTier.Boss:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.BossItem);
                            break;
                        case ItemTier.Lunar:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.LunarItem);
                            break;
                        case ItemTier.VoidTier1:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        case ItemTier.VoidTier2:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        case ItemTier.VoidTier3:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        case ItemTier.VoidBoss:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        default:
                            colorHex = "FFFFFF";
                            break;
                    }


                    localizedName = $"<color=#{colorHex}>{defaultName}</color>";
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
                int last = def.name.LastIndexOf('_');
                string itemName = def.name.Substring(0, last);
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(itemName);
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);

                string localizedName = itemName;

                if (itemDef != null)
                {
                    string defaultName = RoR2.Language.GetString(itemDef.nameToken);
                    string colorHex = "FFFFFF";

                    switch (itemDef.tier)
                    {
                        case ItemTier.Tier1:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier1Item);
                            break;
                        case ItemTier.Tier2:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier2Item);
                            break;
                        case ItemTier.Tier3:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier3Item);
                            break;
                        case ItemTier.Boss:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.BossItem);
                            break;
                        case ItemTier.Lunar:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.LunarItem);
                            break;
                        case ItemTier.VoidTier1:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        case ItemTier.VoidTier2:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        case ItemTier.VoidTier3:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        case ItemTier.VoidBoss:
                            colorHex = RoR2.ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem);
                            break;
                        default:
                            colorHex = "FFFFFF";
                            break;
                    }

                    localizedName = $"<color=#{colorHex}>{defaultName}</color>";
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

        public static List<string> GetPool(RoutePortalType routePortalType, bool isNegative = false, bool isItemPool = false)
        {
            if (isNegative)
            {
                if (isItemPool)
                {
                    return enemyItemPool;
                }
                if (routePortalType == RoutePortalType.MoonTeleporter || routePortalType == RoutePortalType.FalseSon)
                    return bossPool;
                else
                    return generalNegativePool;
            }

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
                case RoutePortalType.FalseSon: return bossPool;
                case RoutePortalType.MoonTeleporter: return bossPool;
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

                var currentDef = StageModifierCatalog.FindModifier(modName);
                if (currentDef == null)
                {
                    Log.Error($"{nameof(SelectModifier)}: Returning null for name {modName}");
                    continue;
                }

                if (!currentDef.isAvailable()) continue;

                if (currentDef.tier == tier || (currentDef.tier < tier && currentDef.isStackable))
                {
                    //Log.Info($"{nameof(SelectModifier)}: Modifier {currentDef.itemDef} with default tier {currentDef.tier} needs to be stacked for tier {tier}");
                    selection.AddChoice(currentDef, currentDef.weight);
                }
                else
                {
                    //Log.Error($"Was searching for modifier with tier {tier} but got modifier {currentDef.itemDef} with tier {currentDef.tier}");
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

    }
}

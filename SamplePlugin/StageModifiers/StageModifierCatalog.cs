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

        public static List<string> additionalDefs = new List<string>();

        public static List<string> itemTypePool = new List<string>();
        public static List<string> enemyItemPool = new List<string>();
        public static List<string> lunarItemPool = new List<string>();


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

        public static void Init(AssetBundle bundle)
        {
            assetBundle = bundle;

            AllDefs.Clear();
            generalPositivePool.Clear();
            generalNegativePool.Clear();
            itemTypePool.Clear();
            rarePool.Clear();
            droneTypePool.Clear();

            generalPositivePool.AddRange(new string[] { });
            generalNegativePool.AddRange(new string[] { "Doppelganger", "SoulCost", "VoidEnemies", "MoreElites", "LunarEnemies", "PowerfulElites", "OnlyFlying" });

            itemTypePool.AddRange(new string[] { "YellowPrinter", "RedPrinter" });
            combatPool.AddRange(new string[] { "YellowPrinter_Damage", "RedPrinter_Damage" });
            healPool.AddRange(new string[] { "YellowPrinter_Healing", "RedPrinter_Healing" });
            utilityPool.AddRange(new string[] { "YellowPrinter_Utility", "RedPrinter_Utility" });


            rarePool.AddRange(new string[] { "BossRedItem", "BossYellowItem" });
            droneTypePool.AddRange(new string[] { "DroneBossGreen", "DroneBossRed", "UpgradeDrones", "MoreDrones" });
            chefPool.AddRange(new string[] { "RedPrinter_FoodRelated", "YellowPrinter_FoodRelated" });



            TryRegArtifact(RiskOfRoutes.allowMysteryArtifact.Value, "Mystery", ModifierTier.Tier2, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowChaosArtifact.Value, "FriendlyFire", ModifierTier.Tier1, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowCommandArtifact.Value, "Command", ModifierTier.Tier3, isNegative: false);
            TryRegArtifact(RiskOfRoutes.allowDelusionArtifact.Value, "Delusion", ModifierTier.Tier3, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowDevotionArtifact.Value, "Devotion", ModifierTier.Tier2, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowDissonanceArtifact.Value, "MixEnemy", ModifierTier.Tier2, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowEnigmaArtifact.Value, "Enigma", ModifierTier.Tier3, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowEvolutionArtifact.Value, "MonsterTeamGainsItems", ModifierTier.Tier1, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowFrailtyArtifact.Value, "WeakAssKnees", ModifierTier.Tier1, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowGlassArtifact.Value, "Glass", ModifierTier.Tier3, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowHonorArtifact.Value, "EliteOnly", ModifierTier.Tier3, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowKinArtifact.Value, "SingleMonsterType", ModifierTier.Tier2, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowMetamorphosisArtifact.Value, "RandomSurvivorOnRespawn", ModifierTier.Tier3, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowRebirthArtifact.Value, "Rebirth", ModifierTier.Tier1, isNegative: false);
            TryRegArtifact(RiskOfRoutes.allowSacrificeArtifact.Value, "Sacrifice", ModifierTier.Tier3, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowSoulArtifact.Value, "WispOnDeath", ModifierTier.Tier1, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowSpiteArtifact.Value, "Bomb", ModifierTier.Tier1, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowSwarmsArtifact.Value, "Swarms", ModifierTier.Tier1, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowVengeanceArtifact.Value, "ShadowClone", ModifierTier.Tier1, isNegative: true);
            TryRegArtifact(RiskOfRoutes.allowDeathArtifact.Value, "TeamDeath", ModifierTier.Tier2, isNegative: true);


            Register(new ModifierDef("Aurelionite", ModifierTier.Tier3, 1f, false, false), "texBuffAurelioniteBlessingIcon");
            Register(new ModifierDef("BossRedItem", ModifierTier.Tier3, 1f, false, false, new string[] { "BossYellowItem", "DroneBossGreen" }), "redBossItem");
            Register(new ModifierDef("BossYellowItem", ModifierTier.Tier2, 1f, false, false, new string[] { "BossRedItem", "DroneBossGreen" }), "yellowBossItem");
            Register(new ModifierDef("DroneBossGreen", ModifierTier.Tier1, 1f, false, false), "uncommonDroneReward");
            Register(new ModifierDef("DroneBossRed", ModifierTier.Tier3, 1f, false, false), "legendaryDroneReward");

            Register(new ModifierDef("MoreElites", ModifierTier.Tier1, 1f, true, true), "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveBossIcon.png");
            Register(new ModifierDef("MoreDrones", ModifierTier.Tier1, 1f, true, false), "RoR2/Base/Common/MiscIcons/texDroneIconOutlined.png");
            Register(new ModifierDef("UpgradeDrones", ModifierTier.Tier3, 1f, true, false), "RoR2/DLC3/UI/texUIdroneupgradeIcon.png");
            Register(new ModifierDef("LunarEnemies", ModifierTier.Tier3, 1f, true, true), "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveLunarIcon.png");
            Register(new ModifierDef("VoidEnemies", ModifierTier.Tier3, 1f, true, true), "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveVoidIcon.png");

            Register(new ModifierDef("YellowPrinter", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, true, false), "yellowPrinter");
            Register(new ModifierDef("RedPrinter", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, true, false), "redPrinter");

            Register(new ModifierDef("YellowPrinter_Damage", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, true, false), "yellowPrinterDamage");
            Register(new ModifierDef("YellowPrinter_Healing", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, true, false), "yellowPrinterHeal");
            Register(new ModifierDef("YellowPrinter_Utility", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, true, false), "yellowPrinterUtility");
            Register(new ModifierDef("YellowPrinter_FoodRelated", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersYellowItem() ?? false, true, false), "yellowPrinterFood");
            Register(new ModifierDef("RedPrinter_Damage", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, true, false), "redPrinterDamage");
            Register(new ModifierDef("RedPrinter_Healing", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, true, false), "redPrinterHealing");
            Register(new ModifierDef("RedPrinter_Utility", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, true, false), "redPrinterUtility");
            Register(new ModifierDef("RedPrinter_FoodRelated", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckPlayersRedItem() ?? false, true, false), "redPrinterFood");


            Register(new ModifierDef("OnlyFlying", ModifierTier.Tier2, 1f, false, true), "onlyFlying");
            Register(new ModifierDef("PowerfulElites", ModifierTier.Tier3, 1f, true, true), "tier2elites");
            Register(new ModifierDef("Repair", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckPlayersHaveConsumedItems() ?? false, false, false), "repair");
            Register(new ModifierDef("WanderingChef", ModifierTier.Tier3, 1f, false, false), "RoR2/DLC3/MealPrep/texMealPrepIcon.png");

            Register(new ModifierDef("Mountain", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckMountainShrine() ?? false, false, false), "mountIcon");
            Register(new ModifierDef("SoulCost", ModifierTier.Tier2, 1f, true, true), "RoR2/DLC2/texBuffSoulCostIcon.png");
            Register(new ModifierDef("Doppelganger", ModifierTier.Tier2, 1f, true, true), "texAurelioniteIcon");



            RegEnemy("SlowOnHit", ModifierTier.Tier1);
            RegEnemy("FlatHealth", ModifierTier.Tier1);
            RegEnemy("BarrierOnCooldown", ModifierTier.Tier1);
            RegEnemy("NearbyDamageBonus", ModifierTier.Tier1);
            RegEnemy("OutOfCombatArmor", ModifierTier.Tier1);
            RegEnemy("PersonalShield", ModifierTier.Tier1);
            RegEnemy("Hoof", ModifierTier.Tier1);
            RegEnemy("ShieldBooster", ModifierTier.Tier1);
            RegEnemy("FlatHealth", ModifierTier.Tier1);

            //RegEnemy("Thorns", ModifierTier.Tier2);
            RegEnemy("SprintOutOfCombat", ModifierTier.Tier2);
            RegEnemy("FragileDamageBonus", ModifierTier.Tier2);
            RegEnemy("CritGlasses", ModifierTier.Tier2);
            RegEnemy("Syringe", ModifierTier.Tier2);
            RegEnemy("ArmorPlate", ModifierTier.Tier2);

            RegEnemy("Behemoth", ModifierTier.Tier3);
            //RegEnemy("CaptainDefensiveMatrix", ModifierTier.Tier3);
            RegEnemy("ImmuneToDebuff", ModifierTier.Tier3);
            RegEnemy("ExtraLife", ModifierTier.Tier3);
            RegEnemy("ArmorReductionOnHit", ModifierTier.Tier3);
            //RegEnemy("PermanentDebuffOnHit", ModifierTier.Tier3);
            RegEnemy("Bear", ModifierTier.Tier3);

            //принтеры

            RegPrinter("ArmorPlate", ModifierTier.Tier1);
            RegPrinter("FlatHealth", ModifierTier.Tier1);
            RegPrinter("Crowbar", ModifierTier.Tier1);
            RegPrinter("DeathMark", ModifierTier.Tier1);
            RegPrinter("DelayedDamage", ModifierTier.Tier1);
            RegPrinter("DronesDropDynamite", ModifierTier.Tier1);
            RegPrinter("Hoof", ModifierTier.Tier1);
            RegPrinter("IgniteOnKill", ModifierTier.Tier1);
            RegPrinter("JumpBoost", ModifierTier.Tier1);
            RegPrinter("Mushroom", ModifierTier.Tier1);
            RegPrinter("ShieldBooster", ModifierTier.Tier1);
            RegPrinter("SprintBonus", ModifierTier.Tier1);
            RegPrinter("StunChanceOnHit", ModifierTier.Tier1);

            RegPrinter("AttackSpeedAndMoveSpeed", ModifierTier.Tier2);
            RegPrinter("AttackSpeedOnCrit", ModifierTier.Tier2);
            RegPrinter("Bandolier", ModifierTier.Tier2);
            RegPrinter("BarrierOnKill", ModifierTier.Tier2);
            RegPrinter("Infusion", ModifierTier.Tier2);
            RegPrinter("Seed", ModifierTier.Tier2);
            RegPrinter("ChainLightning", ModifierTier.Tier2);
            RegPrinter("CritAtLowerElevation", ModifierTier.Tier2);
            RegPrinter("EquipmentMagazine", ModifierTier.Tier2);
            RegPrinter("ExplodeOnDeath", ModifierTier.Tier2);
            RegPrinter("FragileDamageBonus", ModifierTier.Tier2);
            RegPrinter("IncreaseDamageOnMultiKill", ModifierTier.Tier2);
            RegPrinter("KnockBackHitEnemies", ModifierTier.Tier2);
            RegPrinter("NearbyDamageBonus", ModifierTier.Tier2);
            RegPrinter("PrimarySkillShuriken", ModifierTier.Tier2);
            RegPrinter("SpeedBoostPickup", ModifierTier.Tier2);
            RegPrinter("StrengthenBurn", ModifierTier.Tier2);
            RegPrinter("Thorns", ModifierTier.Tier2);

            RegPrinter("Bear", ModifierTier.Tier3);
            RegPrinter("BleedOnHit", ModifierTier.Tier3);
            RegPrinter("CritGlasses", ModifierTier.Tier3);
            RegPrinter("FireRing", ModifierTier.Tier3);
            RegPrinter("IceRing", ModifierTier.Tier3);
            RegPrinter("Missile", ModifierTier.Tier3);
            RegPrinter("StickyBomb", ModifierTier.Tier3);
            RegPrinter("Syringe", ModifierTier.Tier3);

            //лунарные
            RegLunar("GoldOnHit", ModifierTier.Tier1);
            RegLunar("MonstersOnShrineUse", ModifierTier.Tier1);
            RegLunar("FocusConvergence", ModifierTier.Tier1);

            RegLunar("RepeatHeal", ModifierTier.Tier2);
            RegLunar("HalfAttackSpeedHalfCooldowns", ModifierTier.Tier2);
            RegLunar("ShieldOnly", ModifierTier.Tier2);

            RegLunar("LunarBadLuck", ModifierTier.Tier3);
            RegLunar("HalfSpeedDoubleHealth", ModifierTier.Tier3);
        }

        //todo убрать брутфорс из всех пуллов, сделать единый для удаления
        public static void CleanAdditionalDefs()
        {
            Log.Info($"{nameof(CleanAdditionalDefs)}: Removing added defs from pools");

            List<string> toRemove = new List<string>();
            foreach (var pair in AllDefs)
            {
                if (additionalDefs.Contains(pair.Key))
                {
                    toRemove.Add(pair.Key);
                }
            }

            //todo убрать
            Log.Info($"{nameof(CleanAdditionalDefs)}: Defs to remove:");
            foreach (var additionalDef in toRemove)
            {
                Log.Info($"\t{additionalDef}");
            }
            //

            foreach (var def in toRemove)
            {
                AllDefs.Remove(def);
            }

            generalNegativePool.RemoveAll(def => toRemove.Contains(def));

            utilityPool.RemoveAll(def => toRemove.Contains(def));
            healPool.RemoveAll(def => toRemove.Contains(def));
            combatPool.RemoveAll(def => toRemove.Contains(def));
            chefPool.RemoveAll(def => toRemove.Contains(def));


        }

        public static void DisableDefaultPools(bool disablePrinter, bool disableEnemy, bool disableLunar)
        {
            if (disablePrinter)
            {
                Log.Info($"{nameof(DisableDefaultPools)}: Default printer modifier configuration is disabled");

                var defaults = itemTypePool;
                foreach (var defName in defaults) AllDefs.Remove(defName);

                itemTypePool.Clear();
                combatPool.Clear();
                healPool.Clear();
                utilityPool.Clear();

                chefPool.RemoveAll(def => def.EndsWith("_Printer"));
            }
            if (disableEnemy)
            {
                Log.Info($"{nameof(DisableDefaultPools)}: Default enemy item modifier configuration is disabled");

                var defaults = enemyItemPool;
                foreach (var defName in defaults) AllDefs.Remove(defName);

                generalNegativePool.RemoveAll(x => x.EndsWith("_EnemyItem"));
            }
            if (disableLunar)
            {
                Log.Info($"{nameof(DisableDefaultPools)}: Default lunar item modifier configuration is disabled");

                var defaults = lunarItemPool;

                foreach (var defName in defaults) AllDefs.Remove(defName);

                generalNegativePool.RemoveAll(x => x.EndsWith("_LunarItem"));


            }
        }

        public static void Register(ModifierDef def, string iconAddress, bool isArtifact = false, bool isItem = false, bool isAdditional = false)
        {
            if (AllDefs.ContainsKey(def.name))
            {
                Log.Warning($"Trying to register modifier {def.name} that already exists, check config");
                return;
            }

            if (isAdditional) additionalDefs.Add(def.name);

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
        public static void TryRegArtifact(bool isAllowed, string name, ModifierTier tier, bool isNegative, bool isAdditional = false)
        {
            if (!isAllowed) return;

            Func<bool> isAvailable = (name == "TeamDeath")
                ? () => StageModifierDirector.instance?.CheckArtifactOfDeathAvailable() ?? false
                : () => StageModifierDirector.instance?.CheckArtifactAvailable(name) ?? false;

            Register(new ModifierDef(name, tier, 1f, isAvailable, stackable: false, isNegative: isNegative), name, isArtifact: true, isAdditional);

            if (isNegative)
            {
                generalNegativePool.Add(name);
            }
            else
            {
                rarePool.Add(name);
            }
        }

        public static void RegEnemy(string itemName, ModifierTier tier, bool isAdditional = false)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(itemName);
            ItemDef def = ItemCatalog.GetItemDef(index);
            if (def == null)
            {
                Log.Warning($"{nameof(RegEnemy)}: Tried registering enemy item {itemName} but def is null, check config");
                return;
            }

            string modName = itemName + "_EnemyItem";
            if(StageModifierDirector.instance!=null)
            {
                Register(new ModifierDef(modName, tier, 1f, stackable: StageModifierDirector.instance.enemyItemsModifierCanStack, true), itemName, false, true, isAdditional);
            }
            else
            {
                Register(new ModifierDef(modName, tier, 1f, stackable: RiskOfRoutes.enemyItemsModifierCanStack.Value, true), itemName, false, true, isAdditional);
            }
            generalNegativePool.Add(modName);
            enemyItemPool.Add(modName);
        }

        public static void RegPrinter(string itemName, ModifierTier tier, bool isAdditional = false)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(itemName);
            ItemDef def = ItemCatalog.GetItemDef(index);
            if (def == null)
            {
                Log.Warning($"{nameof(RegPrinter)}: Tried registering printer item {itemName} but def is null, check config");
                return;
            }

            string modName = itemName + "_Printer";
            Register(new ModifierDef(modName, tier, 1f, false, false), itemName, false, true, isAdditional);

            if (def.tags.Contains(ItemTag.Damage))
                combatPool.Add(modName);
            else if (def.tags.Contains(ItemTag.Healing))
                healPool.Add(modName);
            else if (def.tags.Contains(ItemTag.Utility))
                utilityPool.Add(modName);


            //todo под вопросом
            if (def.tags.Contains(ItemTag.FoodRelated))
                chefPool.Add(modName);
            //

            itemTypePool.Add(modName);
        }

        public static void RegLunar(string itemName, ModifierTier tier, bool isAdditional = false)
        {
            ItemIndex index = ItemCatalog.FindItemIndex(itemName);
            ItemDef def = ItemCatalog.GetItemDef(index);
            if (def == null)
            {
                Log.Warning($"{nameof(RegLunar)}: Tried registering lunar item {itemName} but def is null, check config");
                return;
            }

            if (def.tier != ItemTier.Lunar)
            {
                Log.Warning($"{nameof(RegLunar)}: Tried registering non-lunar item {itemName} as LunarItemModifier, check config");
                return;
            }

            string modName = itemName + "_LunarItem";
            Register(new ModifierDef(modName, tier, 1f, false, true), itemName, isArtifact: false, isItem: true, isAdditional);
            generalNegativePool.Add(modName);

            lunarItemPool.Add(modName);
        }

        public static ModifierDef FindModifier(string name)
        {
            if (AllDefs.TryGetValue(name, out ModifierDef def))
            {
                return def;
            }
            else
            {
                //todo клиенту может понадобиться знать тир модифаера если от этого будет зависеть UI
                Log.Warning($"{nameof(FindModifier)}: Host has modifier with name {name}");
                //var itemName = name.Split('_')[0];
                //ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(itemName);
                //if (itemIndex == ItemIndex.None)
                //{
                //    Log.Error($"Could not find item by the name {itemName}");
                //    return null;
                //}

                //if(name.EndsWith("_Printer"))
                //{
                //    RegPrinter(itemName, ModifierTier.Tier1, true);
                //}
                //else if(name.EndsWith("_EnemyItem"))
                //{
                //    RegEnemy(itemName, ModifierTier.Tier1, true);
                //}
                //else if(name.EndsWith("_LunarItem"))
                //{
                //    RegLunar(itemName, ModifierTier.Tier1, true);
                //}

                //if (AllDefs.TryGetValue(name, out ModifierDef itemDef))
                //{
                //    return itemDef;
                //}
                //else return null;

                return null;

            }
        }

        public static StageModifier StageModifierFromDef(string def, int stack = 1)
        {
            switch (def)
            {
                case "Aurelionite": return new AurelioniteRewardModifier();
                case "BossRedItem": return new BossRedItemModifier();
                case "BossYellowItem": return new BossYellowItemModifier();
                case "DroneBossGreen": return new DroneBossRewardModifier(dropLegendary: false);
                case "DroneBossRed": return new DroneBossRewardModifier(dropLegendary: true);
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
                case "PowerfulElites": return new Tier2ElitesEarlierModifier();
                case "Repair": return new RepairConsumedItemsModifier();
                case "WanderingChef": return new WanderingChefModifier();
                case "Mountain": return new SaveMountainEffectModifier(stack);
                case "SoulCost": return new SoulCostModifier(stack);
                case "Doppelganger": return new DoppelgangerAttackModifier(stack);
                case "UpgradeDrones": return new UpgradeDronesModifier(stack);

                //артефакты
                case "FriendlyFire": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.FriendlyFire, false);
                case "Command": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Command, true);
                case "Delusion": return new ArtifactModifier(RoR2.CU8Content.Artifacts.Delusion, true);
                case "Devotion": return new ArtifactModifier(RoR2.CU8Content.Artifacts.Devotion, false);
                case "MixEnemy": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.MixEnemy, false);
                case "Enigma": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Enigma, false);
                case "MonsterTeamGainsItems": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.MonsterTeamGainsItems, false);
                case "WeakAssKnees": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.WeakAssKnees, false);
                case "Glass": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Glass, false);
                case "EliteOnly": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.EliteOnly, false);
                case "SingleMonsterType": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.SingleMonsterType, false);
                case "RandomSurvivorOnRespawn": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.RandomSurvivorOnRespawn, false);
                case "Rebirth": return new ArtifactModifier(RoR2.DLC2Content.Artifacts.Rebirth, true);
                case "Sacrifice": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Sacrifice, false);
                case "WispOnDeath": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.WispOnDeath, false);
                case "Bomb": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Bomb, false);
                case "Swarms": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Swarms, false);
                case "ShadowClone": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.ShadowClone, false);
                case "TeamDeath": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.TeamDeath, false);
                //
                case "Mystery": return new ArtifactModifier(MysteryArtifact.Mystery, false);



                default:
                    {
                        if (def.Contains("Printer"))
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
                        else if (def.Contains("EnemyItem"))
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
                        else if (def.Contains("LunarItem"))
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
                            return null;
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
                    return null;
                }

                if (!currentDef.isAvailable()) continue;

                if (currentDef.tier == tier || (currentDef.tier < tier && currentDef.IsStackable()))
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
                return selection.Evaluate(Run.instance.stageRng.nextNormalizedFloat);
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
                Log.Info($"Searching for ingame sprite for{address}");
                return Addressables.LoadAssetAsync<Sprite>(address).WaitForCompletion();
            }

            if (isArtifact)
            {
                Log.Info($"Searching for artifact sprite for{address}");
                ArtifactDef art = ArtifactCatalog.FindArtifactDef(address);
                return art != null ? art.smallIconSelectedSprite : AssetManager.placeholderIcon;
            }

            if (isItem)
            {
                Log.Info($"Searching for item sprite for {address}");
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(address);
                if (itemIndex != ItemIndex.None)
                    return ItemCatalog.GetItemDef(itemIndex).pickupIconSprite;
            }

            if (assetBundle != null && assetBundle.Contains(address))
            {
                Log.Info($"Searching for assetBundle sprite for{address}");
                return assetBundle.LoadAsset<Sprite>(address);
            }

            Log.Info($"Not found sprite for {address}");
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
                string time = DoppelgangerAttackModifier.GetCooldownString(mod.stack);

                return RoR2.Language.GetStringFormatted("MODIFIER_DOPPELGANGER_DESCRIPTION", time);
            }

            return RoR2.Language.GetString(def.descriptionToken) + stackText;
        }
    }
}

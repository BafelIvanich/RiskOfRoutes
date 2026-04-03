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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

//Rare: random tier 3 positive, more credits on negative?
//Chef: guaranteed chef, food printers more likely?
//DroneFocused: DroneBossReward, more drones
//Mountain effect only if 2 or more portals
//printer limit 3
//yellow/red printer only as additional
//the better printer items the more neg credits
//ban list
//texture fix

namespace RiskOfRoutes.StageModifiers
{
    public static class StageModifierCatalog
    {
        private static AssetBundle assetBundle;

        public static Dictionary<string, ModifierDef> AllDefs = new Dictionary<string, ModifierDef>();
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

            void Register(ModifierDef def, string iconAddress, bool isArtifact = false, bool isItem = false)
            {
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
                        if (art == RoR2.RoR2Content.Artifacts.TeamDeath)
                        {
                            nameToken = art.nameToken;
                            descriptionToken = "ARTIFACT_DEATH_DESCRIPTION_SHORT";
                        }
                        else
                        {
                            nameToken = art.nameToken;
                            descriptionToken = art.descriptionToken;
                        }

                        def.nameToken = nameToken;
                        def.descriptionToken = descriptionToken;
                    }
                    else
                    {
                        Log.Error($"Something went wrong for artifact:{def.name}");

                        nameToken = "MODIFIER_" + def.name.ToUpper() + "_NAME";
                        descriptionToken = "MODIFIER_" + def.name.ToUpper() + "_DESCRIPTION";

                        def.nameToken = nameToken;
                        def.descriptionToken = descriptionToken;

                        return;
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
                        Log.Error($"Something went wrong for item:{def.name}");
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

                Log.Info($"Added tokens for modifier {def.name}:NAME - {nameToken}, DESCRIPTION - {descriptionToken}");
            }

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

            Register(new ModifierDef("OnlyFlying", ModifierTier.Tier2, 1f, false, true), "texAurelioniteIcon");
            Register(new ModifierDef("PowerfulElites", ModifierTier.Tier3, 1f, true, true), "texAurelioniteIcon");
            Register(new ModifierDef("Repair", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckPlayersHaveConsumedItems() ?? false, false, false), "texAurelioniteIcon");
            Register(new ModifierDef("WanderingChef", ModifierTier.Tier3, 1f, false, false), "RoR2/DLC3/MealPrep/texMealPrepIcon.png");

            Register(new ModifierDef("Mountain", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckMountainShrine() ?? false, false, false), "RoR2/Base/Common/texMountainIcon.png");
            Register(new ModifierDef("SoulCost", ModifierTier.Tier2, 1f, true, true), "texSoulCostIcon");
            Register(new ModifierDef("Doppelganger", ModifierTier.Tier2, 1f, true, true), "texAurelioniteIcon");

            if (RiskOfRoutes.allowMysteryArtifact.Value) Register(new ModifierDef("Mystery", ModifierTier.Tier2, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Mystery") ?? false, false, true), "Mystery", true);
            if (RiskOfRoutes.allowChaosArtifact.Value) Register(new ModifierDef("FriendlyFire", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("FriendlyFire") ?? false, false, true), "FriendlyFire", true);
            if (RiskOfRoutes.allowCommandArtifact.Value) Register(new ModifierDef("Command", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Command") ?? false, false, false), "Command", true);
            if (RiskOfRoutes.allowDelusionArtifact.Value) Register(new ModifierDef("Delusion", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Delusion") ?? false, false, true), "Delusion", true);
            if (RiskOfRoutes.allowDevotionArtifact.Value) Register(new ModifierDef("Devotion", ModifierTier.Tier2, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Devotion") ?? false, false, true), "Devotion", true);
            if (RiskOfRoutes.allowDissonanceArtifact.Value) Register(new ModifierDef("MixEnemy", ModifierTier.Tier2, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("MixEnemy") ?? false, false, true), "MixEnemy", true);
            if (RiskOfRoutes.allowEnigmaArtifact.Value) Register(new ModifierDef("Enigma", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Enigma") ?? false, false, true), "Enigma", true);
            if (RiskOfRoutes.allowEvolutionArtifact.Value) Register(new ModifierDef("MonsterTeamGainsItems", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("MonsterTeamGainsItems") ?? false, false, true), "MonsterTeamGainsItems", true);
            if (RiskOfRoutes.allowFrailtyArtifact.Value) Register(new ModifierDef("WeakAssKnees", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("WeakAssKnees") ?? false, false, true), "WeakAssKnees", true);
            if (RiskOfRoutes.allowGlassArtifact.Value) Register(new ModifierDef("Glass", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Glass") ?? false, false, true), "Glass", true);
            if (RiskOfRoutes.allowHonorArtifact.Value) Register(new ModifierDef("EliteOnly", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("EliteOnly") ?? false, false, true), "EliteOnly", true);
            if (RiskOfRoutes.allowKinArtifact.Value) Register(new ModifierDef("SingleMonsterType", ModifierTier.Tier2, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("SingleMonsterType") ?? false, false, true), "SingleMonsterType", true);
            if (RiskOfRoutes.allowMetamorphosisArtifact.Value) Register(new ModifierDef("RandomSurvivorOnRespawn", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("RandomSurvivorOnRespawn") ?? false, false, true), "RandomSurvivorOnRespawn", true);
            if (RiskOfRoutes.allowRebirthArtifact.Value) Register(new ModifierDef("Rebirth", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Rebirth") ?? false, false, false), "Rebirth", true);
            if (RiskOfRoutes.allowSacrificeArtifact.Value) Register(new ModifierDef("Sacrifice", ModifierTier.Tier3, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Sacrifice") ?? false, false, true), "Sacrifice", true);
            if (RiskOfRoutes.allowSoulArtifact.Value) Register(new ModifierDef("WispOnDeath", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("WispOnDeath") ?? false, false, true), "WispOnDeath", true);
            if (RiskOfRoutes.allowSpiteArtifact.Value) Register(new ModifierDef("Bomb", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Bomb") ?? false, false, true), "Bomb", true);
            if (RiskOfRoutes.allowSwarmsArtifact.Value) Register(new ModifierDef("Swarms", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("Swarms") ?? false, false, true), "Swarms", true);
            if (RiskOfRoutes.allowVengeanceArtifact.Value) Register(new ModifierDef("ShadowClone", ModifierTier.Tier1, 1f, () => StageModifierDirector.instance?.CheckArtifactAvailable("ShadowClone") ?? false, false, true), "ShadowClone", true);
            if (RiskOfRoutes.allowDeathArtifact.Value) Register(new ModifierDef("TeamDeath", ModifierTier.Tier2, 1f, () => StageModifierDirector.instance?.CheckArtifactOfDeathAvailable() ?? false, false, true), "TeamDeath", true);

            List<string> items = new List<string>();
            items.Add(RoR2.RoR2Content.Items.Hoof.name);
            items.Add(RoR2.RoR2Content.Items.PersonalShield.name);
            items.Add(RoR2.RoR2Content.Items.ArmorPlate.name);

            foreach (var item in items)
            {
                Register(new ModifierDef(item + "_Printer", ModifierTier.Tier1, 1f, false, false), item, false, true);
            }

            foreach (var item in items)
            {
                Register(new ModifierDef(item + "_EnemyItem", ModifierTier.Tier1, 1f, false, true), item, false, true);
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
                case "DroneBossGreen": return new DroneBossRewardModifier(false);
                case "DroneBossRed": return new DroneBossRewardModifier(true);
                case "MoreElites": return new MoreElitesModifier(stack);
                case "MoreDrones": return new MoreDronesModifier(stack);
                case "LunarEnemies": return new LunarEnemiesModifier(stack);
                case "VoidEnemies": return new VoidEnemiesModifier(stack);
                case "YellowPrinter": return new YellowPrinterModifier(stack);
                case "RedPrinter": return new RedPrinterModifier(stack);
                case "OnlyFlying": return new OnlyFlyingEnemiesModifier();
                case "PowerfulElites": return new Tier2ElitesEarlierModifier();
                case "Repair": return new RepairConsumedItemsModifier();
                case "WanderingChef": return new WanderingChefModifier();
                case "Mountain": return new SaveMountainEffectModifier();
                case "SoulCost": return new SoulCostModifier(stack);
                case "Doppelganger": return new DoppelgangerAttackModifier();
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
                        else if(def.Contains("EnemyItem"))
                        {
                            string itemName = def.Split('_')[0];
                            ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(itemName);
                            if (itemIndex == ItemIndex.None)
                            {
                                Log.Error($"Could not find item by the name {itemName}");
                                return null;
                            }
                            EnemyItemsModifier mod = new EnemyItemsModifier(itemName);
                            return mod;
                        }
                        else
                        {
                            return null;
                        }
                    }
            }
        }

        public static List<string> GetPortalTypeModifiers(RoutePortalType routePortalType)
        {
            switch (routePortalType)
            {
                case RoutePortalType.ItemType: return new List<string>();
                case RoutePortalType.DroneType: return new List<string>();
                case RoutePortalType.ChefType: return new List<string>();
                case RoutePortalType.Rare: return new List<string>();
                default: return new List<string>();
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
                return art != null ? art.smallIconSelectedSprite : RiskOfRoutes.placeholderIcon;
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
            return RiskOfRoutes.placeholderIcon;
        }



    }
}

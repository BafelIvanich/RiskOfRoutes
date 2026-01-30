using RiskOfRoutes;
using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin.StageModifiers;
using SamplePlugin.StageModifiers.OtherModifiers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SamplePlugin
{
    [System.Serializable]
    public struct ModifierSync : IEquatable<ModifierSync>
    {
        public string name;
        public string description;
        public int stack;
        public bool isArtifact;

        public ModifierSync(StageModifier mod)
        {
            name = mod.name;
            description = mod.activeText;
            stack = mod.stack;
            isArtifact = mod is ArtifactModifier ? true : false;
        }

        public bool Equals(ModifierSync other)
        {
            return this.name == other.name;
        }
    }

    public class SyncListModifier : SyncListStruct<ModifierSync> { }

    public class StageModifierDirector : NetworkBehaviour
    {
        public static StageModifierDirector instance;

        public static List<GameObject> activePortalInstances;

        public List<StageModifier> activeModifiers { get; set; }
        public List<StageModifier> reservedModifiers { get; set; }
        public List<ModifierDef> bannedModifiers { get; set; }
        public List<string> runArtifactsApplied;

        [SyncVar(hook = nameof(OnModifiersSyncedUpdate))]
        public SyncListModifier modifiersSynced = new SyncListModifier();

        private static void OnModifiersSyncedUpdate()
        {
            Log.Info("SyncedModifiers were updated");
        }

        //public SyncListModifier modifiersSynced = new SyncListModifier();

        public int startingCredits = 200;
        public int routeCredits = 60;
        public int negativeCredits = 30;
        public int uniqueModifierLimit = 5;
        public int printerItemModifierLimit = 3;

        public RouteMap routeMap;
        public RouteMap.RouteNode current;

        [SyncVar(hook = nameof(OnStageUpdate))]
        public int currStage = 0;

        public static List<ItemInfo> itemsInfo =
        [
            //сильные
            new ItemInfo("Syringe", ModifierTier.Tier3, 1f),
            new ItemInfo("Bear", ModifierTier.Tier3, 1f),
            new ItemInfo("FireRing", ModifierTier.Tier3, 1f),
            new ItemInfo("IceRing", ModifierTier.Tier3, 1f),


            //средне
            new ItemInfo("Crowbar", ModifierTier.Tier2, 1f),
            new ItemInfo("NearbyDamageBonus", ModifierTier.Tier2, 1f),
            new ItemInfo("BleedOnHit", ModifierTier.Tier2, 1f),
            new ItemInfo("FragileDamageBonus", ModifierTier.Tier2, 1f),
            new ItemInfo("Missile", ModifierTier.Tier2, 1f),
            new ItemInfo("ExecuteLowHealthElite", ModifierTier.Tier2, 1f),
            new ItemInfo("ExplodeOnDeath", ModifierTier.Tier2, 1f),


            //такое
            new ItemInfo("IgniteOnKill", ModifierTier.Tier1, 1f),
            new ItemInfo("SecondarySkillMagazine", ModifierTier.Tier1, 1f),
            new ItemInfo("Thorns", ModifierTier.Tier1, 1f),
            new ItemInfo("ChainLightning", ModifierTier.Tier1, 1f),
            new ItemInfo("StrengthenBurn", ModifierTier.Tier1, 1f),
            new ItemInfo("KnockBackHitEnemies", ModifierTier.Tier1, 1f),
            new ItemInfo("ShieldBooster", ModifierTier.Tier1, 1f),
            new ItemInfo("EquipmentMagazine",ModifierTier.Tier1,1f),
            new ItemInfo("Seed",ModifierTier.Tier1,1f)

        ];

        public List<ModifierDef> allModifiers;

        public override void OnStartServer()
        {
            base.OnStartServer();

            CreateDirector(Run.instance);
        }

        //private void OnModifiersChanged(SyncList<ModifierSync>.Operation op, int itemIndex)
        //{
        //    Log.Info("Modifiers were changed");
        //}

        private void CreateDirector(Run run)
        {
            Log.Info("Run started");


            runArtifactsApplied = GetArtifactsAppliedThisRun();

            Log.Info("Creating route map");
            routeMap = new RouteMap(run.runRNG);

            current = routeMap.GetStartingNode();
            Log.Info($"Current:{current.x},{current.y} that has {current.GetPaths().Count} paths");
        }

        public void Awake()
        {
            Log.Info("Called method awake in stage modifier director");

            instance = this;

            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<StageModifier>();
            bannedModifiers = new List<ModifierDef>();
            allModifiers = new List<ModifierDef>();
            activePortalInstances = new List<GameObject>();
            DontDestroyOnLoad(gameObject);

            RegisterModDefs();

            Log.Info("StageModifierDirector created");

            RoR2.Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;

            RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
            RoR2.Stage.onServerStageComplete += Stage_onServerStageComplete;

        }

        public void OnDestroy()
        {
            RoR2.Run.onRunDestroyGlobal -= Run_onRunDestroyGlobal;

            RoR2.SceneDirector.onPrePopulateSceneServer -= SceneDirector_onPrePopulateSceneServer;
            RoR2.Stage.onServerStageComplete -= Stage_onServerStageComplete;
        }

        public void RegisterModDefs()
        {
            allModifiers.Add(new ModifierDef("BossRedItem", ModifierTier.Tier3, 1f, false, new string[] { "BossYellowItem", "DroneBossGreen" }));
            allModifiers.Add(new ModifierDef("BossYellowItem", ModifierTier.Tier2, 1f, false, new string[] { "BossRedItem", "DroneBossGreen" }));
            allModifiers.Add(new ModifierDef("DroneBossGreen", ModifierTier.Tier1, 1f, false));
            allModifiers.Add(new ModifierDef("MoreElites", ModifierTier.Tier1, 1f, true));
            allModifiers.Add(new ModifierDef("MoreDrones", ModifierTier.Tier1, 1f, true));
            allModifiers.Add(new ModifierDef("LunarEnemies", ModifierTier.Tier3, 1f, true));
            allModifiers.Add(new ModifierDef("VoidEnemies", ModifierTier.Tier3, 1f, true));
            allModifiers.Add(new ModifierDef("YellowPrinter", ModifierTier.Tier1, 1f, CheckPlayersYellowItem, true));
            allModifiers.Add(new ModifierDef("RedPrinter", ModifierTier.Tier1, 1f, CheckPlayersRedItem, true));
            allModifiers.Add(new ModifierDef("OnlyFlying", ModifierTier.Tier2, 1f, false));
            allModifiers.Add(new ModifierDef("PowerfulElites", ModifierTier.Tier3, 1f, true));
            allModifiers.Add(new ModifierDef("Mystery", ModifierTier.Tier2, 1f, false));
            allModifiers.Add(new ModifierDef("Repair", ModifierTier.Tier3, 1f, CheckPlayersHaveConsumedItems, false));
            allModifiers.Add(new ModifierDef("WanderingChef", ModifierTier.Tier3, 1f, false));
            allModifiers.Add(new ModifierDef("Mountain", ModifierTier.Tier1, 1f, CheckMountainShrine, false));
            allModifiers.Add(new ModifierDef("SoulCost", ModifierTier.Tier2, 1f, true));
            allModifiers.Add(new ModifierDef("Doppelganger", ModifierTier.Tier2, 1f, true));

            //артефакты
            if (RiskOfRoutes.RiskOfRoutes.allowChaosArtifact.Value) allModifiers.Add(new ModifierDef("FriendlyFire", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("FriendlyFire"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowCommandArtifact.Value) allModifiers.Add(new ModifierDef("Command", ModifierTier.Tier3, 1f, () => CheckArtifactAvailable("Command"), true));
            if (RiskOfRoutes.RiskOfRoutes.allowDelusionArtifact.Value) allModifiers.Add(new ModifierDef("Delusion", ModifierTier.Tier3, 1f, () => CheckArtifactAvailable("Delusion"), true));
            if (RiskOfRoutes.RiskOfRoutes.allowDevotionArtifact.Value) allModifiers.Add(new ModifierDef("Devotion", ModifierTier.Tier2, 1f, () => CheckArtifactAvailable("Devotion"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowDissonanceArtifact.Value) allModifiers.Add(new ModifierDef("MixEnemy", ModifierTier.Tier2, 1f, () => CheckArtifactAvailable("MixEnemy"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowEnigmaArtifact.Value) allModifiers.Add(new ModifierDef("Enigma", ModifierTier.Tier3, 1f, () => CheckArtifactAvailable("Enigma"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowEvolutionArtifact.Value) allModifiers.Add(new ModifierDef("MonsterTeamGainsItems", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("MonsterTeamGainsItems"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowFrailtyArtifact.Value) allModifiers.Add(new ModifierDef("WeakAssKnees", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("WeakAssKnees"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowGlassArtifact.Value) allModifiers.Add(new ModifierDef("Glass", ModifierTier.Tier3, 1f, () => CheckArtifactAvailable("Glass"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowHonorArtifact.Value) allModifiers.Add(new ModifierDef("EliteOnly", ModifierTier.Tier3, 1f, () => CheckArtifactAvailable("EliteOnly"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowKinArtifact.Value) allModifiers.Add(new ModifierDef("SingleMonsterType", ModifierTier.Tier2, 1f, () => CheckArtifactAvailable("SingleMonsterType"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowMetamorphosisArtifact.Value) allModifiers.Add(new ModifierDef("RandomSurvivorOnRespawn", ModifierTier.Tier3, 1f, () => CheckArtifactAvailable("RandomSurvivorOnRespawn"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowRebirthArtifact.Value) allModifiers.Add(new ModifierDef("Rebirth", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("Rebirth"), true));
            if (RiskOfRoutes.RiskOfRoutes.allowSacrificeArtifact.Value) allModifiers.Add(new ModifierDef("Sacrifice", ModifierTier.Tier3, 1f, () => CheckArtifactAvailable("Sacrifice"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowSoulArtifact.Value) allModifiers.Add(new ModifierDef("WispOnDeath", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("WispOnDeath"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowSpiteArtifact.Value) allModifiers.Add(new ModifierDef("Bomb", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("Bomb"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowSwarmsArtifact.Value) allModifiers.Add(new ModifierDef("Swarms", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("Swarms"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowVengeanceArtifact.Value) allModifiers.Add(new ModifierDef("ShadowClone", ModifierTier.Tier1, 1f, () => CheckArtifactAvailable("ShadowClone"), false));
            if (RiskOfRoutes.RiskOfRoutes.allowDeathArtifact.Value) allModifiers.Add(new ModifierDef("TeamDeath", ModifierTier.Tier2, 1f, CheckArtifactOfDeathAvailable, false));

            foreach (var item in itemsInfo)
            {
                allModifiers.Add(new ModifierDef(item.internalName + " Printer", item.cost, item.weight, false));
            }


        }


        protected virtual void FixedUpdate()
        {
            if (Run.instance != null)
            {
                routeCredits = (int)(startingCredits * Run.instance.difficultyCoefficient) + 30 * Run.instance.stageClearCount;
            }
        }



        private void Run_onRunDestroyGlobal(Run obj)
        {
            Log.Info($"Run ended, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnEnd();
            }
            reservedModifiers.Clear();
            runArtifactsApplied.Clear();
            modifiersSynced.Clear();
            if (this.gameObject)
            {
                Destroy(this.gameObject);
                Log.Info("\tStageModifierDirector destroyed");
            }

        }

        public List<string> GetArtifactsAppliedThisRun()
        {
            List<string> res = new List<string>();
            foreach (var art in RoR2.RunArtifactManager.enabledArtifactsEnumerable)
            {
                res.Add(art.cachedName);
                Log.Info($"Added {art.cachedName}");
            }
            return res;
        }

        private void SceneDirector_onPrePopulateSceneServer(SceneDirector obj)
        {
            Log.Info($"Stage started, applying {reservedModifiers.Count} modifiers");
            if (reservedModifiers.Count == 0) return;
            Log.Info(reservedModifiers.Count);
            activeModifiers = new List<StageModifier>(reservedModifiers);
            foreach (var mod in reservedModifiers)
            {
                modifiersSynced.Add(new ModifierSync(mod));
            }
            reservedModifiers.Clear();

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
            OnStageUpdate(currStage);
            activePortalInstances.Clear();
        }

        private void OnStageUpdate(int newValue)
        {
            currStage = Run.instance.stageClearCount + 1;
            Log.Info("Method updating syncvar was called");
        }

        private void Stage_onServerStageComplete(RoR2.Stage obj)
        {
            Log.Info($"Stage completed, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers) modifier.OnEnd();

            activeModifiers.Clear();
            modifiersSynced.Clear();

        }

        public List<StageModifier> GetNextStageModifiers(GameObject portal)
        {
            WeightedSelection<ModifierDef> weightedSelection = CreateAvailableModifierSelection(portal);
            var mg = portal.GetComponent<RoutePortalManager>();
            HashSet<string> bannedFromSelection = new HashSet<string>();
            Dictionary<string, int> modifiers = new Dictionary<string, int>();

            if (mg)
            {
                //lemurian temple, habitat
                if ((mg.destinationSceneIndex == 35 || mg.destinationSceneIndex == 21))
                {
                    modifiers.Add("Aurelionite", 1);
                }

                //todo посмотреть их id
                //if ((mg.destinationScene.baseSceneName == "conduitcanyon" || mg.destinationScene.baseSceneName == "repurposedcrater"))
                //{
                //    modifiers.Add("DroneBossRed", 1);
                //}
            }

            int routePortalCredits = routeCredits;
            int limit = 100;
            while (routePortalCredits > 0 && limit > 0)
            {
                limit--;
                ModifierDef currentModifier = SelectModifier(weightedSelection, routePortalCredits, bannedFromSelection);
                if (currentModifier == null) break;

                foreach (var name in currentModifier.conflictList)
                {
                    bannedFromSelection.Add(name);
                }

                if (modifiers.ContainsKey(currentModifier.name))
                {
                    modifiers[currentModifier.name]++;
                }
                else
                {
                    modifiers.Add(currentModifier.name, 1);
                }
                routePortalCredits -= currentModifier.cost;

            }
            Log.Error(weightedSelection.Count);
            List<StageModifier> res = new List<StageModifier>();
            foreach (var mod in modifiers)
            {
                res.Add(StageModifierCatalog.StageModifierFromDef(mod.Key, mod.Value));
            }
            return res;
        }

        private ModifierDef SelectModifier(WeightedSelection<ModifierDef> weightedSelection, int maxCost, HashSet<string> bannedFromSelection)
        {
            WeightedSelection<ModifierDef> filtered = new WeightedSelection<ModifierDef>();

            for (int i = 0; i < weightedSelection.Count; i++)
            {
                WeightedSelection<ModifierDef>.ChoiceInfo choice = weightedSelection.GetChoice(i);
                if (choice.value.cost <= maxCost && !bannedFromSelection.Contains(choice.value.name))
                {
                    filtered.AddChoice(choice);
                }
            }
            if (filtered.Count == 0)
            {
                Log.Error("Could not select modfiier");
                return null;
            }
            else
            {
                Log.Info($"Filtered has {filtered.Count} entries");
            }

            return filtered.Evaluate(RoR2.Run.instance.runRNG.nextNormalizedFloat);
        }

        private WeightedSelection<ModifierDef> CreateAvailableModifierSelection(GameObject portal)
        {
            WeightedSelection<ModifierDef> res = new WeightedSelection<ModifierDef>();

            foreach (var modDef in allModifiers)
            {
                if (modDef.isAvailable())
                {
                    res.AddChoice(modDef, modDef.selectionWeight);
                    Log.Info($"Added {modDef.name}");
                }
            }

            return res;
        }

        public void AddRoutePortalInstance(GameObject portal)
        {
            if (portal != null)
            {
                var mg = portal.GetComponent<RoutePortalManager>();
                if(mg != null)
                {
                    activePortalInstances.Add(portal);
                    Log.Info($"StageModifierDirector: Added portal instance type {mg.routeNode.portalType}");

                }
                else
                {
                    Log.Error("No route manager for added portal");
                }
            }
            else
            {
                Log.Error("Portal is null");
            }
        }

        public void GetModifiersFromRandomPortal()
        {
            if(activePortalInstances == null || activePortalInstances.Count == 0)
            {
                Log.Error("There are no portal instances to get modifiers from");
                return;
            }
            var portal = Run.instance.stageRng.NextElementUniform(activePortalInstances);
            var mg = portal.GetComponent<RoutePortalManager>();
            if (mg != null)
            {
                reservedModifiers = mg.stageModifiers;
                foreach(var p in activePortalInstances)
                {
                    var gi = p.GetComponent<GenericInteraction>();
                    if(gi!=null)
                    {
                        gi.enabled = false;
                    }
                }
            }
            else
            {
                Log.Error("RoutePortalManager wasn available for random portal");
            }
        }


        [Server]
        private bool CheckPlayersRedItem()
        {
            int redItemCount = 0;
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController == null) continue;
                foreach (ItemIndex item in cm.inventory.itemAcquisitionOrder)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    if (itemDef != null)
                    {
                        if (itemDef.tier == ItemTier.Tier3)
                        {
                            redItemCount++;
                            Log.Info($"Found red item:{itemDef}");
                        }
                    }
                }
            }
            Log.Info($"Players have {redItemCount} red items right now");

            return redItemCount > 0;
        }

        [Server]
        private bool CheckPlayersYellowItem()
        {
            int yellowItemCount = 0;
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController == null) continue;
                if (!cm.inventory) continue;
                foreach (ItemIndex item in cm.inventory.itemAcquisitionOrder)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    if (itemDef != null)
                    {
                        if (itemDef.tier == ItemTier.Boss)
                        {

                            yellowItemCount += cm.inventory.GetItemCount(item);
                        }
                    }
                }
            }
            Log.Info($"Players have {yellowItemCount} boss items right now");

            return yellowItemCount > 0;
        }

        //TODO можно проверить и активки
        [Server]
        private bool CheckPlayersHaveConsumedItems()
        {
            int consumedItemsCount = 0;
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController == null) continue;
                if (!cm.inventory) continue;
                foreach (ItemIndex item in cm.inventory.itemAcquisitionOrder)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    if (itemDef != null)
                    {
                        if (itemDef.isConsumed && itemDef != RoR2.DLC1Content.Items.RegeneratingScrapConsumed && itemDef != RoR2.DLC2Content.Items.LowerPricedChestsConsumed && itemDef != RoR2.DLC2Content.Items.TeleportOnLowHealthConsumed)
                        {
                            consumedItemsCount++;
                        }
                    }
                }
            }
            Log.Info($"Players have {consumedItemsCount} consumed items right now");

            return consumedItemsCount > 0;
        }

        [Server]
        private bool CheckMountainShrine()
        {
            if (RoR2.TeleporterInteraction.instance)
            {
                return RoR2.TeleporterInteraction.instance.shrineBonusStacks > 0;
            }
            return false;
        }

        [Server]
        private bool CheckArtifactOfDeathAvailable()
        {
            return (Run.instance.participatingPlayerCount > 1) && (!runArtifactsApplied.Contains("TeamDeath"));
        }

        [Server]
        private bool CheckArtifactAvailable(string name)
        {
            return (!runArtifactsApplied.Contains(name));
        }


        [Server]
        private int CheckPlayersHaveFoodItems()
        {
            List<ItemDef> foodRelated = new List<ItemDef>()
            {
                RoR2.RoR2Content.Items.FlatHealth,
                RoR2.RoR2Content.Items.Mushroom,
                RoR2.RoR2Content.Items.Infusion,
                RoR2.RoR2Content.Items.HealWhileSafe,
                RoR2.DLC1Content.Items.MushroomVoid

            };
            int foodItems = 0;
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController == null) continue;
                if (!cm.inventory) continue;
                foreach (ItemIndex item in cm.inventory.itemAcquisitionOrder)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    if (itemDef != null)
                    {
                        if (foodRelated.Contains(itemDef))
                        {
                            foodItems += cm.inventory.GetItemCount(itemDef);
                        }
                    }
                }
            }
            Log.Info($"Players have {foodItems} food related items right now");
            return foodItems;
        }

        public List<RouteMap.RouteNode> GetAvailableNodes()
        {
            if (current != null)
                return current.GetPaths();
            else
            {
                Log.Error("Current is null");
                return null;
            }
        }
    }

    public struct ItemInfo
    {
        public string internalName;
        public int cost;
        public float weight;

        public ItemInfo(string internalName, int cost, float weight)
        {
            this.internalName = internalName;
            this.cost = cost;
            this.weight = weight;
        }

        public ItemInfo(string internalName, ModifierTier tier, float weight)
        {
            this.internalName = internalName;
            this.cost = tier switch
            {
                ModifierTier.Tier1 => 30,
                ModifierTier.Tier2 => 60,
                ModifierTier.Tier3 => 120,
                _ => 30,
            };
            this.weight = weight;
        }
    }
}


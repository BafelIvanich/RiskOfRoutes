using RiskOfRoutes;
using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin.StageModifiers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class SyncListModifier : SyncListStruct<ModifierSync> { }
    public class SyncListRouteNode : SyncListStruct<RouteNodeSync> { }

    public class StageModifierDirector : NetworkBehaviour
    {
        public static StageModifierDirector instance;

        public static List<GameObject> activePortalInstances;

        public List<StageModifier> activeModifiers { get; set; }
        public List<ModifierSync> reservedModifiers { get; set; }

        public List<ModifierDef> bannedModifiers { get; set; }
        public List<string> runArtifactsApplied;

        [SyncVar(hook = nameof(OnModifiersSyncedUpdate))]
        public SyncListModifier modifiersSynced = new SyncListModifier();

        [SyncVar(hook = nameof(OnRouteNodesSyncedUpdate))]
        public SyncListRouteNode routeNodesSynced = new SyncListRouteNode();

        [SyncVar]
        public int mapWidth;

        [SyncVar]
        public int mapHeight;

        [SyncVar]
        public bool isMoonVisited;

        [SyncVar]
        public Vector2 currentNodeSync = new Vector2(-1, -1);

        private static void OnModifiersSyncedUpdate()
        {
            Log.Info("SyncedModifiers were updated");
        }

        private static void OnRouteNodesSyncedUpdate()
        {
            Log.Info("RouteNodes were updated");
        }

        public int startingCredits = 200;
        public int routeCredits = 60;
        public int negativeCredits = 30;
        public int uniqueModifierLimit = 5;
        public int printerItemModifierLimit = 3;

        public Inventory monsterTeamInventory;

        public RouteMap routeMap;
        public RouteMap.RouteNode currentNode;

        [SyncVar(hook = nameof(OnBaseCurseSyncedUpdate))]
        public float syncedBaseCurse = 0.0f;
        [SyncVar]
        public int soulCostStack = 0;

        private void OnBaseCurseSyncedUpdate(float newValue)
        {
            syncedBaseCurse = newValue;

            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController == null) continue;
                if (cm && cm.GetBody())
                {
                    cm.GetBody().RecalculateStats();
                }
            }
        }

        [SyncVar(hook = nameof(OnStageUpdate))]
        public int currStage = 0;

        public override void OnStartServer()
        {
            base.OnStartServer();

            CreateDirector(Run.instance);
        }

        //TODO обработка случая когда произошел переход на след стейдж но не поменялся нод
        private void CreateDirector(Run run)
        {
            Log.Info("Run started");

            runArtifactsApplied = GetArtifactsAppliedThisRun();

            Log.Info("Creating route map");
            routeMap = new RouteMap(run.runRNG);
            UpdateSyncedMap();

            currentNode = routeMap.GetStartingNode();
            currentNode.visited = true;
            currentNodeSync = new Vector2(currentNode.x, currentNode.y);
            Log.Info($"Current:{currentNode.x},{currentNode.y} that has {currentNode.GetPaths().Count} paths");
        }

        public void UpdateSyncedMap()
        {
            routeNodesSynced.Clear();
            int width = routeMap.width;
            int height = routeMap.height;
            for (int x = 0; x < width + 1; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (routeMap.mapGrid[x, y] != null)
                    {
                        routeNodesSynced.Add(new RouteNodeSync(routeMap.mapGrid[x, y]));
                    }
                    else
                    {
                        routeNodesSynced.Add(new RouteNodeSync(new RouteMap.RouteNode(-1, -1)));
                    }
                }
            }
            mapWidth = width;
            mapHeight = height;
            isMoonVisited = routeMap.isMoonVisited;
        }


        public void Awake()
        {
            if (!NetworkServer.active) return;
            Log.Info("Called method awake in stage modifier director");
            monsterTeamInventory = Object.Instantiate(RiskOfRoutes.RiskOfRoutes.networkedInventoryPrefab).GetComponent<Inventory>();

            instance = this;

            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<ModifierSync>();
            bannedModifiers = new List<ModifierDef>();

            activePortalInstances = new List<GameObject>();
            DontDestroyOnLoad(gameObject);

            //RegisterModDefs();

            Log.Info("StageModifierDirector created");

            RoR2.Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;

            RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
            RoR2.Stage.onServerStageComplete += Stage_onServerStageComplete;

            //
            Log.Info("monsterTeamInventory has value: " + monsterTeamInventory);
            monsterTeamInventory.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;
            NetworkServer.Spawn(monsterTeamInventory.gameObject);

        }

        public void OnDestroy()
        {
            RoR2.Run.onRunDestroyGlobal -= Run_onRunDestroyGlobal;
            RoR2.SceneDirector.onPrePopulateSceneServer -= SceneDirector_onPrePopulateSceneServer;
            RoR2.Stage.onServerStageComplete -= Stage_onServerStageComplete;
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
            routeNodesSynced.Clear();
            if (this.gameObject)
            {
                Destroy(this.gameObject);
                Log.Info("\tStageModifierDirector destroyed");
            }

            if (monsterTeamInventory)
            {
                NetworkServer.Destroy(monsterTeamInventory.gameObject);
            }

            monsterTeamInventory = null;

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

            activeModifiers.Clear();
            foreach (var modifier in reservedModifiers)
            {
                StageModifier mod = StageModifierCatalog.StageModifierFromDef(modifier.name, modifier.stack);
                activeModifiers.Add(mod);
            }

            foreach (var mod in reservedModifiers)
            {
                modifiersSynced.Add(mod);
                if (mod.name == "SoulCost")
                {
                    Log.Info("Found soulcost");
                    soulCostStack = mod.stack;
                }
            }
            reservedModifiers.Clear();

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
            OnStageUpdate(currStage);
            activePortalInstances.Clear();

            SceneDef currentScene = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
            //if (currentScene.sceneType != SceneType.Intermission)
            //    currentNode.visited = true;


            //todo
        }

        public bool IsModifierActive(string name)
        {
            //Log.Info($"Checking for {name} active");
            foreach (var modifier in modifiersSynced)
            {
                if (modifier.name == name) return true;
            }
            return false;
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

            Log.Info($"Modul:{Stage.instance.sceneDef.stageOrder}");
            //todo пиздец нагромождение, логика создания новой карты 5 стейд -> базар -> 1 стейдж 
            if (Stage.instance.sceneDef.stageOrder == 5 || (currentNode.x == 0 && currentNode.y == 0 && Stage.instance.sceneDef.baseSceneName != "bazaar"))
            {
                Log.Info("Trying to create new map");
                if (TeleporterInteraction.instance.sceneExitController.destinationScene.baseSceneName == "moon2" && TeleporterInteraction.instance.isCharged)
                {
                    routeMap.isMoonVisited = true;
                }
                else
                {
                    routeMap = new RouteMap(Run.instance.runRNG);
                    Log.Info("Created new map");
                    currentNode = routeMap.GetStartingNode();
                    currentNodeSync = new Vector2(currentNode.x, currentNode.y);
                    Log.Info($"Current:{currentNode.x},{currentNode.y} that has {currentNode.GetPaths().Count} paths");
                    Log.Info($"Teleporter:{TeleporterInteraction.instance.sceneExitController.useRunNextStageScene}");

                }
            }
            else
            {
                Log.Info("Not creating new map yet");
                Log.Info($"\t{currentNode.x}:{currentNode.y}");
                Log.Info($"\t{Stage.instance.sceneDef.stageOrder}");
            }


            //TODO возможно стоит поменять порядок
            currentNode.visited = true;
            UpdateSyncedMap();

        }

        public List<ModifierSync> GetNextStageModifiers(GameObject portal)
        {
            WeightedSelection<ModifierDef> weightedSelection = CreateAvailableModifierSelection(portal);
            var mg = portal.GetComponent<RoutePortalManager>();
            HashSet<string> bannedFromSelection = new HashSet<string>();
            Dictionary<string, int> modifiers = new Dictionary<string, int>();

            if (mg)
            {
                //lemurian temple, habitat
                if ((mg.destinationSceneIndex == (int)SceneCatalog.FindSceneIndex("lemurianTemple") || mg.destinationSceneIndex == (int)SceneCatalog.FindSceneIndex("habitat")))
                {
                    modifiers.Add("Aurelionite", 1);
                    Log.Info("Added aurelionite");
                }
                else
                {
                    Log.Info("UIndex:" + mg.destinationSceneIndex + $"{SceneCatalog.GetSceneDef((SceneIndex)mg.destinationSceneIndex)}");
                }

                //todo посмотреть их id
                if (mg.destinationSceneIndex == (int)SceneCatalog.FindSceneIndex("repurposedcrater"))
                {
                    modifiers.Add("DroneBossRed", 1);
                }

                if ((mg.destinationSceneIndex == (int)SceneCatalog.FindSceneIndex("conduitcanyon")))
                {
                    modifiers.Add("UpgradeDrones", 1);
                }

            }

            int routePortalCredits = routeCredits;
            int attempts = 100;
            while (routePortalCredits > 0 && attempts > 0 && modifiers.Count < uniqueModifierLimit)
            {
                attempts--;
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
            List<ModifierSync> res = new List<ModifierSync>();
            foreach (var mod in modifiers)
            {
                var modDef = StageModifierCatalog.FindModifier(mod.Key);
                if (modDef != null)
                {
                    res.Add(new ModifierSync() { name = modDef.name, stack = mod.Value, isArtifact = modDef.isArtifact }); //todo

                }
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

            //var mgr = portal.GetComponent<RoutePortalManager>();
            //if(mgr == null)
            //{
            //    Log.Error("Mgr is null");
            //}
            //if(mgr.routeNode == null)
            //{
            //    Log.Error("error");
            //    return res;
            //}

            foreach (var modName in StageModifierCatalog.AllDefs.Keys)
            {
                var modDef = StageModifierCatalog.FindModifier(modName);
                if (modDef.isAvailable())
                {
                    res.AddChoice(modDef, modDef.selectionWeight);
                    //Log.Info($"Added {modDef.name}");
                }
            }

            return res;
        }

        //public List<ModifierDef> GetModifiersFromCategory(RoutePortalType type)
        //{
        //    //switch (type)
        //    //{
        //    //    case RoutePortalType.None: return allModifiers;
        //    //    case RoutePortalType.ItemType: return itemModifiersPool;
        //    //    case RoutePortalType.DroneType: return droneModifiersPool;
        //    //    case RoutePortalType.Rare: return rareModifiersPool;
        //    //    default: return allModifiers;
        //    //}
        //}

        public void AddRoutePortalInstance(GameObject portal)
        {
            if (portal != null)
            {
                var mg = portal.GetComponent<RoutePortalManager>();
                if (mg != null)
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
            if (activePortalInstances == null || activePortalInstances.Count == 0)
            {
                Log.Error("There are no portal instances to get modifiers from");
                return;
            }
            var portal = Run.instance.stageRng.NextElementUniform(activePortalInstances);
            var mg = portal.GetComponent<RoutePortalManager>();
            if (mg != null)
            {
                reservedModifiers = mg.stageModifiers;
                foreach (var p in activePortalInstances)
                {
                    var gi = p.GetComponent<GenericInteraction>();
                    if (gi != null)
                    {
                        gi.enabled = false;
                    }
                }
            }
            else
            {
                Log.Error("RoutePortalManager wasn available for random portal to get Modifiers");
            }
        }

        public void GetRouteNodeFromRandomPortal()
        {
            if (activePortalInstances == null || activePortalInstances.Count == 0)
            {
                Log.Error("There are no portal instances to get routeNode from");
                //TODO взять рандомный
                return;
            }
            var portal = Run.instance.stageRng.NextElementUniform(activePortalInstances);
            var mg = portal.GetComponent<RoutePortalManager>();
            if (mg != null)
            {
                currentNode = mg.routeNode;
                currentNodeSync = new Vector2(currentNode.x, currentNode.y);
                foreach (var p in activePortalInstances)
                {
                    var gi = p.GetComponent<GenericInteraction>();
                    if (gi != null)
                    {
                        gi.enabled = false;
                    }
                }
            }
            else
            {
                Log.Error("RoutePortalManager wasn available for random portal to get Node");
            }
        }


        [Server]
        public bool CheckPlayersRedItem()
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
        public bool CheckPlayersYellowItem()
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
        public bool CheckPlayersHaveConsumedItems()
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
        public bool CheckMountainShrine()
        {
            if (RoR2.TeleporterInteraction.instance)
            {
                return RoR2.TeleporterInteraction.instance.shrineBonusStacks > 0;
            }
            return false;
        }

        [Server]
        public bool CheckArtifactOfDeathAvailable()
        {
            return (Run.instance.participatingPlayerCount > 1) && (!runArtifactsApplied.Contains("TeamDeath"));
        }

        [Server]
        public bool CheckArtifactAvailable(string name)
        {
            if (name == "Mystery")
            {
                Log.Info("Mystery:" + !runArtifactsApplied.Contains(name));
            }
            return (!runArtifactsApplied.Contains(name));
        }


        [Server]
        public int CheckPlayersHaveFoodItems()
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
            if (currentNode != null)
                return currentNode.GetPaths();
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


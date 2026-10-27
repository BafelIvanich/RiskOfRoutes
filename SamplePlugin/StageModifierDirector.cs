using RiskOfRoutes;
using RiskOfRoutes.Helpers;
using RiskOfRoutes.Items;
using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin.StageModifiers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class SyncListModifier : SyncListStruct<ModifierSync> { }
    public class SyncListRouteNode : SyncListStruct<RouteNodeSync> { }

    public class ModifierSlotConfiguration
    {
        public Dictionary<ModifierTier, int> positive = new Dictionary<ModifierTier, int>();
        public Dictionary<ModifierTier, int> negative = new Dictionary<ModifierTier, int>();
    }

    public class StageModifierDirector : NetworkBehaviour
    {
        public static StageModifierDirector instance;

        public static List<GameObject> activePortalInstances;

        public List<StageModifier> activeModifiers { get; set; }
        public List<ModifierSync> reservedModifiers { get; set; }

        public HashSet<string> bannedModifiersRun { get; set; }
        public HashSet<string> bannedModifiersStage { get; set; }

        public List<string> runArtifactsApplied;

        public SyncListModifier modifiersSynced = new SyncListModifier();
        public List<RouteNodeSync> routeNodesSynced = new List<RouteNodeSync>();
        public int mapState = 0;

        public float stageEnterTime = 0;

        [SyncVar]
        public int mapWidth;

        [SyncVar]
        public int mapHeight;

        [SyncVar]
        public bool isMoonVisited;

        [SyncVar]
        public Vector2 currentNodeSync = new Vector2(-1, -1);

        public Inventory monsterTeamInventory;

        public RouteMap routeMap;
        public RouteMap.RouteNode currentNode;

        [SyncVar(hook = nameof(OnBaseCurseSyncedUpdate))]
        public float syncedBaseCurse = 0.0f;
        [SyncVar]
        public int cursePerStack = 0;
        //
        public int punishCommandStack = 0;
        // run configuration
        [SyncVar]
        public float punishmentTime = -1;
        [SyncVar]
        public bool purifyLunarItemModifier;
        [SyncVar]
        public int doppelStackTime = 30;
        [SyncVar]
        public bool modifiersCanBeBanned = true;
        [SyncVar]
        public int commonDefaultStack = 5;
        [SyncVar]
        public int uncommonDefaultStack = 3;
        [SyncVar]
        public int legendaryDefaultStack = 1;
        [SyncVar]
        public int bossDefaultStack = 1;
        [SyncVar]
        public int commonExtraRange = 3;
        [SyncVar]
        public int uncommonExtraRange = 2;
        [SyncVar]
        public int legendaryExtraRange = 1;
        [SyncVar]
        public int bossExtraRange = 1;
        [SyncVar]
        public int chanceForExtra = 0;
        //modifier pools

        public void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<ModifierSync>();
            bannedModifiersRun = new HashSet<string>();
            bannedModifiersStage = new HashSet<string>();
            activePortalInstances = new List<GameObject>();

            Log.Info($"{nameof(Awake)}: StageModifierDirector created");

            if (!NetworkServer.active) return;

            RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;
            RoR2.Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;

            RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
            RoR2.Stage.onServerStageComplete += Stage_onServerStageComplete;
        }
        public void OnDestroy()
        {
            RoR2.Run.onRunDestroyGlobal -= Run_onRunDestroyGlobal;
            RoR2.Run.onRunStartGlobal -= Run_onRunStartGlobal;

            RoR2.SceneDirector.onPrePopulateSceneServer -= SceneDirector_onPrePopulateSceneServer;
            RoR2.Stage.onServerStageComplete -= Stage_onServerStageComplete;
        }

        public void OnConfigUpdated()
        {
            bannedModifiersRun.Clear();

            var bannedPrinterItems = RiskOfRoutes.RiskOfRoutes.bannedPrinterItems;
            var bannedEnemyItems = RiskOfRoutes.RiskOfRoutes.bannedEnemyItems;

            foreach (var item in bannedEnemyItems)
            {
                bannedModifiersRun.Add(item + "_EnemyItem");
            }

            foreach (var item in bannedPrinterItems)
            {
                bannedModifiersRun.Add(item + "_Printer");
            }

            foreach (var itemDef in RoR2.ItemCatalog.allItemDefs)
            {
                if (itemDef.hidden)
                {
                    bannedModifiersRun.Add(itemDef.name + "_EnemyItem");
                    bannedModifiersRun.Add(itemDef.name + "_Printer");
                }


                if (itemDef.ContainsTag(ItemTag.AIBlacklist))
                {
                    if (!RiskOfRoutes.RiskOfRoutes.canAppearWithAIBlacklisted.Value)
                    {
                        bannedModifiersRun.Add(itemDef.name + "_EnemyItem");
                    }
                }

                if (itemDef.tier == ItemTier.Boss)
                {
                    if (!RiskOfRoutes.RiskOfRoutes.enemyItemsAllowBoss.Value)
                    {
                        bannedModifiersRun.Add(itemDef.name + "_EnemyItem");
                    }
                }
                else if (itemDef.tier == ItemTier.VoidTier1 || itemDef.tier == ItemTier.VoidTier2 || itemDef.tier == ItemTier.VoidTier3)
                {
                    if (!RiskOfRoutes.RiskOfRoutes.enemyItemsAllowVoid.Value)
                    {
                        bannedModifiersRun.Add(itemDef.name + "_EnemyItem");
                    }
                }

                //
                if ((itemDef.tier != ItemTier.Tier1 && itemDef.tier != ItemTier.Tier2) || (itemDef.ContainsTag(ItemTag.CannotDuplicate)))
                {
                    bannedModifiersRun.Add(itemDef.name + "_Printer");
                }
            }
        }

        private void Run_onRunStartGlobal(Run run)
        {
            if (!NetworkServer.active) return;

            var invObj = Instantiate(RiskOfRoutes.RiskOfRoutes.networkedInventoryPrefab);
            DontDestroyOnLoad(invObj);

            monsterTeamInventory = invObj.GetComponent<Inventory>();
            invObj.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;

            NetworkServer.Spawn(invObj);

            CreateRunConfiguration();

            Log.Info($"{nameof(Run_onRunStartGlobal)}: Got punishmentTimeMinutes value - {punishmentTime}");

            Log.Info("Creating route map");

            routeMap = new RouteMap(run.runRNG);

            currentNode = routeMap.GetStartingNode();
            currentNode.visited = true;
            currentNodeSync = new Vector2(currentNode.x, currentNode.y);
            Log.Info($"Current:{currentNode.x},{currentNode.y} that has {currentNode.GetPaths().Count} paths");
            routeMap.isDirty = true;
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            //if (!NetworkServer.active) return;

            Log.Info($"{nameof(Run_onRunDestroyGlobal)}: Run ended, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                if (modifier == null)
                {
                    Log.Error($"{nameof(Run_onRunDestroyGlobal)}: Some active modifier was null");
                    continue;
                }
                modifier.OnEnd();
            }
            reservedModifiers.Clear();
            runArtifactsApplied.Clear();
            modifiersSynced.Clear();
            routeNodesSynced.Clear();
            if (this.gameObject)
            {
                Destroy(this.gameObject);
                Log.Info($"{nameof(Run_onRunDestroyGlobal)}: StageModifierDirector destroyed");
            }

            if (monsterTeamInventory)
            {
                NetworkServer.Destroy(monsterTeamInventory.gameObject);
            }
            monsterTeamInventory = null;

        }

        private void SceneDirector_onPrePopulateSceneServer(SceneDirector obj)
        {
            Log.Info($"{nameof(SceneDirector_onPrePopulateSceneServer)}: Stage started, applying {reservedModifiers.Count} modifiers");

            if (reservedModifiers.Count == 0) return;

            activeModifiers.Clear();
            bannedModifiersStage.Clear();

            foreach (var modDef in reservedModifiers)
            {
                StageModifier mod = StageModifierCatalog.StageModifierFromDef(modDef.name, modDef.stack);
                if (mod == null)
                {
                    Log.Error($"{nameof(SceneDirector_onPrePopulateSceneServer)}: Modifier with def {modDef.name} is null");
                    continue;
                }
                activeModifiers.Add(mod);
                if (modifiersCanBeBanned)
                    bannedModifiersStage.Add(modDef.name);
            }

            foreach (var mod in reservedModifiers)
            {
                modifiersSynced.Add(mod);
                if (mod.name == "SoulCost")
                {
                    //todo maybe it can be done in OnStart of hte modifier
                    cursePerStack = mod.stack;
                }
            }

            activePortalInstances.Clear();
            reservedModifiers.Clear();
            foreach (StageModifier modifier in activeModifiers)
            {
                if (modifier != null) modifier.OnStart();
            }

            SceneDef currentScene = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
            stageEnterTime = Run.instance.GetRunStopwatch();
        }


        private void Stage_onServerStageComplete(RoR2.Stage obj)
        {
            Log.Info($"Stage completed, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                if (modifier == null)
                {
                    Log.Error("Some of the modifiers is null");
                    continue;
                }
                modifier.OnEnd();
            }

            activeModifiers.Clear();
            modifiersSynced.Clear();

            Log.Info($"Scene order:{Stage.instance.sceneDef.stageOrder}");

            //logic of stage 5 -> bazaar -> new map
            if (Stage.instance.sceneDef.stageOrder == 5 || (currentNode.x == 0 && currentNode.y == 0 && Stage.instance.sceneDef.baseSceneName != "bazaar") && Stage.instance.sceneDef.sceneType == SceneType.Stage)
            {
                Log.Info("Trying to create new map");
                if (TeleporterInteraction.instance && TeleporterInteraction.instance.sceneExitController.destinationScene.baseSceneName == "moon2" && TeleporterInteraction.instance.isCharged)
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


            currentNode.visited = true;
            routeMap.isDirty = true;
        }

        private void CreateRunConfiguration()
        {
            commonDefaultStack = RiskOfRoutes.RiskOfRoutes.commonDefaultStack.Value;
            uncommonDefaultStack = RiskOfRoutes.RiskOfRoutes.uncommonDefaultStack.Value;
            legendaryDefaultStack = RiskOfRoutes.RiskOfRoutes.legendaryDefaultStack.Value;
            bossDefaultStack = RiskOfRoutes.RiskOfRoutes.bossDefaultStack.Value;

            commonExtraRange = RiskOfRoutes.RiskOfRoutes.commonExtraRange.Value;
            uncommonExtraRange = RiskOfRoutes.RiskOfRoutes.uncommonExtraRange.Value;
            legendaryExtraRange = RiskOfRoutes.RiskOfRoutes.legendaryExtraRange.Value;
            bossExtraRange = RiskOfRoutes.RiskOfRoutes.bossExtraRange.Value;

            chanceForExtra = RiskOfRoutes.RiskOfRoutes.chanceForExtra.Value;

            runArtifactsApplied = GetArtifactsAppliedThisRun();
            if (RiskOfRoutes.RiskOfRoutes.negativeStackPunishment.Value)
                punishmentTime = RiskOfRoutes.RiskOfRoutes.punishmentTimeMinutes.Value;
            else
                punishmentTime = -1;

            purifyLunarItemModifier = RiskOfRoutes.RiskOfRoutes.lunarTurnToPearl.Value;
            doppelStackTime = RiskOfRoutes.RiskOfRoutes.doppelStackTime.Value;
            modifiersCanBeBanned = RiskOfRoutes.RiskOfRoutes.modifiersCanBeBanned.Value;

            //banlist
            bannedModifiersRun.Clear();

            //modifiers

            var bannedPrinterItems = RiskOfRoutes.RiskOfRoutes.bannedPrinterItems;
            var bannedEnemyItems = RiskOfRoutes.RiskOfRoutes.bannedEnemyItems;
            var bannedLunarItems = RiskOfRoutes.RiskOfRoutes.bannedLunarDefs;
            var bannedArtifacts = RiskOfRoutes.RiskOfRoutes.bannedArtifactDefs;
            var bannedModifierDefs = RiskOfRoutes.RiskOfRoutes.bannedModifierDefs;

            foreach (var item in bannedEnemyItems)
            {
                bannedModifiersRun.Add(item + "_EnemyItem");
            }

            foreach (var item in bannedPrinterItems)
            {
                bannedModifiersRun.Add(item + "_Printer");
            }

            foreach (var item in bannedLunarItems)
            {
                bannedModifiersRun.Add(item + "_LunarItem");
            }

            foreach (var art in bannedArtifacts)
            {
                bannedModifiersRun.Add(art);
            }

            foreach (var banned in RiskOfRoutes.RiskOfRoutes.bannedModifierDefs)
            {
                bannedModifiersRun.Add(banned);
            }

            //configured

            var moddedArtifactsInfo = RiskOfRoutes.RiskOfRoutes.moddedArtifactsInfo;
            var moddedLunarsInfo = RiskOfRoutes.RiskOfRoutes.moddedLunarsInfo;

            foreach (var art in RoR2.ArtifactCatalog.artifactDefs)
            {
                if (StageModifierCatalog.artifactTiers.ContainsKey(art.cachedName)) continue;

                if (!moddedArtifactsInfo.Any(defInfo => defInfo.name == art.cachedName)) bannedModifiersRun.Add(art.cachedName);
            }

            foreach (var lunar in StageModifierCatalog.lunarRegisteredDefs)
            {
                if (StageModifierCatalog.lunarItemTiers.ContainsKey(lunar.name)) continue;

                if (!moddedLunarsInfo.Any(defInfo => defInfo.name == lunar.name)) bannedModifiersRun.Add(lunar.name + "_LunarItem");
            }

            StageModifierCatalog.CreateRunPools();
        }

        private void OnEnable()
        {
            RoR2.Stage.onStageStartGlobal += OnStageStartClient;
        }

        private void OnDisable()
        {
            RoR2.Stage.onStageStartGlobal -= OnStageStartClient;
        }

        private void OnStageStartClient(RoR2.Stage obj)
        {
            if (!NetworkServer.active)
            {
                Log.Info($"{nameof(OnStageStartClient)}: on client");
                NetworkUser user = LocalUserManager.GetFirstLocalUser()?.currentNetworkUser;
                if (user == null) return;
                RoR2.Console.instance.SubmitCmd(user, "request_map");
            }
        }

        [ConCommand(commandName = "request_map", flags = ConVarFlags.ExecuteOnServer, helpText = "request map sync for clients")]
        private static void CCRequestMap(ConCommandArgs args)
        {
            if (instance != null && instance.routeMap != null)
            {
                Log.Info($"{nameof(CCRequestMap)}: requested the map");
                instance.routeMap.isDirty = true;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (NetworkServer.active && routeMap != null && routeMap.isDirty)
            {
                UpdateRouteMap();
                routeMap.isDirty = false;
            }
        }

        private void UpdateRouteMap()
        {
            Log.Info($"{nameof(UpdateRouteMap)}: Updating route map");

            int width = routeMap.width;
            int height = routeMap.height;
            RouteNodeSync[] mapUpdated = new RouteNodeSync[(width + 1) * height];

            for (int x = 0; x < width + 1; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = x * height + y;
                    if (routeMap.mapGrid[x, y] != null)
                        mapUpdated[index] = new RouteNodeSync(routeMap.mapGrid[x, y]);
                    else
                        mapUpdated[index] = new RouteNodeSync(new RouteMap.RouteNode(-1, -1));
                }
            }

            mapWidth = width;
            mapHeight = height;
            isMoonVisited = routeMap.isMoonVisited;
            routeNodesSynced = new List<RouteNodeSync>(mapUpdated);
            RpcReceiveFullMap(mapUpdated, width, height, routeMap.isMoonVisited, stageEnterTime);

        }

        [ClientRpc]
        private void RpcReceiveFullMap(RouteNodeSync[] map, int width, int height, bool moon, float hostStageEnterTime)
        {
            mapWidth = width;
            mapHeight = height;
            isMoonVisited = moon;

            routeNodesSynced = new List<RouteNodeSync>(map);
            mapState++;

            stageEnterTime = hostStageEnterTime;

            Debug.Log($"{nameof(RpcReceiveFullMap)}: Got map on client");
        }

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

        public List<string> GetArtifactsAppliedThisRun()
        {
            List<string> res = new List<string>();
            foreach (var art in RoR2.RunArtifactManager.enabledArtifactsEnumerable)
            {
                res.Add(art.cachedName);
                Log.Info($"{nameof(GetArtifactsAppliedThisRun)}: Added {art.cachedName}");
            }
            return res;
        }

        public void GetNextStageModifiers(GameObject portal)
        {

            var mg = portal.GetComponent<RoutePortalManager>();

            if (mg == null || mg.routeNode == null)
            {
                Log.Error($"{nameof(GetNextStageModifiers)}: Portal doesn't have routePortalManager or RouteNode, couldn't get modifiers.");
                return;
            }

            if (mg.isObjectiveNode)
            {
                var pool = StageModifierCatalog.GetPool(RoutePortalType.MoonTeleporter, isNegative: false);

                Log.Info("Boss modifiers pool: ");
                foreach (var mod in pool)
                {
                    Log.Info($"\t{mod}");
                }
            }

            HashSet<string> banList = new HashSet<string>(bannedModifiersRun);
            banList.UnionWith(bannedModifiersStage);

            Dictionary<string, int> modifiers = new Dictionary<string, int>();

            ModifierSlotConfiguration modifierSlots = GetModifierSlots(Run.instance.stageRng, mg.routeNode.portalType, banList);

            if (SceneHelper.IsSceneColossus((SceneIndex)mg.destinationSceneIndex, includeFalseSonScene: false) || CheckPlayersHaveFalseSonHeart() > 0)
            {
                if (!bannedModifiersRun.Contains("Aurelionite"))
                {
                    modifiers.Add("Aurelionite", 1);
                    Log.Info($"{nameof(GetNextStageModifiers)}: Added aurelionite modifier to colossus portal");
                }
                else Log.Warning("Not adding aurelionite reward to collosus portal because modifier is banned");
            }
            if (mg.destinationSceneIndex == (int)SceneCatalog.FindSceneIndex("conduitcanyon"))
            {
                List<string> canyonMod = new List<string>() { "UpgradeDrones", "DroneBossRed" };
                modifiers.Add(Run.instance.stageRng.NextElementUniform(canyonMod), 1);
                Log.Info($"{nameof(GetNextStageModifiers)}: Added rare drone modifier to hardware portal");
            }
            if (mg.routeNode.portalType == RoutePortalType.ChefType)
            {
                if (!bannedModifiersRun.Contains("WanderingChef"))
                {
                    modifiers.Add("WanderingChef", 1);
                    Log.Info($"{nameof(GetNextStageModifiers)}: Added chef modifier to route portal");
                }
                else Log.Warning("Not adding wandering chef to chef portal because modifier is banned");
            }
            //todo might be made more interesting
            if (CheckMountainShrine() && Run.instance.stageRng.nextNormalizedFloat > 0.5f)
            {
                if (!bannedModifiersRun.Contains("Mountain"))
                    modifiers.Add("Mountain", RoR2.TeleporterInteraction.instance.shrineBonusStacks);
            }
            if (CheckRepairAvailable() && Run.instance.stageRng.nextNormalizedFloat > 0.8f)
            {
                if (!bannedModifiersRun.Contains("Repair"))
                    modifiers.Add("Repair", 1);
            }


            //positive
            var positive = modifierSlots.positive;
            foreach (var tier in positive)
            {
                for (int i = 0; i < tier.Value; i++)
                {
                    var pool = StageModifierCatalog.GetPool(mg.routeNode.portalType, isNegative: false);
                    ModifierDef currentModifier = StageModifierCatalog.SelectModifier(banList, mg.routeNode.portalType, tier.Key, pool);
                    if (currentModifier == null) continue;

                    int stack = CalculateStack(currentModifier, tier.Key);
                    if (currentModifier.name == "DamagePrinterModifier" || currentModifier.name == "HealingPrinterModifier" || currentModifier.name == "UtilityPrinterModifier" || currentModifier.name == "FoodRelatedPrinterModifier")
                    {
                        Log.Info($"{nameof(GetNextStageModifiers)}: got printer modifier");

                        var itemPool = StageModifierCatalog.damageItems;

                        if (currentModifier.name == "DamagePrinterModifier")
                        {
                            itemPool = StageModifierCatalog.damageItems;
                        }
                        else if (currentModifier.name == "HealingPrinterModifier")
                        {
                            itemPool = StageModifierCatalog.healingItems;
                        }
                        else if (currentModifier.name == "UtilityPrinterModifier")
                        {
                            itemPool = StageModifierCatalog.utilityItems;
                        }
                        else if (currentModifier.name == "FoodRelatedPrinterModifier")
                        {
                            itemPool = StageModifierCatalog.foodRelatedItems;
                        }

                        for (int j = 0; j < stack; j++)
                        {
                            ModifierDef printerModifier = StageModifierCatalog.SelectModifier(banList, mg.routeNode.portalType, ModifierTier.Tier1, itemPool);
                            if (printerModifier == null)
                            {
                                Log.Error($"{nameof(GetNextStageModifiers)}: got null printer modifier from item pool");
                                break;
                            }

                            banList.Add(printerModifier.name);
                            if (printerModifier.conflictList != null)
                            {
                                foreach (var name in printerModifier.conflictList) banList.Add(name);
                            }

                            if (modifiers.ContainsKey(printerModifier.name))
                                continue;
                            else
                                modifiers.Add(printerModifier.name, 1);

                        }
                        continue;
                    }

                    banList.Add(currentModifier.name);
                    if (currentModifier.conflictList != null)
                    {
                        foreach (var name in currentModifier.conflictList) banList.Add(name);
                    }


                    if (modifiers.ContainsKey(currentModifier.name))
                        modifiers[currentModifier.name] += stack;
                    else
                        modifiers.Add(currentModifier.name, stack);
                }
            }

            //negative
            var negative = modifierSlots.negative;
            var selectedNegativeDefs = new List<ModifierDef>();

            foreach (var tier in negative)
            {
                for (int i = 0; i < tier.Value; i++)
                {
                    var pool = StageModifierCatalog.GetPool(mg.routeNode.portalType, isNegative: true);
                    ModifierDef currentModifier = StageModifierCatalog.SelectModifier(banList, mg.routeNode.portalType, tier.Key, pool);
                    if (currentModifier == null) continue;
                    //точно выбран

                    int stack = CalculateStack(currentModifier, tier.Key);

                    if (currentModifier.name == "EnemyItemModifier")
                    {
                        Log.Info($"Enemy item stack: {stack}");
                        var enemyPool = StageModifierCatalog.GetPool(mg.routeNode.portalType, isNegative: true, isItemPool: true);
                        currentModifier = StageModifierCatalog.SelectModifier(banList, mg.routeNode.portalType, tier.Key, enemyPool);

                        if (currentModifier == null)
                        {
                            Log.Error($"{nameof(GetNextStageModifiers)}: Item is null");
                            continue;
                        }
                        else
                        {
                            Log.Info($"Selected {currentModifier.name} as enemy item modifier");
                        }
                    }

                    banList.Add(currentModifier.name);
                    if (currentModifier.conflictList != null)
                    {
                        foreach (var name in currentModifier.conflictList) banList.Add(name);
                    }


                    if (modifiers.ContainsKey(currentModifier.name))
                        modifiers[currentModifier.name] += stack;
                    else
                    {
                        modifiers.Add(currentModifier.name, stack);
                        selectedNegativeDefs.Add(currentModifier);
                    }
                }
            }

            //
            int punishStack = (int)((Run.instance.GetRunStopwatch() - stageEnterTime) / (punishmentTime * 60f));
            //test
            punishStack += punishCommandStack;
            if (punishStack > 0) Log.Info($"{nameof(GetNextStageModifiers)}: Adding {punishStack} punishment stacks");
            //среди негативных
            //если модифаер стакающийся то добавляется
            var stackableNegatives = selectedNegativeDefs.Where(s => s.isStackable);
            if (punishStack > 0)
            {
                if (stackableNegatives.Count() == 0)
                {
                    Log.Warning($"{nameof(GetNextStageModifiers)}: There are no stackable negative modifiers to apply punish, adding new");
                    var pool = StageModifierCatalog.GetPool(mg.routeNode.portalType, isNegative: true);

                    while (punishStack > 0)
                    {
                        int cap = Mathf.Min(punishStack, 3);

                        int targetTierInt = Run.instance.stageRng.RangeInt(1, cap + 1);
                        ModifierTier targetTier = (ModifierTier)targetTierInt;

                        var mod = StageModifierCatalog.SelectModifier(banList, mg.routeNode.portalType, targetTier, pool);

                        while (mod == null && targetTierInt > 1)
                        {
                            targetTierInt--;
                            targetTier = (ModifierTier)targetTierInt;
                            mod = StageModifierCatalog.SelectModifier(banList, mg.routeNode.portalType, targetTier, pool);

                        }

                        if (mod == null)
                        {
                            Log.Warning($"{nameof(GetNextStageModifiers)}: Couldnt add new negative modifiers");
                            break;
                        }

                        banList.Add(mod.name);
                        if (mod.conflictList != null)
                        {
                            foreach (var name in mod.conflictList)
                            {
                                banList.Add(name);
                            }
                        }

                        punishStack -= targetTierInt;

                        int addedStacks = 0;
                        if (punishStack > 0 && mod.isStackable)
                        {
                            addedStacks = Run.instance.stageRng.RangeInt(0, punishStack + 1);
                            punishStack -= addedStacks;
                        }

                        modifiers.Add(mod.name, 1 + addedStacks);

                        Log.Info($"{nameof(GetNextStageModifiers)}: Punish added new modifier {mod.name} with stack {1 + addedStacks}");

                    }
                }
                else
                {
                    for (int i = 0; i < punishStack; i++)
                    {
                        var randStackable = Run.instance.stageRng.NextElementUniform(stackableNegatives.ToArray());
                        if (modifiers.ContainsKey(randStackable.name))
                        {
                            modifiers[randStackable.name] += 1;
                            Log.Info($"{nameof(GetNextStageModifiers)}: Added 1 additional stack to {randStackable.name}");
                        }
                    }
                }
            }



            List<ModifierSync> res = new List<ModifierSync>();
            foreach (var mod in modifiers)
            {
                var modDef = StageModifierCatalog.FindModifier(mod.Key);
                if (modDef == null) return;

                res.Add(new ModifierSync(modDef, mod.Value)); //todo
            }
            mg.stageModifiers.Clear();
            foreach (var mod in res)
            {
                mg.stageModifiers.Add($"{mod.name}={mod.stack}={mod.isArtifact}");
            }
            //mg.stageModifiers.Add($"BossSwarmModifier=1=false"); //todo удалить
        }

        private int CalculateStack(ModifierDef def, ModifierTier targetTier)
        {
            //Log.Info($"CurrentTier: {currentTier}, targetTier: {targetTier}");
            var currentTier = def.tier;
            if (currentTier == targetTier)
            {
                return 1;
            }
            if (currentTier == ModifierTier.Tier1 && targetTier == ModifierTier.Tier2 || currentTier == ModifierTier.Tier2 && targetTier == ModifierTier.Tier3)
            {
                return 2;
            }
            if (currentTier == ModifierTier.Tier1 && targetTier == ModifierTier.Tier3)
            {
                return 3;
            }
            else
            {
                return 1;
            }

        }

        public ModifierSlotConfiguration GetModifierSlots(Xoroshiro128Plus rng, RoutePortalType type, HashSet<string> banned)
        {
            var selection = new WeightedSelection<ModifierSlotConfiguration>();
            if (type != RoutePortalType.Rare)
            {
                selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier1, 2 } }, negative = { { ModifierTier.Tier1, 1 } } }, 1.0f);
                selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier1, 2 } }, negative = { { ModifierTier.Tier2, 1 } } }, 1.0f);
                selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier2, 1 } }, negative = { { ModifierTier.Tier1, 2 } } }, 1.0f);
            }

            selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier3, 1 } }, negative = { { ModifierTier.Tier3, 1 } } }, 0.5f);
            selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier3, 1 } }, negative = { { ModifierTier.Tier2, 1 }, { ModifierTier.Tier1, 1 } } }, 0.5f);

            var res = selection.Evaluate(rng.nextNormalizedFloat);

            if (res.positive.ContainsKey(ModifierTier.Tier3))
            {
                var pool = StageModifierCatalog.GetPool(type, isNegative: false);
                var isTier3Available = (StageModifierCatalog.SelectModifier(banned, type, ModifierTier.Tier3, pool) != null);
                if (!isTier3Available)
                {
                    Log.Warning($"{nameof(GetModifierSlots)}: Couldnt get Tier3 modifier for node type {type}, using Tier2");
                    res.positive.Remove(ModifierTier.Tier3);
                    if (res.positive.ContainsKey(ModifierTier.Tier2))
                        res.positive[ModifierTier.Tier2] = res.positive[ModifierTier.Tier2] + 1; //todo tier 2 also should be replaceable with stacked tier1
                    else
                        res.positive.Add(ModifierTier.Tier2, 2);
                }
            }

            //foreach (var tier in res.positive)
            //{
            //    //Log.Info($"\t\t\tTier:{tier.Key}, amount: {tier.Value}");
            //}

            //foreach (var tier in res.negative)
            //{
            //    //Log.Info($"\t\t\tTier:{tier.Key}, amount: {tier.Value}");
            //}


            return res;
        }

        public bool IsModifierActive(string name)
        {
            foreach (var modifier in modifiersSynced)
            {
                if (modifier.name == name) return true;
            }
            return false;
        }

        public void AddRoutePortalInstance(GameObject portal)
        {
            if (portal == null)
            {
                Log.Error($"{nameof(AddRoutePortalInstance)}: Route portal is null, cant add instance");
                return;
            }

            var mg = portal.GetComponent<RoutePortalManager>();
            if (mg == null)
            {
                Log.Error($"{nameof(AddRoutePortalInstance)}: Route portal manager is null, cant add instance");
                return;
            }

            activePortalInstances.Add(portal);
            //Log.Info($"{nameof(AddRoutePortalInstance)}: Added portal instance type {mg.routeNode.portalType}");

        }

        public void GetModifiersFromRandomPortal()
        {
            if (activePortalInstances == null || activePortalInstances.Count == 0)
            {
                Log.Warning($"{nameof(GetModifiersFromRandomPortal)}: There are no portal instances to get modifiers from");
                return;
            }
            var portal = Run.instance.stageRng.NextElementUniform(activePortalInstances);

            var mg = portal.GetComponent<RoutePortalManager>();
            if (mg == null)
            {
                Log.Warning($"{nameof(GetModifiersFromRandomPortal)}: Route portal manager is null, cant get modifiers");
                return;
            }

            reservedModifiers.Clear();
            foreach (var modStr in mg.stageModifiers)
            {
                string[] parts = modStr.Split('=');
                StageModifierDirector.instance.reservedModifiers.Add(new ModifierSync { name = parts[0], stack = int.Parse(parts[1]) });
            }

            foreach (var p in activePortalInstances)
            {
                var genericInteraction = p.GetComponent<GenericInteraction>();
                if (genericInteraction != null)
                {
                    genericInteraction.enabled = false;
                }
            }
        }

        public void GetRouteNodeFromRandomPortal()
        {
            if (activePortalInstances == null || activePortalInstances.Count == 0)
            {
                Log.Warning($"{nameof(GetRouteNodeFromRandomPortal)}: There are no portal active instances to get routeNode from");
                return;
            }

            Log.Info($"{nameof(GetRouteNodeFromRandomPortal)}: Trying to get routeNode from {activePortalInstances.Count} portal instances");

            var portal = Run.instance.stageRng.NextElementUniform(activePortalInstances);
            var mg = portal.GetComponent<RoutePortalManager>();

            if (mg == null)
            {
                Log.Warning($"{nameof(GetRouteNodeFromRandomPortal)}: Selected portal instance missing RoutePortalManager");
                return;
            }

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
                            redItemCount += cm.inventory.GetItemCount(item);
                            Log.Info($"{nameof(CheckPlayersRedItem)}: Found red item:{itemDef}");
                        }
                    }
                }
            }
            Log.Info($"{nameof(CheckPlayersRedItem)}: Players have {redItemCount} red items right now");

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
                            Log.Info($"{nameof(CheckPlayersYellowItem)}: Found yellow item:{itemDef}");
                        }
                    }
                }
            }
            Log.Info($"{nameof(CheckPlayersYellowItem)}: Players have {yellowItemCount} yellow items right now");

            return yellowItemCount > 0;
        }

        //TODO equipment can be added
        [Server]
        public bool CheckRepairAvailable()
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

        public int CheckPlayersHaveFalseSonHeart()
        {
            int stack = 0;
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.playerCharacterMasterController == null) continue;
                if (!cm.inventory) continue;
                foreach (ItemIndex item in cm.inventory.itemAcquisitionOrder)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    if (itemDef != null)
                    {
                        if (itemDef.itemIndex == ModItemsManager.falseSonBless.itemIndex)
                        {
                            stack += cm.inventory.GetItemCount(itemDef);
                        }
                    }
                }
            }
            Log.Info($"Players have {stack} false son bless right now");
            return stack;
        }

        public List<RouteMap.RouteNode> GetAvailableNodes()
        {
            if (currentNode != null)
                return currentNode.GetPaths();
            else
            {
                Log.Error($"{nameof(GetAvailableNodes)}: Current node is null");
                return null;
            }
        }


    }

}


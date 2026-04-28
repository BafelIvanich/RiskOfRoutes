using RiskOfRoutes;
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
        public List<ModifierDef> bannedModifiers { get; set; }
        public List<string> runArtifactsApplied;

        public SyncListModifier modifiersSynced = new SyncListModifier();
        public List<RouteNodeSync> routeNodesSynced = new List<RouteNodeSync>();

        public int mapState = 0;

        public float stageEnterTime = 0; //todo

        [SyncVar]
        public int mapWidth;

        [SyncVar]
        public int mapHeight;

        [SyncVar]
        public bool isMoonVisited;

        [SyncVar]
        public Vector2 currentNodeSync = new Vector2(-1, -1);

        public int uniqueModifierLimit = 5;
        public float punishmentTime = -1;

        public Inventory monsterTeamInventory;

        public RouteMap routeMap;
        public RouteMap.RouteNode currentNode;

        [SyncVar(hook = nameof(OnBaseCurseSyncedUpdate))]
        public float syncedBaseCurse = 0.0f;
        [SyncVar]
        public int soulCostStack = 0;

        /////

        public void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<ModifierSync>();
            bannedModifiers = new List<ModifierDef>();
            activePortalInstances = new List<GameObject>();

            if (!NetworkServer.active) return;

            Log.Info($"{nameof(Awake)}:StageModifierDirector created");

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

        private void Run_onRunStartGlobal(Run run)
        {
            Log.Info($"{nameof(Run_onRunStartGlobal)}: Run started");

            if (NetworkServer.active)
            {
                var invObj = Instantiate(RiskOfRoutes.RiskOfRoutes.networkedInventoryPrefab);
                DontDestroyOnLoad(invObj);

                monsterTeamInventory = invObj.GetComponent<Inventory>();
                invObj.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;

                NetworkServer.Spawn(invObj);
            }

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

        private void CreateRunConfiguration()
        {
            runArtifactsApplied = GetArtifactsAppliedThisRun();
            if (RiskOfRoutes.RiskOfRoutes.negativeStackPunishment.Value)
                punishmentTime = RiskOfRoutes.RiskOfRoutes.punishmentTimeMinutes.Value;
            else
                punishmentTime = -1;

            var enemyTier1Config = RiskOfRoutes.RiskOfRoutes.tier1EnemyItems.Value.Split(",").ToList();
            var enemyTier2Config = RiskOfRoutes.RiskOfRoutes.tier2EnemyItems.Value.Split(",").ToList();
            var enemyTier3Config = RiskOfRoutes.RiskOfRoutes.tier3EnemyItems.Value.Split(",").ToList();

            var printerTier1Config = RiskOfRoutes.RiskOfRoutes.tier1PrinterItems.Value.Split(",").ToList();
            var printerTier2Config = RiskOfRoutes.RiskOfRoutes.tier2PrinterItems.Value.Split(",").ToList();
            var printerTier3Config = RiskOfRoutes.RiskOfRoutes.tier3PrinterItems.Value.Split(",").ToList();

            var lunarTier1Config = RiskOfRoutes.RiskOfRoutes.tier1LunarItems.Value.Split(",").ToList();
            var lunarTier2Config = RiskOfRoutes.RiskOfRoutes.tier2LunarItems.Value.Split(",").ToList();
            var lunarTier3Config = RiskOfRoutes.RiskOfRoutes.tier3LunarItems.Value.Split(",").ToList();

            foreach (var enemyTier1 in enemyTier1Config) { StageModifierCatalog.RegEnemy(enemyTier1, ModifierTier.Tier1); Log.Info($"{enemyTier1} registering as Tier1 enemy item"); }
            foreach (var enemyTier2 in enemyTier2Config) { StageModifierCatalog.RegEnemy(enemyTier2, ModifierTier.Tier1); Log.Info($"{enemyTier2} registering as Tier2 enemy item"); }
            foreach (var enemyTier3 in enemyTier3Config) { StageModifierCatalog.RegEnemy(enemyTier3, ModifierTier.Tier1); Log.Info($"{enemyTier3} registering as Tier3 enemy item"); }

            foreach (var printerTier1 in printerTier1Config) { StageModifierCatalog.RegPrinter(printerTier1, ModifierTier.Tier1); Log.Info($"{printerTier1} registering as Tier1 printer item"); }
            foreach (var printerTier2 in printerTier2Config) { StageModifierCatalog.RegPrinter(printerTier2, ModifierTier.Tier2); Log.Info($"{printerTier2} registering as Tier2 printer item"); }
            foreach (var printerTier3 in printerTier3Config) { StageModifierCatalog.RegPrinter(printerTier3, ModifierTier.Tier3); Log.Info($"{printerTier3} registering as Tier3 printer item"); }

            foreach (var lunarTier1 in lunarTier1Config) { StageModifierCatalog.RegLunar(lunarTier1, ModifierTier.Tier1); Log.Info($"{lunarTier1} registering as Tier1 lunar item"); }
            foreach (var lunarTier2 in lunarTier1Config) { StageModifierCatalog.RegLunar(lunarTier2, ModifierTier.Tier2); Log.Info($"{lunarTier2} registering as Tier2 lunar item"); }
            foreach (var lunarTier3 in lunarTier1Config) { StageModifierCatalog.RegLunar(lunarTier3, ModifierTier.Tier3); Log.Info($"{lunarTier3} registering as Tier3 lunar item"); }
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            Log.Info($"{nameof(Run_onRunDestroyGlobal)}: Run ended, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                if (modifier == null)
                {
                    Log.Error($"{nameof(Run_onRunDestroyGlobal)}: Some active modifier is null");
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


        public override void OnStartServer()
        {
            base.OnStartServer();

            //CreateDirector(Run.instance);
        }

        //TODO обработка случая когда произошел переход на след стейдж но не поменялся нод
        private void CreateDirector(Run run)
        {
            Log.Info("Run started");

            runArtifactsApplied = GetArtifactsAppliedThisRun();

            Log.Info("Creating route map");
            routeMap = new RouteMap(run.runRNG);

            currentNode = routeMap.GetStartingNode();
            currentNode.visited = true;
            currentNodeSync = new Vector2(currentNode.x, currentNode.y);
            Log.Info($"Current:{currentNode.x},{currentNode.y} that has {currentNode.GetPaths().Count} paths");
            routeMap.isDirty = true;

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
            RpcReceiveFullMap(mapUpdated, width, height, routeMap.isMoonVisited, punishmentTime, stageEnterTime);

        }

        [ClientRpc]
        private void RpcReceiveFullMap(RouteNodeSync[] map, int width, int height, bool moon, float hostPunishmentTime, float hostStageEnterTime)
        {
            mapWidth = width;
            mapHeight = height;
            isMoonVisited = moon;

            routeNodesSynced = new List<RouteNodeSync>(map);
            mapState++;

            punishmentTime = hostPunishmentTime;
            stageEnterTime = hostStageEnterTime;

            Debug.Log($"{nameof(RpcReceiveFullMap)}: Got map on client");
            if (hostPunishmentTime != -1)
            {
                Debug.Log($"{nameof(RpcReceiveFullMap)}: Host has punish time of {hostPunishmentTime}");
            }
            else
            {
                Debug.Log($"{nameof(RpcReceiveFullMap)}: Host has punish disabled");
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

            activeModifiers.Clear();
            foreach (var modifier in reservedModifiers)
            {
                StageModifier mod = StageModifierCatalog.StageModifierFromDef(modifier.name, modifier.stack);
                if (mod == null)
                {
                    Log.Error($"{nameof(SceneDirector_onPrePopulateSceneServer)}: Modifier with def {modifier.name} is null");
                    continue;
                }
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


            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
            activePortalInstances.Clear();


            reservedModifiers.Clear();

            SceneDef currentScene = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
            stageEnterTime = Run.instance.fixedTime;
            Log.Info($"{nameof(SceneDirector_onPrePopulateSceneServer)}: StageEnterTime: {stageEnterTime}");
            //if (currentScene.sceneType != SceneType.Intermission)
            //    currentNode.visited = true;


            //todo
        }


        private void Stage_onServerStageComplete(RoR2.Stage obj)
        {
            Log.Info($"Stage completed, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers) modifier.OnEnd();

            activeModifiers.Clear();
            modifiersSynced.Clear();

            Log.Info($"Modul:{Stage.instance.sceneDef.stageOrder}");
            //todo пиздец нагромождение, логика создания новой карты 5 стейд -> базар -> 1 стейдж 
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


            //TODO возможно стоит поменять порядок
            //todo nullreference и софтлок при переходе на луну
            currentNode.visited = true;
            routeMap.isDirty = true;
        }

        public void GetNextStageModifiers(GameObject portal)
        {
            var mg = portal.GetComponent<RoutePortalManager>();

            if (mg == null || mg.routeNode == null)
            {
                Log.Error($"{nameof(GetNextStageModifiers)}: Portal doesn't have routePortalManager or RouteNode, couldn't get modifiers.");
                return;
            }

            HashSet<string> bannedFromSelection = new HashSet<string>();
            Dictionary<string, int> modifiers = new Dictionary<string, int>();

            ModifierSlotConfiguration modifierSlots = GetModifierSlots(Run.instance.stageRng, mg.routeNode.portalType);

            if (SceneHelper.IsSceneColossus((SceneIndex)mg.destinationSceneIndex, includeFalseSonScene: false))
            {
                modifiers.Add("Aurelionite", 1);
                Log.Info($"{nameof(GetNextStageModifiers)}: Added aurelionite modifier to colossus portal");
            }
            if (mg.destinationSceneIndex == (int)SceneCatalog.FindSceneIndex("conduitcanyon"))
            {
                List<string> canyonMod = new List<string>() { "UpgradeDrones", "DroneBossRed" };
                modifiers.Add(Run.instance.stageRng.NextElementUniform(canyonMod), 1);
                Log.Info($"{nameof(GetNextStageModifiers)}: Added rare drone modifier to hardware portal");
            }
            if (mg.routeNode.portalType == RoutePortalType.ChefType)
            {
                modifiers.Add("WanderingChef", 1);
                Log.Info($"{nameof(GetNextStageModifiers)}: Added chef modifier to route portal");
            }
            //todo условие можно сделать интереснее
            if (CheckMountainShrine() && Run.instance.stageRng.nextNormalizedFloat > 0.5f)
            {
                modifiers.Add("Mountain", RoR2.TeleporterInteraction.instance.shrineBonusStacks);
            }
            if (CheckPlayersHaveConsumedItems() && Run.instance.stageRng.nextNormalizedFloat > 0.8f)
            {
                modifiers.Add("Repair", 1);
            }

            //positive
            var positive = modifierSlots.positive;
            foreach (var tier in positive)
            {
                for (int i = 0; i < tier.Value; i++)
                {
                    ModifierDef currentModifier = StageModifierCatalog.SelectModifier(bannedFromSelection, mg.routeNode.portalType, tier.Key);
                    if (currentModifier == null) continue;
                    //точно
                    bannedFromSelection.Add(currentModifier.name);
                    if (currentModifier.conflictList != null)
                    {
                        foreach (var name in currentModifier.conflictList) bannedFromSelection.Add(name);
                    }

                    //TODO
                    //if(!currentModifier.IsStackable() && currentModifier.)

                    int stack = CalculateStack(currentModifier.tier, tier.Key);
                    if (modifiers.ContainsKey(currentModifier.name))
                        modifiers[currentModifier.name] += stack;
                    else
                        modifiers.Add(currentModifier.name, stack);
                }
            }

            //negative

            var negative = modifierSlots.negative;
            foreach (var tier in negative)
            {
                for (int i = 0; i < tier.Value; i++)
                {
                    ModifierDef currentModifier = StageModifierCatalog.SelectModifier(bannedFromSelection, mg.routeNode.portalType, tier.Key, isNegative: true);
                    if (currentModifier == null) continue;
                    //точно выбран
                    bannedFromSelection.Add(currentModifier.name);
                    if (currentModifier.conflictList != null)
                    {
                        foreach (var name in currentModifier.conflictList) bannedFromSelection.Add(name);
                    }

                    Log.Info($"Modifier to calculate stacks for is {currentModifier.name} with tier {currentModifier.tier}");
                    int stack = CalculateStack(currentModifier.tier, tier.Key);
                    Log.Info($"\t Stack for {currentModifier.name}:{stack}");
                    if (modifiers.ContainsKey(currentModifier.name))
                        modifiers[currentModifier.name] += stack;
                    else
                        modifiers.Add(currentModifier.name, stack);
                }
            }

            //
            int punishStack = (int)((Run.instance.fixedTime - stageEnterTime) / (punishmentTime * 60f));
            Log.Info($"{nameof(GetNextStageModifiers)}: Adding punishment stacks");
            //среди негативных
            //если модифаер стакающийся то добавляется
            //

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
        }

        private int CalculateStack(ModifierTier currentTier, ModifierTier targetTier)
        {
            Log.Info($"CurrentTier: {currentTier}, targetTier: {targetTier}");
            if (currentTier == targetTier)
            {
                Log.Info("RETURNING 1");
                return 1;
            }
            if (currentTier == ModifierTier.Tier1 && targetTier == ModifierTier.Tier2)
            {
                Log.Info("RETURNING 2");
                return 2;
            }
            if (currentTier == ModifierTier.Tier1 && targetTier == ModifierTier.Tier3)
            {
                Log.Info("RETURNING 3");
                return 3;
            }
            else
            {
                Log.Error("ELSEJjA");
                return 1;
            }

        }

        public static ModifierSlotConfiguration GetModifierSlots(Xoroshiro128Plus rng, RoutePortalType type)
        {
            var selection = new WeightedSelection<ModifierSlotConfiguration>();
            if (type != RoutePortalType.Rare)
            {
                selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier1, 1 } }, negative = { { ModifierTier.Tier1, 1 } } }, 1.0f);
                selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier1, 2 } }, negative = { { ModifierTier.Tier2, 1 } } }, 1.0f);
                selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier2, 1 } }, negative = { { ModifierTier.Tier1, 2 } } }, 1.0f);
                //todo верунть
            }

            selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier3, 1 } }, negative = { { ModifierTier.Tier3, 1 } } }, 0.3f);
            selection.AddChoice(new ModifierSlotConfiguration { positive = { { ModifierTier.Tier3, 1 } }, negative = { { ModifierTier.Tier2, 1 }, { ModifierTier.Tier1, 1 } } }, 0.3f);

            var res = selection.Evaluate(rng.nextNormalizedFloat);
            Log.Info("\tSelected slots:");
            Log.Info("\t\tPositive:");
            foreach (var tier in res.positive)
            {
                Log.Info($"\t\t\tTier:{tier.Key}, amount: {tier.Value}");
            }
            Log.Info("\t\tNegative");
            foreach (var tier in res.negative)
            {
                Log.Info($"\t\t\tTier:{tier.Key}, amount: {tier.Value}");
            }

            return res;
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
            }

            activePortalInstances.Add(portal);
            Log.Info($"{nameof(AddRoutePortalInstance)}: Added portal instance type {mg.routeNode.portalType}");

        }

        public void GetModifiersFromRandomPortal()
        {
            if (activePortalInstances == null || activePortalInstances.Count == 0)
            {
                Log.Error($"{nameof(GetModifiersFromRandomPortal)}: There are no portal instances to get modifiers from");
                return;
            }
            var portal = Run.instance.stageRng.NextElementUniform(activePortalInstances);

            var mg = portal.GetComponent<RoutePortalManager>();
            if (mg == null)
            {
                Log.Error($"{nameof(GetModifiersFromRandomPortal)}: Route portal manager is null, cant get modifiers");
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

        public RouteMap.RouteNode GetRouteNodeFromRandomPortal()
        {
            if (activePortalInstances == null || activePortalInstances.Count == 0)
            {
                Log.Error($"{nameof(GetRouteNodeFromRandomPortal)}: There are no portal active instances to get routeNode from");
                return null;
            }

            Log.Info($"{nameof(GetRouteNodeFromRandomPortal)}: Trying to get routeNode from {activePortalInstances.Count} portal instances");

            var portal = Run.instance.stageRng.NextElementUniform(activePortalInstances);
            var mg = portal.GetComponent<RoutePortalManager>();

            if (mg == null)
            {
                Log.Error($"{nameof(GetRouteNodeFromRandomPortal)}: Selected portal instance missing RoutePortalManager");
                return null;
            }

            currentNode = mg.routeNode;
            currentNodeSync = new Vector2(currentNode.x, currentNode.y); //todo позже убрать после измененной логики смены currentNode

            foreach (var p in activePortalInstances)
            {
                var gi = p.GetComponent<GenericInteraction>();
                if (gi != null)
                {
                    gi.enabled = false;
                }
            }

            return currentNode;
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
                Log.Error($"{nameof(GetAvailableNodes)}: Current node is null");
                return null;
            }
        }
    }

}


using RoR2;
using RoR2.ExpansionManagement;
using SamplePlugin;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RiskOfRoutes
{
    public class RoutesExpansion
    {
        public static ExpansionDef routesExpansion = ScriptableObject.CreateInstance<ExpansionDef>();

        public static void InitializeExpansion(GameObject networkedObj)
        {
            routesExpansion.nameToken = "EXPANSION_ROUTES_NAME";
            routesExpansion.descriptionToken = "EXPANSION_ROUTES_DESCRIPTION";
            routesExpansion.iconSprite = AssetManager.expansionIcon;
            routesExpansion.disabledIconSprite = AssetManager.lockedIcon;
            routesExpansion.runBehaviorPrefab = networkedObj;

            R2API.ContentAddition.AddExpansionDef(routesExpansion);

            //хуки
            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterEventSpawnRoutePortals;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += TeleporterGetRoutePortal;
            On.RoR2.DirectorCore.TrySpawnObject += OnVanillaPortalSpawn;
            On.RoR2.BazaarController.Start += ReplaceBazaarExits;
            On.RoR2.SeerStationController.SetRunNextStageToTarget += SeerStationController_SetRunNextStageToTarget;

            AddRouteManagersToPortalPrefabs();
            AddHologramControllerToTeleporter();

        }

        private static void AddRouteManagersToPortalPrefabs()
        {
            var shopPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion();
            var goldshoresPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalGoldshores/PortalGoldshores.prefab").WaitForCompletion();
            var colossusPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/PortalColossus.prefab").WaitForCompletion();
            var hardwareProgPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/HardwareProgPortal.prefab").WaitForCompletion();
            var hardwareProgHauntPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/HardwareProgPortal_Haunt.prefab").WaitForCompletion();
            var destinationPortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/PM DestinationPortal.prefab").WaitForCompletion();
            var seerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion();

            List<GameObject> vanillaPortals = new List<GameObject>() { shopPortalPrefab, colossusPortalPrefab, goldshoresPortalPrefab, hardwareProgPortalPrefab, hardwareProgHauntPortalPrefab, destinationPortalPrefab, seerPrefab };
            foreach (var portal in vanillaPortals)
            {
                Log.Info($"{nameof(AddRouteManagersToPortalPrefabs)}: Adding route portal manager to portal {portal.name}");

                var mg = portal.GetComponent<RoutePortalManager>();
                if (mg == null)
                {
                    portal.AddComponent<RoutePortalManager>();
                }

                Log.Info($"{nameof(AddRouteManagersToPortalPrefabs)}: Added route portal manager to {portal.name} prefab");
            }
        }

        private static void AddHologramControllerToTeleporter()
        {
            var teleporterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").WaitForCompletion();

            if (teleporterPrefab != null)
            {
                var hologramController = teleporterPrefab.AddComponent<ModifierHologramController>();

                hologramController.droneTex = AssetManager.droneSymbolTexture;
                hologramController.rewardTex = AssetManager.rewardSymbolTexture;
                hologramController.aurelioniteTex = AssetManager.aurelioniteSymbolTexture;
            }
        }

        private static void TeleporterGetRoutePortal(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator)
        {
            orig(self, activator);

            if (!Run.instance.IsExpansionEnabled(routesExpansion)) return;

            if (!self.isCharged) return;

            if (Stage.instance.sceneDef.baseSceneName == "conduitcanyon")
            {
                Log.Info($"{nameof(TeleporterGetRoutePortal)}: Creating node to Solus Wing stage");

                var hardwareNode = StageModifierDirector.instance.routeMap.CreateHardwareNode(StageModifierDirector.instance.currentNode);
                StageModifierDirector.instance.currentNode = hardwareNode;
                StageModifierDirector.instance.currentNodeSync = new Vector2(hardwareNode.x, hardwareNode.y);
            }
            else
            {
                Log.Info($"{nameof(TeleporterGetRoutePortal)}: Trying to get node and modifiers from random portal");

                StageModifierDirector.instance.GetRouteNodeFromRandomPortal();
                if (RiskOfRoutes.teleporterUseRandomMods.Value)
                    StageModifierDirector.instance.GetModifiersFromRandomPortal(); //todo пересмотреть
            }
        }

        private static void SeerStationController_SetRunNextStageToTarget(On.RoR2.SeerStationController.orig_SetRunNextStageToTarget orig, SeerStationController self)
        {
            orig(self);

            if (!Run.instance.IsExpansionEnabled(routesExpansion)) return;

            if (!NetworkServer.active || SceneManager.GetActiveScene().name != "bazaar")
            {
                return;
            }

            SceneIndex seerTargetScene = (SceneIndex)self.targetSceneDefIndex;

            GameObject shopPortalObj = null;
            SceneExitController shopExitController = null;
            RoR2.SceneExitController[] exits = UnityEngine.Object.FindObjectsOfType<RoR2.SceneExitController>(true);
            foreach (var exit in exits)
            {
                if (exit.useRunNextStageScene || exit.gameObject.name.Contains("Shop"))
                {
                    shopPortalObj = exit.gameObject;
                    shopExitController = exit;
                    shopExitController.useRunNextStageScene = true;
                    Log.Info($"{nameof(SeerStationController_SetRunNextStageToTarget)}: Found shop portal with name {exit.gameObject.name}");
                    break;
                }
            }

            if (!shopPortalObj)
            {
                Log.Error($"{nameof(SeerStationController_SetRunNextStageToTarget)}: Could not find shopportal on bazaar scene");
                return;
            }

            if (!shopPortalObj.GetComponent<RoutePortalManager>())
            {
                Log.Error($"{nameof(SeerStationController_SetRunNextStageToTarget)}: Shop portal doesn't have manager");
                return;
            }

            var seerRouteManager = self.gameObject.GetComponent<RoutePortalManager>();

            var mg = shopPortalObj.GetComponent<RoutePortalManager>();
            int originalSceneIndex = (int)mg.destinationSceneIndex;
            mg.destinationSceneIndex = (int)seerTargetScene;
            if (seerRouteManager)
            {
                mg.stageModifiers.Clear();
                foreach (var mod in seerRouteManager.stageModifiers) mg.stageModifiers.Add(mod);
            }
            else
            {
                Log.Warning($"{nameof(SeerStationController_SetRunNextStageToTarget)}: Seer doensn't have route portal manager, getting random mods for shop portal");
                StageModifierDirector.instance.GetNextStageModifiers(shopPortalObj);
            }

            SceneDef targetSceneDef = SceneCatalog.GetSceneDef((SceneIndex)seerTargetScene);

            if (SceneHelper.IsSceneColossus(targetSceneDef.sceneDefIndex))
            {
                mg.routeNode = seerRouteManager.routeNode;
                mg.nodeSync = new Vector2(mg.routeNode.x, mg.routeNode.y);
            }
            else if (targetSceneDef.sceneType == SceneType.Intermission)
            {
                Log.Info($"{nameof(SeerStationController_SetRunNextStageToTarget)}:Seer scene is intermission, disabling portal mods");
                mg.isUsingModifiers = false;
            }

            Log.Info($"{nameof(SeerStationController_SetRunNextStageToTarget)}: Updated exit destination:{originalSceneIndex}->{mg.destinationSceneIndex}");
        }


        private static void ReplaceBazaarExits(On.RoR2.BazaarController.orig_Start orig, BazaarController self)
        {
            orig(self);

            if (!Run.instance.IsExpansionEnabled(routesExpansion)) return;

            if (!NetworkServer.active) return;

            Log.Info($"{nameof(ReplaceBazaarExits)}: Replacing shop portal");

            GameObject shopPortalObject = null;
            RoR2.SceneExitController[] exits = UnityEngine.GameObject.FindObjectsOfType<RoR2.SceneExitController>(true);
            foreach (var exit in exits)
            {
                if (exit.useRunNextStageScene || exit.gameObject.name.Contains("Shop"))
                {
                    shopPortalObject = exit.gameObject;
                    Log.Info($"{nameof(ReplaceBazaarExits)}:Found shop portal with name {exit.gameObject.name}");
                    break;
                }
            }

            if (shopPortalObject == null)
            {
                Log.Error($"{nameof(ReplaceBazaarExits)}:Shop portal was not found");
                return;
            }

            Vector3 pos = shopPortalObject.transform.position;
            Quaternion rot = shopPortalObject.transform.rotation;

            NetworkServer.Destroy(shopPortalObject);

            GameObject shopRoutePortalPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion();
            GameObject shopRoutePortal = UnityEngine.Object.Instantiate(shopRoutePortalPrefab, pos, rot);

            var shopRouteManager = shopRoutePortal.GetComponent<RoutePortalManager>();
            if (shopRouteManager == null)
            {
                Log.Error($"{nameof(ReplaceBazaarExits)}: Shop portal doesn't have manager");
                return;
            }

            shopRouteManager.destinationSceneIndex = (int)(RoR2.Run.instance.nextStageScene.sceneDefIndex);
            shopRouteManager.isOval = true;
            shopRouteManager.isUsingModifiers = true;
            shopRouteManager.isUsingSceneExit = true;
            shopRouteManager.routeNode = StageModifierDirector.instance.currentNode;
            shopRouteManager.nodeSync = new Vector2(shopRouteManager.routeNode.x, shopRouteManager.routeNode.y); //todo замена node sync сразу в замене routeNode
            shopRouteManager.enabled = true; //TODO ПРОВЕРИТЬ В ИГРЕ
            StageModifierDirector.instance.GetNextStageModifiers(shopRoutePortal);
            Log.Info($"{nameof(ReplaceBazaarExits)}: Shop portal was replaced");

            NetworkServer.Spawn(shopRoutePortal);
            Log.Info($"{nameof(ReplaceBazaarExits)}: Shop portal spawned on network");

            SceneExitController portalSceneExit = shopRoutePortal.GetComponent<SceneExitController>();

            if (portalSceneExit == null || self.seerStations == null)
            {
                if (portalSceneExit == null) Log.Error($"{nameof(ReplaceBazaarExits)}: Portal's sceneExitController is missing, couldn't set seer's destination");
                if (self.seerStations == null) Log.Error($"{nameof(ReplaceBazaarExits)}: SeerStations are missing for some reason");
                return;
            }

            List<SeerStationController> newSeerStations = new List<SeerStationController>();
            foreach (var oldSeerStationController in self.seerStations)
            {
                if (oldSeerStationController == null) continue;

                var oldSeerStationObj = oldSeerStationController.gameObject;

                Vector3 seerPos = oldSeerStationObj.transform.position;
                Quaternion seerRot = oldSeerStationObj.transform.rotation;
                int oldTargetSceneIndex = oldSeerStationController.targetSceneDefIndex;

                NetworkServer.Destroy(oldSeerStationObj);

                GameObject seerStationPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion();
                GameObject newSeer = UnityEngine.Object.Instantiate(seerStationPrefab, seerPos, seerRot);

                var newController = newSeer.GetComponent<SeerStationController>();
                if (newController == null)
                {
                    Log.Error($"{nameof(ReplaceBazaarExits)}: Seer station controller is missing");
                    return;
                }

                newController.targetSceneDefIndex = oldTargetSceneIndex;
                newController.explicitTargetSceneExitController = portalSceneExit;

                var seerRouteManager = newSeer.GetComponent<RoutePortalManager>();
                if (seerRouteManager == null)
                {
                    Log.Error($"{nameof(ReplaceBazaarExits)}: Seer station prefab missing RoutePortalManager");
                    return;
                }

                seerRouteManager.isSeerStation = true;
                seerRouteManager.destinationSceneIndex = oldTargetSceneIndex;

                if (SceneHelper.IsSceneColossus((SceneIndex)oldTargetSceneIndex))
                    seerRouteManager.routeNode = StageModifierDirector.instance.routeMap.CreateColossusNode(StageModifierDirector.instance.currentNode, currentLayer: true);
                else
                    seerRouteManager.routeNode = StageModifierDirector.instance.currentNode;

                seerRouteManager.nodeSync = new Vector2(seerRouteManager.routeNode.x, seerRouteManager.routeNode.y);

                if (!SceneHelper.IsSceneIntermission((SceneIndex)oldTargetSceneIndex))
                {
                    seerRouteManager.isUsingModifiers = true;
                    StageModifierDirector.instance.GetNextStageModifiers(newSeer);
                }
                else
                {
                    seerRouteManager.isUsingModifiers = false;
                }

                seerRouteManager.enabled = true; //todo проверить в игре

                NetworkServer.Spawn(newSeer);
                newSeerStations.Add(newController);
                Log.Info($"{nameof(ReplaceBazaarExits)}: SeerStation was replaced");

            }

            self.seerStations = newSeerStations.ToArray();
        }

        private static GameObject OnVanillaPortalSpawn(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            var obj = orig(self, directorSpawnRequest);

            if (!NetworkServer.active) return obj;

            if (!Run.instance.IsExpansionEnabled(routesExpansion)) return obj;

            if (obj == null) return obj;

            if (directorSpawnRequest.spawnCard == null || directorSpawnRequest.spawnCard.name == null) return obj;

            var spawnCardName = directorSpawnRequest.spawnCard.name;
            if (!spawnCardName.Contains("Portal")) return obj;

            SceneDef currentScene = RoR2.SceneCatalog.GetSceneDefForCurrentScene();

            Log.Info($"{nameof(OnVanillaPortalSpawn)}: Director spawned {spawnCardName} portal");

            var mgr = obj.GetComponent<RoutePortalManager>();
            if (!mgr)
            {
                Log.Error($"{nameof(OnVanillaPortalSpawn)}: RoutePortalManager is missing for portal {spawnCardName}");
                return obj;
            }

            Log.Info($"{nameof(OnVanillaPortalSpawn)}: Is manager enabled for {spawnCardName} - {mgr.enabled}");

            if (spawnCardName == "iscRoutePortal") return obj;

            if (spawnCardName == "iscColossusPortal")
            {
                mgr.destinationSceneIndex = SceneHelper.GetGreenPortalSceneIndex();
                mgr.isOval = true;
                mgr.isUsingModifiers = Stage.instance.sceneDef.stageOrder != 3;

                RouteMap.RouteNode colossusNode = null;

                //зеленый портал вызывается с Goldshores или с магазина
                if (currentScene != null && currentScene.sceneType == SceneType.Intermission)
                {
                    Log.Info($"{nameof(OnVanillaPortalSpawn)}:Spawned colossus portal from intermission scene");
                    StageModifierDirector.instance.currentNode.visited = false;
                    colossusNode = StageModifierDirector.instance.routeMap.CreateColossusNode(StageModifierDirector.instance.currentNode, currentLayer: true);
                }
                else
                {
                    Log.Info($"{nameof(OnVanillaPortalSpawn)}:Spawned colossus portal from default scene");
                    colossusNode = StageModifierDirector.instance.routeMap.CreateColossusNode(StageModifierDirector.instance.currentNode);
                }

                mgr.routeNode = colossusNode;
                mgr.nodeSync = new Vector2(colossusNode.x, colossusNode.y);

                Log.Info($"{nameof(OnVanillaPortalSpawn)}: Succesfuly set node to colossus");
            }
            else if (spawnCardName == "iscGoldshoresPortal")
            {
                mgr.isOval = true;

                if (SceneManager.GetActiveScene().name != "goldshores")
                {
                    Log.Info($"{nameof(OnVanillaPortalSpawn)}: Spawned portal leading to goldshores");
                    mgr.isUsingModifiers = false;
                    mgr.routeNode = StageModifierDirector.instance.GetAvailableNodes()[0];
                    mgr.nodeSync = new Vector2(mgr.routeNode.x, mgr.routeNode.y);
                }
                else
                {
                    Log.Info($"{nameof(OnVanillaPortalSpawn)}: Spawned portal leading from goldshores");
                    mgr.destinationSceneIndex = (int)Run.instance.nextStageScene.sceneDefIndex;
                    mgr.isUsingModifiers = true;
                    mgr.isUsingSceneExit = false;
                    mgr.routeNode = StageModifierDirector.instance.currentNode;
                    mgr.nodeSync = new Vector2(mgr.routeNode.x, mgr.routeNode.y);
                }

            }
            else if (spawnCardName == "iscShopPortal")
            {
                mgr.isOval = true;
                var exMgr = obj.GetComponent<SceneExitController>();
                if (exMgr && !exMgr.useRunNextStageScene)
                {
                    Log.Info($"{nameof(OnVanillaPortalSpawn)}: Spawned portal leading to shop");

                    var routes = StageModifierDirector.instance.GetAvailableNodes();
                    if (routes == null || routes.Count == 0)
                    {
                        Log.Info("node zero");
                        mgr.routeNode = StageModifierDirector.instance.routeMap.root;
                        mgr.nodeSync = new Vector2(0, 0);
                    }
                    else
                    {
                        mgr.routeNode = StageModifierDirector.instance.GetAvailableNodes()[0]; //todo рандом метод
                        mgr.nodeSync = new Vector2(mgr.routeNode.x, mgr.routeNode.y);
                    }
                    mgr.isUsingSceneExit = false;
                }
                //else по сути обрабатывается в методе BazaarOnStart 

            }
            else if (spawnCardName == "iscHardwareProgPortal" || spawnCardName == "iscHardwareProgPortal_Haunt")
            {

                mgr.isOval = false;

                if (Stage.instance.sceneDef.stageOrder != 3)
                {
                    if (spawnCardName == "iscHardwareProgPortal_Haunt")
                        Log.Info($"{nameof(OnVanillaPortalSpawn)}: Spawned portal leading to Solus Wing");
                    else
                        Log.Error($"{nameof(OnVanillaPortalSpawn)}: For some reason it's incorrect spawn card");
                    mgr.isUsingModifiers = false;
                }
                else
                {
                    mgr.destinationSceneIndex = (int)SceneCatalog.FindSceneIndex("conduitcanyon");
                    mgr.isUsingModifiers = true;

                }
                var hardwareNode = StageModifierDirector.instance.routeMap.CreateHardwareNode(StageModifierDirector.instance.currentNode);
                mgr.routeNode = hardwareNode;
                mgr.nodeSync = new Vector2(hardwareNode.x, hardwareNode.y);
            }
            else if (spawnCardName == "iscDestinationPortal")
            {
                mgr.isOval = true;
                mgr.isUsingModifiers = true;
                mgr.routeNode = StageModifierDirector.instance.GetAvailableNodes()[0]; //TODO
                mgr.destinationSceneIndex = (int)Run.instance.nextStageScene.sceneDefIndex;
            }

            if (mgr.isUsingModifiers)
                StageModifierDirector.instance.GetNextStageModifiers(obj);

            Log.Info($"{nameof(OnVanillaPortalSpawn)}: Enabled manager for portal - {spawnCardName}");
            mgr.enabled = true;
            mgr.nodeSync = new Vector2(mgr.routeNode.x, mgr.routeNode.y); //todo убатьвапть на всякий случай

            return obj;
        }


        public static void SpawnRoutePortals(Vector3 position)
        {
            if (!Run.instance.IsExpansionEnabled(routesExpansion))
            {
                Log.Error($"{nameof(SpawnRoutePortals)}: Method spawning route portals was called, but expansion is disabled");
                return;
            }

            List<SceneDef> scenes = SceneHelper.GetNextStagesScenesList();

            if (scenes.Count == 0)
            {
                Log.Error($"{nameof(SpawnRoutePortals)}: No stages available next");
                return;
            }

            Util.ShuffleList(scenes, Run.instance.stageRng);

            var nodes = StageModifierDirector.instance.GetAvailableNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                Log.Info($"{nameof(SpawnRoutePortals)}: Nodes count - {nodes.Count}:{i}");

                var node = nodes[i];

                SceneDef destination = new SceneDef();
                if (RiskOfRoutes.useUniqueDestinations.Value) //todo обработка если нодов путей больше чем доступных уникальных сцен
                {
                    int index = scenes.Count - 1;
                    destination = scenes[index];
                    scenes.RemoveAt(index);
                }
                else
                {
                    destination = Run.instance.runRNG.NextElementUniform(scenes);
                }

                SpawnCard routePortal = AssetManager.CreateRoutePortalSpawnCard(destination);

                if (routePortal == null)
                {
                    Log.Error($"{nameof(SpawnRoutePortals)}: RoutePortal spawncard is null");
                    return;
                }

                DirectorSpawnRequest request = new DirectorSpawnRequest(routePortal, new DirectorPlacementRule
                {
                    minDistance = 10f,
                    maxDistance = 40f,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = position
                }, Run.instance.stageRng);

                GameObject portalObject = DirectorCore.instance.TrySpawnObject(request);

                if (portalObject == null) return;

                var mg = portalObject.GetComponent<RoutePortalManager>();

                mg.enabled = true; //todo проверить

                if (node == null)
                {
                    Log.Warning($"{nameof(SpawnRoutePortals)}: Child route node is null, skipping");
                    continue;
                }

                mg.routeNode = node;
                mg.portalType = (int)node.portalType;
                mg.nodeSync = new Vector2(node.x, node.y);
                Log.Info($"MG route node:{mg.routeNode.portalType}:{mg.routeNode.x},{mg.routeNode.y}");

                mg.isUsingModifiers = true;
                mg.isUsingSceneExit = true;
                StageModifierDirector.instance.GetNextStageModifiers(portalObject);

                Vector3 facingVector = position - portalObject.transform.position;
                facingVector.y = 0;
                if (facingVector != Vector3.zero)
                {
                    portalObject.transform.rotation = Quaternion.LookRotation(facingVector);
                }
                Vector3 rdir = portalObject.transform.right;

                StageModifierDirector.instance.AddRoutePortalInstance(portalObject);
            }

        }

        private static void TeleporterEventSpawnRoutePortals(TeleporterInteraction interaction)
        {
            if (Run.instance.IsExpansionEnabled(routesExpansion))
                SpawnRoutePortals(interaction.transform.position);
        }
    }
}

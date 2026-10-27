using RiskOfRoutes.Items;
using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RiskOfRoutes.Helpers
{
    public class CommandHelper : MonoBehaviour
    {
        [ConCommand(commandName = "configDefs", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void CCGetModdedConfig(ConCommandArgs args)
        {
            Log.Info("Config artifact defs");
            foreach (var info in RiskOfRoutes.moddedArtifactsInfo)
            {
                Log.Info($"\tDef: {info.name} - Tier:{info.tier} - isNegative:{info.isNegative}");
            }

            Log.Info("Config lunar defs");
            foreach (var info in RiskOfRoutes.moddedLunarsInfo)
            {
                Log.Info($"\tDef: {info.name} - Tier:{info.tier} - isNegative:{info.isNegative}");
            }
        }

        [ConCommand(commandName = "availableArtifacts", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void CCArtifactsAvailable(ConCommandArgs args)
        {
            if (StageModifierDirector.instance == null) return;

            Log.Info("Artifacts available");
            foreach (var art in RoR2.ArtifactCatalog.artifactDefs)
            {
                string modName = art.cachedName;
                bool isBanned = StageModifierDirector.instance.bannedModifiersRun.Contains(modName);

                if (StageModifierCatalog.RunDefs.TryGetValue(modName, out ModifierDef def))
                {
                    string banString = isBanned ? "Banned" : "Available";
                    string type = def.isNegative ? "Negative" : "Positive";
                    Log.Info($"{banString} {modName} - Tier: {def.tier} - Type: {type}");
                }
                else
                {
                    Log.Warning($"Not registered {modName}");
                }
            }
        }

        [ConCommand(commandName = "availableLunars", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void CCLunarsAvailable(ConCommandArgs args)
        {
            if (StageModifierDirector.instance == null) return;

            Log.Info("Lunars available");
            foreach (var art in StageModifierCatalog.lunarRegisteredDefs)
            {
                string modName = art.name + "_LunarItem";
                bool isBanned = StageModifierDirector.instance.bannedModifiersRun.Contains(modName);

                if (StageModifierCatalog.RunDefs.TryGetValue(modName, out ModifierDef def))
                {
                    string banString = isBanned ? "Banned" : "Available";
                    string type = def.isNegative ? "Negative" : "Positive";
                    Log.Info($"{banString} {modName} - Tier: {def.tier} - Type: {type}");
                }
                else
                {
                    Log.Warning($"Not registered {modName}");
                }
            }
        }

        [ConCommand(commandName = "runPools", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void CCRunPools(ConCommandArgs args)
        {
            if (StageModifierDirector.instance == null) return;

            Log.Info("active general negative pool:");
            foreach (var name in StageModifierCatalog.generalNegativePool)
            {
                if (StageModifierDirector.instance.bannedModifiersRun.Contains(name)) continue;

                if (StageModifierCatalog.RunDefs.TryGetValue(name, out ModifierDef def))
                    Log.Info($"\t Tier:{def.tier} - {name}");
            }

            Log.Info("active rare pool:");
            foreach (var name in StageModifierCatalog.rarePool)
            {
                if (StageModifierDirector.instance.bannedModifiersRun.Contains(name)) continue;

                if (StageModifierCatalog.RunDefs.TryGetValue(name, out ModifierDef def))
                    Log.Info($"\t Tier:{def.tier} - {name}");
            }
        }

        [ConCommand(commandName = "nextStages", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void CCGetNextStagesScenes(ConCommandArgs args)
        {
            Log.Info("Next stages available:");
            foreach (var scene in SceneHelper.GetNextStagesScenesList().choices)
            {
                Log.Info($"\t{scene.value.cachedName}:weight - {scene.weight}");
            }
            Log.Info(Run.instance.nextStageScene.destinationsGroup);
        }

        [ConCommand(commandName = "enabledMods", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void CCGetEnabledMods(ConCommandArgs args)
        {

            foreach (var c in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                Log.Info($"Plugin: {c.Key}, version: {c.Value.Metadata.Version}");
            }

            Log.Info($"CLeanst: {CleanestHud.Plugin.Id}");
            Log.Info($"Is it? {BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(CleanestHud.Plugin.Id)}");
        }
        [ConCommand(commandName = "bannedStage", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers banned for stage")]
        private static void CCGetBannedStage(ConCommandArgs args)
        {
            Log.Info("Banned on stage: ");
            foreach (var mod in StageModifierDirector.instance.bannedModifiersStage)
            {
                Log.Info($"\t{mod}");
            }
        }

        [ConCommand(commandName = "tonode", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCAdvanceToNode(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int i = (int)args.TryGetArgInt(0);

            StageModifierDirector.instance.punishCommandStack = i;
            Log.Info($"Set additional punish to {i}");

            Log.Info("Banned on stage: ");
            foreach (var mod in StageModifierDirector.instance.bannedModifiersStage)
            {
                Log.Info($"\t{mod}");
            }
        }

        [ConCommand(commandName = "rollmap", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCRollMap(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;
            int currentFloor = StageModifierDirector.instance.currentNode.y;

            Log.Info("Creating new route map");
            var routeMap = StageModifierDirector.instance.routeMap;
            routeMap = new RouteMap(Run.instance.runRNG);

            var currentNode = StageModifierDirector.instance.currentNode;

            currentNode = routeMap.GetRandomNodeOnFloor(currentFloor, Run.instance.stageRng);
            currentNode.visited = true;
            Log.Info($"Current:{currentNode.x},{currentNode.y} that has {currentNode.GetPaths().Count} paths");
            routeMap.isDirty = true;

            StageModifierDirector.instance.routeMap = routeMap;
            StageModifierDirector.instance.currentNode = currentNode;
            StageModifierDirector.instance.currentNodeSync = new Vector2(currentNode.x, currentNode.y);
        }

        [ConCommand(commandName = "allDefs", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCAllModifierDefs(ConCommandArgs args)
        {
            Log.Info("All modifier defs");
            foreach (var def in StageModifierCatalog.AllDefs)
            {
                bool isAdditional = StageModifierCatalog.additionalDefs.Contains(def.Key);

                Log.Info($"\t name:{def.Key}, tier: {def.Value.tier}, isAdditional: {isAdditional}");
            }
        }

        [ConCommand(commandName = "runDefs", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCRunDefs(ConCommandArgs args)
        {
            Log.Info("All run defs");
            foreach (var def in StageModifierCatalog.RunDefs)
            {
                bool isAdditional = StageModifierCatalog.additionalDefs.Contains(def.Key);

                Log.Info($"\t name:{def.Key}, tier: {def.Value.tier}, isAdditional: {isAdditional}");
            }
        }

        [ConCommand(commandName = "bannedRun", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers banned for run")]
        private static void CCGetBannedRun(ConCommandArgs args)
        {
            Log.Info("Banned in run: ");
            foreach (var mod in StageModifierDirector.instance.bannedModifiersRun)
            {
                Log.Info($"\t{mod}");
            }
        }

        [ConCommand(commandName = "bossAspectHook", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers banned for run")]
        private static void CCBossAscpect(ConCommandArgs args)
        {
            //RoR2.SpawnCard.onSpawnedServerGlobal += OnSpawnCardOnSpawnedServerGlobal;
        }



        [ConCommand(commandName = "configBanned", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers banned for run")]
        private static void CCGetConfigBanned(ConCommandArgs args)
        {
            Log.Info("Banned in config: ");
            var str = RiskOfRoutes.bannedModifierDefs.ToString();
            Log.Info($"{str}");
        }

        [ConCommand(commandName = "printerPool", flags = ConVarFlags.ExecuteOnServer, helpText = "Get printers info")]
        private static void CCGetAvailablePrinters(ConCommandArgs args)
        {
            Log.Info("Printer items: ");

            Log.Info("\tUtility pool: ");
            foreach (var pr in StageModifierCatalog.utilityPool)
            {
                Log.Info($"\t\t{pr}");
            }
            Log.Info("Heal pool: ");
            foreach (var pr in StageModifierCatalog.healPool)
            {
                Log.Info($"\t\t{pr}");
            }
            Log.Info("Damage pool: ");
            foreach (var pr in StageModifierCatalog.combatPool)
            {
                Log.Info($"\t\t{pr}");
            }
            Log.Info("Food-related pool: ");
            foreach (var pr in StageModifierCatalog.chefPool)
            {
                Log.Info($"\t\t{pr}");
            }
        }

        [ConCommand(commandName = "enemyPool", flags = ConVarFlags.ExecuteOnServer, helpText = "Get enemy info")]
        private static void CCGetAvailableEnemyItems(ConCommandArgs args)
        {
            Log.Info("Enemy items: ");
            foreach (var en in StageModifierCatalog.generalNegativePool)
            {
                if (en.EndsWith("_EnemyItem"))
                    Log.Info($"\t{en}");
            }
        }

        [ConCommand(commandName = "lunarPool", flags = ConVarFlags.ExecuteOnServer, helpText = "Get lunars info")]
        private static void CCGetAvailableLunarItems(ConCommandArgs args)
        {
            Log.Info("Lunar items: ");
            foreach (var en in StageModifierCatalog.generalNegativePool)
            {
                if (en.EndsWith("_LunarItem"))
                    Log.Info($"\t{en}");
            }
        }

        [ConCommand(commandName = "artifactPool", flags = ConVarFlags.ExecuteOnServer, helpText = "Get artifacts info")]
        private static void CCGetAvailableArtifacts(ConCommandArgs args)
        {
            Log.Info("general negative pool: ");
            foreach (var art in StageModifierCatalog.generalNegativePool.Where(def => !StageModifierDirector.instance.bannedModifiersRun.Contains(def)))
            {
                Log.Info($"\t{art}");
            }
            Log.Info("rare pool: ");
            foreach (var art in StageModifierCatalog.rarePool.Where(def => !StageModifierDirector.instance.bannedModifiersRun.Contains(def)))
            {
                Log.Info($"\t{art}");
            }
        }

        [ConCommand(commandName = "artifactDefs", flags = ConVarFlags.ExecuteOnServer, helpText = "Get artifacts info")]
        private static void CCGetAllArtifactDefs(ConCommandArgs args)
        {
            Log.Info("Artifacts defs: ");
            foreach (var art in ArtifactCatalog.artifactDefs)
            {
                Log.Info($"\t{art.cachedName} - {art.nameToken}");
            }
        }

        [ConCommand(commandName = "reinit", flags = ConVarFlags.ExecuteOnServer, helpText = "Reinit stageModifierCatalog")]
        private static void CCReinit(ConCommandArgs args)
        {
            StageModifierCatalog.Init(AssetManager.bundle);
            ModItemsManager.InitializeItems();
        }

        [ConCommand(commandName = "green", flags = ConVarFlags.ExecuteOnServer, helpText = "Get nexeet stages")]
        private static void CCGetNextGreenStage(ConCommandArgs args)
        {
            //var scene = RiskOfRoutes.RiskOfRoutes.GetGreenPortalScene();
            //Log.Info($"{scene.cachedName}");
            Log.Info($"Modul:{Stage.instance.sceneDef.stageOrder}");

        }


        [ConCommand(commandName = "dccs", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all cards for enemies that can spawn on current stage")]
        private static void CCGetDccs(ConCommandArgs args)
        {
            Log.Info("Available enemies");
            var sel = ClassicStageInfo.instance.monsterSelection;
            if (sel != null)
            {
                for (int i = 0; i < sel.Count; i++)
                {
                    var ch = sel.GetChoice(i).value;
                    Log.Info($"DirectorCard {ch.spawnCard.name}, cost: {ch.cost}, weight: {ch.selectionWeight}");
                }
            }
        }

        [ConCommand(commandName = "node", flags = ConVarFlags.ExecuteOnServer, helpText = "Node info")]
        private static void CCGetCurrentNode(ConCommandArgs args)
        {

            var nodes = StageModifierDirector.instance.routeNodesSynced;
            if (nodes.Count == 0) Log.Error("none");
            foreach (var node in nodes)
            {
                Log.Info($"{node.x}.{node.y}:{node.portalType}-{node.visited}");
            }

            var cnode = StageModifierDirector.instance.currentNode;


            Log.Info($"Current node is {cnode.x},{cnode.y} with {cnode.GetPaths().Count} children, visited {cnode.visited}");
            Log.Info($"\tNext scene:{Run.instance.nextStageScene.cachedName}");
            Log.Info($"\tCurr scene:{Stage.instance.sceneDef.cachedName}");
            Log.Info($"\tLoop clear count:{Run.instance.loopClearCount}");
            Log.Info($"Child node: {cnode.GetPaths()[0].x}.{cnode.GetPaths()[0].y}: {cnode.GetPaths()[0].portalType}");
        }

        [ConCommand(commandName = "availableElites", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all elites that can spawn on current stage")]
        private static void CCGetElitesAvailable(ConCommandArgs args)
        {
            FieldInfo eliteTiersField = typeof(CombatDirector).GetField("eliteTiers", BindingFlags.NonPublic | BindingFlags.Static);
            if (eliteTiersField != null)
            {
                var eliteTiers = (CombatDirector.EliteTierDef[])eliteTiersField.GetValue(null);
                Log.Info($"Tier count{eliteTiers.Length}");
                for (int i = 0; i < eliteTiers.Length; i++)
                {
                    Log.Info($"Tier{i + 1}: {eliteTiers[i].availableDefs.Count}");
                    foreach (var def in eliteTiers[i].availableDefs)
                    {
                        if (def != null)
                            Log.Info($"\t {def.name}");
                    }
                }
            }
        }

        [ConCommand(commandName = "availableItemDefs", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all itemdefs")]
        private static void CCGetItemDefs(ConCommandArgs args)
        {
            Log.Info("Enemies can have");
            foreach (var itemDef in ItemCatalog.allItemDefs.Where(def => def.DoesNotContainTag(ItemTag.AIBlacklist)))
            {
                Log.Info($"\t{itemDef.name}, {itemDef.requiredExpansion}");
            }
        }


        [ConCommand(commandName = "punish", flags = ConVarFlags.ExecuteOnServer, helpText = "punish <i>")]
        private static void CCSetPunish(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int i = (int)args.TryGetArgInt(0);

            StageModifierDirector.instance.punishCommandStack = i;
            Log.Info($"Set additional punish to {i}");
        }

        [ConCommand(commandName = "numbers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all elites that can spawn on current stage")]
        private static void GetCoeff(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;

            if (args.senderMaster)
            {
                args.senderMaster.GetBody().inventory.GiveItemPermanent(RoR2Content.Items.AlienHead);
                args.senderMaster.GetBody().inventory.GiveItemPermanent(RoR2Content.Items.Knurl);
                args.senderMaster.GetBody().inventory.GiveItemPermanent(RoR2Content.Items.ExtraLifeConsumed);
                Log.Info($"Red:{StageModifierDirector.instance.CheckPlayersRedItem()}, yellow:{StageModifierDirector.instance.CheckPlayersYellowItem()}");
            }

        }

        [ConCommand(commandName = "give_money", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCGiveMoney(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;

            if (args.senderMaster)
            {
                args.senderMaster.GiveMoney(1000);
                args.senderMaster.GiveMoneyWithOnLevelUpFreeUnlock(1000);
                Log.Info("Gave 1000 gold to " + args.senderMaster.name);

            }
        }

        [ConCommand(commandName = "itemTags", flags = ConVarFlags.ExecuteOnServer, helpText = "ItemTags")]
        private static void CCItemTags(ConCommandArgs args)
        {
            Log.Info("Items with damage tag");
            foreach (var item in ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.Damage) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with healing tag");
            foreach (var item in ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.Healing) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with utility tag");
            foreach (var item in ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.Utility) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with food-related tag");
            foreach (var item in ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.FoodRelated) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with ai_blacklist tag");
            foreach (var item in ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.AIBlacklist))
                {
                    Log.Info($"\t{item.name}:{Language.GetString(item.nameToken)}");
                }
            }
        }



        [ConCommand(commandName = "activeModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get currently active modifiers")]
        private static void CCGetActiveModifiers(ConCommandArgs args)
        {
            Log.Info("Getting synced modifiers");
            if (StageModifierDirector.instance != null)
            {
                var mods = StageModifierDirector.instance.modifiersSynced;
                if (mods != null)
                {
                    if (mods.Count > 0)
                    {
                        foreach (var modifier in mods)
                        {
                            Log.Info($"{modifier.name}:{modifier.stack}");
                        }
                    }
                    else
                    {
                        Log.Info("There are no modifiers");
                    }
                }
                else
                {
                    Log.Error("Modifiers list is null");
                }
            }

        }

        [ConCommand(commandName = "getSyncData", flags = ConVarFlags.ExecuteOnServer, helpText = "Get syned vars values")]
        private static void CCGetSyncedData(ConCommandArgs args)
        {
            Log.Info("Getting synced modifiers");
            if (StageModifierDirector.instance != null)
            {
                Log.Info("Modifiers synced");
                foreach (var mod in StageModifierDirector.instance.modifiersSynced)
                {
                    Log.Info($"{mod.name}:{mod.stack}:{mod.isArtifact}");
                }
            }
            else
            {
                Log.Info("StageModifierDirector is null");
            }

        }


        [ConCommand(commandName = "reservedModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers that would be active on next stage")]
        private static void CCGetReservedModifiers(ConCommandArgs args)
        {
            if (StageModifierDirector.instance != null)
            {
                var mods = StageModifierDirector.instance.reservedModifiers;
                if (mods != null)
                {
                    if (mods.Count > 0)
                    {
                        foreach (var modifier in mods)
                        {
                            Log.Info(modifier);
                        }
                    }
                    else
                    {
                        Log.Info("There are no modifiers");
                    }

                }
                else
                {
                    Log.Error("Modifiers list is null");
                }
            }
            else
            {
                Log.Error("It is null");
            }

        }


        [ConCommand(commandName = "charge", flags = ConVarFlags.ExecuteOnServer)]
        static void CCTeleporterCharge(ConCommandArgs args)
        {
            if (!TeleporterInteraction.instance) return;

            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;

            var tp = TeleporterInteraction.instance;

            tp.holdoutZoneController.charge = 0.99f;

            if (tp.bossGroup && tp.bossGroup.combatSquad)
            {

                List<CharacterMaster> bossMasters = new List<CharacterMaster>(tp.bossGroup.combatSquad.readOnlyMembersList);

                foreach (CharacterMaster master in bossMasters)
                {
                    CharacterBody body = master.GetBody();

                    if (body && body.healthComponent && body.healthComponent.alive)
                    {
                        body.healthComponent.Suicide(null, null, DamageType.Generic);
                    }
                }
            }
        }


        [ConCommand(commandName = "greenportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCSpawnGreenPortal(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscColossusPortal");

            if (!spawnCard)
            {
                spawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

        }

        [ConCommand(commandName = "mount", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCSpawnMountainShrine(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscColossusPortal");
            spawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/ShrineBoss/iscShrineBoss.asset").WaitForCompletion();


            if (!spawnCard)
            {
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

            Log.Info("Mount availalbe:" + StageModifierDirector.instance?.CheckMountainShrine());

        }



        [ConCommand(commandName = "shopportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCSpawnShopPortal(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShopPortal");

            if (!spawnCard)
            {
                spawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));
        }

        [ConCommand(commandName = "virtualportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCSpawnVirtualPortal(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscHardwareProgPortal_Haunt");

            if (!spawnCard)
            {
                spawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC3/iscHardwareProgPortal_Haunt.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

        }

        [ConCommand(commandName = "goldportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCSpawnGoldPortal(ConCommandArgs args)
        {
            if (!RiskOfRoutes.allowDebug.Value || !NetworkServer.active) return;
            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscGoldshoresPortal");

            if (!spawnCard)
            {
                spawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

        }

        [ConCommand(commandName = "fing", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCFindShopPortal(ConCommandArgs args)
        {
            GameObject obj = GameObject.Find("Node");
            if (obj == null)
            {
                Log.Error("Could not find it");
            }
            else
            {
                Log.Info("Found it");
                foreach (var c in obj.GetComponentsInChildren<Component>())
                {
                    Log.Info(c.GetType());
                }
                var mg = obj.GetComponent<SceneExitController>();
                Log.Info(mg.destinationScene);
                Log.Info(mg.useRunNextStageScene);
            }
        }


    }
}

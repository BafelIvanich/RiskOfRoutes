using RiskOfRoutes;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class CommandHelper : MonoBehaviour
    {
        [ConCommand(commandName = "nextStages", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void GetNextStagesScenesCommand(ConCommandArgs args)
        {
            Log.Info("Next stages available:");
            foreach (var scene in RiskOfRoutes.RiskOfRoutes.GetNextStagesScenesList())
            {
                Log.Info(scene.cachedName);
            }
            Log.Info(Run.instance.nextStageScene.destinationsGroup);

        }

        [ConCommand(commandName = "green", flags = ConVarFlags.ExecuteOnServer, helpText = "Get nexeet stages")]
        private static void GetNextGreenStage(ConCommandArgs args)
        {
            //var scene = RiskOfRoutes.RiskOfRoutes.GetGreenPortalScene();
            //Log.Info($"{scene.cachedName}");
            Log.Info($"Modul:{Stage.instance.sceneDef.stageOrder}");

        }

        [ConCommand(commandName = "enemyitems", flags = ConVarFlags.ExecuteOnServer, helpText = "Give enemies items")]
        private static void GiveEnemiesItems(ConCommandArgs args)
        {
            TeamManager.instance.GiveTeamItem(TeamIndex.Monster, RoR2.RoR2Content.Items.Hoof.itemIndex, 5);
        }

        [ConCommand(commandName = "mapon", flags = ConVarFlags.ExecuteOnServer, helpText = "Give enemies items")]
        private static void MapOn(ConCommandArgs args)
        {
            var panel = RiskOfRoutes.RiskOfRoutes.routeMapPanel;

            var synced = StageModifierDirector.instance.routeNodesSynced;
            if(synced == null)
            {
                Log.Info("null synced data");
            }
            else
            {
                Log.Info("not null synced");
                foreach(var node in synced)
                {
                    Log.Info($"\t{node.portalType}:{node.x}.{node.y}:{node.visited}");
                }
            }

            var mapScript = panel.GetComponent<RouteMapUI>();
            mapScript.DrawMap();
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            var juice = panel.GetComponent<UIJuice>();

            panel.SetActive(true);
            if (canvasGroup != null && juice != null)
            {
                juice.TransitionPanFromTop();
                juice.TransitionAlphaFadeIn();
            }
        }

        [ConCommand(commandName = "upgradeDrones", flags = ConVarFlags.ExecuteOnServer, helpText = "Upgrade drones")]
        private static void UpgradePlayerDrones(ConCommandArgs args)
        {
            if (args.senderMaster)
            {
                CharacterBody cb = args.senderMaster.GetBody();
                MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(cb.master.netId);
                if (minionGroup == null)
                {
                    return;
                }
                MinionOwnership[] members = minionGroup.members;
                foreach (MinionOwnership minionOwnership in members)
                {
                    if (!minionOwnership)
                    {
                        continue;
                    }
                    CharacterMaster master = minionOwnership.GetComponent<CharacterMaster>();
                    if (master && master.inventory)
                    {
                        master.inventory.GiveItemPermanent(DLC3Content.Items.DroneUpgradeHidden);
                    }
                }
            }
        }

        [ConCommand(commandName = "mapoff", flags = ConVarFlags.ExecuteOnServer, helpText = "Give enemies items")]
        private static void MapOff(ConCommandArgs args)
        {
            RiskOfRoutes.RiskOfRoutes.routeMapPanel.SetActive(false);
        }

        [ConCommand(commandName = "dccs", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all cards for enemies that can spawn on current stage")]
        private static void GetDccs(ConCommandArgs args)
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

        [ConCommand(commandName = "node", flags = ConVarFlags.ExecuteOnServer, helpText = "node")]
        private static void GetCurrentNode(ConCommandArgs args)
        {
            var node = StageModifierDirector.instance.currentNode;
            if (node == null)
            {
                Log.Error("Current node is null");
            }
            else
            {
                Log.Info($"Current node is {node.x},{node.y} with {node.GetPaths().Count} children, visited {node.visited}");
                Log.Info($"\tNext scene:{Run.instance.nextStageScene.cachedName}");
                Log.Info($"\tCurr scene:{Stage.instance.sceneDef.cachedName}");
                Log.Info($"\tLoop clear count:{Run.instance.loopClearCount}");
            }
        }

        [ConCommand(commandName = "availableElites", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all elites that can spawn on current stage")]
        private static void GetElitesAvailable(ConCommandArgs args)
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

        [ConCommand(commandName = "numbers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all elites that can spawn on current stage")]
        private static void GetCoeff(ConCommandArgs args)
        {
            if (args.senderMaster)
            {
                args.senderMaster.GetBody().inventory.GiveItemPermanent(DLC3Content.Items.DroneUpgradeHidden);
            }

        }

        [ConCommand(commandName = "give_money", flags = ConVarFlags.ExecuteOnServer, helpText = "Gives 1000 gold to the executing player")]
        private static void CCGiveMoney(ConCommandArgs args)
        {
            if (args.senderMaster)
            {
                args.senderMaster.GiveMoney(1000);
                args.senderMaster.GiveMoneyWithOnLevelUpFreeUnlock(1000);
                Log.Info("Gave 1000 gold to " + args.senderMaster.name);

                NetworkUser nu = args.senderMaster.playerCharacterMasterController.networkUser;
                if (nu)
                {
                    nu.AwardLunarCoins(100);
                }
            }
        }



        [ConCommand(commandName = "activeModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get currently active modifiers")]
        private static void GetActiveModifiersCommand(ConCommandArgs args)
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


        [ConCommand(commandName = "reservedModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers that would be active on next stage")]
        private static void GetReservedModifiersCommand(ConCommandArgs args)
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
                            Log.Info(modifier.reservedText);
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

        [ConCommand(commandName = "checkOneShot", flags = ConVarFlags.ExecuteOnServer, helpText = "Check if players have one shot prot (for one of modifiers)")]
        private static void CheckOneShotProtection(ConCommandArgs args)
        {
            var instances = PlayerCharacterMasterController.instances;
            foreach (var playerCharacterMaster in PlayerCharacterMasterController.instances)
            {
                Log.Info($"{playerCharacterMaster.body.hasOneShotProtection}:{playerCharacterMaster.name}");
                playerCharacterMaster.body.hasOneShotProtection = false;
            }
        }

        //телпорт сразу
        [ConCommand(commandName = "charge", flags = ConVarFlags.ExecuteOnServer, helpText = "Charges TP and kills boss")]
        static void ChargeTeleporter(ConCommandArgs args)
        {
            if (!TeleporterInteraction.instance) return;

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

            //Log.Info("Teleporter charged and bosses eliminated.");
        }

        [ConCommand(commandName = "tp", flags = ConVarFlags.ExecuteOnServer, helpText = "tp <x> <y> <z> - teleport local player to coordinates.")]
        private static void CC_Teleport(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            if (args.Count != 3)
            {
                Log.Info("Usage: tp <x> <y> <z>");
                return;
            }

            float x = (float)args.TryGetArgFloat(0);
            float y = (float)args.TryGetArgFloat(1);
            float z = (float)args.TryGetArgFloat(2);

            var body = args.GetSenderBody();
            if (!body) return;

            var newPos = new Vector3(x, y, z);
            body.characterMotor?.Motor.SetPositionAndRotation(newPos, body.transform.rotation);
            body.transform.position = newPos;
        }

        [ConCommand(commandName = "greenportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void SpawnGreenPortal(ConCommandArgs args)
        {

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscColossusPortal");

            if (!spawnCard)
            {
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
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
        private static void SpawnMountainShrine(ConCommandArgs args)
        {

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscColossusPortal");
            spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/ShrineBoss/iscShrineBoss.asset").WaitForCompletion();


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



        [ConCommand(commandName = "shopportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void SpawnShopPortal(ConCommandArgs args)
        {

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShopPortal");

            if (!spawnCard)
            {
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
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
        private static void SpawnVirtualPortal(ConCommandArgs args)
        {

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
        private static void SpawnGoldPortal(ConCommandArgs args)
        {

            CharacterBody body = args.senderMaster.GetBody();
            Vector3 position = body.transform.position + body.transform.forward * 5f;
            Quaternion rotation = Quaternion.LookRotation(-body.transform.forward);
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscGoldshoresPortal");

            if (!spawnCard)
            {
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
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
        private static void FindShopPortal(ConCommandArgs args)
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

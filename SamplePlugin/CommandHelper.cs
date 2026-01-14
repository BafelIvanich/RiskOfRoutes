using RiskOfRoutes;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class CommandHelper:MonoBehaviour
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
            var scene = RiskOfRoutes.RiskOfRoutes.GetGreenPortalScene();
            Log.Info($"{scene.cachedName}");
        }

        [ConCommand(commandName = "dccs", flags = ConVarFlags.ExecuteOnServer, helpText = "Get nexeet stages")]
        private static void GetDccs(ConCommandArgs args)
        {
            //var stageInfo = ClassicStageInfo.instance;
            //var categories = stageInfo.monsterCategories;
            //float temp = 0;
            //foreach (var c in categories.categories)
            //{
            //    //TODO stageInfo.interactableDccsPool. --- interactableCategories помечен как deprecated стоит посмотреть что за DCCS
            //    //еще есть             stageInfo.GetInteractableDccsPool
            //    Log.Info($"Category:{c.name}");
            //    foreach(var f in c.cards)
            //    {
            //        Log.Info($"Card:{f.spawnCard.directorCreditCost}");
            //    }
            //}

            //CharacterMaster master = args.senderMaster;
            //if (!master || !master.inventory)
            //{
            //    master = PlayerCharacterMasterController.instances[0].master;
            //    if (!master || !master.inventory) return;
            //}

            //Log.Info($"Giving all broken items to {master.name}...");

            //foreach (ItemDef itemDef in ItemCatalog.allItemDefs)
            //{
            //    if (itemDef.isConsumed)
            //    {
            //        if (itemDef == DLC1Content.Items.RegeneratingScrapConsumed) continue;
            //        if (itemDef == DLC2Content.Items.LowerPricedChestsConsumed) continue;

            //        master.inventory.GiveItem(itemDef, 1);

            //    }
            //}

            Log.Info($"{Stage.instance.sceneDef.stageOrder}");


        }

        [ConCommand(commandName = "give_money", flags = ConVarFlags.ExecuteOnServer, helpText = "Gives 1000 gold to the executing player")]
        private static void CCGiveMoney(ConCommandArgs args)
        {
            if (args.senderMaster)
            {
                args.senderMaster.GiveMoney(1000);
                args.senderMaster.GiveMoneyWithOnLevelUpFreeUnlock(1000);
                Debug.Log("Gave 1000 gold to " + args.senderMaster.name);

                NetworkUser nu = args.senderMaster.playerCharacterMasterController.networkUser;
                if (nu)
                {
                    nu.AwardLunarCoins(100);
                }
            }
            else
            {
                Debug.LogWarning("Command sender has no character master.");
            }
        }

        [ConCommand(commandName = "activeModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get currently active modifiers")]
        private static void GetActiveModifiersCommand(ConCommandArgs args)
        {
            if (StageModifierDirector.Instance != null)
            {
                var mods = StageModifierDirector.Instance.activeModifiers;
                if (mods != null)
                {
                    if (mods.Count > 0)
                    {
                        foreach (var modifier in mods)
                        {
                            Log.Info($"{modifier.activeText}:{modifier.stack}");
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
            if (StageModifierDirector.Instance != null)
            {
                var mods = StageModifierDirector.Instance.reservedModifiers;
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

            Log.Info("Teleporter charged and bosses eliminated.");
        }

        [ConCommand(commandName = "tp", flags = ConVarFlags.ExecuteOnServer, helpText = "tp <x> <y> <z> - teleport local player to coordinates.")]
        private static void CC_Teleport(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            if (args.Count != 3)
            {
                Debug.Log("Usage: tp <x> <y> <z>");
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
                Debug.LogError("Could not load iscPortalColossus. Trying fallback name...");
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                Debug.LogError("Failed to load Green Portal SpawnCard.");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

            if (spawnedObject)
            {
                Debug.Log("Successfully spawned Green Portal at " + position);
            }
            else
            {
                Debug.LogError("DirectorCore failed to spawn the portal.");
            }
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
                Debug.LogError("Could not load iscPortalColossus. Trying fallback name...");
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                Debug.LogError("Failed to load Shop Portal SpawnCard.");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

            if (spawnedObject)
            {
                Debug.Log("Successfully spawned Shop Portal at " + position);
            }
            else
            {
                Debug.LogError("DirectorCore failed to spawn the portal.");
            }
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
                Debug.LogError("Could not load iscPortalColossus. Trying fallback name...");
                spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/PortalColossus/iscPortalColossus.asset").WaitForCompletion();
            }

            if (!spawnCard)
            {
                Debug.LogError("Failed to load GoldShores Portal SpawnCard.");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng));

            if (spawnedObject)
            {
                Debug.Log("Successfully spawned GoldShores Portal at " + position);
            }
            else
            {
                Debug.LogError("DirectorCore failed to spawn the portal.");
            }
        }

        [ConCommand(commandName = "fing", flags = ConVarFlags.ExecuteOnServer)]
        private static void FindShopPortal(ConCommandArgs args)
        {
            GameObject obj = GameObject.Find("PortalGoldshores(Clone)");
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

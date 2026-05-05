using RiskOfRoutes;
using RiskOfRoutes.StageModifiers;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class CommandHelper : MonoBehaviour
    {
        [ConCommand(commandName = "nextStages", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void CCGetNextStagesScenes(ConCommandArgs args)
        {
            Log.Info("Next stages available:");
            foreach (var scene in SceneHelper.GetNextStagesScenesList())
            {
                Log.Info(scene.cachedName);
            }
            Log.Info(Run.instance.nextStageScene.destinationsGroup);

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
            Log.Info("Artifacts available: ");
            foreach (var art in StageModifierCatalog.generalNegativePool.Where(def => StageModifierCatalog.FindModifier(def).isArtifact))
            {
                Log.Info($"\t{art}");
            }
            foreach (var art in StageModifierCatalog.rarePool.Where(def => StageModifierCatalog.FindModifier(def).isArtifact))
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
            foreach (var itemDef in RoR2.ItemCatalog.allItemDefs.Where(def => def.DoesNotContainTag(ItemTag.AIBlacklist)))
            {
                Log.Info($"\t{itemDef.name}, {itemDef.requiredExpansion}");
            }
        }


        [ConCommand(commandName = "punish", flags = ConVarFlags.ExecuteOnServer, helpText = "punish <i>")]
        private static void CCSetPunish(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int i = (int)args.TryGetArgInt(0);

            StageModifierDirector.instance.commandAddedStack = i;
            Log.Info($"Set additional punish to {i}");
        }

        //[ConCommand(commandName = "numbers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get all elites that can spawn on current stage")]
        //private static void GetCoeff(ConCommandArgs args)
        //{
        //    if (args.senderMaster)
        //    {
        //        args.senderMaster.GetBody().inventory.GiveItemPermanent(RoR2.RoR2Content.Items.AlienHead);
        //        args.senderMaster.GetBody().inventory.GiveItemPermanent(RoR2.RoR2Content.Items.Knurl);
        //        args.senderMaster.GetBody().inventory.GiveItemPermanent(RoR2.RoR2Content.Items.ExtraLifeConsumed);
        //        Log.Info($"Red:{StageModifierDirector.instance.CheckPlayersRedItem()}, yellow:{StageModifierDirector.instance.CheckPlayersYellowItem()}");
        //    }

        //}


        [ConCommand(commandName = "markDirty", flags = ConVarFlags.ExecuteOnServer, helpText = "Mark map as dirty")]
        private static void CCSetMapDirty(ConCommandArgs args)
        {
            StageModifierDirector.instance.routeMap.isDirty = true;
        }


        [ConCommand(commandName = "give_money", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCGiveMoney(ConCommandArgs args)
        {
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
            foreach (var item in RoR2.ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.Damage) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{RoR2.Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with healing tag");
            foreach (var item in RoR2.ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.Healing) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{RoR2.Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with utility tag");
            foreach (var item in RoR2.ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.Utility) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{RoR2.Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with food-related tag");
            foreach (var item in RoR2.ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.FoodRelated) && (item.tier == ItemTier.Tier1 || item.tier == ItemTier.Tier2))
                {
                    Log.Info($"\t{item.name}:{RoR2.Language.GetString(item.nameToken)}");
                }
            }

            Log.Info("Items with ai_blacklist tag");
            foreach (var item in RoR2.ItemCatalog.allItemDefs)
            {
                if (item.tags.Contains(ItemTag.AIBlacklist))
                {
                    Log.Info($"\t{item.name}:{RoR2.Language.GetString(item.nameToken)}");
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
        private static void CCSpawnMountainShrine(ConCommandArgs args)
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

            Log.Info("Mount availalbe:" + StageModifierDirector.instance?.CheckMountainShrine());

        }



        [ConCommand(commandName = "shopportal", flags = ConVarFlags.ExecuteOnServer)]
        private static void CCSpawnShopPortal(ConCommandArgs args)
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
        private static void CCSpawnVirtualPortal(ConCommandArgs args)
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
        private static void CCSpawnGoldPortal(ConCommandArgs args)
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

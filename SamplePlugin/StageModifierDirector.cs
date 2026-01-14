using RiskOfRoutes;
using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin.StageModifiers;
using SamplePlugin.StageModifiers.BossRewardModifiers;
using SamplePlugin.StageModifiers.CombatModifiers;
using SamplePlugin.StageModifiers.OtherModifiers;
using SamplePlugin.StageModifiers.SceneModifiers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class StageModifierDirector : MonoBehaviour
    {
        public static StageModifierDirector instance;
        public List<StageModifier> activeModifiers { get; private set; }

        public List<ArtifactDef> runArtifactsApplied;

        public int startingCredits = 60;
        public int routeCredits = 60;
        public int negativeCredits = 30;
        //red printer, yellow, drone interactables discounts
        //TODO бан лист

        public List<ModifierDef> allModifiers = new List<ModifierDef>();

        public List<StageModifier> reservedModifiers;
        public void Awake()
        {
            instance = this;
            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<StageModifier>();

            Log.Info("StageModifierDirector created");

            RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;
            RoR2.Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;

            RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
            RoR2.Stage.onServerStageComplete += Stage_onServerStageComplete;
        }

        public void RegisterModDefs()
        {
            allModifiers.Add(new ModifierDef("RedPrinter",30,1f,CheckPlayersRedItem));
            allModifiers.Add(new ModifierDef("YellowPrinter", 30, 1f, CheckPlayersYellowItem));
            allModifiers.Add(new ModifierDef("MoreDrones", 30, 1f));
        }

        protected virtual void FixedUpdate()
        {
            if (Run.instance != null)
            {
                routeCredits = (int)(startingCredits * Run.instance.difficultyCoefficient) + 30 * Run.instance.stageClearCount;
            }
        }


        private void Run_onRunStartGlobal(Run obj)
        {
            Log.Info("Run started");
            activeModifiers.Clear();
            reservedModifiers.Clear();
            runArtifactsApplied = GetArtifactsAppliedThisRun();
        }
        private void Run_onRunDestroyGlobal(Run obj)
        {
            Log.Info("On run ended");
            Log.Info($"Run ended, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnEnd();
            }
            reservedModifiers.Clear();

        }

        public List<ArtifactDef> GetArtifactsAppliedThisRun()
        {
            List<ArtifactDef> res = new List<ArtifactDef>();
            foreach (var art in RoR2.RunArtifactManager.enabledArtifactsEnumerable)
            {
                res.Add(art);
                Log.Info($"Added {art}");
            }
            return res;
        }

        //TODO ПРАВКИ В ПОРЯДОК ВЫПОЛНЕНИЯ
        private void SceneDirector_onPrePopulateSceneServer(SceneDirector obj)
        {
            Log.Info($"Stage started, applying {reservedModifiers.Count} modifiers");
            if (reservedModifiers.Count == 0) return;
            Log.Info(reservedModifiers.Count);
            activeModifiers = new List<StageModifier>(reservedModifiers);
            reservedModifiers.Clear();

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
        }

        private void SceneDirector_onPostPopulateSceneServer(SceneDirector obj)
        {
            Log.Info($"Stage started, applying {reservedModifiers.Count} modifiers");
            if (reservedModifiers.Count == 0) return;
            Log.Info(reservedModifiers.Count);
            activeModifiers = new List<StageModifier>(reservedModifiers);
            reservedModifiers.Clear();

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
        }


        private void Stage_onServerStageComplete(RoR2.Stage obj)
        {
            Log.Info($"Stage completed, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnEnd();
            }

            SceneDef nextStageScene = Run.instance.nextStageScene;
            routeCredits = (int)(100 * Mathf.Pow(1.15f, Run.instance.stageClearCount));
            activeModifiers.Clear();
        }

        private void Stage_onServerStageBegin(RoR2.Stage obj)
        {
            Log.Info($"Stage started, applying {reservedModifiers.Count} modifiers");
            if (reservedModifiers.Count == 0) return;
            Log.Info(reservedModifiers.Count);
            activeModifiers = new List<StageModifier>(reservedModifiers);
            reservedModifiers.Clear();

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
        }

        public List<StageModifier> GetNextStageModifiers(GameObject portal)
        {
            WeightedSelection<StageModifier> weightedSelection = CreateAvailableModifierSelection(portal);
            var res = new List<StageModifier>();
            var mg = portal.GetComponent<RoutePortalManager>();
            bool hasBossRewardModifier = false;
            AurelioniteRewardModifier aurelioniteRewardModifier = null;
            if (mg)
            {
                if ((mg.destinationScene.baseSceneName == "lemuriantemple" || mg.destinationScene.baseSceneName == "habitat"))
                {
                    aurelioniteRewardModifier = new AurelioniteRewardModifier();
                    res.Add(aurelioniteRewardModifier);
                    hasBossRewardModifier = true;
                }

                if ((mg.destinationScene.baseSceneName == "conduitcanyon" || mg.destinationScene.baseSceneName == "repurposedcrater"))
                {
                    res.Insert(0, new DroneBossRewardModifier(0, dropLegendary: true));
                    hasBossRewardModifier = true;
                }
            }

            int routePortalCredits = routeCredits;
            int num = 100;
            while (routePortalCredits > 0 && num > 0)
            {
                num--;
                StageModifier currentModifier = SelectModifier(weightedSelection, routePortalCredits);
                //в weightedselection один printeritem с выбранным предметом, здесь рероллит
                if (currentModifier == null) break;
                if (currentModifier is PrinterItemModifier) currentModifier = new PrinterItemModifier();
                Log.Info($"SELECTED MODIFIER:{currentModifier.name}");
                bool isDuplicate = false;

                foreach (StageModifier selectedModifier in res)
                {
                    if (currentModifier.GetType() == selectedModifier.GetType())
                    {
                        if (selectedModifier is PrinterItemModifier)
                        {
                            Log.Info("Its item mod");
                            if (selectedModifier.name == currentModifier.name)
                            {
                                Log.Info($"SAME ITEM:{selectedModifier.name},{currentModifier.name}");

                            }
                            else
                            {
                                Log.Info("Different item printers");
                                continue;
                            }
                        }
                        if (selectedModifier is ArtifactModifier && selectedModifier.name != currentModifier.name)
                        {
                            continue;
                        }
                        if (selectedModifier.stackable)
                        {
                            selectedModifier.stack++;
                            routePortalCredits -= currentModifier.cost;
                            Log.Info($"Stack here {currentModifier}");
                        }


                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    routePortalCredits -= currentModifier.cost;
                    res.Add(currentModifier);
                    Log.Info($"Added mod {currentModifier.name} to res");

                }
            }

            return res;

        }

        //почти идентично SceneDirector.SelectCard()
        private StageModifier SelectModifier(WeightedSelection<StageModifier> weightedSelection, int maxCost)
        {
            WeightedSelection<StageModifier> filtered = new WeightedSelection<StageModifier>();

            for (int i = 0; i < weightedSelection.Count; i++)
            {
                WeightedSelection<StageModifier>.ChoiceInfo choice = weightedSelection.GetChoice(i);
                //Log.Info($"{i}/{weightedSelection.Count}: {choice.value.cost} / {maxCost}::{choice.value.name}");
                if (choice.value.cost <= maxCost)
                {
                    filtered.AddChoice(choice);
                    //Log.Info($"Ti fultered was added {choice.value.name}");
                }
            }
            if (filtered.Count == 0)
            {
                return null;
            }


            return filtered.Evaluate(RoR2.Run.instance.runRNG.nextNormalizedFloat);
        }

        private WeightedSelection<ModifierDef> CreateAvailableModifierSelection(GameObject portal)
        {
            WeightedSelection<ModifierDef> res = new WeightedSelection<ModifierDef>();
            
            foreach(var modDef in allModifiers)
            {
                if (modDef.isAvailable())
                {
                    res.AddChoice(modDef,modDef.selectionWeight);
                }
            }

            return res;

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

        //вес артефакта на элиток может увеличиваться с числом пройденных стейджей, артефакт на смерть команды доступен только в мультиплеере
        private WeightedSelection<ArtifactModifier> CreateArtifactAvailableSelection()
        {
            WeightedSelection<ArtifactModifier> res = new WeightedSelection<ArtifactModifier>();

            if (RiskOfRoutes.RiskOfRoutes.allowChaosArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.FriendlyFire, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowCommandArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.Command, 200, 1f, true);
            if (RiskOfRoutes.RiskOfRoutes.allowDelusionArtifact.Value) AddArtifact(res, RoR2.CU8Content.Artifacts.Delusion, 100, 1f, true);
            if (RiskOfRoutes.RiskOfRoutes.allowDevotionArtifact.Value) AddArtifact(res, RoR2.CU8Content.Artifacts.Devotion, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowDissonanceArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.MixEnemy, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowEnigmaArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.Enigma, 100, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowEvolutionArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.MonsterTeamGainsItems, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowFrailtyArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.WeakAssKnees, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowGlassArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.Glass, 200, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowHonorArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.EliteOnly, 200, 0.5f);
            if (RiskOfRoutes.RiskOfRoutes.allowKinArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.SingleMonsterType, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowMetamorphosisArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.RandomSurvivorOnRespawn, 200, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowRebirthArtifact.Value) AddArtifact(res, RoR2.DLC2Content.Artifacts.Rebirth, 60, 1f, true);
            if (RiskOfRoutes.RiskOfRoutes.allowSacrificeArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.Sacrifice, 100, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowSoulArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.WispOnDeath, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowSpiteArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.Bomb, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowSwarmsArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.Swarms, 60, 1f);
            if (RiskOfRoutes.RiskOfRoutes.allowVengeanceArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.ShadowClone, 60, 1f);



            if (Run.instance.participatingPlayerCount > 1)
            {
                if (RiskOfRoutes.RiskOfRoutes.allowDeathArtifact.Value) AddArtifact(res, RoR2.RoR2Content.Artifacts.TeamDeath, 60, 1f);
            }

            Log.Info($"Added {res.Count} choices");
            return res;
        }

        private void AddArtifact(WeightedSelection<ArtifactModifier> sel, ArtifactDef art, int cost, float weight, bool isPositive = false)
        {
            if (art != null)
            {
                if (runArtifactsApplied.Contains(art))
                {
                    Log.Debug($"Artifact {art.cachedName} was already applied, cannotadd");
                    return;
                }
                sel.AddChoice(new ArtifactModifier(art, cost, isPositive), weight);
                Log.Info($"Succesfully added artifact {art.cachedName}");
            }
            else
            {
                Log.Error($"Could not find artifact by name {name}");
            }
        }

    }
}


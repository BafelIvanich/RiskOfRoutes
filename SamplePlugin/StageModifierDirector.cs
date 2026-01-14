using RiskOfRoutes;
using RoR2;
using SamplePlugin.StageModifiers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class StageModifierDirector : MonoBehaviour
    {
        public static StageModifierDirector Instance;
        public List<StageModifier> activeModifiers { get; private set; }

        public int routeCredits = 200;
        //red printer, yellow, drone interatables discounts

        public List<StageModifier> reservedModifiers;
        public void Awake()
        {
            Instance = this;
            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<StageModifier>();

            Log.Info("StagaModifier managaer created");


            RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;


            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
            RoR2.Stage.onServerStageComplete += Stage_onServerStageComplete;

            //RoR2.SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;

            //RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;

        }

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

        private void Run_onRunStartGlobal(Run obj)
        {
            Log.Info("Run started");
            activeModifiers.Clear();
            reservedModifiers.Clear();
        }

        private void Stage_onServerStageComplete(RoR2.Stage obj)
        {
            Log.Info($"Stage completed, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnEnd();
            }

            SceneDef nextStageScene = Run.instance.nextStageScene;
            routeCredits = 200 + nextStageScene.stageOrder * 50;
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
            WeightedSelection<StageModifier> weightedSelection = CreateWeightedSelection(portal);
            var res = new List<StageModifier>();
            var mg = portal.GetComponent<RoutePortalManager>();

            int routePortalCredits = 200;
            int num = 100;
            while (routePortalCredits > 0 && num > 0)
            {
                num--;
                StageModifier mod = SelectModifier(weightedSelection, routePortalCredits);
                if (mod == null) break;
                bool isDuplicate = false;

                foreach (StageModifier m in res)
                {
                    if (mod.GetType() == m.GetType())
                    {
                        if (m.stackable)
                        {
                            m.stack++;
                            routePortalCredits -= mod.cost;
                            Log.Info($"Stack here {mod}");
                        }
                        if (m is PrinterItemModifier && m.name != mod.name) break;
                        isDuplicate = true;

                        break;
                    }
                }
                if (!isDuplicate)
                {
                    routePortalCredits -= mod.cost;
                    res.Add(mod);
                    Log.Info($"Added mod {mod.name} to res");

                }
            }
            if (mg)
            {
                if (res.Count > 0 && mg.destinationScene.baseSceneName == "lemuriantemple" || mg.destinationScene.baseSceneName == "habitat")
                {
                    res.Insert(0, new AurelioniteRewardModifier());
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

        private WeightedSelection<StageModifier> CreateWeightedSelection(GameObject portal)
        {
            WeightedSelection<StageModifier> res = new WeightedSelection<StageModifier>();
            res.AddChoice(new BossItemModifier(), 0.5f);
            res.AddChoice(new PrinterItemModifier(), 0.5f);
            res.AddChoice(new MoreDronesModifier(), 0.5f);
            res.AddChoice(new WanderingChefModifier(), 0.5f);
            res.AddChoice(new MoreElitesModifier(), 0.5f);
            res.AddChoice(new VoidEnemiesModifier(), 0.5f);
            res.AddChoice(new LunarEnemiesModifier(), 0.5f);
            if (portal)
            {
                if (Run.instance.nextStageScene.stageOrder >= 3)
                {
                    //res.AddChoice(new ArtifactModifier(), 0.5f);
                }
                if (CheckPlayersRedItem())
                {
                    res.AddChoice(new RedPrinterModifier(), 0.5f);
                }
                if (CheckPlayersYellowItem())
                {
                    res.AddChoice(new YellowPrinterModifier(), 0.5f);
                }
                if (CheckPlayersHaveConsumedItems())
                {
                    res.AddChoice(new RepairConsumedItemsModifier(), 1.0f);
                }
                var mg = portal.GetComponent<RoutePortalManager>();

                if (mg.destinationScene.baseSceneName == "dampcavesimple")
                {
                    //TODO Позже будет уникальный
                    Log.Info("UNIQUE MODIFIER FOR IMP CAVE");
                }
                if (mg.destinationScene.baseSceneName == "lemuriantemple")
                {
                    Log.Info("UNIQUE MODIFIER FOR LEMURIAN TEMPLE");
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

    }
}

using RiskOfRoutes;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Networking;

namespace SamplePlugin
{
    public class StageModifierManager: MonoBehaviour
    {
        public static StageModifierManager Instance;
        public List<StageModifier> activeModifiers {  get; private set; }

        public int RouteCredits = 200;

        public HashSet<StageModifier> availableModifiers;

        //red printer, yellow, drone interatables discounts

        public List<StageModifier> reservedModifiers;

        bool postStageThird = false;

        public void Awake()
        {
            Instance = this;
            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<StageModifier>();
            availableModifiers = new HashSet<StageModifier>();

            availableModifiers.Add(new MoreDronesModifier());
            availableModifiers.Add(new PrinterItemModifier());
            availableModifiers.Add(new BossItemModifier());

            Log.Info("StagaModifier managaer created");

            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
            RoR2.Stage.onServerStageComplete += Stage_onServerStageComplete;
        }

        private void Stage_onServerStageComplete(RoR2.Stage obj)
        {
            Log.Info($"Stage completed, clearing {activeModifiers.Count} modifiers");

            foreach (StageModifier modifier in activeModifiers)
            {
                modifier.OnEnd();
            }

            SceneDef nextStageScene = Run.instance.nextStageScene;
            RouteCredits = 200 + nextStageScene.stageOrder*50;
            activeModifiers.Clear();
        }

        private void Stage_onServerStageBegin(RoR2.Stage obj)
        {
            Log.Info($"Stage started, applying {reservedModifiers.Count} modifiers");

            //checks for extending modfiiers pool
            //TODO потом придется поменять логику, чтобы модификаторы заново считались как это происходит в scenedirector, а то при пропаже у игрока желтых красных модификатор остается (пока используется hashset)
            if(!postStageThird && Run.instance.nextStageScene.stageOrder >=3)
            {
                postStageThird = true;
                availableModifiers.Add(new ArtifactModifier());
            }
            if(CheckPlayersRedItem())
            {
                availableModifiers.Add(new YellowPrinterModifier());
            }
            if (CheckPlayersYellowItem())
            {
                availableModifiers.Add(new YellowPrinterModifier());
            }

            activeModifiers = new List<StageModifier>(reservedModifiers);
            reservedModifiers.Clear();

            foreach(StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
        }

        public void GetNextStageModifiers()
        {

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

                            yellowItemCount+= cm.inventory.GetItemCount(item);
                        }
                    }
                }
            }
            Log.Info($"Players have {yellowItemCount} boss items right now");

            return yellowItemCount > 0;
        }
    }
}

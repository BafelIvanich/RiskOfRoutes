using RiskOfRoutes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SamplePlugin
{
    public class StageModifierManager: MonoBehaviour
    {
        public static StageModifierManager Instance;
        public List<StageModifier> activeModifiers {  get; private set; }

        public int RouteCredits;

        //red printer, yellow, drone interatables discounts

        public List<StageModifier> reservedModifiers;

        public void Awake()
        {
            Instance = this;
            activeModifiers = new List<StageModifier>();
            reservedModifiers = new List<StageModifier>();
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
                Log.Info("in loop");
            }

            activeModifiers.Clear();
        }

        private void Stage_onServerStageBegin(RoR2.Stage obj)
        {
            Log.Info($"Stage started, applying {reservedModifiers.Count} modifiers");
            activeModifiers = new List<StageModifier>(reservedModifiers);
            reservedModifiers.Clear();

            foreach(StageModifier modifier in activeModifiers)
            {
                modifier.OnStart();
            }
        }
    }
}

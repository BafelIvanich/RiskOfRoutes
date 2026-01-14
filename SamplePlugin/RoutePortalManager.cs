using RoR2;
using SamplePlugin;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


namespace RiskOfRoutes
{
    public class RoutePortalManager : NetworkBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public SceneExitController sceneExitController;

        public int modifierCredits;

        public SceneDef destinationScene;

        public List<StageModifier> stageModifiers = new List<StageModifier>();

        [SyncVar]
        public string modifierTextSynced;



        public void Start()
        {
            if (NetworkServer.active && Run.instance)
            {
                purchaseInteraction.SetAvailable(true);

                string positiveText = "Positive:\n";
                string negativeText = "Negative:\n";

                if(stageModifiers!=null)
                {
                    foreach (var stageModifier in stageModifiers)
                    {
                        if (stageModifier.negative)
                        {
                            negativeText += stageModifier.reservedText+'\n';
                        }
                        else
                        {
                            positiveText += stageModifier.reservedText + '\n';
                        }
                    }
                }
                else
                {
                    Log.Error("NoModifiers");
                }


                    modifierTextSynced = $"{positiveText}\n{negativeText}";
                Log.Info(positiveText + negativeText);
            }

            purchaseInteraction.onPurchase.AddListener(OnPurchase);

            sceneExitController.useRunNextStageScene = true;

        }

        [Server]
        public void OnPurchase(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'RoutePortalManager::OnPurchase(RoR2.Interactor)' called on client");
                return;
            }

            if(stageModifiers!=null && stageModifiers.Count>0)
            {
                StageModifierManager.Instance.reservedModifiers = stageModifiers;
            }
            
            Log.Info($"Trying {StageModifierManager.Instance.activeModifiers.Count}");

            purchaseInteraction.SetAvailable(false);
            Run.instance.nextStageScene = destinationScene;
            //Run.instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName("itfrozenwall");
            sceneExitController.Begin();
        }

        public void AddStageModifier(StageModifier mod)
        {
           stageModifiers.Add(mod);
        }

        public void GetModifiersForPortal()
        {

        }

    }
}

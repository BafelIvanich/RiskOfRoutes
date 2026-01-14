using RoR2;
using SamplePlugin;
using System.Collections.Generic;
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

        public StageModifier stageModifier;

        [SyncVar]
        public string modifierTextSynced;



        public void Start()
        {
            if (NetworkServer.active && Run.instance)
            {
                purchaseInteraction.SetAvailable(true);

                if (stageModifier != null)
                {
                    modifierTextSynced = stageModifier.text;
                }
                else
                {
                    modifierTextSynced = "No Modifier";
                }

            }

            //stageModifiers = CreateModifiers(modifierCredits);

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

            StageModifierManager.Instance.reservedModifiers.Add(stageModifier);
            Log.Info($"Trying {StageModifierManager.Instance.activeModifiers.Count}");

            purchaseInteraction.SetAvailable(false);
            Run.instance.nextStageScene = destinationScene;
            sceneExitController.Begin();
        }

    }
}

using RoR2;
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




        public void Start()
        {
            if (NetworkServer.active && Run.instance)
            {
                purchaseInteraction.SetAvailable(true);
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

            purchaseInteraction.SetAvailable(false);
            Run.instance.nextStageScene = destinationScene;
            sceneExitController.Begin();
        }

    }
}

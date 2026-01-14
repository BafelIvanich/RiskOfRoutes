using RoR2;
using SamplePlugin;
using SamplePlugin.StageModifiers;
using System.Collections.Generic;
using UnityEngine.Networking;


namespace RiskOfRoutes
{
    public class RoutePortalManager : NetworkBehaviour
    {
        public GenericInteraction genericInteraction;
        public SceneExitController sceneExitController;
        //для seer'a
        public PurchaseInteraction purchaseInteraction;
        public bool isVanillaPortal = false;

        public int modifierCredits;
        public bool isSeerStation;

        public SceneDef destinationScene;

        public List<StageModifier> stageModifiers = new List<StageModifier>();

        [SyncVar]
        public string modifierTextSynced;


        public void Awake()
        {
            genericInteraction = GetComponent<GenericInteraction>();
            sceneExitController = GetComponent<SceneExitController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();

            if (!genericInteraction) Log.Error("PurchaseInteraction missing!");
            if (!sceneExitController) Log.Error("SceneExitController missing!");
        }

        public void Start()
        {
            UpdateText();

            Log.Info("Here");
            if (isSeerStation)
            {
                if (purchaseInteraction)
                {
                    purchaseInteraction.onPurchase.AddListener(OnSeerActivation);
                    return;
                }
                else
                {
                    Log.Error("Error with seer purchase interaction");
                }
            }
            genericInteraction.onActivation.AddListener(OnPortalActivation);

            if (!isVanillaPortal)
            {
                sceneExitController.useRunNextStageScene = true;
            }

        }

        public void UpdateText()
        {
            Log.Info("Called method UpdateText");
            if (NetworkServer.active && Run.instance)
            {
                string positiveText = "Positive:\n";
                string negativeText = "Negative:\n";

                if (stageModifiers != null)
                {
                    foreach (var stageModifier in stageModifiers)
                    {
                        if (stageModifier.negative)
                        {
                            negativeText += $"{stageModifier.reservedText} - x{stageModifier.stack}" + '\n';
                        }
                        else
                        {
                            positiveText += $"{stageModifier.reservedText} - x{stageModifier.stack}" + '\n';
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
        }

        [Server]
        public void OnPortalActivation(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                Log.Warning("[Server] function 'RoutePortalManager::OnPurchase(RoR2.Interactor)' called on client");
                return;
            }

            if (stageModifiers != null && stageModifiers.Count > 0)
            {
                StageModifierDirector.instance.reservedModifiers = stageModifiers;
            }

            Log.Info($"Trying {StageModifierDirector.instance.activeModifiers.Count}");

            if (!isVanillaPortal)
            {
                genericInteraction.enabled = false;
                Run.instance.nextStageScene = destinationScene;
                Log.Info($"inside RPM: {destinationScene.cachedName}");
                //Run.instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName("computationalexchange");
                sceneExitController.Begin();

            }
        }

        [Server]
        public void OnSeerActivation(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                Log.Warning("[Server] function 'RoutePortalManager::OnPurchase(RoR2.Interactor)' called on client");
                return;
            }

            if (stageModifiers != null && stageModifiers.Count > 0)
            {
                StageModifierDirector.instance.reservedModifiers = stageModifiers;
                Log.Info("Seer got modifiers");
            }
        }

        public void AddStageModifier(StageModifier mod)
        {
            stageModifiers.Add(mod);
        }



    }
}

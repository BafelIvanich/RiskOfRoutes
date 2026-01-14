using RoR2;
using SamplePlugin;
using SamplePlugin.StageModifiers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace RiskOfRoutes
{
    public class RoutePortalManager : NetworkBehaviour
    {
        public GenericInteraction genericInteraction;
        public SceneExitController sceneExitController;
        public bool isVanillaPortal = false;

        public int modifierCredits;

        public SceneDef destinationScene;

        public List<StageModifier> stageModifiers = new List<StageModifier>();

        [SyncVar]
        public string modifierTextSynced;


        public void Awake()
        {
            genericInteraction = GetComponent<GenericInteraction>();
            sceneExitController = GetComponent<SceneExitController>();

            if (!genericInteraction) Log.Error("PurchaseInteraction missing!");
            if (!sceneExitController) Log.Error("SceneExitController missing!");
        }

        public void Start()
        {
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

            genericInteraction.onActivation.AddListener(OnActivation);

            if (!isVanillaPortal)
            {
                sceneExitController.useRunNextStageScene = true;
            }

        }

        [Server]
        public void OnActivation(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'RoutePortalManager::OnPurchase(RoR2.Interactor)' called on client");
                return;
            }

            if (stageModifiers != null && stageModifiers.Count > 0)
            {
                StageModifierManager.Instance.reservedModifiers = stageModifiers;
            }

            Log.Info($"Trying {StageModifierManager.Instance.activeModifiers.Count}");

            if (!isVanillaPortal)
            {
                genericInteraction.enabled = false;
                Run.instance.nextStageScene = destinationScene;
                Log.Info($"inside RPM: {destinationScene.cachedName}");
                //Run.instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName("itfrozenwall");
                sceneExitController.Begin();

            }
        }

        public void AddStageModifier(StageModifier mod)
        {
            stageModifiers.Add(mod);
        }

    }
}

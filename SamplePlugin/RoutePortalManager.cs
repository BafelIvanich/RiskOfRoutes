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
        public PurchaseInteraction purchaseInteraction; //для seer'a

        [SyncVar]
        public bool isUsingModifiers = false;
        [SyncVar]
        public bool isOval = false;
        [SyncVar]
        public bool isSeerStation = false;
        [SyncVar]
        public bool isUsingSceneExit = false;

        public RouteMap.RouteNode routeNode;
        public List<StageModifier> stageModifiers = new List<StageModifier>();

        public SyncListString iconNames = new SyncListString();

        [SyncVar(hook = nameof(UpdateSyncedText))]
        public string modifierTextSynced;

        [SyncVar]
        public int destinationSceneIndex;

        public void Awake()
        {
            genericInteraction = GetComponent<GenericInteraction>();
            sceneExitController = GetComponent<SceneExitController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();

            if (!genericInteraction) Log.Error("PurchaseInteraction missing!");
            if (!sceneExitController) Log.Error("SceneExitController missing!");
            if (isSeerStation && !purchaseInteraction) Log.Error("PurchaseInteraction missing");
        }

        public void Start()
        {

            if (isSeerStation)
            {
                if (purchaseInteraction)
                {
                    purchaseInteraction.onPurchase.AddListener(OnSeerActivation);
                    if (NetworkServer.active)
                        UpdateText();
                    return;
                }
                else
                {
                    Log.Error("Error with seer purchase interaction");
                    return;
                }
            }
            genericInteraction.onActivation.AddListener(OnPortalActivation);

            if (isUsingSceneExit)
            {
                sceneExitController.useRunNextStageScene = true;
            }
        }



        public override void OnStartClient()
        {
            base.OnStartClient();
            Log.Info("Called method onStartClient");

            iconNames.Callback -= OnIconsChanged;
            iconNames.Callback += OnIconsChanged;

            if (this.enabled)
            {
                UpdateBubbles();
            }

        }

        private void OnIconsChanged(SyncListString.Operation op, int itemIndex)
        {
            UpdateBubbles();
        }

        public void UpdateText()
        {
            Log.Info("Called method UpdateText");
            if (NetworkServer.active && Run.instance)
            {
                iconNames.Clear();

                List<string> positiveNames = new List<string>();
                List<string> negativeNames = new List<string>();

                string positiveText = $"Positive:\n";
                string negativeText = "Negative:\n";

                if (stageModifiers != null && stageModifiers.Count > 0)
                {
                    Log.Info(stageModifiers.Count);
                    foreach (var stageModifier in stageModifiers)
                    {
                        Log.Info(stageModifier.GetType());
                        if (stageModifier.negative)
                        {
                            negativeText += $"{stageModifier.reservedText} - x{stageModifier.stack}" + '\n';
                            negativeNames.Add(stageModifier.name);
                        }
                        else
                        {
                            positiveText += $"{stageModifier.reservedText} - x{stageModifier.stack}" + '\n';
                            positiveNames.Add(stageModifier.name);
                        }
                    }
                    foreach (var name in positiveNames)
                        iconNames.Add(name);
                    foreach (var name in negativeNames)
                        iconNames.Add(name);
                }
                else
                {
                    Log.Error("NoModifiers");
                }

                if (routeNode != null)
                {
                    modifierTextSynced = $"{routeNode.portalType}:{routeNode.x}.{routeNode.y}:\n{positiveText}\n{negativeText}";
                }
                else
                {
                    //TODO
                    //Log.Error("Route node is null");
                    modifierTextSynced = $"Null node {positiveText}\n{negativeText}";
                }

                //Log.Info(positiveText + negativeText);
                UpdateBubbles();
            }
        }

        private void UpdateSyncedText(string newText)
        {
            Log.Info($"Updating synced text with new text:{newText}");
            modifierTextSynced = newText;
        }

        public void UpdateBubbles()
        {
            if (isSeerStation) return;

            var portalCenter = transform.Find("PortalCenter");
            if (portalCenter == null)
            {
                Log.Error("Portal center is null");
                return;
            }

            for (int i = portalCenter.childCount - 1; i >= 0; i--)
            {
                var child = portalCenter.GetChild(i);
                if (child.name.Contains("Bubble"))
                {
                    var renderers = child.GetComponentsInChildren<Renderer>(true);
                    foreach (var rend in renderers)
                    {
                        foreach (var m in rend.materials)
                        {
                            if (m != null) Destroy(m);
                        }
                    }

                    Destroy(child.gameObject);
                }
            }
            AttachBubblesToPortal();
        }


        private void AttachBubblesToPortal()
        {
            if (isSeerStation) return;
            //Log.Info("Really attaching");

            float portalScaleX = 1f;
            float portalScaleY = 1f;

            if (isOval)
            {
                portalScaleX = 0.7f;
                portalScaleY = 1.26f;
            }

            float arcAngle = 60f;
            float startAngle = 90f;
            float offsetAngle = 0;
            float radius = 5f;

            for (int i = 0; i < iconNames.Count; i++)
            {
                //Log.Info($"act {iconNames.Count}");
                if (RiskOfRoutes.iconBubblePrefab == null)
                {
                    RiskOfRoutes.iconBubblePrefab = RiskOfRoutes.CreateIconBubblePrefab();
                }
                var bubble = Object.Instantiate(RiskOfRoutes.iconBubblePrefab);
                bubble.SetActive(true);

                bubble.transform.SetParent(transform.Find("PortalCenter").transform, false);
                bubble.transform.localScale = new Vector3(1f / portalScaleX, 1f / portalScaleY, 1f / portalScaleX);
                bubble.transform.localRotation = Quaternion.identity;

                //float stepAngle = arcAngle / modifiers.Count;
                float stepAngle = 20;
                offsetAngle = (180 - arcAngle) / 2;
                startAngle = 90 - offsetAngle;

                float stepAngleRad = (stepAngle * i) * Mathf.Deg2Rad;

                float x = -(Mathf.Cos(stepAngleRad) * radius / (1f));
                float y = (Mathf.Sin(stepAngleRad) * radius / (1f));
                bubble.transform.localPosition = new Vector3(x, y, 0);

                var portalActiveDisplay = bubble.transform.Find("PortalActiveDisplay");
                var portal = portalActiveDisplay.Find("Portal");
                if (portal != null)
                {
                    var rend = portal.GetComponent<Renderer>();
                    var mat = rend.material;

                    mat.SetTexture("_Cloud1Tex", null);
                    mat.SetTexture("_Cloud2Tex", null);

                    mat.SetFloat("_Boost", 1.0f);
                    mat.SetFloat("_AlphaBoost", 1.0f);
                    mat.SetFloat("_DistortionStrength", 0f);

                    mat.SetTextureScale("_MainTex", new Vector2(2.0f, 2.0f));
                    mat.SetTextureOffset("_MainTex", new Vector2(-0.5f, -0.5f));

                    Texture2D tex = null;
                    if (iconNames[i] != "")
                    {
                        var icon = IconCatalog.ModifierIconFromDef(iconNames[i]);
                        if (icon == null)
                        {
                            Log.Info("Uing placehdolde");
                            tex = RiskOfRoutes.placeholderIcon.texture;
                        }
                        else
                            tex = icon.texture;
                    }
                    else
                    {
                        tex = RiskOfRoutes.placeholderIcon.texture;
                    }


                    mat.SetTexture("_MainTex", tex);
                    rend.materials = new Material[] { mat, mat };
                    //Log.Info("Bubble was set up");
                }
            }
        }

        //todo texture clamp
        private Texture2D SpriteToTexture(Sprite sprite)
        {
            int offsetPix = 3;

            int newWidth = (int)sprite.rect.width + 2 * offsetPix;
            int newHeight = (int)sprite.rect.width + 2 * offsetPix;

            Color[] spritePixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);

            Texture2D newTexture = new Texture2D(newWidth, newHeight);
            Color[] pixels = new Color[newWidth * newHeight];

            newTexture.SetPixels(offsetPix, offsetPix, (int)sprite.rect.width, (int)sprite.rect.height, spritePixels);

            return newTexture;

        }

        //TODO смена портала после активации телепортаци
        public void OnPortalActivation(Interactor interactor)
        {
            genericInteraction.interactability = Interactability.Disabled;
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

            if (routeNode != null)
            {
                StageModifierDirector.instance.currentNode = routeNode;

                if (isUsingSceneExit)
                {
                    Run.instance.nextStageScene = SceneCatalog.GetSceneDef((SceneIndex)destinationSceneIndex);
                    Log.Info($"inside RPM: {Run.instance.nextStageScene.cachedName}");

                    //Run.instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName("ai_test");
                    foreach (SceneExitController exitController in InstanceTracker.GetInstancesList<SceneExitController>())
                    {
                        exitController.destinationScene = SceneCatalog.GetSceneDef((SceneIndex)destinationSceneIndex);
                    }

                    sceneExitController.Begin();
                }


            }
            else
            {
                genericInteraction.enabled = false;
                Log.Info("Interacting with vanilla portal");
                var mg = StageModifierDirector.activePortalInstances[0].GetComponent<RoutePortalManager>();
                if (mg != null)
                {
                    StageModifierDirector.instance.currentNode = mg.routeNode;
                }
            }
        }

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
                Run.instance.nextStageScene = SceneCatalog.GetSceneDef((SceneIndex)destinationSceneIndex);
                Log.Info("Seer got modifiers");
            }
        }

    }
}

public enum RoutePortalType
{
    None, Rare, ItemType, DroneType, ChefType, Colossus, Hardware, SolusWing, Goldshores, Shop, FalseSon, Starting
}

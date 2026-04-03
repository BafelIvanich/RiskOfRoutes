using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin;
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
        public List<ModifierSync> stageModifiers = new List<ModifierSync>();

        public SyncListString modDefNames = new SyncListString();

        [SyncVar(hook = nameof(UpdateSyncedText))]
        public string modifierTextSynced;

        [SyncVar]
        public Vector2 nodeSync = new Vector2(-1, -1);

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

            modDefNames.Callback -= OnIconsChanged;
            modDefNames.Callback += OnIconsChanged;

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
                modDefNames.Clear();

                List<string> positiveNames = new List<string>();
                List<string> negativeNames = new List<string>();

                string positiveText = $"Positive:\n";
                string negativeText = "Negative:\n";

                var modifierDefs = new Dictionary<ModifierDef, int>();
                foreach (var modSync in stageModifiers)
                {
                    modifierDefs?.Add(StageModifierCatalog.FindModifier(modSync.name), modSync.stack);
                }

                if (stageModifiers != null && stageModifiers.Count > 0)
                {
                    foreach (var stageModifier in modifierDefs)
                    {
                        var def = stageModifier.Key;
                        var stack = stageModifier.Value;

                        if (def.isNegative)//todo
                        {
                            negativeText += $"{RoR2.Language.GetStringFormatted(def.descriptionToken)} - x{stack}" + '\n';
                            negativeNames.Add(def.name);
                        }
                        else
                        {
                            positiveText += $"{def.descriptionToken} - x{stack}" + '\n';
                            positiveNames.Add(def.name);
                        }
                    }
                    foreach (var name in positiveNames)
                        modDefNames.Add(name);
                    foreach (var name in negativeNames)
                        modDefNames.Add(name);
                }
                else
                {
                    Log.Error("NoModifiers");
                }

                if (routeNode != null)
                {
                    modifierTextSynced = $"{routeNode.portalType}:{routeNode.x}.{routeNode.y}:\n{positiveText}\n{negativeText}";
                    nodeSync = new Vector2(routeNode.x, routeNode.y);
                }
                else
                {
                    //TODO
                    Log.Error("Route node is null");
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

            for (int i = 0; i < modDefNames.Count; i++)
            {
                //Log.Info($"act {modDefNames.Count}");
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
                var donut = portalActiveDisplay.Find("Donut");
                var portalEdges = portalActiveDisplay.Find("PortalEdges");
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
                    if (modDefNames[i] != "")
                    {
                        var modDef = StageModifierCatalog.FindModifier(modDefNames[i]);
                        var icon = modDef.icon;
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

                if (portalEdges && donut != null && RiskOfRoutes.usePositiveNegativeColors.Value)
                {
                    Color positive = new Color(36f / 255f, 191f / 255f, 0f, 1f);
                    Color negative = new Color(255f / 255f, 0f / 255f, 0f / 255f, 1f) * 1.8f;

                    var donutRend = donut.GetComponent<Renderer>();
                    var edgesRend = portalEdges.GetComponent<Renderer>();

                    var donutMat = donutRend.materials[0];
                    var edgesMat = edgesRend.materials[0];

                    if (modDefNames[i] != "")
                    {
                        var modDef = StageModifierCatalog.FindModifier(modDefNames[i]);
                        donutMat.SetColor("_TintColor", modDef.isNegative ? negative : positive);
                        edgesMat.SetColor("_TintColor", modDef.isNegative ? negative : positive);
                    }


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
                StageModifierDirector.instance.currentNodeSync = nodeSync;

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
                    StageModifierDirector.instance.currentNodeSync = mg.nodeSync;
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

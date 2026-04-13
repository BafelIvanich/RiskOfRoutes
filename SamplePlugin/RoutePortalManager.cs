using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin;
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

        public SyncListString stageModifiers = new SyncListString();

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
                    return;
                }
                else
                {
                    Log.Error("Error with seer purchase interaction");
                    return;
                }
            }
            //UpdateBubbles();
            genericInteraction.onActivation.AddListener(OnPortalActivation);

            if (isUsingSceneExit)
            {
                sceneExitController.useRunNextStageScene = true;
            }
        }


        public void OnDestroy()
        {
            //if (stageModifiers != null)
            //{
            //    stageModifiers.Callback -= OnModifiersChanged;
            //}

            if (genericInteraction != null)
            {
                genericInteraction.onActivation.RemoveListener(OnPortalActivation);
            }

            if (purchaseInteraction != null)
            {
                purchaseInteraction.onPurchase.RemoveListener(OnSeerActivation);
            }
        }


        public override void OnStartClient()
        {
            base.OnStartClient();
            Log.Info("Called method onStartClient");

            stageModifiers.Callback += OnModifiersChanged;
            stageModifiers.Callback -= OnModifiersChanged;

            if (this.enabled)
            {
                //UpdateBubbles();
            }
        }

        private void OnModifiersChanged(SyncListString.Operation op, int itemIndex)
        {
            Log.Info("Modifiers were changed, updating bubbles");
            //UpdateBubbles();
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
            //AttachBubblesToPortal();
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

            for (int i = 0; i < stageModifiers.Count; i++)
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
                    if (stageModifiers[i] != "")
                    {
                        var modDef = StageModifierCatalog.FindModifier(stageModifiers[i]);
                        if (modDef != null && modDef.icon != null)
                        {
                            tex = modDef.icon.texture;
                        }
                        else
                        {
                            Log.Info("Uing placehdolde");
                            tex = RiskOfRoutes.placeholderIcon.texture;
                        }
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

                    if (stageModifiers[i] != "")
                    {
                        var modDef = StageModifierCatalog.FindModifier(stageModifiers[i]);
                        if (modDef != null)
                        {
                            donutMat.SetColor("_TintColor", modDef.isNegative ? negative : positive);
                            edgesMat.SetColor("_TintColor", modDef.isNegative ? negative : positive);
                        }
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
                StageModifierDirector.instance.reservedModifiers.Clear();
                foreach (var modStr in stageModifiers)
                {
                    string[] parts = modStr.Split('=');
                    StageModifierDirector.instance.reservedModifiers.Add(new ModifierSync { name = parts[0], stack = int.Parse(parts[1]), isArtifact = bool.Parse(parts[2]) });
                }
            }

            Log.Info($"Trying {StageModifierDirector.instance.activeModifiers.Count}");

            if (routeNode != null)
            {
                StageModifierDirector.instance.currentNode = routeNode;
                StageModifierDirector.instance.currentNodeSync = nodeSync;

                if (isUsingSceneExit)
                {
                    SceneDef targetSceneDef = SceneCatalog.GetSceneDef((SceneIndex)destinationSceneIndex);

                    if (targetSceneDef != null && targetSceneDef.sceneType == SceneType.Stage)
                    {
                        Run.instance.nextStageScene = targetSceneDef;
                        Log.Info($"inside RPM: updated nextStageScene to {Run.instance.nextStageScene.cachedName}");
                    }

                    //Run.instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName("ai_test");
                    foreach (SceneExitController exitController in InstanceTracker.GetInstancesList<SceneExitController>())
                    {
                        exitController.destinationScene = targetSceneDef;
                        exitController.useRunNextStageScene = false;
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
                StageModifierDirector.instance.reservedModifiers.Clear();
                foreach (var modStr in stageModifiers)
                {
                    string[] parts = modStr.Split('=');
                    StageModifierDirector.instance.reservedModifiers.Add(new ModifierSync { name = parts[0], stack = int.Parse(parts[1]), isArtifact = bool.Parse(parts[2]) });
                }
                SceneDef targetSceneDef = SceneCatalog.GetSceneDef((SceneIndex)destinationSceneIndex);

                if (targetSceneDef != null && targetSceneDef.sceneType == SceneType.Stage)
                {
                    Run.instance.nextStageScene = targetSceneDef;
                }
                Log.Info("Seer got modifiers");
            }
        }

    }
}

public enum RoutePortalType
{
    None, Rare, ItemType, DroneType, ChefType, Colossus, Hardware, SolusWing, Goldshores, Shop, FalseSon, Starting, Combat, Heal, Utility
}

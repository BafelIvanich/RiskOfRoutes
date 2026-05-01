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
        [SyncVar(hook = nameof(OnOvalChanged))]
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

        public bool isDirty = true;

        public bool usePositiveNegativeColors;
        public bool usePortalMaterial;
        public bool usePortalColor;

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
            RiskOfRoutes.usePortalTypeColors.SettingChanged += OnConfigChanged;
            RiskOfRoutes.usePositiveNegativeColors.SettingChanged += OnConfigChanged;
            RiskOfRoutes.usePortalMaterial.SettingChanged += OnConfigChanged;


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
            RiskOfRoutes.usePortalTypeColors.SettingChanged -= OnConfigChanged;
            RiskOfRoutes.usePositiveNegativeColors.SettingChanged -= OnConfigChanged;
            RiskOfRoutes.usePortalMaterial.SettingChanged -= OnConfigChanged;

            if (genericInteraction != null)
            {
                genericInteraction.onActivation.RemoveListener(OnPortalActivation);
            }

            if (purchaseInteraction != null)
            {
                purchaseInteraction.onPurchase.RemoveListener(OnSeerActivation);
            }
        }

        private void OnConfigChanged(object sender, System.EventArgs e)
        {
            isDirty = true;
        }

        protected virtual void Update()
        {
            if (isDirty)
            {
                UpdateBubbles();
                UpdatePortalMaterials();
                isDirty = false;
            }
        }


        public override void OnStartClient()
        {
            base.OnStartClient();
            Log.Info("Called method onStartClient");

            stageModifiers.Callback -= OnModifiersChanged;
            stageModifiers.Callback += OnModifiersChanged;

            if (this.enabled && stageModifiers.Count > 0)
            {
                UpdateBubbles();
            }
        }

        private void OnModifiersChanged(SyncListString.Operation op, int itemIndex)
        {
            Log.Info("Modifiers were changed, updating bubbles");
            UpdateBubbles();
        }

        private void OnOvalChanged(bool value)
        {
            isOval = value;
            Log.Info("got value for is Oval");

            if (this.enabled && stageModifiers.Count > 0)
            {
                UpdateBubbles();
            }
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

        public void UpdatePortalMaterials()
        {
            //Log.Info($"Portal name:{this.name}");
            if (!this.name.Contains("Route")) return;
            GameObject routePortal = this.gameObject;
            //var renderers = routePortal.GetComponentsInChildren<Renderer>();

            var portalCenter = routePortal.transform.Find("PortalCenter");
            if (portalCenter == null) Log.Error("Portal center is null");
            var donut = portalCenter.transform.Find("Donut");
            var quad = portalCenter.transform.Find("Quad");
            var edgeRenderer = donut.GetComponent<Renderer>();
            var centerRenderer = quad.GetComponent<Renderer>();


            Texture2D edge = null;
            Texture2D center = null;

            Log.Info("Using black and white");
            edge = AssetManager.bundle.LoadAsset<Texture2D>("bwCenter");
            center = AssetManager.bundle.LoadAsset<Texture2D>("bwEdge");
            //edge = AssetManager.bundle.LoadAsset<Texture2D>("darkerBlueEdge");
            //center = AssetManager.bundle.LoadAsset<Texture2D>("darkerBlueCenter");

            foreach (var renderer in new Renderer[] { edgeRenderer, centerRenderer })
            {
                if (renderer.material.name.Contains("Center"))
                {
                    if (RiskOfRoutes.usePortalMaterial.Value)
                    {
                        Material mat = SceneCatalog.GetSceneDef((SceneIndex)destinationSceneIndex).portalMaterial;
                        if (mat == null)
                        {
                            Log.Error($"{nameof(UpdatePortalMaterials)}: Couldnt set scene material because mat is null");
                            return;
                        }
                        renderer.material = mat;
                    }
                    else
                    {
                        renderer.material.SetTexture("_RemapTex", center);

                        if (routeNode == null)
                        {
                            Log.Error($"{nameof(UpdatePortalMaterials)}: Couldnt set color for portal because route node is null");
                            return;
                        }
                        if (RiskOfRoutes.usePortalTypeColors.Value)
                        {
                            renderer.material.SetColor("_TintColor", GetPortalTypeColor(routeNode.portalType));
                        }
                        else
                        {
                            renderer.material.SetColor("_TintColor", GetPortalTypeColor(RoutePortalType.None));
                        }
                    }
                }
                else if (renderer.material.name.Contains("Edge"))
                {
                    renderer.material.SetTexture("_RemapTex", edge);
                    if (RiskOfRoutes.usePortalTypeColors.Value)
                    {
                        renderer.material.SetColor("_TintColor", GetPortalTypeColor(routeNode.portalType));
                    }
                    else
                    {
                        renderer.material.SetColor("_TintColor", GetPortalTypeColor(RoutePortalType.None));
                    }
                }

            }
        }

        private Color GetPortalTypeColor(RoutePortalType type)
        {
            switch (type)
            {
                case RoutePortalType.Combat:
                    return GetHDRColor(191, 77, 0, 1.4f);

                case RoutePortalType.Utility:
                    return GetHDRColor(22, 0, 191, 2.0f);

                case RoutePortalType.Heal:
                    return GetHDRColor(34, 191, 0, 1.5f);

                case RoutePortalType.ChefType:
                    return GetHDRColor(121, 191, 10, 1.5f);

                case RoutePortalType.Rare:
                    return GetHDRColor(191, 8, 0, 2.0f);

                case RoutePortalType.DroneType:
                    return GetHDRColor(27, 66, 191, 2.0f);

                case RoutePortalType.ItemType:
                default:
                    return GetHDRColor(10, 64, 191, 1.0f);
            }
        }

        private Color GetHDRColor(int r, int g, int b, float intensity)
        {
            float multiplier = Mathf.Pow(2f, intensity);
            return new Color(
                (r / 255f) * multiplier,
                (g / 255f) * multiplier,
                (b / 255f) * multiplier,
                1f
            );
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
                if (AssetManager.iconBubblePrefab == null)
                {
                    Log.Warning($"{nameof(AttachBubblesToPortal)}: Bubble prefab is missing");
                    AssetManager.iconBubblePrefab = AssetManager.CreateIconBubblePrefab();
                }
                var bubble = Object.Instantiate(AssetManager.iconBubblePrefab);
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
                        string[] parts = stageModifiers[i].Split('=');
                        string name = parts[0];

                        //Log.Info($"StageModfieir:{name}");
                        var modDef = StageModifierCatalog.FindModifier(name);
                        if (modDef != null && modDef.icon != null)
                        {
                            if (modDef.name != "Doppelganger")
                            {
                                tex = modDef.icon.texture;
                            }
                            else
                            {
                                tex = ModUIManager.GetHostSprite().texture;
                                //todo цвет темный сделать
                            }
                        }
                        else
                        {
                            Log.Info("Uing placehdolde");
                            tex = AssetManager.placeholderIcon.texture;
                        }
                    }
                    else
                    {
                        tex = AssetManager.placeholderIcon.texture;
                    }


                    mat.SetTexture("_MainTex", tex);
                    rend.materials = new Material[] { mat, mat };
                    //Log.Info("Bubble was set up");
                }

                if (portalEdges && donut != null && RiskOfRoutes.usePositiveNegativeColors.Value)
                {
                    //Log.Info("Changing colors");
                    Color positive = new Color(36f / 255f, 191f / 255f, 0f, 1f);
                    Color negative = new Color(255f / 255f, 0f / 255f, 0f / 255f, 1f) * 1.8f;

                    var donutRend = donut.GetComponent<Renderer>();
                    var edgesRend = portalEdges.GetComponent<Renderer>();

                    var donutMat = donutRend.materials[0];
                    var edgesMat = edgesRend.materials[0];

                    string[] parts = stageModifiers[i].Split('=');
                    string name = parts[0];

                    if (name != "")
                    {
                        var modDef = StageModifierCatalog.FindModifier(name);
                        if (modDef != null)
                        {
                            donutMat.SetColor("_TintColor", modDef.isNegative ? negative : positive);
                            edgesMat.SetColor("_TintColor", modDef.isNegative ? negative : positive);
                            //Log.Info("Succesfully changed colors");
                        }
                    }


                }
            }
        }

        //todo texture clamp
        private Texture2D SpriteToTexture(Sprite sprite)
        {
            return null;
        }

        //TODO смена портала после активации телепортаци
        public void OnPortalActivation(Interactor interactor)
        {
            genericInteraction.interactability = Interactability.Disabled;
            if (!NetworkServer.active) return;

            if (stageModifiers != null && stageModifiers.Count > 0)
            {
                StageModifierDirector.instance.reservedModifiers.Clear();
                StageModifierDirector.instance.bannedModifiers.Clear();
                foreach (var modStr in stageModifiers)
                {
                    string[] parts = modStr.Split('=');
                    StageModifierDirector.instance.reservedModifiers.Add(new ModifierSync { name = parts[0], stack = int.Parse(parts[1]), isArtifact = bool.Parse(parts[2]) });
                    StageModifierDirector.instance.bannedModifiers.Add(parts[0]);
                }
            }

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
                        Log.Info($"{nameof(OnPortalActivation)}: updated nextStageScene to {Run.instance.nextStageScene.cachedName}");
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
                Log.Info($"{nameof(OnPortalActivation)}: Interacting with vanilla portal");
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
            if (!NetworkServer.active) return;

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
                Log.Info($"{nameof(OnSeerActivation)}: Seer got modifiers");
            }
        }

    }
}

public enum RoutePortalType
{
    None, Rare, ItemType, DroneType, ChefType, Colossus, Hardware, SolusWing, Goldshores, Shop, FalseSon, Starting, Combat, Heal, Utility
}

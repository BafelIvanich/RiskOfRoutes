using LeTai.Asset.TranslucentImage;
using RiskOfRoutes.StageModifiers;
using RoR2;
using RoR2.UI;
using SamplePlugin;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace RiskOfRoutes.UI
{
    public class ModUIManager : MonoBehaviour
    {
        public static GameObject routePortalPanel;
        public static GameObject routeMapPanel;
        public static GameObject globalCanvas;

        public ContextManager contextManager;

        private RoutePortalManager lastViewedManager = null;
        private Vector2 lastViewedNodeCoords = new Vector2(-1, -1);
        private int lastViewedMapState = -1;

        public static void Initialize()
        {
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.Refresh += AddIconsToArtifactDisplay;
            //On.RoR2.UI.GameEndReportPanelController.SetDisplayData += AddMapToStats;

            if (globalCanvas == null)
            {
                Log.Warning("Canvas is null creating new one");
                globalCanvas = new GameObject("RoutesMenuCanvas");
                DontDestroyOnLoad(globalCanvas);

                Canvas canvas = globalCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10;

                CanvasScaler scaler = globalCanvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
        }

        private static void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);

            if (!Run.instance.IsExpansionEnabled(RoutesExpansion.routesExpansion))
            {
                Log.Warning($"{nameof(HUD_Awake)}: Expansion is disabled, map ui is not attached");
                return;
            }

            var uiManager = self.gameObject.AddComponent<ModUIManager>();
            var hud = self;

            var contextNotification = self.transform.Find("MainContainer/MainUIArea/SpringCanvas/RightCluster/ContextNotification");
            if (contextNotification != null)
            {
                var mgr = contextNotification.GetComponent<ContextManager>();
                if (mgr != null)
                {
                    uiManager.contextManager = mgr;
                }
                else
                {
                    Log.Error($"{nameof(HUD_Awake)}: Couldnt get contextManager");
                }
            }

            if (routePortalPanel != null)
            {
                //var oldBlur = routePortalPanel.GetComponent<TranslucentImage>();
                //if (oldBlur != null && oldBlur.material != null) Destroy(oldBlur.material);
                Destroy(routePortalPanel);
            }

            if (routeMapPanel != null)
            {
                //var oldBlur = routeMapPanel.GetComponent<TranslucentImage>();
                //if (oldBlur != null && oldBlur.material != null) Destroy(oldBlur.material);
                Destroy(routeMapPanel);
            }

            SetupRoutePortalPanel();
            SetupRouteMapPanel();

            if (routePortalPanel != null)
            {
                routePortalPanel.transform.SetParent(hud.mainContainer.transform, false);
            }
            else
                Log.Error("Route portal panel is null");

            if (routeMapPanel != null)
            {
                var mapCanvasGroup = routeMapPanel.GetComponent<CanvasGroup>();

                routeMapPanel.SetActive(false);

                var scoreboardJuice = hud.scoreboardPanel.GetComponent<UIJuice>();
                var panelUiJuice = routeMapPanel.AddComponent<UIJuice>();

                var mapScript = routeMapPanel.GetComponent<RouteMapUI>();
                if (scoreboardJuice != null)
                {
                    //Log.Info("Juice not null");
                    panelUiJuice.transitionDuration = 0.2f;
                    panelUiJuice.panningMagnitude = scoreboardJuice.panningMagnitude;

                    panelUiJuice.panningRect = mapScript.mainContainer;
                    panelUiJuice.canvasGroup = mapCanvasGroup;
                }

                routeMapPanel.transform.SetParent(hud.mainContainer.transform, false);
            }

            routePortalPanel.SetActive(false);
        }

        private static void AddMapToStats(On.RoR2.UI.GameEndReportPanelController.orig_SetDisplayData orig, GameEndReportPanelController self, GameEndReportPanelController.DisplayData newDisplayData)
        {
            orig(self, newDisplayData);

            var obj = self.gameObject; //GameEndReportPanel
            Transform contentTransform = self.transform.Find("SafeArea (JUICED)/BodyArea/StatsAndChatArea/StatsContainer/Stats Body/ScrollView/Viewport/Content");

            if (contentTransform != null) Log.Info("Found it");

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("RouteMapPanel");
            if (prefab == null)
            {
                Log.Error($"{nameof(SetupRouteMapPanel)}: Couldn't load RouteMapPanel");
                return;
            }
            var statMap = Instantiate(prefab);

            var script = statMap.GetComponent<RouteMapUI>();
            if (StageModifierDirector.instance != null)
            {
                script.DrawMap(StageModifierDirector.instance.currentNodeSync, false);
            }
            else
            {
                Log.Error("it is null cant draw");
            }

            statMap.transform.SetParent(contentTransform);

            var layout = statMap.AddComponent<LayoutElement>();
            layout.preferredHeight = 300;

            //блюр
            TranslucentImage blur = statMap.AddComponent<TranslucentImage>();
            Shader blurShader = Shader.Find("UI/TranslucentImage");
            blur.material = new Material(blurShader);
            blur.color = new Color(0, 0, 0, 0.7f);

            script.barImageHorizontal.gameObject.AddComponent<TooltipProvider>();
            script.barImageVertical.gameObject.AddComponent<TooltipProvider>();

            foreach (var label in new GameObject[] { script.labelHorizontal, script.labelVertical })
            {
                if (label == null)
                {
                    Log.Error($"{nameof(SetupRouteMapPanel)}: No label found");
                    return;
                }
                label.gameObject.AddComponent<TooltipProvider>();
            }
            //statMap.transform.localPosition = Vector3.zero;
            //statMap.transform.localScale = Vector3.one;



            statMap.SetActive(true);

        }

        private static void AddIconsToArtifactDisplay(On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.orig_Refresh orig, CurrentRunArtifactDisplayDataDriver self)
        {
            if (RunArtifactManager.enabledArtifactsEnumerable.GetEnumerator().MoveNext())
            {
                orig(self);
            }
            if (self.artifactDisplayPanelController && self.artifactDisplayPanelController.iconContainer)
            {
                if (StageModifierDirector.instance == null || StageModifierDirector.instance.modifiersSynced.Count == 0)
                {
                    orig(self);
                    return;
                }

                self.dirty = false;
                AddCustomIcon(self.artifactDisplayPanelController);

                if (!self.artifactDisplayPanelController.panelObject.activeSelf)
                {
                    self.artifactDisplayPanelController.panelObject.SetActive(true);
                }
            }
        }

        private static void AddCustomIcon(ArtifactDisplayPanelController controller)
        {
            foreach (var mod in StageModifierDirector.instance.modifiersSynced)
            {
                if (mod.isArtifact) continue;
                if (mod.name.EndsWith("_EnemyItem")) continue; //todo оптимизация?
                ModifierDef def = StageModifierCatalog.FindModifier(mod.name);
                if (def == null) continue;

                string iconName = mod.name;
                Transform container = controller.iconContainer;

                if (container.Find(iconName)) continue;

                GameObject iconObj = new GameObject(iconName);
                iconObj.transform.SetParent(container, false);

                Image img = iconObj.AddComponent<Image>();

                if (def.name != "Doppelganger")
                {
                    img.sprite = def.spriteIcon != null ? def.spriteIcon : AssetManager.placeholderIcon;
                }
                else
                {
                    img.sprite = GetHostSprite();
                    img.color = new Color(163f / 255f, 61f / 255f, 205f / 255f, 1f);
                }

                LayoutElement layout = iconObj.AddComponent<LayoutElement>();
                layout.preferredHeight = 32f;
                layout.preferredWidth = 32f;
                layout.flexibleHeight = 0f;
                layout.flexibleWidth = 0f;

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(32f, 32f);

                AddStackTextToImage(mod.stack, img);
                //AddIconToImage("smth", img);

                TooltipProvider tooltip = iconObj.AddComponent<TooltipProvider>();
                var modDef = StageModifierCatalog.FindModifier(mod.name);

                tooltip.titleToken = Language.GetString(modDef.nameToken);
                tooltip.bodyToken = StageModifierCatalog.GetModifierDescriptionFormatted(mod, modDef);
                //tooltip.titleColor = modStr.tooltipColor;
                tooltip.titleColor = new Color(246f / 255f, 90f / 255f, 58f / 255f);

                if (controller.panelObject && !controller.panelObject.activeSelf)
                {
                    controller.panelObject.SetActive(true);
                }
            }
        }

        public static void AddStackTextToImage(int modStack, Image img)
        {
            GameObject stackTextObj = new GameObject("StackText");
            stackTextObj.transform.SetParent(img.transform, false);

            HGTextMeshProUGUI textComp = stackTextObj.AddComponent<HGTextMeshProUGUI>();
            RectTransform rect = textComp.GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            textComp.font = AssetManager.bomb;
            textComp.fontSize = 12f;

            textComp.alignment = TextAlignmentOptions.TopRight;
            if (modStack > 1) textComp.text = $"x{modStack}";
        }

        public static void AddIconToImage(string spritePath, Image img)
        {
            GameObject stackTextObj = new GameObject("StackText");
            stackTextObj.transform.SetParent(img.transform, false);

            Image spriteAdded = stackTextObj.AddComponent<Image>();
            spriteAdded.sprite = RoR2Content.Items.Pearl.pickupIconSprite;
            RectTransform rect = spriteAdded.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            //textComp.font = AssetManager.bomb;
            //textComp.fontSize = 12f;

            //textComp.alignment = TextAlignmentOptions.TopRight;
            //if (modStack > 1) textComp.text = $"x{modStack}";
        }



        private static void SetupRoutePortalPanel()
        {
            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("RoutePortalPortraitPanel");
            if (prefab == null)
            {
                Log.Error("What?");
            }

            TranslucentImage blur = prefab.AddComponent<TranslucentImage>();
            Shader blurShader = Shader.Find("UI/TranslucentImage");
            blur.material = new Material(blurShader);
            blur.color = new Color(0, 0, 0, 0.7f);
            //текст //плейсхолдер
            var text = prefab.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null)
            {
                Log.Error("No text meshprougui");
            }
            if (text == null) Log.Error("Not found");
            text.text = "Placeholder";
            text.font = AssetManager.bomb;

            routePortalPanel = Instantiate(prefab);
        }

        private static void SetupRouteMapPanel()
        {
            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("RouteMapPanel");
            if (prefab == null)
            {
                Log.Error($"{nameof(SetupRouteMapPanel)}: Couldn't load RouteMapPanel");
                return;
            }
            routeMapPanel = Instantiate(prefab);

            //блюр
            TranslucentImage blur = routeMapPanel.AddComponent<TranslucentImage>();
            Shader blurShader = Shader.Find("UI/TranslucentImage");
            blur.material = new Material(blurShader);
            blur.color = new Color(0, 0, 0, 0.7f);

            var script = routeMapPanel.GetComponent<RouteMapUI>();
            script.barImageHorizontal.gameObject.AddComponent<TooltipProvider>();
            script.barImageVertical.gameObject.AddComponent<TooltipProvider>();

            foreach (var label in new GameObject[] { script.labelHorizontal, script.labelVertical })
            {
                if (label == null)
                {
                    Log.Error($"{nameof(SetupRouteMapPanel)}: No label found");
                    return;
                }
                label.gameObject.AddComponent<TooltipProvider>();
            }
        }

        private void RoutePortalPanelUpdateText(GameObject routePortalPanel, RoutePortalManager mg)
        {
            Log.Info("Huy");
            var textComponents = routePortalPanel.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI panelText = null;
            foreach (var text in textComponents)
            {
                if (text.name == "BodyLabel")
                {
                    panelText = text;
                    break;
                }
            }
            Log.Info("2");

            var imageComponents = routePortalPanel.GetComponentsInChildren<RawImage>();
            RawImage sceneImage = null;
            foreach (var image in imageComponents)
            {
                if (image.name == "ActualImage")
                {
                    sceneImage = image;
                    break;
                }
            }
            Log.Info("3");


            if (panelText == null || sceneImage == null)
            {
                Log.Error("SMTH");
                return;
            }
            Log.Info("4");



            if (mg.destinationSceneIndex != -1)
            {
                SceneDef sceneDef = SceneCatalog.GetSceneDef((SceneIndex)mg.destinationSceneIndex);

                if (sceneDef != null && sceneDef.portalMaterial != null)
                {
                    Log.Info("Changin");
                    sceneImage.texture = sceneDef.portalMaterial.mainTexture;
                }
                else
                {
                    if (sceneDef == null) Log.Error("sceneDef is null");
                    else
                    {
                        Log.Error("portalMaterial is null");
                    }
                }
            }
            else
            {
                Log.Error("Destination scene index is none");
            }

            string positiveText = $"<style=cIsHealing>{Language.GetString("POSITIVE")}</style>" + '\n';
            string negativeText = $"<style=cDeath>{Language.GetString("NEGATIVE")}</style>" + '\n';
            bool hasPositive = false;
            bool hasNegative = false;

            Log.Info("Imagae was replaced");

            var grid = routePortalPanel.GetComponentInChildren<GridLayoutGroup>();

            for (int i = grid.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(grid.transform.GetChild(i).gameObject);
            }

            routePortalPanel.gameObject.SetActive(true);

            foreach (var modStr in mg.stageModifiers)
            {
                string[] parts = modStr.Split('=');
                if (parts.Length < 2) continue;

                string modName = parts[0];
                int modStack = int.Parse(parts[1]);

                ModifierDef def = StageModifierCatalog.FindModifier(modName);
                if (def == null) continue;

                ModifierSync tempMod = new ModifierSync { name = modName, stack = modStack };


                string addedText = "";
                if (RiskOfRoutes.routePanelShowsOnlyNames.Value)
                {
                    //addedText = $"- {RoR2.Language.GetString(def.nameToken)} - x{tempMod.stack}";
                    addedText = $"- {StageModifierCatalog.GetModifierNameFormatted(tempMod, def)}";
                }
                else
                {
                    addedText = $"- {StageModifierCatalog.GetModifierDescriptionFormatted(tempMod, def)}";
                }

                if (!def.isNegative)
                {
                    positiveText = positiveText + addedText + '\n';
                    hasPositive = true;
                }
                else
                {
                    negativeText = negativeText + addedText + '\n';
                    hasNegative = true;
                }

                GameObject obj = new GameObject("ModifierIcon");
                Image img = obj.AddComponent<Image>();
                if (def.name != "Doppelganger")
                {
                    img.sprite = def.spriteIcon != null ? def.spriteIcon : AssetManager.placeholderIcon;

                }
                else
                {
                    img.sprite = GetHostSprite();
                    img.color = new Color(0.4f, 0.15f, 0.5f, 1f);
                }
                img.transform.SetParent(grid.transform, false);

                obj.AddComponent<LayoutElement>();
                var layout = obj.GetComponent<LayoutElement>();
                layout.minWidth = 16f;
                layout.minHeight = 16f;
                layout.preferredWidth = 64f;
                layout.preferredHeight = 64;

                AddStackTextToImage(modStack, img);
            }

            string portalType = "";
            panelText.text = portalType + (hasPositive ? positiveText : "") + (hasNegative ? negativeText : "");

            OnRoutePortalPanelUpdated?.Invoke(routePortalPanel, mg);
        }

        public static event Action<GameObject, RoutePortalManager> OnRoutePortalPanelUpdated;

        public static Sprite GetHostSprite()
        {
            if (CharacterMaster.instancesList != null && CharacterMaster.instancesList.Count > 0)
            {
                CharacterBody host = CharacterMaster.instancesList[0].GetBody();

                if (host != null && host.portraitIcon != null)
                {
                    Texture2D portraitTex = host.portraitIcon as Texture2D;

                    if (portraitTex != null)
                    {
                        return Sprite.Create(
                            portraitTex,
                            new Rect(0, 0, portraitTex.width, portraitTex.height),
                            new Vector2(0.5f, 0.5f)
                        );
                    }
                }
            }

            return AssetManager.placeholderIcon;
        }



        private void Update()
        {
            if (routePortalPanel == null || routeMapPanel == null) return;
            if (!Run.instance) return;
            if (!Run.instance.IsExpansionEnabled(RoutesExpansion.routesExpansion)) return;

            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser != null && localUser.cachedBody != null)
            {
                bool isHoldingTab = localUser.inputPlayer != null && localUser.inputPlayer.GetButton("info");
                bool isLookingAtPortal = false;
                RoutePortalManager currentManager = null;


                var dr = localUser.cachedBody.GetComponent<InteractionDriver>();
                if (dr != null && dr.currentInteractable != null)
                {
                    currentManager = dr.currentInteractable.GetComponentInChildren<RoutePortalManager>();

                    if (!currentManager) currentManager = dr.currentInteractable.GetComponentInParent<RoutePortalManager>();

                    if (currentManager != null && currentManager.isUsingModifiers)
                        isLookingAtPortal = true;
                }

                //панель
                if (isLookingAtPortal)
                {
                    if (lastViewedManager != currentManager)
                    {
                        lastViewedManager = currentManager;
                        RoutePortalPanelUpdateText(routePortalPanel, currentManager);
                        if (currentManager.stageModifiers.Count > 0)
                        {
                        }
                    }

                    if (!routePortalPanel.activeSelf) routePortalPanel.SetActive(true);
                }
                else
                {
                    if (routePortalPanel.activeSelf) routePortalPanel.SetActive(false);
                    lastViewedManager = null;
                }

                //TODO оптимизация тут карта перерисовывается слишком много раз
                //карта
                bool isStateChanged = lastViewedMapState != StageModifierDirector.instance.mapState;

                bool shouldShowMap = isHoldingTab || isLookingAtPortal || isStateChanged;

                ContextManager contextManager = this.contextManager;
                if (shouldShowMap)
                {
                    Vector2 targetNodeCoords = isLookingAtPortal ? currentManager.nodeSync : StageModifierDirector.instance.currentNodeSync;

                    bool isObjective = isLookingAtPortal ? currentManager.isObjectiveNode : false;

                    var mapScript = routeMapPanel.GetComponent<RouteMapUI>();
                    if (StageModifierDirector.instance.punishmentTime != -1)
                        mapScript.DrawBar();



                    bool highlightChanged = targetNodeCoords != lastViewedNodeCoords;

                    if (highlightChanged || !routeMapPanel.activeSelf || isStateChanged)
                    {
                        mapScript.DrawMap(targetNodeCoords, isObjective);
                        lastViewedNodeCoords = targetNodeCoords;
                        lastViewedMapState = StageModifierDirector.instance.mapState;
                    }

                    if (!routeMapPanel.activeSelf)
                    {
                        routeMapPanel.SetActive(true);
                        var juice = routeMapPanel.GetComponent<UIJuice>();
                        if (juice != null)
                        {
                            juice.TransitionPanFromTop();
                            juice.TransitionAlphaFadeIn();
                        }
                    }


                    //
                    //if (contextManager != null && contextManager.contextManager != null && ModSupport.CleanestHudCompat.enabled)

                    if (contextManager != null && contextManager.contextDisplay != null && ModSupport.CleanestHudCompat.enabled)
                    {
                        Vector3 textPos = new Vector3(-250, 60, 0);
                        if (contextManager.contextDisplay.transform.localPosition != textPos)
                        {
                            contextManager.contextDisplay.transform.localPosition = textPos;
                        }
                    }
                    else
                    {
                        //Log.Error("Wrong with contextManager");
                    }

                }
                else if (routeMapPanel.activeSelf)
                {
                    routeMapPanel.SetActive(false);
                    lastViewedNodeCoords = new Vector2(-1, -1);

                    //if (contextManager != null && contextManager.contextManager != null && ModSupport.CleanestHudCompat.enabled)
                    if (contextManager != null && contextManager.contextDisplay != null)
                    {
                        Vector3 textPos = new Vector3(172.8f, 0, 0);
                        if (contextManager.contextDisplay.transform.localPosition != textPos)
                        {
                            contextManager.contextDisplay.transform.localPosition = textPos;
                        }
                    }

                }
                else
                {
                    if (contextManager != null && contextManager.contextDisplay != null)
                    {
                        Vector3 textPos = new Vector3(172.8f, 0, 0);
                        if (contextManager.contextDisplay.transform.localPosition != textPos)
                        {
                            contextManager.contextDisplay.transform.localPosition = textPos;
                        }
                    }
                }
            }
        }
    }
}

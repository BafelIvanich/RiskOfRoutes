using LeTai.Asset.TranslucentImage;
using RiskOfRoutes.StageModifiers;
using RoR2;
using RoR2.UI;
using SamplePlugin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace RiskOfRoutes
{
    public class ModUIManager : MonoBehaviour
    {
        public static GameObject routePortalPanel;
        public static GameObject routeMapPanel;

        private RoutePortalManager lastViewedManager = null;
        private Vector2 lastViewedNodeCoords = new Vector2(-1, -1);
        private int lastViewedMapState = -1;

        public static void Initialize()
        {
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.Refresh += CurrentRunArtifactDisplayDataDriver_Refresh;
        }

        private static void CurrentRunArtifactDisplayDataDriver_Refresh(On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.orig_Refresh orig, CurrentRunArtifactDisplayDataDriver self)
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
                if (mod.name == "Repair" || mod.name.EndsWith("_EnemyItem")) continue; //todo оптимизация?
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
                    img.sprite = def.icon != null ? def.icon : AssetManager.placeholderIcon;
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

                RoR2.UI.TooltipProvider tooltip = iconObj.AddComponent<RoR2.UI.TooltipProvider>();
                var modDef = StageModifierCatalog.FindModifier(mod.name);

                tooltip.titleToken = RoR2.Language.GetString(modDef.nameToken);
                tooltip.bodyToken = StageModifierCatalog.GetModifierDescriptionFormatted(mod, modDef);
                //tooltip.titleColor = modStr.tooltipColor;
                tooltip.titleColor = new Color(246f / 255f, 90f / 255f, 58f / 255f);

                if (controller.panelObject && !controller.panelObject.activeSelf)
                {
                    controller.panelObject.SetActive(true);
                }
            }
        }

        private static void AddStackTextToImage(int modStack, Image img)
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

            if (routePortalPanel != null)
            {
                var oldBlur = routePortalPanel.GetComponent<TranslucentImage>();
                if (oldBlur != null && oldBlur.material != null) Destroy(oldBlur.material);
                Destroy(routePortalPanel);
            }

            if (routeMapPanel != null)
            {
                var oldBlur = routeMapPanel.GetComponent<TranslucentImage>();
                if (oldBlur != null && oldBlur.material != null) Destroy(oldBlur.material);
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
                if (mapCanvasGroup == null)
                {
                    Log.Info("Map canvas group is null");
                    mapCanvasGroup = routeMapPanel.AddComponent<CanvasGroup>();
                }

                mapCanvasGroup.alpha = 1.0f;
                routeMapPanel.SetActive(false);


                var scoreboardJuice = hud.scoreboardPanel.GetComponent<UIJuice>();
                var panelUiJuice = routeMapPanel.AddComponent<UIJuice>();
                //TODO
                var mapScript = routeMapPanel.GetComponent<RouteMapUI>();
                if (scoreboardJuice != null)
                {
                    Log.Info("Juice not null");
                    panelUiJuice.transitionDuration = 0.2f;
                    panelUiJuice.panningMagnitude = scoreboardJuice.panningMagnitude;

                    panelUiJuice.panningRect = mapScript.mainContainer;
                    panelUiJuice.canvasGroup = mapCanvasGroup;
                }

                routeMapPanel.transform.SetParent(hud.mainContainer.transform, false); //к objective panel
                //routeMapPanel.transform.SetParent(hud.scoreboardPanel.transform, true);
                if (mapScript != null)
                {
                    //mapScript.DrawMap();
                }
                else
                {
                    Log.Error("Script is not available");
                }

            }

            //if (RiskOfRoutes.bundle == null)
            //{
            //    Log.Error("Assets not loaded");
            //    return;
            //}

            routePortalPanel.SetActive(false);
        }

        private static void SetupRoutePortalPanel()
        {
            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("RoutePortalPortraitPanel");
            if (prefab == null)
            {
                Log.Error("What?");
            }
            //блюр
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

            routePortalPanel = Object.Instantiate(prefab);
        }

        private static void SetupRouteMapPanel()
        {
            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("RouteMapPanel");
            if (prefab == null)
            {
                Log.Error($"{nameof(SetupRouteMapPanel)}: Couldn't load RouteMapPanel");
                return;
            }
            routeMapPanel = Object.Instantiate(prefab);

            //блюр
            TranslucentImage blur = prefab.AddComponent<TranslucentImage>();
            Shader blurShader = Shader.Find("UI/TranslucentImage");
            blur.material = new Material(blurShader);
            blur.color = new Color(0, 0, 0, 0.7f);

            var script = routeMapPanel.GetComponent<RouteMapUI>();

        }

        private void RoutePortalPanelUpdateText(GameObject routePortalPanel, RoutePortalManager mg)
        {
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

            if (panelText == null || sceneImage == null) return;

            var tex = SceneCatalog.GetSceneDef((SceneIndex)mg.destinationSceneIndex).portalMaterial;
            sceneImage.texture = tex.mainTexture;

            string positiveText = $"<style=cIsHealing>{RoR2.Language.GetString("POSITIVE")}</style>" + '\n';
            string negativeText = $"<style=cDeath>{RoR2.Language.GetString("NEGATIVE")}</style>" + '\n';
            bool hasPositive = false;
            bool hasNegative = false;

            var grid = routePortalPanel.GetComponentInChildren<GridLayoutGroup>();

            for (int i = grid.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(grid.transform.GetChild(i).gameObject);
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
                    addedText = $"- {RoR2.Language.GetString(def.nameToken)} - x{tempMod.stack}";
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
                    img.sprite = def.icon != null ? def.icon : AssetManager.placeholderIcon;

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

        }

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
                        if (currentManager.stageModifiers.Count > 0)
                        {
                            RoutePortalPanelUpdateText(routePortalPanel, currentManager);
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

                if (shouldShowMap)
                {
                    Vector2 targetNodeCoords = isLookingAtPortal ? currentManager.nodeSync : StageModifierDirector.instance.currentNodeSync;

                    var mapScript = routeMapPanel.GetComponent<RouteMapUI>();



                    bool highlightChanged = targetNodeCoords != lastViewedNodeCoords;

                    if (highlightChanged || !routeMapPanel.activeSelf || isStateChanged)
                    {
                        mapScript.DrawMap(targetNodeCoords);
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

                }
                else if (routeMapPanel.activeSelf)
                {
                    routeMapPanel.SetActive(false);
                    lastViewedNodeCoords = new Vector2(-1, -1);
                }
            }
        }
    }
}

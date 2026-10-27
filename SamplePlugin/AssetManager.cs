using R2API;
using RiskOfRoutes.UI;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RiskOfRoutes
{
    public partial class AssetManager
    {
        public static AssetBundle bundle;

        public static GameObject routePortalPrefab;
        public static GameObject iconBubblePrefab;
        public static GameObject chefPrefab;
        public static GameObject popoutPrefab;
        public static GameObject popoutChoicePrefab;

        public static GameObject falseSonHeartPrefab;
        public static Sprite falseSonHeartIcon;

        public static Texture2D droneSymbolTexture;
        public static Texture2D rewardSymbolTexture;
        public static Texture2D aurelioniteSymbolTexture;

        public static InteractableSpawnCard tier1Printer;
        public static InteractableSpawnCard tier2Printer;
        public static InteractableSpawnCard tier3Printer;
        public static InteractableSpawnCard bossPrinter;
        public static InteractableSpawnCard wanderingChefSpawnCard;

        public static TMP_FontAsset bomb;

        public static Sprite placeholderIcon;
        public static Sprite expansionIcon;
        public static Sprite modIcon;
        public static Sprite lockedIcon;

        public static Sprite mysteryEnabled;
        public static Sprite mysteryDisabled;

        public static ObjectScaleCurve pingCurve;

        public static Animator progressBarAnimator;

        public static void Initialize(AssetBundle assetBundle)
        {
            bundle = assetBundle;

            routePortalPrefab = CreateRoutePortalPrefab();
            iconBubblePrefab = CreateIconBubblePrefab();
            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");
            popoutPrefab = CreatePopoutPrefab();
            popoutChoicePrefab = CreatePopoutChoicePrefab();

            droneSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterDrone");
            rewardSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterReward");
            aurelioniteSymbolTexture = bundle.LoadAsset<Texture2D>("teleporterAurelionite");

            tier1Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion();
            tier2Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset").WaitForCompletion();
            tier3Printer = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion();
            bossPrinter = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion();
            CreateChefSpawnCard();

            mysteryEnabled = bundle.LoadAsset<Sprite>("texArtifactMysteryEnabled");
            mysteryDisabled = bundle.LoadAsset<Sprite>("texArtifactMysteryDisabled");

            falseSonHeartPrefab = bundle.LoadAsset<GameObject>("PickupFalseSonHeart");
            var falseSonMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/FalseSon/matFalseSon.mat").WaitForCompletion();
            var mdl = falseSonHeartPrefab.transform.Find("mdlFalseSonHeart");
            var meshRend = mdl.gameObject.GetComponent<MeshRenderer>();
            if (meshRend != null && falseSonMat != null)
            {
                meshRend.material = falseSonMat;
            }
            falseSonHeartIcon = bundle.LoadAsset<Sprite>("falseSonHeartIcon");

            bomb = Addressables.LoadAssetAsync<TMP_FontAsset>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropShadow.asset").WaitForCompletion();

            placeholderIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            expansionIcon = bundle.LoadAsset<Sprite>("routesExpansionIcon");
            modIcon = bundle.LoadAsset<Sprite>("modIcon");
            lockedIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();

            GameObject pingPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/PingIndicator.prefab").WaitForCompletion();
            PingIndicator pingIndicator = pingPrefab.GetComponent<PingIndicator>();
            pingCurve = pingIndicator.pingObjectScaleCurve;



            CheckAssetsLoaded();
        }

        private static GameObject CreateRoutePortalPrefab()
        {
            GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion(), "RoutePortal");

            Object.Destroy(routePortal.GetComponent<GenericInteraction>());
            Object.Destroy(routePortal.GetComponent<SceneExitController>());

            var scaleCurve = routePortal.GetComponentInChildren<ObjectScaleCurve>();

            //vanilla is 0.7 1.26
            float portalScaleX = 1f;
            float portalScaleY = 1f;
            routePortal.name = "RoutePortal";
            routePortal.transform.localScale = new Vector3(portalScaleX, portalScaleY, portalScaleX);

            var portalCenter = routePortal.transform.Find("PortalCenter");
            if (portalCenter != null)
            {
                var pointLight = portalCenter.Find("Point Light");
                if (pointLight != null)
                {
                    var light = pointLight.GetComponent<Light>();
                    if (light != null)
                    {
                        light.intensity = 2;

                    }
                    else
                    {
                        Log.Error("Not ifound lighthh");
                    }
                }
                else
                {
                    Log.Info("Not found poitn lignt");
                }
            }
            else
            {
                Log.Info("Not found center");
            }

            var renderers = routePortal.GetComponentsInChildren<Renderer>();

            RoutePortalManager mgr = routePortal.AddComponent<RoutePortalManager>();

            Texture2D edge = null;
            Texture2D center = null;

            if (RiskOfRoutes.usePortalTypeColors.Value)
            {
                //Log.Info("Using black and white");
                edge = bundle.LoadAsset<Texture2D>("bwCenter");
                center = bundle.LoadAsset<Texture2D>("bwEdge");
            }
            else
            {
                edge = bundle.LoadAsset<Texture2D>("darkerBlueEdge");
                center = bundle.LoadAsset<Texture2D>("darkerBlueCenter");
            }

            foreach (var renderer in renderers)
            {
                if (renderer.material.name.Contains("Center"))
                {
                    if (RiskOfRoutes.usePortalMaterial.Value)
                    {
                        //renderer.material = mat;
                    }
                    else
                    {
                        renderer.material.SetTexture("_RemapTex", center);
                    }
                }
            }

            GenericInteraction interaction = routePortal.AddComponent<GenericInteraction>();
            SceneExitController sceneExitController = routePortal.AddComponent<SceneExitController>();
            GenericDisplayNameProvider genericDisplayNameProvider = routePortal.AddComponent<GenericDisplayNameProvider>();
            GenericInspectInfoProvider genericInspectInfo = routePortal.AddComponent<GenericInspectInfoProvider>();

            //todo tab inscpect
            mgr.genericInteraction = interaction;
            mgr.sceneExitController = sceneExitController;

            interaction.contextToken = RoR2.Language.GetString("ENTER_ROUTE_PORTAL");

            return routePortal;
        }

        //todo replace edge and center textures to bw
        public static GameObject CreateIconBubblePrefab()
        {
            var seerPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab").WaitForCompletion(), "seer");

            Transform displayPivot = seerPrefab.transform.Find("Model").Find("DisplayPivot");
            Transform portalDisplay = displayPivot.Find("PortalActiveDisplay");

            if (portalDisplay)
            {
                GameObject parent = new GameObject("IconBubblePrefab");

                Object.DontDestroyOnLoad(parent);
                parent.SetActive(false);
                portalDisplay.SetParent(parent.transform);
                portalDisplay.transform.localPosition = new Vector3(0, 0f, 0);
                portalDisplay.transform.localRotation = Quaternion.Euler(0f, 180f, 0);
                portalDisplay.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                var pointLight = portalDisplay.transform.Find("Point Light");
                var light = pointLight.GetComponent<Light>();
                light.intensity = 1;
                var mr = portalDisplay.GetComponentsInChildren<Renderer>();
                foreach (var rend in mr)
                {
                    if (rend.name == "Portal")
                    {
                        var mat = rend.materials[0];

                        mat.SetTexture("_Cloud1Tex", null);
                        mat.SetTexture("_Cloud2Tex", null);

                        mat.SetFloat("_Boost", 1.0f);
                        mat.SetFloat("_AlphaBoost", 1.0f);
                        mat.SetFloat("_DistortionStrength", 0f);

                        mat.SetTextureScale("_MainTex", new Vector2(1.75f, 1.75f));
                        mat.SetTextureOffset("_MainTex", new Vector2(-0.4f, -0.4f));
                        rend.materials = new Material[] { mat, mat };
                    }

                }

                GameObject.DestroyImmediate(seerPrefab);
                return parent;

            }
            return seerPrefab;
        }

        //todo optimization
        public static GameObject CreatePopoutPrefab()
        {
            //main part
            var characterSelectUIPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/CharacterSelectUI.prefab").WaitForCompletion();
            var popoutPanelPrefabTransform = characterSelectUIPrefab.transform.Find("SafeArea/RightHandPanel/PopoutPanelContainer/PopoutPanelPrefab");

            var defaultPopout = popoutPanelPrefabTransform.gameObject;

            var routesPopoutPrefab = PrefabAPI.InstantiateClone(defaultPopout, "routesPopout");

            HGPopoutPanel hgPopoutPanel = routesPopoutPrefab.GetComponent<HGPopoutPanel>();
            var popoutController = routesPopoutPrefab.AddComponent<RoutesPopoutController>();

            var contentContainer = hgPopoutPanel.popoutPanelContentContainer.gameObject;
            var contentContainerRect = contentContainer.GetComponent<RectTransform>();

            popoutController.container = contentContainerRect;
            popoutController.title = hgPopoutPanel.popoutPanelTitleText;
            popoutController.subtitle = hgPopoutPanel.popoutPanelSubtitleText;

            Object.DestroyImmediate(hgPopoutPanel);


            var dyn = routesPopoutPrefab.transform.Find("Canvas/Main/DynamicText");
            if (dyn != null)
                GameObject.DestroyImmediate(dyn.gameObject);

            RectTransform rootRect = routesPopoutPrefab.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = new Vector2(0.5f, 0.5f);
                rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.sizeDelta = new Vector2(860f, 650f);
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.localScale = Vector3.one;
            }

            Transform mainTransform = routesPopoutPrefab.transform.Find("Canvas/Main");

            var mainSizeFitter = mainTransform.GetComponent<ContentSizeFitter>();
            if (mainSizeFitter != null)
                Object.DestroyImmediate(mainSizeFitter);

            RectTransform mainRect = mainTransform.GetComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            var vlg = mainTransform.GetComponent<VerticalLayoutGroup>() ?? mainTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 10f;

            string[] bgElements = { "BlurPanel", "Background", "BorderImage", "CancelButton" };
            foreach (var name in bgElements)
            {
                var t = mainTransform.Find(name);
                if (t != null)
                {
                    var le = t.GetComponent<LayoutElement>() ?? t.gameObject.AddComponent<LayoutElement>();
                    le.ignoreLayout = true;
                }
            }

            var titleObj = mainTransform.Find("Title and Subtitle");
            if (titleObj != null)
            {
                var le = titleObj.GetComponent<LayoutElement>() ?? titleObj.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 75f;
                le.flexibleHeight = 0f;
            }

            var rand = mainTransform.Find("RandomButtonContainer");

            //scroll

            GameObject scrollView = new GameObject("ScrollView", typeof(RectTransform));
            scrollView.transform.SetParent(mainTransform, false);

            if (rand != null)
            {
                scrollView.transform.SetSiblingIndex(rand.GetSiblingIndex());
            }

            var scrollLayout = scrollView.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 350f;
            scrollLayout.preferredHeight = 440f;
            scrollLayout.flexibleHeight = 1f;
            scrollLayout.flexibleWidth = 1f;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = new Vector2(-22f, 0f);

            contentContainer.transform.SetParent(viewport.transform, false);

            contentContainerRect.anchorMin = new Vector2(0f, 1f);
            contentContainerRect.anchorMax = new Vector2(1f, 1f);
            contentContainerRect.pivot = new Vector2(0.5f, 1f);
            contentContainerRect.anchoredPosition = Vector2.zero;
            contentContainerRect.sizeDelta = new Vector2(0f, 0f);

            var grid = contentContainer.GetComponent<GridLayoutGroup>() ?? contentContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(72f, 72f);
            grid.spacing = new Vector2(8f, 8f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            grid.padding = new RectOffset(6, 6, 6, 6);

            var sizeFitter = contentContainer.GetComponent<ContentSizeFitter>() ?? contentContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.content = contentContainerRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 35f;

            var ruleBookViewer = characterSelectUIPrefab.transform.Find("SafeArea/RightHandPanel/RuleVerticalLayout/RuleBookViewerVertical");
            if (ruleBookViewer != null)
            {
                var sbTransform = ruleBookViewer.Find("Scrollbar Vertical");
                if (sbTransform != null)
                {
                    GameObject scrollbar = Object.Instantiate(sbTransform.gameObject, scrollView.transform, false);
                    scrollbar.name = "Scrollbar Vertical";

                    RectTransform sbRect = scrollbar.GetComponent<RectTransform>();
                    sbRect.anchorMin = new Vector2(1f, 0f);
                    sbRect.anchorMax = new Vector2(1f, 1f);
                    sbRect.pivot = new Vector2(1f, 0.5f);
                    sbRect.offsetMin = Vector2.zero;
                    sbRect.offsetMax = Vector2.zero;
                    sbRect.sizeDelta = new Vector2(18f, 0f);

                    var mpScrollbar = scrollbar.GetComponent<MPScrollbar>() ?? scrollbar.GetComponent<Scrollbar>();
                    if (mpScrollbar != null)
                    {
                        scrollRect.verticalScrollbar = mpScrollbar as Scrollbar;
                        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
                    }
                }
            }


            //routesPopoutPrefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            if (rand != null)
            {
                var randLayout = rand.GetComponent<LayoutElement>() ?? rand.gameObject.AddComponent<LayoutElement>();
                randLayout.preferredHeight = 50f;
                randLayout.flexibleHeight = 0f;

                var randomButton = rand.transform.Find("RandomButton");
                if (randomButton != null)
                    Object.DestroyImmediate(randomButton.gameObject);

                var hor = rand.gameObject.GetComponent<HorizontalLayoutGroup>() ?? rand.gameObject.AddComponent<HorizontalLayoutGroup>();
                hor.childControlWidth = true;
                hor.childControlHeight = true;
                hor.childForceExpandWidth = true;
                hor.childForceExpandHeight = true;
                hor.spacing = 12f;

                var genericButton = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/GenericMenuButton.prefab").WaitForCompletion();

                var defaultButton = Object.Instantiate(genericButton, rand);
                var TMDefault = defaultButton.GetComponent<LanguageTextMeshController>();
                TMDefault.token = "Default";
                var textComponent = defaultButton.transform.Find("ButtonText").GetComponent<HGTextMeshProUGUI>();
                textComponent.m_textAlignment = TextAlignmentOptions.Center;
                popoutController.defaultButton = defaultButton;

                var allButton = Object.Instantiate(genericButton, rand);
                var TMAll = allButton.GetComponent<LanguageTextMeshController>();
                TMAll.token = "All";
                var textComponentAll = allButton.transform.Find("ButtonText").GetComponent<HGTextMeshProUGUI>();
                textComponentAll.m_textAlignment = TextAlignmentOptions.Center;
                popoutController.allButton = allButton;

                var noneButton = Object.Instantiate(genericButton, rand);
                var TMNone = noneButton.GetComponent<LanguageTextMeshController>();
                TMNone.token = "None";
                var textComponentNone = noneButton.transform.Find("ButtonText").GetComponent<HGTextMeshProUGUI>();
                textComponentNone.m_textAlignment = TextAlignmentOptions.Center;
                popoutController.noneButton = noneButton;
            }

            return routesPopoutPrefab;

        }

        public static GameObject CreatePopoutChoicePrefab()
        {

            var characterSelectUIPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/CharacterSelectUI.prefab").WaitForCompletion();
            var ruleChoicePrefabTransform = characterSelectUIPrefab.transform.Find("SafeArea/RightHandPanel/RuleVerticalLayout/RuleBookViewerVertical/RuleChoicePrefab");

            GameObject defaultChoicePrefab = ruleChoicePrefabTransform.gameObject;

            var labelObj = defaultChoicePrefab.transform.Find("VoteLabel");
            //HGTextMeshProUGUI textComp1 = labelObj.GetComponent<HGTextMeshProUGUI>();
            //textComp1.alignment = TextAlignmentOptions.TopRight;

            var checkboxTemplate = defaultChoicePrefab.transform.Find("ButtonSelectionHighlight, Checkbox");
            var popoutChoice = PrefabAPI.InstantiateClone(defaultChoicePrefab, "popoutChoiceRoutes");

            var checkboxTemplate1 = popoutChoice.transform.Find("ButtonSelectionHighlight, Checkbox");
            var checkboxTemplate2 = popoutChoice.transform.Find("ButtonSelectionHighlight, Disable");
            var checkboxTemplate3 = popoutChoice.transform.Find("ButtonSelectionHighlight, Locked");

            var labelObjNew = popoutChoice.transform.Find("VoteLabel");

            List<GameObject> checkboxStates = new List<GameObject>();
            //positive negative
            for (int i = 0; i < 2; i++)
            {
                Color highlightColor = new Color(0.61f, 0.9f, 0.38f, 1f);
                if (i == 1) highlightColor = new Color(1f, 0.48f, 0.48f);
                for (int j = 1; j <= 3; j++)
                {
                    var selectState = Object.Instantiate(checkboxTemplate, popoutChoice.transform);

                    var highlight = selectState.transform.Find("Highlight");
                    if (highlight != null)
                    {
                        var highlightImage = highlight.GetComponent<Image>();
                        if (highlightImage != null) highlightImage.color = highlightColor;
                    }

                    var checkboxImage = selectState.transform.Find("Checkbox/CheckboxImage");
                    if (checkboxImage != null)
                    {
                        var checkImg = checkboxImage.GetComponent<Image>();
                        if (checkImg != null) checkImg.color = highlightColor;
                    }

                    selectState.name = $"{(i == 1 ? "Negative" : "Positive")}_Tier{j}";

                    if (labelObj != null)
                    {
                        var label = Object.Instantiate(labelObj, selectState.transform);
                        HGTextMeshProUGUI textComp = label.GetComponent<HGTextMeshProUGUI>();
                        if (textComp != null)
                        {
                            textComp.color = highlightColor;
                            textComp.text = $"Tier {j}";
                            textComp.alignment = TextAlignmentOptions.TopRight;
                            textComp.m_textAlignment = TextAlignmentOptions.TopRight;
                        }
                    }

                    checkboxStates.Add(selectState.gameObject);
                }
            }

            var defaultEnabledState = Object.Instantiate(checkboxTemplate, popoutChoice.transform);
            defaultEnabledState.name = "DefaultEnabled";
            checkboxStates.Add(defaultEnabledState.gameObject);

            if (checkboxTemplate1 != null) GameObject.DestroyImmediate(checkboxTemplate1.gameObject);
            if (checkboxTemplate2 != null) GameObject.DestroyImmediate(checkboxTemplate2.gameObject);
            if (checkboxTemplate3 != null) GameObject.DestroyImmediate(checkboxTemplate3.gameObject);
            if (labelObjNew != null) GameObject.DestroyImmediate(labelObjNew.gameObject);

            //foreach (var c in popoutChoice.GetComponentsInChildren<Canvas>(true))
            //{
            //    Object.DestroyImmediate(c);
            //}
            foreach (var r in popoutChoice.GetComponentsInChildren<RefreshCanvasDrawOrder>(true))
            {
                Object.DestroyImmediate(r);
            }
            foreach (var gr in popoutChoice.GetComponentsInChildren<GraphicRaycaster>(true))
            {
                Object.DestroyImmediate(gr);
            }

            var rc = popoutChoice.GetComponent<RuleChoiceController>();
            var controller = popoutChoice.AddComponent<PopoutChoiceController>();

            if (rc != null)
            {
                controller.hgButton = rc.hgButton;
                controller.image = rc.image;
                controller.tooltipProvider = rc.tooltipProvider;
            }

            controller.checkboxStates = checkboxStates.ToArray();

            if (rc != null)
                GameObject.DestroyImmediate(rc);

            return popoutChoice;
        }




        private static void CreateChefSpawnCard()
        {
            Log.Info("Trying to create CHEF");
            InteractableSpawnCard card = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            GameObject lunarTable = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/Bazaar_LunarTable.prefab").WaitForCompletion();

            var mealPrep = chefPrefab;

            //Объекты на столе
            var steak1 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FlatHealth/DisplaySteakFlat.prefab").WaitForCompletion().InstantiateClone("steak1");
            var steak2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FlatHealth/DisplaySteakFlat.prefab").WaitForCompletion().InstantiateClone("steak2");
            var steak3 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FlatHealth/DisplaySteakFlat.prefab").WaitForCompletion().InstantiateClone("steak3");
            var seed = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Seed/PickupSeed.prefab").WaitForCompletion().InstantiateClone("seed");
            var cleaver = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/meshChefCleaverGhost1.FBX").WaitForCompletion().InstantiateClone("cleaverGhost");
            var wok = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/lemuriantemple/Assets/LTWok.prefab").WaitForCompletion().InstantiateClone("wok");

            steak1.transform.SetParent(mealPrep.transform, false);
            steak2.transform.SetParent(mealPrep.transform, false);
            steak3.transform.SetParent(mealPrep.transform, false);
            seed.transform.SetParent(mealPrep.transform, false);
            cleaver.transform.SetParent(mealPrep.transform, false);
            wok.transform.SetParent(mealPrep.transform, false);
            lunarTable.transform.SetParent(mealPrep.transform, false);

            steak1.transform.localPosition = new Vector3();
            steak1.transform.rotation = Quaternion.Euler(new Vector3());
            steak1.transform.localScale = new Vector3();

            steak1.transform.localPosition = new Vector3(0.806f, -0.061f, 1.474f);
            steak1.transform.localRotation = Quaternion.Euler(272.0293f, 356.7472f, 66.6001f);
            steak1.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            steak2.transform.localPosition = new Vector3(-0.522f, -0.061f, 2.164f);
            steak2.transform.localRotation = Quaternion.Euler(272.0291f, 245.6929f, 66.6f);
            steak2.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            steak3.transform.localPosition = new Vector3(-0.492f, 0.088f, 2.209f);
            steak3.transform.localRotation = Quaternion.Euler(274.538f, 270.5f, 90.0001f);
            steak3.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            seed.transform.localPosition = new Vector3(0.366f, -0.162f, 1.872f);
            seed.transform.localRotation = Quaternion.Euler(48.0785f, 327.5262f, 319.4f);
            seed.transform.localScale = new Vector3(0.1043f, 0.1043f, 0.1043f);

            //it actually has different cleaver prefab but its not loading
            cleaver.transform.localPosition = new Vector3(0.922f, -0.7f, -0.01f);
            cleaver.transform.localRotation = Quaternion.Euler(0f, 205.2114f, 127.0116f);
            cleaver.transform.localScale = new Vector3(1f, 1f, 1f);

            wok.transform.localPosition = new Vector3(-0.28f, 0.7f, -1.79f);
            wok.transform.localRotation = Quaternion.Euler(270f, 302.2861f, 0f);
            wok.transform.localScale = new Vector3(3.1129f, 3.1129f, 3.1129f);

            lunarTable.transform.localPosition = new Vector3(-0.289f, -1.256f, 0.01f);
            lunarTable.transform.localRotation = Quaternion.Euler(270f, 90f, 0f);
            lunarTable.transform.localScale = new Vector3(0.9041f, 0.9041f, 0.9041f);

            var cleaverMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Chef/matChef_Cleaver.mat").WaitForCompletion();
            var renderer = cleaver.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = cleaverMat;
            }

            //todo different materials
            //var tableMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/goolake/matGoolakeStoneStructs.mat").WaitForCompletion();
            //var rendererTable = lunarTable.GetComponentInChildren<Renderer>();
            //if(rendererTable != null)
            //{
            //    rendererTable.material = tableMat;
            //}

            card.prefab = chefPrefab;
            card.directorCreditCost = 0;
            card.sendOverNetwork = true;
            card.hullSize = HullClassification.Human;
            card.orientToFloor = true;
            card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            wanderingChefSpawnCard = card;
        }

        public static InteractableSpawnCard CreateRoutePortalSpawnCard(SceneDef destination)
        {
            var m = routePortalPrefab.GetComponent<RoutePortalManager>();
            m.destinationSceneIndex = (int)destination.sceneDefIndex;

            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            interactableSpawnCard.name = "iscRoutePortal";
            interactableSpawnCard.prefab = routePortalPrefab;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.hullSize = HullClassification.Human;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
            interactableSpawnCard.directorCreditCost = 0;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.orientToFloor = false;
            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            return interactableSpawnCard;
        }

        //TODO rewrite
        public static void CheckAssetsLoaded()
        {
            bool loadingFailed = false;

            if (bundle == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(bundle)}");
            }

            if (routePortalPrefab == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(routePortalPrefab)}");
            }

            if (iconBubblePrefab == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(iconBubblePrefab)}");
            }

            if (chefPrefab == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(chefPrefab)}");
            }

            if (droneSymbolTexture == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(droneSymbolTexture)}");
            }

            if (rewardSymbolTexture == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(rewardSymbolTexture)}");
            }

            if (aurelioniteSymbolTexture == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(aurelioniteSymbolTexture)}");
            }

            if (tier1Printer == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(tier1Printer)}");
            }

            if (tier2Printer == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(tier2Printer)}");
            }

            if (tier3Printer == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(tier3Printer)}");
            }

            if (bossPrinter == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(bossPrinter)}");
            }

            if (wanderingChefSpawnCard == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(wanderingChefSpawnCard)}");
            }

            if (bomb == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(bomb)}");
            }

            if (placeholderIcon == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(placeholderIcon)}");
            }

            if (pingCurve == null)
            {
                loadingFailed = true;
                Log.Warning($"{nameof(CheckAssetsLoaded)} Failed to load {nameof(pingCurve)}");
            }

            if (loadingFailed)
            {
                Log.Error($"{nameof(CheckAssetsLoaded)}: One or more assets failed to load");
            }
            else
            {
                Log.Info($"{nameof(CheckAssetsLoaded)}: All assets loaded");
            }
        }

    }
}

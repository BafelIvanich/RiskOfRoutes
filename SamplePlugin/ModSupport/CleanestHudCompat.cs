using RiskOfRoutes.UI;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfRoutes.ModSupport
{
    internal class CleanestHudCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(CleanestHud.Plugin.Id);
                    Log.Info("IS IS ENBALBESD: " + _enabled);
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Hooks()
        {
            RouteMapUI.OnRouteMapDrawn += RouteMapUI_OnRouteMapDrawn;
            RouteMapUI.OnPunishmentBarDrawn += RouteMapUI_OnPunishmentBarDrawn;
            ModUIManager.OnRoutePortalPanelUpdated += ModUIManager_OnRoutePortalPanelUpdated;
        }

        //todo shorten to not iterate everytime
        //todo script for routePortalPanel
        private static void ModUIManager_OnRoutePortalPanelUpdated(GameObject routePortalPanel, RoutePortalManager mg)
        {
            if (enabled && RiskOfRoutes.cleanestHudCompat.Value)
            {
                var survivorColor = CleanestHud.HudChanges.HudColor.SurvivorColor;

                switch (mg.routeNode.portalType)
                {
                    case RoutePortalType.Combat:
                        survivorColor = new Color(191f / 255f, 77f / 255f, 0f / 255f, 1.0f);
                        break;

                    case RoutePortalType.Utility:
                        survivorColor = new Color(22f / 255f, 0f / 255f, 191f / 255f, 1.0f);
                        break;

                    case RoutePortalType.Heal:
                        survivorColor = new Color(34f / 255f, 191f / 255f, 0f / 255f, 1.0f);
                        break;

                    case RoutePortalType.ChefType:
                        survivorColor = new Color(121f / 255f, 191f / 255f, 10f / 255f, 1.0f);
                        break;

                    case RoutePortalType.Rare:
                        survivorColor = new Color(191f / 255f, 8f / 255f, 0f / 255f, 1.0f);
                        break;

                    case RoutePortalType.DroneType:
                        survivorColor = new Color(27f / 255f, 66f / 255f, 191f / 255f, 1.0f);
                        break;

                    case RoutePortalType.ItemType:
                    default:
                        survivorColor = new Color(10f / 255f, 64f / 255f, 191f / 255f, 1.0f);
                        break;
                }

                Log.Info("ПогнаооФж");
                var generalOutline = routePortalPanel.transform.Find("Outline").gameObject;

                GameObject isReadyPanel = null;
                Image cleanestHudOutline = null;
                foreach (var skillIcon in CleanestHud.Main.MyHud.skillIcons)
                {
                    isReadyPanel = skillIcon.isReadyPanelObject;
                    cleanestHudOutline = isReadyPanel.GetComponent<Image>();
                    //if (cleanestHudOutline != null) break;
                }

                generalOutline.GetComponent<Image>().sprite = cleanestHudOutline.sprite;
                generalOutline.GetComponent<Image>().color = survivorColor;

                var portraitOutline = routePortalPanel.transform.Find("PortraitRect/Outline").gameObject;

                portraitOutline.GetComponent<Image>().sprite = cleanestHudOutline.sprite;
                portraitOutline.GetComponent<Image>().color = survivorColor;

                var portraitRect = routePortalPanel.transform.Find("PortraitRect").gameObject;
                portraitRect.GetComponent<Image>().color = new Color(0.102f, 0.0118f, 0.102f, 0.576f);

                var bodyRect = routePortalPanel.transform.Find("BodyRect (1)").gameObject;
                bodyRect.GetComponent<Image>().color = new Color(0.102f, 0.0118f, 0.102f, 0.576f);

            }
        }

        private static void RouteMapUI_OnPunishmentBarDrawn(GameObject obj)
        {
            if (enabled && RiskOfRoutes.cleanestHudCompat.Value)
            {
                var script = obj.GetComponent<RouteMapUI>();
                if (script == null)
                {
                    return;
                }
                var survivorColor = CleanestHud.HudChanges.HudColor.SurvivorColor;

                script.barImageHorizontal.color = survivorColor;
                script.barImageVertical.color = survivorColor;
            }
        }

        private static void RouteMapUI_OnRouteMapDrawn(GameObject routeMap)
        {
            if (enabled && RiskOfRoutes.cleanestHudCompat.Value)
            {
                Log.Info($"Survivor color: {CleanestHud.HudChanges.HudColor.SurvivorColor}");
                var script = routeMap.GetComponent<RouteMapUI>();
                if (script == null)
                {
                    return;
                }
                routeMap.transform.localPosition = RiskOfRoutes.cleanestHudOption.Value == RiskOfRoutes.CleanestHUDOptions.BottomCenter ? RouteMapUI.CleanestHUDCenterPos : RouteMapUI.CleanestHUDBottomRightPos;
                routeMap.transform.localEulerAngles = RiskOfRoutes.cleanestHudOption.Value == RiskOfRoutes.CleanestHUDOptions.BottomCenter ? new Vector3(0, 0, 0) : new Vector3(0, 6, 0);
                var rect = routeMap.GetComponent<RectTransform>();
                rect.sizeDelta = RouteMapUI.CleanestHudRectSize;

                Log.Info("Got it");
                var survivorColor = CleanestHud.HudChanges.HudColor.SurvivorColor;

                var nodeContainer = script.nodeContainer;
                var containerBGImage = nodeContainer.GetComponent<UnityEngine.UI.Image>();
                if (containerBGImage != null)
                {
                    containerBGImage.color = new Color(0.102f, 0.0118f, 0.102f, 0.576f);
                }
                else
                {
                    Log.Error("Container BG Image is null");
                }

                GameObject isReadyPanel = null;
                Image cleanestHudOutline = null;
                foreach (var skillIcon in CleanestHud.Main.MyHud.skillIcons)
                {
                    isReadyPanel = skillIcon.isReadyPanelObject;
                    cleanestHudOutline = isReadyPanel.GetComponent<Image>();
                    //if (cleanestHudOutline != null) break;
                }

                var outlineObj = script.mainContainer.Find("Outline").gameObject;
                var outlineImage = outlineObj.GetComponent<Image>();
                Log.Info($"Outline image: {outlineImage.name}, cleanest hud outline: {cleanestHudOutline.name}");

                outlineImage.sprite = cleanestHudOutline.sprite;
                outlineImage.color = survivorColor;

                //bars
                var horizontalBars = script.horizontalBars;
                var horBarOutline = horizontalBars.transform.Find("HorizontalBarContainer/Outline");
                var horLabelRect = horizontalBars.transform.Find("HorizontalBarContainer/LabelRect");

                if (horBarOutline != null)
                {
                    var img = horBarOutline.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = cleanestHudOutline.sprite;
                        img.color = survivorColor;
                    }
                }

                if (horLabelRect != null)
                {
                    var img = horLabelRect.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(0.102f, 0.0118f, 0.102f, 0.576f);
                    }
                }

                var horLabelOutline = horizontalBars.transform.Find("HorizontalLabelContainer/Outline");
                var horLabelLabelRect = horizontalBars.transform.Find("HorizontalLabelContainer/LabelRect");

                if (horLabelOutline != null)
                {
                    var img = horLabelOutline.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = cleanestHudOutline.sprite;
                        img.color = survivorColor;
                    }
                }

                if (horLabelLabelRect != null)
                {
                    var img = horLabelLabelRect.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(0.102f, 0.0118f, 0.102f, 0.576f);
                    }
                }

                var verticalBars = script.verticalBars;
                if (verticalBars != null)
                {
                    var vertLabelOutline = verticalBars.transform.Find("VerticalLabelContainer/Outline");
                    var vertLabelLabelRect = verticalBars.transform.Find("VerticalLabelContainer/LabelRect");

                    if (vertLabelOutline != null)
                    {
                        var img = vertLabelOutline.GetComponent<Image>();
                        if (img != null)
                        {
                            img.sprite = cleanestHudOutline.sprite;
                            img.color = survivorColor;
                        }
                    }

                    if (vertLabelLabelRect != null)
                    {
                        var img = vertLabelLabelRect.GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = new Color(0.102f, 0.0118f, 0.102f, 0.576f);
                        }
                    }

                    var vertBarOutline = verticalBars.transform.Find("VerticalBarContainer/Outline");
                    var vertBarLabelRect = verticalBars.transform.Find("VerticalBarContainer/LabelRect");

                    if (vertBarOutline != null)
                    {
                        var img = vertBarOutline.GetComponent<Image>();
                        if (img != null)
                        {
                            img.sprite = cleanestHudOutline.sprite;
                            img.color = survivorColor;
                        }
                    }

                    if (vertBarLabelRect != null)
                    {
                        var img = vertBarLabelRect.GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = new Color(0.102f, 0.0118f, 0.102f, 0.576f);
                        }
                    }
                }
            }
        }
    }
}

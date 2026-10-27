using RoR2;
using RoR2.UI;
using SamplePlugin;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfRoutes.UI
{
    public class RouteMapUI : MonoBehaviour
    {
        private Dictionary<RouteNodeSync, RectTransform> MapNodes = new Dictionary<RouteNodeSync, RectTransform>();

        public GameObject nodePrefab;
        public GameObject linePrefab;
        public RectTransform nodeContainer;
        public RectTransform mainContainer;
        public GameObject horizontalBars;
        public GameObject verticalBars;
        public Image barImageHorizontal;
        public Image barImageVertical;
        public GameObject labelHorizontal;
        public GameObject labelVertical;

        public bool isMapVertical;
        public bool isBarVertical;

        public Sprite empty;
        public Sprite drone;
        public Sprite chef;
        public Sprite item;
        public Sprite rare;
        public Sprite objective;
        public Sprite shop;
        public Sprite goldShores;
        public Sprite colossus;
        public Sprite hardware;
        public Sprite voidFields;
        public Sprite celestial;
        public Sprite falseSon;
        public Sprite lineImage;
        public Sprite starting;
        public Sprite solusWing;
        public Sprite heal;
        public Sprite combat;
        public Sprite utility;

        public static Vector3 CleanestHUDCenterPos = new Vector3(0, -300, 0);
        public static Vector3 CleanestHUDBottomRightPos = new Vector3(580, -480, 0);

        public static Vector3 DefaultPos = new Vector3(0, -500, 0);

        public static Vector2 CleanestHudRectSize = new Vector2(-1300, -900);
        public static Vector2 DefaultRectSize = new Vector2(-1140, -900);

        public void DrawMap()
        {
            DrawMap(new Vector2(-1, -1));
        }

        public void DrawBar()
        {
            isBarVertical = RiskOfRoutes.punishmentBarOrientation.Value == RiskOfRoutes.MapOrientation.Vertical;

            if (StageModifierDirector.instance.punishmentTime == -1)
            {
                verticalBars.SetActive(false);
                horizontalBars.SetActive(false);
                return;
            }
            else
            {
                if (isBarVertical)
                {
                    verticalBars.SetActive(true);
                    horizontalBars.SetActive(false);
                }
                else
                {
                    verticalBars.SetActive(false);
                    horizontalBars.SetActive(true);
                }
            }



            float percentShown = (Run.instance.GetRunStopwatch() - StageModifierDirector.instance.stageEnterTime) / (StageModifierDirector.instance.punishmentTime * 60f) % 1.0f;
            int punishStack = (int)((Run.instance.GetRunStopwatch() - StageModifierDirector.instance.stageEnterTime) / (StageModifierDirector.instance.punishmentTime * 60f));

            //Log.Info($"Bar Image {barImageVertical.fillAmount}");
            //Log.Info($"Current stage enter time: {StageModifierDirector.instance.stageEnterTime}");
            //Log.Info($"Current punish time in run: {StageModifierDirector.instance.punishmentTime}");

            barImageHorizontal.fillAmount = percentShown;
            barImageVertical.fillAmount = percentShown;

            barImageHorizontal.color = RiskOfRoutes.punishmentBarColor.Value;
            barImageVertical.color = RiskOfRoutes.punishmentBarColor.Value;

            foreach (var image in new Image[] { barImageHorizontal, barImageVertical })
            {
                var barTooltip = image.GetComponent<TooltipProvider>();
                if (barTooltip == null)
                {
                    Log.Error($"{nameof(DrawMap)}: No tooltip provider for bar images");
                    return;
                }
                barTooltip.titleToken = "PUNISHMENT_BAR_NAME";
                barTooltip.bodyToken = Language.GetStringFormatted("PUNISHMENT_BAR_DESCRIPTION", (int)StageModifierDirector.instance.punishmentTime);
                barTooltip.titleColor = RiskOfRoutes.punishmentBarColor.Value;
            }

            foreach (var label in new GameObject[] { labelHorizontal, labelVertical })
            {
                if (label == null)
                {
                    Log.Error($"{nameof(DrawMap)}: No label found");
                    return;
                }
                var labelTooltip = label.GetComponent<TooltipProvider>();
                if (labelTooltip == null)
                {
                    Log.Error($"{nameof(DrawMap)}: No tooltip provider for punish label");
                    return;
                }
                labelTooltip.titleToken = Language.GetStringFormatted("PUNISHMENT_VALUE_NAME", punishStack);
                labelTooltip.bodyToken = "PUNISHMENT_VALUE_DESCRIPTION";
                labelTooltip.titleColor = RiskOfRoutes.punishmentBarColor.Value;

                var text = label.GetComponent<TextMeshProUGUI>();
                if (text == null)
                {
                    Log.Error($"{nameof(DrawMap)}: No text component found");
                    return;
                }

                if (label == labelVertical)
                    text.text = $"{punishStack}";
                else text.text = Language.GetStringFormatted("PUNISHMENT_VALUE_NAME", punishStack);
            }

            OnPunishmentBarDrawn?.Invoke(gameObject);
        }

        public static event Action<GameObject> OnPunishmentBarDrawn;

        public void DrawMap(Vector2 selectedNodeCoords, bool isObjective = false)
        {
            transform.localPosition = DefaultPos;

            //Log.Info($"{nameof(DrawMap)}: Coords of selected node:{selectedNodeCoords}");

            DrawBar();

            ClearContainer();
            MapNodes.Clear();

            //todo for attaching to scoreboard option
            //isMapVertical = true;

            int mapWidth = StageModifierDirector.instance.mapWidth;
            int mapHeight = StageModifierDirector.instance.mapHeight;
            bool isMoonVisited = StageModifierDirector.instance.isMoonVisited;

            bool isFalseSonVisited = false;

            List<RouteNodeSync> map = StageModifierDirector.instance.routeNodesSynced;
            if (map == null || map.Count == 0)
            {
                if (map == null) Log.Error($"{nameof(DrawMap)}: Map is null");
                if (map.Count == 0) Log.Error($"{nameof(DrawMap)}: Map is empty");
                return;
            }

            float scaleMultiplier = RiskOfRoutes.routeMapSize.Value / 100f;

            RectTransform mapRect = GetComponent<RectTransform>();
            mapRect.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1f);

            var iconsSize = 42f * (RiskOfRoutes.routeMapIconSize.Value / 100f);

            var selectedNode = GetNode((int)selectedNodeCoords.x, (int)selectedNodeCoords.y, map, mapHeight);
            //Log.Info($"{nameof(DrawMap)}: Selected node:{selectedNode.x}:{selectedNode.y}:{selectedNode.portalType}-{selectedNode.visited}");

            Transform linesContainer = new GameObject("Lines", typeof(RectTransform)).transform;
            linesContainer.SetParent(nodeContainer, false);
            Transform nodesContainer = new GameObject("Nodes", typeof(RectTransform)).transform;
            nodesContainer.SetParent(nodeContainer, false);

            var pingCurve = AssetManager.pingCurve;

            GameObject objectiveNode = Instantiate(nodePrefab, nodesContainer);
            RectTransform objectiveRect = objectiveNode.GetComponent<RectTransform>();
            //ноды
            //ширина карты исключая пустые строки резервированные под порталы длс2 длс3
            //int effectiveWidth = map.GetEffectiveWidth(selectedNode);
            List<int> visibleRows = GetEffectiveWidth(selectedNode, mapWidth, mapHeight, map);
            int effectiveWidth = visibleRows.Count;

            float containerWidth = nodeContainer.rect.width;
            float containerHeight = nodeContainer.rect.height;

            float xPadding = containerWidth / 20;
            float yPadding = containerHeight / 10;

            float mapContainerWidth = containerWidth - xPadding * 2;
            float mapContainerHeight = containerHeight - yPadding * 2;

            float xStep = isMapVertical ? mapContainerWidth / (effectiveWidth - 1) : mapContainerWidth / mapHeight;
            float yStep = isMapVertical ? mapContainerHeight / mapHeight : mapContainerHeight / (effectiveWidth - 1);

            objectiveRect.anchorMin = new Vector2(0.5f, 0.5f);
            objectiveRect.anchorMax = new Vector2(0.5f, 0.5f);

            float objX = isMapVertical ? 0 : mapContainerWidth / 2;
            float objY = isMapVertical ? mapContainerHeight / 2 : 0;

            objectiveRect.anchoredPosition = new Vector2(objX, objY);
            objectiveRect.sizeDelta = new Vector2(75f, 75f);

            var img = objectiveNode.GetComponent<Image>();
            img.sprite = objective;
            //for later boss modifiers
            //ModUIManager.AddIconToImage("smth", img);
            if (isMoonVisited)
                img.color = new Color(0.887f, 0.870f, 0.172f, 1f);

            //width+1 потому что для порталов слой
            for (int mx = 0; mx < mapWidth + 1; mx++)
            {
                int visualX = visibleRows.IndexOf(mx);
                if (visualX == -1) continue;

                for (int my = 0; my < mapHeight; my++)
                {

                    var currentNode = GetNode(mx, my, map, mapHeight);

                    if (currentNode.portalType == RoutePortalType.FalseSon && currentNode.visited) isFalseSonVisited = true;

                    bool isSpecialHidden = currentNode.portalType == RoutePortalType.Colossus ||
                        currentNode.portalType == RoutePortalType.FalseSon ||
                        currentNode.portalType == RoutePortalType.Hardware ||
                        currentNode.portalType == RoutePortalType.SolusWing;

                    //Log.Info($"\tIn RouteMapUI: currentNode:{currentNode.x}.{currentNode.y},{currentNode.portalType}");
                    //Log.Info($"Selected node:{selectedNode.x},{selectedNode.y}, {selectedNode.portalType}");

                    if (currentNode.x == -1 || currentNode.y == -1) continue;
                    if (currentNode.portalType == RoutePortalType.None && my != 0) continue;
                    if (isSpecialHidden && currentNode.visited == false && !currentNode.Equals(selectedNode)) continue;

                    float xPos;
                    float yPos;

                    if (isMapVertical)
                    {
                        if (my == 0)
                        {
                            xPos = 0;
                            yPos = -(mapContainerHeight / 2);
                        }
                        else
                        {
                            xPos = -(mapContainerWidth / 2) + visualX * xStep;
                            yPos = -(mapContainerHeight / 2) + my * yStep;
                        }
                    }
                    else
                    {
                        if (my == 0)
                        {
                            xPos = -(mapContainerWidth / 2);
                            yPos = 0;
                        }
                        else
                        {
                            xPos = -(mapContainerWidth / 2) + my * xStep;
                            yPos = -(mapContainerHeight / 2) + visualX * yStep;
                        }
                    }


                    GameObject routeNode = Instantiate(nodePrefab, nodesContainer);
                    RectTransform routeNodeRect = routeNode.GetComponent<RectTransform>();

                    //TODO еслиd выбранный

                    if (currentNode.Equals(selectedNode) && pingCurve != null && !isMoonVisited && isObjective)
                    {
                        //Log.Info("Curve is not null");
                        var curve = routeNode.AddComponent<ObjectScaleCurve>();
                        curve.useOverallCurveOnly = pingCurve.useOverallCurveOnly;
                        curve.overallCurve = pingCurve.overallCurve;
                        curve.curveX = pingCurve.curveX;
                        curve.curveY = pingCurve.curveY;
                        curve.curveZ = pingCurve.curveZ;
                        curve.timeMax = pingCurve.timeMax;

                        curve.enabled = false;
                        curve.enabled = true;

                    }

                    routeNodeRect.anchorMin = new Vector2(0.5f, 0.5f);
                    routeNodeRect.anchorMax = new Vector2(0.5f, 0.5f);
                    routeNodeRect.anchoredPosition = new Vector2(xPos, yPos);
                    routeNodeRect.sizeDelta = new Vector2(iconsSize, iconsSize);


                    bool isHighlighted = currentNode.visited || currentNode.Equals(selectedNode);

                    SetIconAndTooltip(routeNode, GetNode(mx, my, map, mapHeight).portalType, mx, my, isHighlighted);

                    MapNodes.Add(currentNode, routeNodeRect);
                }
            }

            if (isMoonVisited || isObjective)
            {
                //Log.Info("Curve is not null");
                var curve = objectiveNode.AddComponent<ObjectScaleCurve>();
                curve.useOverallCurveOnly = pingCurve.useOverallCurveOnly;
                curve.overallCurve = pingCurve.overallCurve;
                curve.curveX = pingCurve.curveX;
                curve.curveY = pingCurve.curveY;
                curve.curveZ = pingCurve.curveZ;
                curve.timeMax = pingCurve.timeMax;

                curve.enabled = false;
                curve.enabled = true;

            }

            foreach (var pair in MapNodes)
            {
                var parentNode = pair.Key;
                var startRect = pair.Value;


                //луна
                if (parentNode.y == mapHeight - 1)
                {
                    DrawLine(startRect.anchoredPosition, objectiveRect.anchoredPosition, linesContainer, parentNode.visited && isMoonVisited);
                }

                foreach (var childNode in GetChildren(parentNode, map, mapHeight, true))
                {
                    if ((parentNode.portalType == RoutePortalType.Colossus || parentNode.portalType == RoutePortalType.FalseSon) && !(childNode.visited || childNode.Equals(selectedNode))) continue;

                    //todo пиздец нагромождение, фикс бага с двумя ребрами идущими от коллосуса и фолс сана
                    if (parentNode.portalType == RoutePortalType.Colossus && childNode.portalType != RoutePortalType.Colossus && childNode.portalType != RoutePortalType.FalseSon && isFalseSonVisited) continue;


                    if (MapNodes.TryGetValue(childNode, out RectTransform endRect))
                    {
                        DrawLine(startRect.anchoredPosition, endRect.anchoredPosition, linesContainer, childNode.visited && parentNode.visited);

                        if (RiskOfRoutes.showIntermissionNodes.Value && parentNode.visited && childNode.visited)
                            DrawIntermissionNodes(nodesContainer, startRect, childNode, endRect);
                    }
                }
            }


            OnRouteMapDrawn?.Invoke(gameObject);

        }

        public static event Action<GameObject> OnRouteMapDrawn;


        private void DrawIntermissionNodes(Transform nodesContainer, RectTransform startRect, RouteNodeSync childNode, RectTransform endRect)
        {
            var intermissions = new List<RoutePortalType>();
            if (childNode.shopVisited) intermissions.Add(RoutePortalType.Shop);
            if (childNode.goldshoresVisited) intermissions.Add(RoutePortalType.Goldshores);
            if (childNode.voidFieldsVisited) intermissions.Add(RoutePortalType.Arena);

            for (int i = 0; i < intermissions.Count; i++)
            {
                RoutePortalType portalType = intermissions[i];

                float step = i + 1;
                float lim = 1 + intermissions.Count;

                GameObject routeNode = Instantiate(nodePrefab, nodesContainer);
                RectTransform routeNodeRect = routeNode.GetComponent<RectTransform>();

                routeNodeRect.anchorMin = new Vector2(0.5f, 0.5f);
                routeNodeRect.anchorMax = new Vector2(0.5f, 0.5f);
                routeNodeRect.sizeDelta = new Vector2(36f, 36f);

                routeNodeRect.anchoredPosition = startRect.anchoredPosition * ((lim - step) / lim) + endRect.anchoredPosition * (step / lim);

                SetIconAndTooltip(routeNode, portalType, -1, -1, true);
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Transform lineContainer, bool visited = false)
        {
            GameObject lineObj = Instantiate(linePrefab, lineContainer);
            RectTransform rect = lineObj.GetComponent<RectTransform>();

            Vector2 dir = end - start;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (visited)
            {
                lineObj.GetComponent<Image>().color = new Color(0.887f, 0.870f, 0.172f, 1f);
            }
            lineObj.GetComponent<Image>().sprite = lineImage;

            rect.sizeDelta = new Vector2(distance, 3f);
            rect.anchoredPosition = start + dir / 2;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
        }

        //TODO
        private void SetIconAndTooltip(GameObject routeNode, RoutePortalType portalType, int x = -1, int y = -1, bool visited = false)
        {
            var icon = empty;
            switch (portalType)
            {
                case RoutePortalType.None: return;
                case RoutePortalType.Rare: icon = rare; break;
                case RoutePortalType.ItemType: icon = item; break;
                case RoutePortalType.DroneType: icon = drone; break;
                case RoutePortalType.ChefType: icon = chef; break;
                case RoutePortalType.Colossus: icon = colossus; break;
                case RoutePortalType.Hardware: icon = hardware; break;
                case RoutePortalType.SolusWing: icon = solusWing; break;
                case RoutePortalType.Goldshores: icon = goldShores; break;
                case RoutePortalType.Shop: icon = shop; break;
                case RoutePortalType.Arena: icon = voidFields; break;
                case RoutePortalType.FalseSon: icon = falseSon; break;
                case RoutePortalType.Combat: icon = combat; break;
                case RoutePortalType.Heal: icon = heal; break;
                case RoutePortalType.Utility: icon = utility; break;
                case RoutePortalType.Starting: icon = starting; break;
                default: icon = empty; break;
            }

            var image = routeNode.GetComponent<Image>();
            TooltipProvider tooltip = routeNode.AddComponent<TooltipProvider>();
            //tooltip.titleToken = $"{x},{y}:Visited:{visited}";
            //tooltip.bodyToken = "test";
            string typeName = portalType.ToString().ToUpper();

            tooltip.titleToken = $"ROUTENODE_{typeName}_NAME";
            tooltip.bodyToken = $"ROUTENODE_{typeName}_DESCRIPTION";

            //tooltip.titleColor = mod.tooltipColor;
            tooltip.titleColor = new Color(246f / 255f, 90f / 255f, 58f / 255f);
            image.sprite = icon;
            if (visited)
                image.color = new Color(0.887f, 0.870f, 0.172f, 1f);
        }

        private void ClearContainer()
        {
            for (int i = nodeContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(nodeContainer.GetChild(i).gameObject);
            }
        }

        public List<int> GetEffectiveWidth(RouteNodeSync selectedNode, int width, int height, List<RouteNodeSync> map)
        {
            //максимум width+1
            List<int> visibleRows = new List<int>();
            int res = 0;
            for (int x = 0; x < width + 1; x++)
            {
                bool hasVisibleNode = false;
                for (int y = 0; y < height; y++)
                {
                    var node = GetNode(x, y, map, height);

                    if (node.x == -1 || node.y == -1 || node.portalType == RoutePortalType.None) continue;

                    bool isSpecialHidden = node.portalType == RoutePortalType.Colossus ||
                                            node.portalType == RoutePortalType.FalseSon ||
                                            node.portalType == RoutePortalType.Hardware ||
                                            node.portalType == RoutePortalType.SolusWing;

                    if (!isSpecialHidden || node.visited || node.Equals(selectedNode))
                    {
                        hasVisibleNode = true;
                        break;
                    }
                }

                if (hasVisibleNode) visibleRows.Add(x);
            }
            //Log.Info("Rows that has visible nodes:");
            //foreach (var i in visibleRows)
            //{
            //    Log.Info($"\barTooltip{i}");
            //}
            return visibleRows;
        }

        public List<RouteNodeSync> GetChildren(RouteNodeSync node, List<RouteNodeSync> map, int height, bool usePortalNode = false)
        {
            var res = new List<RouteNodeSync>();

            if (node.leftX != -1 && node.leftY != -1)
                res.Add(GetNode(node.leftX, node.leftY, map, height));

            if (node.frontX != -1 && node.frontY != -1)
                res.Add(GetNode(node.frontX, node.frontY, map, height));

            if (node.rightX != -1 && node.rightY != -1)
                res.Add(GetNode(node.rightX, node.rightY, map, height));

            if (usePortalNode && node.bonusX != -1 && node.bonusY != -1)
                res.Add(GetNode(node.bonusX, node.bonusY, map, height));

            return res;
        }

        public RouteNodeSync GetNode(int x, int y, List<RouteNodeSync> map, int mapHeight)
        {
            int idx = x * mapHeight + y;

            if (map != null && idx >= 0 && idx < map.Count) return map[idx];

            return new RouteNodeSync();
        }
    }
}

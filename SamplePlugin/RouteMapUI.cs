using RoR2;
using RoR2.UI;
using SamplePlugin;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static RouteMap;

namespace RiskOfRoutes
{
    public class RouteMapUI : MonoBehaviour
    {
        private Dictionary<RouteNodeSync, RectTransform> MapNodes = new Dictionary<RouteNodeSync, RectTransform>();

        public GameObject nodePrefab;
        public GameObject linePrefab;
        public RectTransform nodeContainer;
        public RectTransform mainContainer;

        public bool isVertical;

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

        public void DrawMap()
        { 
            DrawMap(new Vector2(-1,-1));
        }


        public void DrawMap(Vector2 selectedNodeCoords)
        {
            ClearContainer();
            MapNodes.Clear();

            var map = StageModifierDirector.instance.routeNodesSynced;
            int mapWidth = StageModifierDirector.instance.mapWidth;
            int mapHeight = StageModifierDirector.instance.mapHeight;
            bool isMoonVisited = StageModifierDirector.instance.isMoonVisited;
            if (map == null)
            {
                Log.Error("Map is not created yet");
                return;
            }
            else
            {
                Log.Info($"Map is {mapWidth}");
            }

            var selectedNode = GetNode((int)selectedNodeCoords.x,(int)selectedNodeCoords.y, map, mapHeight);

            Transform linesContainer = new GameObject("Lines", typeof(RectTransform)).transform;
            linesContainer.SetParent(nodeContainer, false);
            Transform nodesContainer = new GameObject("Nodes", typeof(RectTransform)).transform;
            nodesContainer.SetParent(nodeContainer, false);

            GameObject objectiveNode = Instantiate(nodePrefab, nodesContainer);
            RectTransform objectiveRect = objectiveNode.GetComponent<RectTransform>();
            //ноды
            //ширина карты исключая пустые строки резервированные под порталы длс2 длс3
            //int effectiveWidth = map.GetEffectiveWidth(selectedNode);
            int effectiveWidth = GetEffectiveWidth(selectedNode, mapWidth, mapHeight, map);
            Log.Info($"Effective width:{effectiveWidth}");

            float containerWidth = nodeContainer.rect.width;
            float containerHeight = nodeContainer.rect.height;

            float xPadding = containerWidth / 20;
            float yPadding = containerHeight / 10;

            float mapContainerWidth = containerWidth - (xPadding * 2);
            float mapContainerHeight = containerHeight - (yPadding * 2);

            float xStep = isVertical ? (mapContainerWidth / (effectiveWidth - 1)) : (mapContainerWidth / mapHeight);
            float yStep = isVertical ? (mapContainerHeight / mapHeight) : (mapContainerHeight / (effectiveWidth - 1));

            objectiveRect.anchorMin = new Vector2(0.5f, 0.5f);
            objectiveRect.anchorMax = new Vector2(0.5f, 0.5f);

            float objX = isVertical ? 0 : (mapContainerWidth / 2);
            float objY = isVertical ? (mapContainerHeight / 2) : 0;

            objectiveRect.anchoredPosition = new Vector2(objX, objY);
            objectiveRect.sizeDelta = new Vector2(75f, 75f);

            var img = objectiveNode.GetComponent<Image>();
            img.sprite = objective;
            if (isMoonVisited)
                img.color = new Color(0.887f, 0.870f, 0.172f, 1f);

            //width+1 потому что для порталов слой
            for (int mx = 0; mx < mapWidth + 1; mx++)
            {
                for (int my = 0; my < mapHeight; my++)
                {

                    var currentNode = GetNode(mx, my, map,mapHeight);
                    bool isSpecialHidden = (currentNode.portalType == RoutePortalType.Colossus ||
                        currentNode.portalType == RoutePortalType.FalseSon ||
                        currentNode.portalType == RoutePortalType.Hardware ||
                        currentNode.portalType == RoutePortalType.SolusWing);

                    Log.Info($"\tIn RouteMapUI: currentNode:{currentNode.x}.{currentNode.y},{currentNode.portalType}");
                    Log.Info($"Selected node:{selectedNode.x},{selectedNode.y}, {selectedNode.portalType}");

                    if (currentNode.x == -1 || currentNode.y == -1) continue;
                    if (currentNode.portalType == RoutePortalType.None && my != 0) continue;
                    if (isSpecialHidden && (currentNode.visited == false && !currentNode.Equals(selectedNode))) continue;

                    float xPos;
                    float yPos;

                    if (isVertical)
                    {
                        if (my == 0)
                        {
                            xPos = 0;
                            yPos = -(mapContainerHeight / 2);
                        }
                        else
                        {
                            xPos = -(mapContainerWidth / 2) + (mx * xStep);
                            yPos = -(mapContainerHeight / 2) + (my * yStep);
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
                            xPos = -(mapContainerWidth / 2) + (my * xStep);
                            yPos = -(mapContainerHeight / 2) + (mx * yStep);
                        }
                    }


                    GameObject routeNode = Instantiate(nodePrefab, nodesContainer);
                    RectTransform routeNodeRect = routeNode.GetComponent<RectTransform>();

                    //TODO если выбранный


                    GameObject pingPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/PingIndicator.prefab").WaitForCompletion();
                    PingIndicator pingIndicator = pingPrefab.GetComponent<PingIndicator>();
                    ObjectScaleCurve pingCurve = pingIndicator.pingObjectScaleCurve;

                    if (currentNode.Equals(selectedNode) && pingCurve != null)
                    {
                        Log.Info("Curve is not null");
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
                    routeNodeRect.sizeDelta = (mx == 0 && my == 0) ? new Vector2(4f, 4f) : new Vector2(48f, 48f);

                    bool isHighlighted = currentNode.visited || currentNode.Equals(selectedNode);

                    SetIcon(routeNode, GetNode(mx, my,map, mapHeight).portalType, mx, my, isHighlighted);

                    MapNodes.Add(currentNode, routeNodeRect);
                }
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

                foreach (var childNode in GetChildren(parentNode, map, mapHeight,true))
                {
                    if ((parentNode.portalType == RoutePortalType.Colossus || parentNode.portalType == RoutePortalType.FalseSon) && !childNode.visited) continue;


                    if (MapNodes.TryGetValue(childNode, out RectTransform endRect))
                    {
                        DrawLine(startRect.anchoredPosition, endRect.anchoredPosition, linesContainer, childNode.visited && parentNode.visited);
                    }
                }
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

        //TODO потом можно убрать tooltip и координаты в аргументе
        private void SetIcon(GameObject routeNode, RoutePortalType portalType, int x, int y, bool visited = false)
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
                case RoutePortalType.FalseSon: icon = falseSon; break;
                default: icon = empty; break;
            }

            var image = routeNode.GetComponent<Image>();
            RoR2.UI.TooltipProvider tooltip = routeNode.AddComponent<RoR2.UI.TooltipProvider>();
            tooltip.titleToken = $"{x},{y}:Visited:{visited}";
            tooltip.bodyToken = "test";
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
                UnityEngine.Object.DestroyImmediate(nodeContainer.GetChild(i).gameObject);
            }
        }

        public int GetEffectiveWidth(RouteNodeSync selectedNode,int width, int height, SyncListRouteNode map)
        {
            //максимум width+1
            int res = 0;
            for (int x = 0; x < width + 1; x++)
            {
                bool flag = false;
                for (int y = 0; y < height; y++)
                {
                    var node = GetNode(x, y, map, height);

                    if (node.x == -1 || node.y == -1 || node.portalType == RoutePortalType.None) continue;

                    bool isSpecialHidden = (node.portalType == RoutePortalType.Colossus ||
                                            node.portalType == RoutePortalType.FalseSon ||
                                            node.portalType == RoutePortalType.Hardware ||
                                            node.portalType == RoutePortalType.SolusWing);

                    if (!isSpecialHidden || node.visited || node.Equals(selectedNode))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag) res++;
            }
            return res;
        }

        public List<RouteNodeSync> GetChildren(RouteNodeSync node, SyncListRouteNode map, int height, bool usePortalNode = false)
        {
            var res = new List<RouteNodeSync>();    

            if(node.leftX != -1 && node.leftY != -1)
                res.Add(GetNode(node.leftX, node.leftY, map, height));

            if (node.frontX != -1 && node.frontY != -1)
                res.Add(GetNode(node.frontX, node.frontY, map, height));

            if (node.rightX != -1 && node.rightY != -1)
                res.Add(GetNode(node.rightX, node.rightY, map, height));

            if (usePortalNode && (node.bonusX != -1 && node.bonusY != -1))
                res.Add(GetNode(node.bonusX, node.bonusY, map, height));
                



            return res;
        }

        public RouteNodeSync GetNode(int x, int y, SyncListRouteNode map, int mapHeight)
        {
            //Log.Info($"\tMap size:{map.Count}");
            //Log.Info($"\tTrying to access {x},{y}, index {y*mapHeight + x}");
            return map.GetItem(x * mapHeight + y);
        }
    }
}

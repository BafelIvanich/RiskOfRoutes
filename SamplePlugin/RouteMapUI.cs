using RoR2;
using RoR2.UI;
using SamplePlugin;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RiskOfRoutes
{
    public class RouteMapUI : MonoBehaviour
    {
        private Dictionary<RouteMap.RouteNode, RectTransform> MapNodes = new Dictionary<RouteMap.RouteNode, RectTransform>();

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

        public void DrawMap(RouteMap.RouteNode selectedNode = null)
        {
            ClearContainer();
            MapNodes.Clear();

            var map = StageModifierDirector.instance.routeNodesSynced;
            if (map == null)
            {
                Log.Error("Map is not created yet");
                return;
            }
            else
            {
                Log.Info($"Map is {map.width}");
            }


            Transform linesContainer = new GameObject("Lines", typeof(RectTransform)).transform;
            linesContainer.SetParent(nodeContainer, false);
            Transform nodesContainer = new GameObject("Nodes", typeof(RectTransform)).transform;
            nodesContainer.SetParent(nodeContainer, false);

            GameObject objectiveNode = Instantiate(nodePrefab, nodesContainer);
            RectTransform objectiveRect = objectiveNode.GetComponent<RectTransform>();
            //ноды
            //ширина карты исключая пустые строки резервированные под порталы длс2 длс3
            int effectiveWidth = map.GetEffectiveWidth(selectedNode);
            Log.Info($"Effective width:{effectiveWidth}");

            float containerWidth = nodeContainer.rect.width;
            float containerHeight = nodeContainer.rect.height;

            float xPadding = containerWidth / 20;
            float yPadding = containerHeight / 10;

            float mapContainerWidth = containerWidth - (xPadding * 2);
            float mapContainerHeight = containerHeight - (yPadding * 2);

            float xStep = isVertical ? (mapContainerWidth / (effectiveWidth - 1)) : (mapContainerWidth / map.height);
            float yStep = isVertical ? (mapContainerHeight / map.height) : (mapContainerHeight / (effectiveWidth - 1));

            objectiveRect.anchorMin = new Vector2(0.5f, 0.5f);
            objectiveRect.anchorMax = new Vector2(0.5f, 0.5f);

            float objX = isVertical ? 0 : (mapContainerWidth / 2);
            float objY = isVertical ? (mapContainerHeight / 2) : 0;

            objectiveRect.anchoredPosition = new Vector2(objX, objY);
            objectiveRect.sizeDelta = new Vector2(75f, 75f);

            var img = objectiveNode.GetComponent<Image>();
            img.sprite = objective;
            if (map.isMoonVisited)
                img.color = new Color(0.887f, 0.870f, 0.172f, 1f);

            //width+1 потому что для порталов слой
            for (int mx = 0; mx < map.width + 1; mx++)
            {
                for (int my = 0; my < map.height; my++)
                {
                    var currentNode = map.mapGrid[mx, my];
                    if (currentNode == null) continue;
                    if (currentNode.portalType == RoutePortalType.None && my != 0) continue;
                    if ((currentNode.portalType == RoutePortalType.Colossus || currentNode.portalType == RoutePortalType.FalseSon || currentNode.portalType == RoutePortalType.Hardware || currentNode.portalType == RoutePortalType.SolusWing) && (currentNode.visited == false && currentNode != selectedNode)) continue;

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

                    if (currentNode == selectedNode && pingCurve != null)
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

                    bool isHighlighted = map.mapGrid[mx, my].visited || map.mapGrid[mx, my] == selectedNode;

                    SetIcon(routeNode, map.mapGrid[mx, my].portalType, mx, my, isHighlighted);


                    MapNodes.Add(currentNode, routeNodeRect);
                }
            }

            foreach (var pair in MapNodes)
            {
                var parentNode = pair.Key;
                var startRect = pair.Value;


                //луна
                if (parentNode.y == map.height - 1)
                {
                    DrawLine(startRect.anchoredPosition, objectiveRect.anchoredPosition, linesContainer, parentNode.visited && map.isMoonVisited);
                }

                foreach (var childNode in parentNode.GetPaths(true))
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
    }
}

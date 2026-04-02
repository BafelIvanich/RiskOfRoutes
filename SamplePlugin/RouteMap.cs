using RiskOfRoutes;
using System.Collections.Generic;



public class RouteMap
{
    public int width;
    public int height;
    public int startingRoutes = 3;

    public bool isMoonVisited = false;

    public RouteNode[,] mapGrid;
    public RouteNode root;

    public RouteMap(Xoroshiro128Plus rng)
    {
        width = 3;
        height = 5;
        //+1 к ширине под порталы длс2 длс3
        mapGrid = new RouteNode[width + 1, height];
        //старт нод
        mapGrid[0, 0] = new RouteNode(0, 0);
        root = mapGrid[0, 0];
        root.visited = true;
        root.portalType = RoutePortalType.Starting;


        for (int i = 0; i < width; i++)
            for (int j = 1; j < height; j++)
                mapGrid[i, j] = new RouteNode(i, j);

        List<RouteNode> availablePaths = SetupPaths(rng);
        availablePaths.Sort((a, b) => a.x.CompareTo(b.x));

        if (availablePaths.Count >= 1) root.left = availablePaths[0];
        if (availablePaths.Count >= 2) root.front = availablePaths[1];
        if (availablePaths.Count >= 3) root.right = availablePaths[2];
        SetupRewards(rng);

    }

    public void SetupRewards(Xoroshiro128Plus rng)
    {
        //TODO PLACEHOLDER
        for (int i = 0; i < width; i++)
        {
            if (mapGrid[i, 3].portalType != RoutePortalType.None)
            {
                mapGrid[i, 3].portalType = RoutePortalType.ChefType;
            }
            if (mapGrid[i, 2].portalType != RoutePortalType.None)
            {
                mapGrid[i, 2].portalType = rng.NextElementUniform(new List<RoutePortalType>() { RoutePortalType.ItemType, RoutePortalType.DroneType, RoutePortalType.Rare });
            }
            if (mapGrid[i, 1].portalType != RoutePortalType.None)
            {
                mapGrid[i, 1].portalType = rng.NextElementUniform(new List<RoutePortalType>() { RoutePortalType.ItemType, RoutePortalType.DroneType, RoutePortalType.Rare });
            }
            if (mapGrid[i, 4].portalType != RoutePortalType.None)
            {
                mapGrid[i, 4].portalType = rng.NextElementUniform(new List<RoutePortalType>() { RoutePortalType.ItemType, RoutePortalType.DroneType, RoutePortalType.Rare });
            }
        }
    }

    public List<RouteNode> SetupPaths(Xoroshiro128Plus rng)
    {
        List<RouteNode> pathNodes = new List<RouteNode>();
        List<int> nums = new List<int>();
        for (int i = 0; i < width; i++)
        {
            nums.Add(i);
        }
        for (int i = 0; i < startingRoutes; i++)
        {
            if (nums.Count == 0) break;
            Log.Info("Nums" + nums.Count);
            int num = rng.NextElementUniform(nums);
            nums.Remove(num);

            var node = mapGrid[num, 1];
            if (node == null) continue;

            node.portalType = RoutePortalType.Rare;
            CreateRoute(node, node.x, node.y, rng);
            pathNodes.Add(node);
        }
        return pathNodes;
    }


    public void CreateRoute(RouteNode curr, int x, int y, Xoroshiro128Plus rng)
    {
        if (y >= height - 1) return;

        List<RouteNode> available = new List<RouteNode>();

        for (int i = 0; i < 3; i++)
        {
            int nx = x - 1 + i;
            int ny = y + 1;

            if (nx < 0 || nx > width - 1) continue;
            if (i == 0 && mapGrid[nx, y].right != null) continue; //сосед слева
            //if (i == 1 && mapGrid[nx, y].front != null) continue;
            if (i == 2 && mapGrid[nx, y].left != null) continue;

            available.Add(mapGrid[x - 1 + i, y + 1]);
        }
        if (available.Count == 0) return;
        var next = rng.NextElementUniform(available);

        if (available.Count > 1 && rng.nextNormalizedFloat < 0.2f)
        {
            available.Remove(next);
            var branchNext = rng.NextElementUniform(available);
            branchNext.portalType = RoutePortalType.Rare;
            Connect(curr, branchNext);
            CreateRoute(branchNext, branchNext.x, branchNext.y, rng);
        }

        next.portalType = RoutePortalType.Rare;
        Connect(curr, next);
        CreateRoute(next, next.x, next.y, rng);
    }

    //currentLayer для случаев: goldshores/shop в colossus
    public RouteNode CreateColossusNode(RouteNode current, bool currentLayer = false)
    {
        if (current.y == height - 1 || current.y >= 3) return null;
        int y = currentLayer ? current.y : current.y + 1;

        Log.Info("Creating colossus node");
        //проверка есть ли
        RouteNode colossusNode = null;
        for (int i = 0; i < width + 1; i++)
        {
            if (mapGrid[i, y] != null && (mapGrid[i, y].portalType == RoutePortalType.Colossus || mapGrid[i, y].portalType == RoutePortalType.FalseSon))
            {
                colossusNode = mapGrid[i, y];
                Log.Info("Floor already has colossus node");
                return colossusNode;
            }
        }
        Log.Info("Has no colossus nodes");

        //в пустое место
        for (int offset = 0; offset < width; offset++)
        {
            int[] closest = { current.x + offset, current.x - offset };
            foreach (int x in closest)
            {
                if (x >= 0 && x < width && mapGrid[x, y].portalType == RoutePortalType.None)
                {
                    colossusNode = mapGrid[x, y];
                    break;
                }
            }
            if (colossusNode != null) break;
        }

        //места нет
        if (colossusNode == null)
        {
            Log.Warning("No place for colossus node, placing on extra");
            mapGrid[width, y] = new RouteNode(width, y);
            colossusNode = mapGrid[width, y];
            //colossusNode = mapGrid[6,y];
        }

        if (colossusNode != null)
        {
            colossusNode.portalType = y != 3 ? RoutePortalType.Colossus : RoutePortalType.FalseSon;
            current.portalNode = colossusNode;

            ConnectToRandomNodeNextFloor(colossusNode);

            return colossusNode;
        }
        else
        {
            Log.Error("Still null");
        }
        return null;
    }



    public RouteNode CreateHardwareNode(RouteNode current)
    {
        //if (current.y == height - 1) return null;
        int y = current.y + 1;

        Log.Info("Creating hardware node");

        RouteNode hardwareNode = null;
        for (int i = 0; i < width + 1; i++)
        {
            if (mapGrid[i, y] != null && (mapGrid[i, y].portalType == RoutePortalType.Hardware || mapGrid[i, y].portalType == RoutePortalType.SolusWing))
            {
                hardwareNode = mapGrid[i, y];
                Log.Info("Floor already has hardware node");
                return hardwareNode;
            }
        }
        Log.Info("Has no hardware nodes");

        //в пустое место
        for (int offset = 0; offset < width; offset++)
        {
            int[] closest = { current.x + offset, current.x - offset };
            foreach (int x in closest)
            {
                if (x >= 0 && x < width && mapGrid[x, y].portalType == RoutePortalType.None)
                {
                    hardwareNode = mapGrid[x, y];
                    break;
                }
            }
            if (hardwareNode != null) break;
        }
        //места нет
        if (hardwareNode == null)
        {
            Log.Warning("No place for hardware node, placing on extra");
            mapGrid[width, y] = new RouteNode(width, y);
            hardwareNode = mapGrid[width, y];
        }


        if (hardwareNode != null)
        {
            hardwareNode.portalType = y != 4 ? RoutePortalType.Hardware : RoutePortalType.SolusWing;
            current.portalNode = hardwareNode;

            return hardwareNode;
        }
        else
        {
            Log.Error("Still null");
        }
        return null;
    }



    private void ConnectToRandomNodeNextFloor(RouteNode node)
    {
        int y = node.y;
        for (int x = 0; x < width; x++)
        {
            if (mapGrid[x, y + 1] != null && mapGrid[x, y + 1].portalType != RoutePortalType.None)
            {
                node.front = mapGrid[x, y + 1];
            }
        }
    }

    private void Connect(RouteNode curr, RouteNode next)
    {
        if (curr.x - next.x == -1)
        {
            curr.right = next;
        }
        else if (curr.x - next.x == 0)
        {
            curr.front = next;
        }
        else
        {
            curr.left = next;
        }
    }

    public RouteNode GetStartingNode()
    {
        return mapGrid[0, 0];
    }

    public class RouteNode
    {
        public RoutePortalType portalType;

        public int x;
        public int y;

        public bool visited;

        public RouteNode left;
        public RouteNode front;
        public RouteNode right;

        public RouteNode portalNode;

        public RouteNode(int x, int y)
        {
            portalType = RoutePortalType.None;
            this.x = x;
            this.y = y;
        }

        public List<RouteNode> GetPaths(bool usePortalNode = false)
        {
            List<RouteMap.RouteNode> res = new List<RouteMap.RouteNode>();
            if (left != null) res.Add(left);
            if (front != null) res.Add(front);
            if (right != null) res.Add(right);
            if (portalNode != null && usePortalNode) res.Add(portalNode);
            return res;
        }

        public override string ToString()
        {
            return $"Node:{x},{y} with {GetPaths().Count} paths";
        }
    }

}
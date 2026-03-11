using RiskOfRoutes;
using System.Collections.Generic;



public class RouteMap
{
    public int width = 6;
    public const int height = 5;
    public RouteNode[,] mapGrid = new RouteNode[6, 5];

    public RouteMap(Xoroshiro128Plus rng)
    {
        mapGrid = new RouteNode[width, height];
        //старт нод
        mapGrid[0, 0] = new RouteNode(0, 0);
        for (int i = 0; i < width; i++)
        {
            for (int j = 1; j < height; j++)
            {
                mapGrid[i, j] = new RouteNode(i, j);
            }
        }
        Log.Info("Geeeet");
        SetupPaths(rng);
        if (mapGrid[0, 1] != null) mapGrid[0, 0].left = mapGrid[0, 1];
        if (mapGrid[1, 1] != null) mapGrid[0, 0].front = mapGrid[1, 1];
        if (mapGrid[2, 1] != null) mapGrid[0, 0].right = mapGrid[2, 1];
        Log.Info($"Children:{mapGrid[0, 0].GetPaths().Count}");
        SetupRewards();

    }

    public void SetupRewards()
    {
        Log.Error("Setup rewards");
        for (int i = 0; i < width; i++)
        {
            if (mapGrid[i, 3].portalType != RoutePortalType.None)
            {
                mapGrid[i, 3].portalType = RoutePortalType.ChefType;
                Log.Info("Reward set");
            }
            else
            {
                Log.Error(mapGrid[i, 2].portalType);
            }
        }
    }

    public void SetupPaths(Xoroshiro128Plus rng)
    {
        for (int i = 0; i < width; i++)
        {
            var node = mapGrid[i, 1];
            node.portalType = RoutePortalType.Rare;
            CreateRoute(node, node.x, node.y, rng);
            Log.Info($"Created route starting with :{mapGrid[i, 1]}");
            if (mapGrid[i, 1] == null)
            {
                Log.Error("\tWTF");
            }
            else
            {
                Log.Info("\tGood");
            }
            //PrintRoute(node);
        }
    }

    private void PrintRoute(RouteNode node)
    {
        RouteNode curr = node;
        while (curr != null)
        {
            var children = curr.GetPaths();
            //Log.Info($"Route {curr.x},{curr.y} has {children.Count} paths");
            //foreach(var n in children)
            //{
            //    Log.Info($"\t{n.x},{n.y}");
            //}
        }
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
        next.portalType = RoutePortalType.Rare;
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
        CreateRoute(next, next.x, next.y, rng);
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

        public RouteNode left;
        public RouteNode front;
        public RouteNode right;

        public RouteNode(int x, int y)
        {
            portalType = RoutePortalType.None;
            this.x = x;
            this.y = y;
        }

        public List<RouteNode> GetPaths()
        {
            List<RouteMap.RouteNode> res = new List<RouteMap.RouteNode>();
            if (left != null) res.Add(left);
            if (front != null) res.Add(front);
            if (right != null) res.Add(right);
            return res;
        }

        public override string ToString()
        {
            return $"Node:{x},{y} with {GetPaths().Count} paths";
        }
    }

}
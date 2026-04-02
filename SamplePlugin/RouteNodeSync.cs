using System;

namespace SamplePlugin
{
    [System.Serializable]
    public struct RouteNodeSync : IEquatable<RouteNodeSync>
    {
        public RoutePortalType portalType;

        public int x = -1;
        public int y = -1;

        public int frontX = -1;
        public int frontY = -1;

        public int leftX = -1;
        public int leftY = -1;

        public int rightX = -1;
        public int rightY = -1;

        public int bonusX = -1;
        public int bonusY = -1;

        public bool visited;

        public RouteNodeSync(RouteMap.RouteNode node)
        {
            x = node.x;
            y = node.y;
            visited = node.visited;
            portalType = node.portalType;

            var front = node.front;
            if (front != null)
            {
                frontX = front.x;
                frontY = front.y;
            }
            var left = node.left;
            if (left != null)
            {
                leftX = left.x;
                leftY = left.y;
            }
            var right = node.right;
            if (node.right != null)
            {
                rightX = right.x;
                rightY = right.y;
            }

            if (node.portalNode != null)
            {
                bonusX = node.portalNode.x;
                bonusY = node.portalNode.y;
            }

        }

        public bool Equals(RouteNodeSync other)
        {
            return (this.x == other.x) && (this.y == other.y);
        }
    }
}


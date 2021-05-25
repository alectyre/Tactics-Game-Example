using PathfindingCore;

namespace GridPathfinding
{
	public class GridNode : PathfinderNode
	{
		public enum NodeType { Open, Closed };
		public NodeType type;

		//Grid coordinates
		public int x;
		public int y;

        public override string ToString()
        {
			return "GridNode[" + x + ", " + y + "]";
        }
    }
}
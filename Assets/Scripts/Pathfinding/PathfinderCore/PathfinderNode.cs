using System.Collections.Generic;

namespace PathfindingCore
{
	/// <summary>
	/// Search node for use with PathFinder and PathFinderMover classes. Subclass PathFinderNode to create nodes appropriate
	/// for your map type. When constructing a map, fill the adjacent List nodes this node is connected to, and add data for
	/// your PathfinderMover classes to use to evaluate movement costs.
	/// </summary>
	public class PathfinderNode
	{
        public List<PathfinderNode> adjacent = new List<PathfinderNode>();
    }
}
using Priority_Queue;
using System.Collections.Generic;
using System.Linq;

namespace PathfindingCore
{
	/// <summary>
	/// Abstract PathfinderBase class.
	/// </summary>
	public abstract class PathfinderBase<T> where T : PathfinderNode, new()
	{
		#region Pathfinding functions

		/// <summary>
		/// Finds the shortest path from start to target for mover. Returns null if no path exists.
		/// </summary>
		/// <param name="mover">The mover to evaluate pathing costs</param>
		/// <param name="start">The node to start from</param>
		/// <param name="target">The node to path to</param>
		/// <returns></returns>
		public static List<T> FindPath(PathfinderMover<T> mover, T start, T target)
		{
			if (mover == null || start == null || target == null)
			{
				return null;
			}

			if (start == target)
				return new List<T>() { start };

			SimplePriorityQueue<T> openList = new SimplePriorityQueue<T>();
			HashSet<T> closedList = new HashSet<T>();
			Dictionary<T, PathfinderData> pathfinderData = new Dictionary<T, PathfinderData>();

			T current = start;

			int runCount = 0;

			//A* algorithm
			while (current != target)
			{
				runCount++;

				closedList.Add(current);
				PathfinderData currentData = DataForNode(current, pathfinderData);

				foreach (T adjacent in current.adjacent)
				{
					if (!mover.Passable(current, adjacent))
						continue;

					PathfinderData adjacentData = DataForNode(adjacent, pathfinderData);

					float cost = currentData.G + mover.CostForMove(current, adjacent);

					if (cost < adjacentData.G)
					{
						if (openList.Contains(adjacent))
							openList.Remove(adjacent);

						if (closedList.Contains(adjacent))
							closedList.Remove(adjacent);
					}

					if (!openList.Contains(adjacent) && !closedList.Contains(adjacent))
					{
						adjacentData.G = cost;
						adjacentData.H = mover.Heuristic(adjacent, target, start);
						adjacentData.parent = current;
						pathfinderData[adjacent] = adjacentData;

						openList.Enqueue(adjacent, adjacentData.F);
					}
				}

				//If we've reached the end of the open list and haven't found the target, there is no path
				if (openList.Count == 0)
				{
					return null;
				}

				current = openList.Dequeue();
			}

			List<T> path = RetracePath(current, pathfinderData);

			return path;
		}

		/// <summary>
		/// Finds all reachable nodes from start for mover within maxCost.
		/// </summary>
		/// <param name="mover">The mover to evaluate pathing costs</param>
		/// <param name="start">The node to start from</param>
		/// <param name="maxCost">The maximum cost of a reachable tile</param>
		/// <returns></returns>
		public static List<T> FindAllReachable(PathfinderMover<T> mover, T start, float maxCost)
		{
			if (mover == null || start == null)
				return null;

			Queue<T> queue = new Queue<T>();
			HashSet<T> reachable = new HashSet<T>();
			Dictionary<T, PathfinderData> pathfinderData = new Dictionary<T, PathfinderData>();

			queue.Enqueue(start);
			reachable.Add(start);

			//Cost conscious breadth first search
			while (queue.Count > 0)
			{
				T node = queue.Dequeue() as T;
				PathfinderData nodeData = DataForNode(node, pathfinderData);

				//Check cost to move to each adjacent node
				foreach (T adjacent in node.adjacent)
				{
					if (!mover.Passable(node, adjacent))
						continue;

					PathfinderData adjacentData = DataForNode(adjacent, pathfinderData);

					float cost = nodeData.G + mover.CostForMove(node, adjacent);

					if (cost > maxCost)
						continue;

					//Check if this is a shorter path to an already checked node,
					//and if so set it to be checked again with the updated cost
					if (cost < adjacentData.G && reachable.Contains(adjacent))
						reachable.Remove(adjacent);

					if (!reachable.Contains(adjacent))
					{
						adjacentData.G = cost;
						pathfinderData[adjacent] = adjacentData;

						reachable.Add(adjacent);
						queue.Enqueue(adjacent);
					}
				}
			}

			return reachable.ToList();
		}

        #endregion


        #region Wrapper struct to store operation data for nodes

        /// <summary>
        /// Wrapper struct for PathfinderNode that holds pathfinding data
        /// </summary>
        private struct PathfinderData
		{
			public float G; //Cost of current path to this node
			public float H; //Estimated cost from this node to the target
			public float F { get { return G + H; } }
			public T parent; //Used in A*
			public T pathfinderNode; //The PathfinderNode this data is wrapping
		}

		#endregion


		#region Utility functions

		/// <summary>
		/// Searches the Dictionary for an existing PathfinderData for the given node,
		/// and creates and adds a new PathfinderData if one is not found.
		/// </summary>
		/// <param name="node">The node whose data to search for</param>
		/// <param name="pathfinderData">A Dictionary storing PatherinderData per PathfinderNode</param>
		/// <returns></returns>
		private static PathfinderData DataForNode(T node, Dictionary<T, PathfinderData> pathfinderData)
		{
			if (pathfinderData.TryGetValue(node, out PathfinderData data))
			{
				return data;
			}
			else
			{
				data = new PathfinderData();
				data.pathfinderNode = node;
				pathfinderData.Add(node, data);

				return data;
			}
		}

		/// <summary>
		/// Uses PathfinderData.parent to retrace path from target node to search's starting node.
		/// </summary>
		/// <param name="target">The node to retrace from</param>
		/// <param name="pathfinderData">The node the search started from</param>
		/// <returns></returns>
		private static List<T> RetracePath(T target, Dictionary<T, PathfinderData> pathfinderData)
		{
			List<T> path = new List<T>() { target };
			PathfinderData data = DataForNode(target, pathfinderData);

			while (data.parent != null)
			{
				T parent = data.parent;
				path.Add(parent);
				data = DataForNode(parent, pathfinderData);
			}

			path.Reverse();

			return path;
		}

		#endregion
	}
}
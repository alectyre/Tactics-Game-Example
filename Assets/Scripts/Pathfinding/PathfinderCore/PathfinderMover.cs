namespace PathfindingCore
{
	/// <summary>
	/// Mover class for use with PathFinder and PathFinderNode classes. Subclass to define a mover appropriate to your map type.
	/// </summary>
	public abstract class PathfinderMover<T> where T : PathfinderNode
	{
		/// <summary>
		/// Override to return the estimated value of moving from node to target.
		/// </summary>
		/// <param name="node">The node being moved from</param>
		/// <param name="target">The node being moved into</param>
		/// <param name="start">The starting location of the move</param>
		/// <returns>An estimate of the cost of moving from node to target</returns>
		public abstract float Heuristic(T node, T target, T start);

		/// <summary>
		/// Override to return the cost for moving between node and adjacent.
		/// </summary>
		/// <param name="node">The node being moved from</param>
		/// <param name="adjacent">The node being moved into</param>
		/// <returns>The cost of moving from node to adjacent</returns>
		public abstract float CostForMove(T node, T adjacent);

		/// <summary>
		/// Override to return if it is possible for the mover to move between node and adjacent.
		/// </summary>
		/// /// <param name="node">The node being moved from</param>
		/// <param name="adjacent">The node being moved into</param>
		/// <returns>If it is possible to move from node to adjacent</returns>
		public abstract bool Passable(T node, T adjacent);
	}
}
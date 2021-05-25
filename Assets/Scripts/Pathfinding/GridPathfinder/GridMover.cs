using UnityEngine;
using PathfindingCore;

namespace GridPathfinding
{
	public class GridMover : PathfinderMover<GridNode>
	{
		private const float OrthognalMovementCost = 1.0f;
		private const float DiagonalMovementCost = 1.4f;

		public override float Heuristic(GridNode node, GridNode target, GridNode start)
		{
			//Straight line distance
			float dx = Mathf.Abs(node.x - target.x);
			float dy = Mathf.Abs(node.y - target.y);
			float H = OrthognalMovementCost * (dx + dy) + (DiagonalMovementCost - 2 * OrthognalMovementCost) * Mathf.Min(dx, dy);

			//Tie breaking with preference for straight lines toward target
			//Cross product between node-target vector and start-target vector scales with difference in vector direction
			float dx1 = node.x - target.x;
			float dy1 = node.y - target.y;
			float dx2 = start.x - target.x;
			float dy2 = start.y - target.y;
			float cross = Mathf.Abs(dx1 * dy2 - dx2 * dy1);
			H += cross * 0.001f;

			//Note that because the Pathfinder checks adjacent nodes sequentially, the order of the adjacent List will
			//determine how ties are broken

			return H;
		}

		public override float CostForMove(GridNode node, GridNode adjacent)
		{
			//Closed to Open
			if (node.type == GridNode.NodeType.Closed && adjacent.type == GridNode.NodeType.Open)
				return node.x == adjacent.x || node.y == adjacent.y ? OrthognalMovementCost : DiagonalMovementCost;

			//Open to Open
			if (node.type == GridNode.NodeType.Open && adjacent.type == GridNode.NodeType.Open)
				return node.x == adjacent.x || node.y == adjacent.y ? OrthognalMovementCost : DiagonalMovementCost;

			//Default. Probably don't want to reach this.
			Debug.LogError("Unhandled NodeType.");
			return 0;
		}

		public override bool Passable(GridNode node, GridNode adjacent)
		{
			switch (adjacent.type)
			{
				case GridNode.NodeType.Closed:
					return false;

				case GridNode.NodeType.Open:
					return !IsDiagonalMovement(node, adjacent);

				default:
					return true;
			}
		}

		private bool IsDiagonalMovement(GridNode node, GridNode adjacent)
        {
			return node.x != adjacent.x && node.y != adjacent.y;
		}
	}
}
using System.Collections.Generic;
using UnityEngine;
using GridPathfinding;

namespace Gameplay
{
    /// <summary>
    /// Responsible for building map data and providing Gameplay level pathfinding functions.
    /// </summary>
    public class Map : MonoBehaviour
    {
        [SerializeField] private Tile[] tiles;

        private GridMover mover;
        private Dictionary<Tile, GridNode> gridNodesByTiles;
        private Dictionary<GridNode, Tile> tilesByGridNode;

        private const float AdjacentTileSearchDist = 1.1f;

        public Tile[] Tiles => tiles;



        private void Awake()
        {
            mover = new GridMover();

            RebuildGridNodes();
            RecalculateTileAdjacencies();
        }

        /// <summary>
        /// Finds the closest Tile to a point in world space.
        /// </summary>
        /// <param name="point">A point in world space.</param>
        /// <returns>The tile closest to the point.</returns>
        public Tile ClosestTileToPoint(Vector3 point)
        {
            Tile closestTile = null;
            float sqrDist = float.MaxValue;

            foreach(Tile tile in tiles)
            {
                if((tile.transform.position - point).sqrMagnitude < sqrDist)
                {
                    closestTile = tile;
                    sqrDist = (tile.transform.position - point).sqrMagnitude;
                }
            }

            return closestTile;
        }

        public Tile[] FindShortestPath(Tile start, Tile end)
        {
            if (start == null || end == null)
            {
                Debug.LogError("Null start passed to Map.FindReachable.");
                return new Tile[0];
            }

            if (gridNodesByTiles.TryGetValue(start, out GridNode startGridNode) &&
                gridNodesByTiles.TryGetValue(end, out GridNode endGridNode))
            {
                List<GridNode> reachableGridNodes = GridPathfinder.FindPath(mover, startGridNode, endGridNode);

                if (reachableGridNodes != null)
                {
                    List<Tile> reachableTiles = new List<Tile>();

                    foreach (GridNode gridNode in reachableGridNodes)
                    {
                        reachableTiles.Add(tilesByGridNode[gridNode]);
                    }

                    return reachableTiles.ToArray();
                }
            }

            return new Tile[0];
        }

        /// <summary>
        /// Finds all reachable tiles with maxCost of the start tile.
        /// </summary>
        /// <param name="start">The tile to start the search from.</param>
        /// <param name="maxCost">The maximum cost to reach a reachable tile.</param>
        /// <returns>A list of all reachable tiles.</returns>
        public Tile[] FindReachable(Tile start, int maxCost)
        {
            if (start == null)
            {
                Debug.LogError("Null start passed to Map.FindReachable.");
                return new Tile[0];
            }

            if (gridNodesByTiles.TryGetValue(start, out GridNode startGridNode))
            {
                GridNode[] reachableGridNodes = GridPathfinder.FindAllReachable(mover, startGridNode, maxCost).ToArray();
                
                List<Tile> reachableTiles = new List<Tile>();

                foreach (GridNode gridNode in reachableGridNodes)
                {
                    reachableTiles.Add(tilesByGridNode[gridNode]);
                }

                return reachableTiles.ToArray();
            }

            return new Tile[0];
        }

        public int CostOfPath(Tile[] path)
        {
            if(path == null || path.Length < 2)
            {
                Debug.LogError("CostOfPath Error - path null or too short.");
                return 0;
            }

            float cost = 0;
            gridNodesByTiles.TryGetValue(path[0], out GridNode currentNode);

            for (int i = 0; i < path.Length - 1; i++) 
            {
                gridNodesByTiles.TryGetValue(path[0], out GridNode nextNode);
                cost += mover.CostForMove(currentNode, nextNode);
                currentNode = nextNode;
            }

            return (int)cost;
        }

        /// <summary>
        /// Creates a new GridNode for each tile and generates 'bi-direction dictionary' associating Tiles and GridNodes
        /// </summary>
        private void RebuildGridNodes()
        {
            gridNodesByTiles= new Dictionary<Tile, GridNode>();
            tilesByGridNode = new Dictionary<GridNode, Tile>();

            for (int i = 0; i < tiles.Length; i++)
            {
                Tile tile = tiles[i];

                if(tile == null)
                {
                    Debug.LogError("Null tile found during Map.RebuildGridNodes");
                    continue;
                }

                tile.OnOccupancyChanged += HandleTileOccupancyChanged;

                GridNode gridNode = new GridNode();
                gridNode.x = RoundTileIndex(tile.transform.position.x);
                gridNode.y = RoundTileIndex(tile.transform.position.z);

                gridNodesByTiles.Add(tile, gridNode);
                tilesByGridNode.Add(gridNode, tile);

                HandleTileOccupancyChanged(tile);
            }
        }

        private void HandleTileOccupancyChanged(Tile tile)
        {
            if(gridNodesByTiles.TryGetValue(tile, out GridNode gridNode))
                gridNode.type = tile.Occupied ? GridNode.NodeType.Closed : GridNode.NodeType.Open;
        }

        /// <summary>
        /// Distance check to find adjacent GridNodes.
        /// </summary>
        private void RecalculateTileAdjacencies()
        {
            const float AdjacentTileSearchDistSqrd = AdjacentTileSearchDist * AdjacentTileSearchDist;

            for (int i = 0; i < tiles.Length; i++)
            {
                Tile tile = tiles[i];

                if (tile == null)
                {
                    Debug.LogError("Null tile found during Map.RecalculateTileAdjacencies");
                    continue;
                }

                GridNode gridNode = gridNodesByTiles[tile];

                for (int u = i + 1; u < tiles.Length; u++)
                {
                    Tile otherTile = tiles[u];

                    if ((tile.transform.position - otherTile.transform.position).sqrMagnitude < AdjacentTileSearchDistSqrd)
                    {
                        GridNode otherGridNode = gridNodesByTiles[otherTile];
                        gridNode.adjacent.Add(otherGridNode);
                        otherGridNode.adjacent.Add(gridNode);
                    }
                }
            }
        }

        private int RoundTileIndex(float value)
        {
            if (value >= 0)
                return Mathf.FloorToInt(value) + ((value % 1) < 0.5f ? 0 : 1);
            else
                return Mathf.FloorToInt(value) + ((value % 1) < -0.5f ? 0 : 1);
        }
    }
}
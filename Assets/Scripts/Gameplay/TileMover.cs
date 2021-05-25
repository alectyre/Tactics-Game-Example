using System;
using System.Collections;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Responsible for Gameplay movement of this GameObject on Map Tiles
    /// </summary>
    public class TileMover : MonoBehaviour
    {
        public delegate void TileMoverEvent(TileMover tileMover);


        [SerializeField] private Map map;
        
        [Header("Movement Characteristics")]
        public float speed = 4f;
        public float minDist = 0.05f;
        public float slowDist = 0.5f;


        public event TileMoverEvent OnStoppedOnTile;
        public event TileMoverEvent OnMoved;


        private Tile currentTile;
        private Coroutine traversePathCoroutine;


        public Tile CurrentTile => currentTile;
        public Vector3 Velocity { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool IsMoving { get { return CurrentSpeed != 0; } }


        /// <summary>
        /// Moves immediately to the given tile.
        /// </summary>
        /// <param name="tile">The tile to move to</param>
        public void MoveToTileImmediate(Tile tile)
        {
            if (tile == null)
            {
                Debug.LogError("Null tile passed to DemoPlayer.OccupyTile.");
                return;
            }

            currentTile.Occupied = false; //Leave old tile

            transform.position = tile.transform.position;

            currentTile = tile;
            currentTile.Occupied = true;

            OnStoppedOnTile?.Invoke(this);
        }

        /// <summary>
        /// Immediately moves to the tile closest to current position.
        /// </summary>
        public void MoveToClosestTileImmediate()
        {
            MoveToClosestTileImmediate(transform.position);
        }

        /// <summary>
        /// Immediately moves to the tile closest to the given point.
        /// </summary>
        /// <param name="point">The point to find the tile closest to</param>
        public void MoveToClosestTileImmediate(Vector3 point)
        {
            currentTile = map.ClosestTileToPoint(point);

            if(currentTile != null)
                MoveToTileImmediate(currentTile);
        }

        /// <summary>
        /// Moves to tile at speed, if path is available.
        /// </summary>
        /// <param name="tile">Tile to move to</param>
        /// <returns>The path taken to destination, null if no path available</returns>
        public Tile[] MoveToTile(Tile tile)
        {
            if (tile == null)
            {
                Debug.LogError("Null tile passed to DemoPlayer.MoveDemoPlayerToTile.");
                return null;
            }

            CancelMove();

            Tile[] pathTiles = map.FindShortestPath(currentTile, tile);

            if (pathTiles != null && pathTiles.Length > 0)
            {
                traversePathCoroutine = StartCoroutine(TraversePathCoroutine(pathTiles, () =>
                {
                    MoveToTileImmediate(pathTiles[pathTiles.Length - 1]);
                }));

                return pathTiles;
            }

            return null;
        }

        /// <summary>
        /// Cancels any movement in progress.
        /// </summary>
        public void CancelMove()
        {
            if (traversePathCoroutine != null)
            {
                StopCoroutine(traversePathCoroutine);
                traversePathCoroutine = null;

                MoveToClosestTileImmediate();
            }
        }

        /// <summary>
        /// Moves this GameObject along the provided path over time. 
        /// </summary>
        /// <param name="path">The path to traverse</param>
        /// <param name="onTraverseComplete">Called when the traversal completes</param>
        /// <returns></returns>
        private IEnumerator TraversePathCoroutine(Tile[] path, Action onTraverseComplete)
        {
            if (path == null || path.Length == 0)
            {
                Debug.LogError("Null or zero length path in DemoPlayer.TraversePathCoroutine");
                if (onTraverseComplete != null)
                    onTraverseComplete.Invoke();
                yield break;
            }

            float distRemaining = Vector3.Magnitude(path[0].transform.position - transform.position);
            for (int i = 0; i < path.Length - 1; i++)
                distRemaining += Vector3.Magnitude(path[i + 1].transform.position - path[i].transform.position);

            int pathIndex = 0;

            while (distRemaining > minDist)
            {
                Vector3 nextPos = path[pathIndex].transform.position;
                
                float distToNextPos = Vector3.Magnitude(nextPos - transform.position);
                float slowingFactor = distRemaining < slowDist ? distRemaining / slowDist : 1;
                float deltaMove = speed * Time.deltaTime * slowingFactor; //calculate the distance to move this frame

                //Move across points as needed per frame
                while (pathIndex != path.Length - 1 &&
                        deltaMove > distToNextPos) //crossed over to a new pos
                {
                    deltaMove -= distToNextPos; //subtract distToNextPos from deltaMove
                    distRemaining -= distToNextPos; //subtract distToNextPos from pathDistRemaining

                    pathIndex++; //increment to the next pos and retest
                    nextPos = path[pathIndex].transform.position;
                    distToNextPos = Vector3.Magnitude(nextPos - transform.position);
                }

                Vector3 moveDir = (nextPos - transform.position).normalized;
                Vector3 deltaPosition = moveDir * deltaMove;

                transform.position = transform.position + deltaPosition;
                distRemaining -= deltaMove;

                Velocity = deltaPosition;
                CurrentSpeed = Velocity.magnitude / Time.deltaTime;
                
                OnMoved?.Invoke(this);

                yield return null;
            }

            transform.position = path[path.Length - 1].transform.position; //move to exactly final position

            Velocity = Vector3.zero;
            CurrentSpeed = 0;

            traversePathCoroutine = null;

            onTraverseComplete?.Invoke();
        }
    }
}
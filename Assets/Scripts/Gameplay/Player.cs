using System;
using UnityEngine;
using Gameplay.UI;

namespace Gameplay
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Map map;
        [SerializeField] private TileMover tileMover;
        [SerializeField] private TileDrawer tileDrawer;
        [SerializeField] private PlayerStatBar movementPointsBar;
        [Header("Player Stats")]
        [SerializeField] private PlayerStat movementPoints;

        private Tile[] reachableTiles;


        public PlayerStat MovementPoints { get { return movementPoints; } }


        public void RefreshMovementPoints()
        {
            movementPoints.Value = movementPoints.MaxValue;
        }


        #region Unity lifecycle functions

        private void OnEnable()
        {
            tileMover.OnStoppedOnTile += HandleMovedToNewTile;

            foreach (Tile tile in map.Tiles)
            {
                tile.OnTileClicked += HandleTileClicked;
            }
        }

        private void OnDisable()
        {
            tileMover.OnStoppedOnTile -= HandleMovedToNewTile;

            foreach (Tile tile in map.Tiles)
            {
                tile.OnTileClicked -= HandleTileClicked;
            }
        }

        private void Start()
        {
            movementPointsBar.PlayerStat = movementPoints;

            tileMover.MoveToClosestTileImmediate();

            RefreshMovementPoints();
        }

        #endregion


        #region Movement event handlers

        private void HandleMovedToNewTile(TileMover tileMover)
        {
            if (movementPoints <= 0)
                RefreshMovementPoints();

            reachableTiles = map.FindReachable(tileMover.CurrentTile, (int)movementPoints);
            DrawReachable();
        }

        private void HandleTileClicked(Tile tile)
        {
            if (tileMover.IsMoving || tile == tileMover.CurrentTile || !CanReachTile(tile))
                return;

            Tile[] pathTiles = tileMover.MoveToTile(tile);

            if (pathTiles != null)
            {
                movementPoints.Value = Mathf.Max(movementPoints - map.CostOfPath(pathTiles), 0);
                DrawPath(pathTiles);
            }
        }

        #endregion


        #region Utility functions

        private void DrawReachable()
        {
            tileDrawer.Clear();
            tileDrawer.DrawTiles(reachableTiles);
        }

        private void DrawPath(Tile[] path)
        {
            tileDrawer.Clear();
            tileDrawer.DrawTiles(path);
        }

        private bool CanReachTile(Tile tile)
        {
            return reachableTiles != null && Array.IndexOf(reachableTiles, tile) != -1;
        }

        #endregion
    }
}
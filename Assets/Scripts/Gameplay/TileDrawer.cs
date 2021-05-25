using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Visualizes tiles for Gameplay purposes.
    /// </summary>
    public class TileDrawer : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        private List<GameObject> prefabInstances = new List<GameObject>();

        /// <summary>
        /// Draws a visualization of the given tiles.
        /// </summary>
        /// <param name="tiles">The tiles to draw</param>
        public void DrawTiles(Tile[] tiles)
        {
            if(tiles == null)
            {
                Debug.LogError("Null argument passed to TileDrawer.DrawTiles");
                return;
            }

            foreach(Tile tile in tiles)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.transform.position = tile.transform.position;
                prefabInstances.Add(instance);
            }
        }

        /// <summary>
        /// Clears all tile visualizations.
        /// </summary>
        public void Clear()
        {
            foreach (GameObject prefabInstance in prefabInstances)
                Destroy(prefabInstance);

            prefabInstances.Clear();
        }
    }
}
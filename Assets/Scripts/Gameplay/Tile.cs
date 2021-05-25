using UnityEngine;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] bool occupied;

        public delegate void TileEvent(Tile tile);
        public event TileEvent OnTileClicked;
        public event TileEvent OnOccupancyChanged;


        public bool Occupied
        {
            get { return occupied; }
            set
            {
                bool isDifferent = occupied == value;
                occupied = value;
                if (isDifferent)
                    OnOccupancyChanged?.Invoke(this);
            }
        }

        private void OnMouseUpAsButton()
        {
            OnTileClicked?.Invoke(this);
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                OnOccupancyChanged?.Invoke(this);
        }
    }
}
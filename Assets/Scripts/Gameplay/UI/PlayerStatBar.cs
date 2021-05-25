using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class PlayerStatBar : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private bool clampToMin;

        private PlayerStat playerStat;


        public PlayerStat PlayerStat
        {
            get { return playerStat; }
            set
            {
                if(gameObject.activeSelf && playerStat != null)
                    playerStat.OnStatChanged -= HandlePlayerStatChanged;

                playerStat = value;

                if (gameObject.activeSelf && playerStat != null)
                {
                    playerStat.OnStatChanged += HandlePlayerStatChanged;
                    HandlePlayerStatChanged(playerStat);
                }
            }
        }


        private void OnEnable()
        {
            if (playerStat != null)
            {
                playerStat.OnStatChanged += HandlePlayerStatChanged;

                HandlePlayerStatChanged(playerStat);
            }
        }

        private void OnDisable()
        {
            if (playerStat != null)
                playerStat.OnStatChanged -= HandlePlayerStatChanged;
        }

        private void HandlePlayerStatChanged(PlayerStat playerStat)
        {
            if (clampToMin)
            {
                //Technically remapping not clamping, but I think the variable name makes more sense
                fill.fillAmount = (playerStat.Value - playerStat.MinValue) / (playerStat.MaxValue -playerStat.MinValue);
            }
            else
            {
                fill.fillAmount = playerStat.Value / playerStat.MaxValue;
            }
        }
    }
}

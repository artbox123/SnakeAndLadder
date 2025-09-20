using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class PlayerSelection : GameManager
    {
        [SerializeField] private Image selectedPlayer;

        // Start is called before the first frame update
        void Start()
        {
            SetPlayerImage(PlayerInfo.Instance.playerIcon);
        }

        public void SetPlayerImage(int Index)
        {
            PlayerInfo.Instance.playerIcon = Index;
            selectedPlayer.sprite = GetPlayerImage(Index);
        }
    }
}
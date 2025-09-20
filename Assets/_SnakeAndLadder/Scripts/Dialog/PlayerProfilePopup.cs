using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArtboxGames
{
    public class PlayerProfilePopup : MonoBehaviour
    {
        [SerializeField] private Image playerImage;
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TMP_InputField inputPlayerName;
        [SerializeField] private TextMeshProUGUI playerCoins;

        [SerializeField] private TextMeshProUGUI totalPlayed;
        [SerializeField] private TextMeshProUGUI totalLose;
        [SerializeField] private TextMeshProUGUI totalWin;

        private void OnEnable()
        {
            playerName.text = HomeScreen.Instance.playerName.text;
            inputPlayerName.text = HomeScreen.Instance.playerName.text;
            playerImage.sprite = HomeScreen.Instance.playerImage.sprite;
            playerCoins.text = PlayerInfo.Instance.coins.ToString();
        }

        // Start is called before the first frame update
        void Start()
        {
            SetPlayerStatistics();
        }

        private void SetPlayerStatistics()
        {
            totalPlayed.text = PlayerInfo.Instance.totalPlayed.ToString("00");
            totalLose.text = (PlayerInfo.Instance.totalPlayed - PlayerInfo.Instance.totalWin).ToString("00");
            totalWin.text = PlayerInfo.Instance.totalWin.ToString("00");
        }

        public void Edit_Yes()
        {
            HomeScreen.Instance.playerName.text = inputPlayerName.text;
            playerName.text = inputPlayerName.text;
            PlayerInfo.Instance.userName = inputPlayerName.text.Trim();
        }
    }
}
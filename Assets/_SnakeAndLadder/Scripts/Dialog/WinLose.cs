using UnityEngine;
using TMPro;

namespace ArtboxGames
{
    public class WinLose : GameManager
    {
        [SerializeField] private TextMeshProUGUI winCoins;

        // Start is called before the first frame update
        void Start()
        {
            if (name == "GameWin")
            {
                PlaySound(5);
                PlayerInfo.Instance.totalWin++;
                PlayerInfo.Instance.UpdateCoins(CoinAction.Add, PlayerInfo.Instance.bootAmount * 2);
                winCoins.text = (PlayerInfo.Instance.bootAmount * 2).ToString();
            }
        }
    }
}
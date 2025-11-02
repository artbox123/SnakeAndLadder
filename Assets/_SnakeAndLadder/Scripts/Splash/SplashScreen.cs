using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArtboxGames
{
    public class SplashScreen : GameManager
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI fillPercent;

        // Start is called before the first frame update
        void Start()
        {
            PlayerInfo.Instance.login = 1;
            if (string.IsNullOrEmpty(PlayerInfo.Instance.userName))
            {
                PlayerInfo.Instance.userID = SystemInfo.deviceUniqueIdentifier;
                PlayerInfo.Instance.userName = "Guest_" + Random.Range(100, 1000);
                PlayerInfo.Instance.userImage = "0";
            }
            LoadScene("Home", null, 1f);
        }
    }
}
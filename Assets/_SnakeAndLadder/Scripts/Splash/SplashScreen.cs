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
            if (PlayerInfo.Instance.login == 0)
            {
                LoadScene("Login", null, 1, fillImage, fillPercent);
            }
            else if (PlayerInfo.Instance.login >= 1)
            {
                LoadScene("Home", null, 1, fillImage, fillPercent);
            }
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class SocialLogin : GameManager
    {
        public static SocialLogin Instance;

        [Header("Message Box")]
        [SerializeField] private GameObject m_loadingPanel;
        [SerializeField] private GameObject m_messageBox;
        [SerializeField] private TextMeshProUGUI m_message;

        void Awake()
        {
            InitializeMessageBox(m_loadingPanel, m_messageBox, m_message);
        }

        private void Start()
        {
            SetScreenRes();
            LoginAsGuest();
            AdsManager.Instance.ShowBanner();
        }

        public void LoginAsGuest()
        {
            PlayerInfo.Instance.login = 1;
            if (string.IsNullOrEmpty(PlayerInfo.Instance.userName))
            {
                PlayerInfo.Instance.userID = SystemInfo.deviceUniqueIdentifier;
                PlayerInfo.Instance.userName = "Guest_" + Random.Range(100, 1000);
                PlayerInfo.Instance.userImage = "0";
            }
            LoadScene("Home", m_loadingPanel, 1f);
        }

        private void SetScreenRes()
        {
            float ratio = (float)Screen.height / (float)Screen.width;
            //Debug.Log("=== ratio : " + ratio);
            if (ratio >= 2f)
            {
                GetComponent<CanvasScaler>().matchWidthOrHeight = 0f;
            }
            else if (ratio <= 1.35f)
            {
                GetComponent<CanvasScaler>().matchWidthOrHeight = 0.77f;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArtboxGames
{
    public class HomeScreen : GameManager
    {
        public static HomeScreen Instance;

        public TextMeshProUGUI playerName;
        public Image playerImage;
        public TextMeshProUGUI playerCoins;
        [SerializeField] private GameObject loader;

        [Header("Message Box")]
        [SerializeField] private GameObject m_loadingPanel;
        [SerializeField] private GameObject m_messageBox;
        [SerializeField] private TextMeshProUGUI m_message;

        [Header("Popups")]
        public GameObject playerWaiting;
        public GameObject createOrJoin;
        [SerializeField] private GameObject playConfirmation;
        [SerializeField] private GameObject settings;
        [SerializeField] private GameObject themeSelection;
        [SerializeField] private GameObject playerSelection;
        [SerializeField] private GameObject dailySpin;
        [SerializeField] private GameObject howToPlay;
        public GameObject WatchVideo;
        [SerializeField] private GameObject rateUs;
        [SerializeField] private GameObject quitGame;

        private void Awake()
        {
            Instance = this;
            InitializeMessageBox(m_loadingPanel, m_messageBox, m_message);
        }

        // Start is called before the first frame update
        void Start()
        {
            SetScreen();
            SetPlayerProfile();
            Invoke(nameof(HideBanner), 1f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (playerWaiting.activeSelf)
                {

                }
                else if (createOrJoin.activeSelf)
                {
                    createOrJoin.SetActive(false);
                }
                else if (playConfirmation.activeSelf)
                {
                    playConfirmation.SetActive(false);
                }
                else if (settings.activeSelf)
                {
                    settings.SetActive(false);
                }
                else if (themeSelection.activeSelf)
                {
                    themeSelection.SetActive(false);
                }
                else if (playerSelection.activeSelf)
                {
                    playerSelection.SetActive(false);
                }
                else if (dailySpin.activeSelf)
                {
                    dailySpin.SetActive(false);
                }
                else if (howToPlay.activeSelf)
                {
                    howToPlay.SetActive(false);
                }
                else if (WatchVideo.activeSelf)
                {
                    WatchVideo.SetActive(false);
                }
                else if (rateUs.activeSelf)
                {
                    rateUs.SetActive(false);
                }
                else if (quitGame.activeSelf)
                {
                    quitGame.SetActive(false);
                }
                else
                {
                    quitGame.SetActive(true);
                }
            }
        }

        private void HideBanner()
        {
            AdsManager.Instance.HideBannerView();
        }   

        private void SetPlayerProfile()
        {
            playerName.text = PlayerInfo.Instance.userName;
            playerCoins.text = PlayerInfo.Instance.coins.ToString();
            StartCoroutine(LoadImageFromPath(PlayerInfo.Instance.userImage, playerImage));
        }

        public void Play(int playType)
        {
            if (playType == 0)
            {
                if (!CheckBalance(1000))
                    return;
                PlayerInfo.Instance.playType = PlayType.Play;
                PlayerInfo.Instance.bootAmount = 1000;
                PlayerData mainPlayer = new PlayerData();
                mainPlayer.playerID = PlayerInfo.Instance.userID;
                mainPlayer.playerName = PlayerInfo.Instance.userName;
                mainPlayer.playerIcon = PlayerInfo.Instance.playerIcon;
                ServerCode.Instance.joinedPlayer.Add(mainPlayer);

                PlayerData computer = new PlayerData();
                computer.playerID = "Computer";
                computer.playerName = "Computer";
                computer.playerIcon = 4;
                ServerCode.Instance.joinedPlayer.Add(computer);

                LoadScene("Game", m_loadingPanel, 1);
            }
            else if (playType == 1)
            {
                if (!CheckBalance(2000))
                    return;
                PlayerInfo.Instance.playType = PlayType.PlayOnline;
                PlayerInfo.Instance.bootAmount = 2000;
                m_loadingPanel.SetActive(true);
                ServerCode.Instance.CreateRoom(false);
            }
        }

        public void MoreGame()
        {
            Application.OpenURL("https://play.google.com/store/apps/developer?id=Artbox+Infotech");
        }

        public void ShowAds()
        {
            if (AdsManager.Instance != null)
            {
                AdsManager.Instance.ShowInterstitialAd();
            }
        }
    }
}
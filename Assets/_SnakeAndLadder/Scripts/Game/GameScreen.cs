using TMPro;
using UnityEngine;

namespace ArtboxGames
{
    public class GameScreen : GameManager
    {
        public static GameScreen Instance;

        [Header("Message Box")]
        [SerializeField] private GameObject m_loadingPanel;
        [SerializeField] private GameObject m_messageBox;
        [SerializeField] private TextMeshProUGUI m_message;

        public GameObject gameWin;
        public GameObject gameLose;
        [SerializeField] private GameObject gamePause;

        private void Awake()
        {
            Instance = this;
            InitializeMessageBox(m_loadingPanel, m_messageBox, m_message);
        }

        // Start is called before the first frame update
        void Start()
        {
            SetScreen();

            if (PlayerInfo.Instance.playType != PlayType.Play && !ServerCode.Instance.botActivated)
                InvokeRepeating("CheckNetwork", 2f, 6f);

            AdsManager.Instance.ShowBannerView();

            PlayerInfo.Instance.UpdateCoins(CoinAction.Minuse, PlayerInfo.Instance.bootAmount);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (gameWin.activeSelf || gameLose.activeSelf)
                {
                    LoadScene("Home", m_loadingPanel);
                    DisconnectPlayer();
                }
                else if (gamePause.activeSelf)
                {
                    gamePause.SetActive(false);
                }
                else
                {
                    gamePause.SetActive(true);
                }
            }
        }

        private void CheckNetwork()
        {
            StartCoroutine(CheckInternet((isConnected) =>
            {
                if (!isConnected)
                {
                    CancelInvoke("CheckNetwork");
                    ShowMessageBox("You lost internet connection!, please try again");
                }
            }));
        }
    }
}
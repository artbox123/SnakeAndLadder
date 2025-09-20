using Google;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class SocialLogin : GameManager
    {
        public static SocialLogin Instance;

        private string webClientId = "595749289064-g061417vji6bl38464h54us5gkimripk.apps.googleusercontent.com";

        private GoogleSignInConfiguration configuration;

        private loginCallBack l_callback;
        private string playerName;
        private string playerImage;
        private int login = 1;

        [Header("Message Box")]
        [SerializeField] private GameObject m_loadingPanel;
        [SerializeField] private GameObject m_messageBox;
        [SerializeField] private TextMeshProUGUI m_message;

        void Awake()
        {
            InitializeMessageBox(m_loadingPanel, m_messageBox, m_message);
            l_callback = loginCallBack.None;

            configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestEmail = true,
                RequestIdToken = true
            };
        }

        private void Start()
        {
            SetScreenRes();

            PlayerInfo.Instance.login = 0;
            if (PlayerInfo.Instance.login == 2)
                OnSignOut();

            AdsManager.Instance.ShowBannerView();
        }

        public void LoginAsGuest()
        {
            PlayerInfo.Instance.login = 1;
            login = 1;
            if (string.IsNullOrEmpty(PlayerInfo.Instance.userName))
            {
                PlayerInfo.Instance.userID = SystemInfo.deviceUniqueIdentifier;
                PlayerInfo.Instance.userName = "Guest_" + Random.Range(100, 1000);
                PlayerInfo.Instance.userImage = "0";
            }
            LoadScene("Home", m_loadingPanel, 1f);
        }   

        public void OnSignIn()
        {
            m_loadingPanel.SetActive(true);
            StartCoroutine(CheckInternet((isConnected) =>
            {
                if (isConnected)
                {
                    GoogleSignIn.Configuration = configuration;
                    GoogleSignIn.Configuration.UseGameSignIn = false;
                    GoogleSignIn.Configuration.RequestIdToken = true;
                    AddStatusText("Calling SignIn");

                    GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
                      OnAuthenticationFinished);
                }
                else
                {
                    ShowMessageBox("No internet connection!");
                }
            }));
        }

        public void OnSignOut()
        {
            AddStatusText("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
            SceneManager.LoadScene("Login");
        }

        public void OnDisconnect()
        {
            AddStatusText("Calling Disconnect");
            GoogleSignIn.DefaultInstance.Disconnect();
        }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted)
            {
                using (IEnumerator<System.Exception> enumerator =
                        task.Exception.InnerExceptions.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                        AddStatusText("Got Error: " + error.Status + " " + error.Message);
                        l_callback = loginCallBack.Cancel;
                    }
                    else
                    {
                        AddStatusText("Got Unexpected Exception?!?" + task.Exception);
                        l_callback = loginCallBack.Failed;
                    }
                }
            }
            else if (task.IsCanceled)
            {
                AddStatusText("Canceled");
                l_callback = loginCallBack.Cancel;
            }
            else
            {
                AddStatusText("Welcome: " + task.Result.DisplayName + "!");
                playerName = task.Result.DisplayName;
                playerImage = task.Result.ImageUrl.ToString();
                login = 2;
                l_callback = loginCallBack.Success;
            }
        }

        public void OnSignInSilently()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddStatusText("Calling SignIn Silently");

            GoogleSignIn.DefaultInstance.SignInSilently()
                  .ContinueWith(OnAuthenticationFinished);
        }

        public void OnGamesSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;

            AddStatusText("Calling Games SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }

        private List<string> messages = new List<string>();
        void AddStatusText(string text)
        {
            if (messages.Count == 5)
            {
                messages.RemoveAt(0);
            }
            messages.Add(text);
            string txt = "";
            foreach (string s in messages)
            {
                txt += "\n" + s;
            }
            Debug.Log("=== Message : " + txt);
        }

        private enum loginCallBack
        {
            None,
            Cancel,
            Success,
            Failed
        }

        private void Update()
        {
            if (l_callback == loginCallBack.Cancel)
            {
                ShowMessageBox("User cancelled login");
            }
            else if (l_callback == loginCallBack.Success)
            {
                PlayerInfo.Instance.userName = playerName;
                PlayerInfo.Instance.userImage = playerImage;
                PlayerInfo.Instance.login = login;
                LoadScene("Home", DoNotDestroy.Instance.loadingPanel);
            }
            else if (l_callback == loginCallBack.Failed)
            {
                ShowMessageBox("Login failed!");
            }
            l_callback = loginCallBack.None;
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
using System.Collections.Generic;
using UnityEngine;

namespace ArtboxGames
{
    public class PlayerInfo : MonoBehaviour
    {
        public static PlayerInfo Instance;
        public List<string> playerName;
        public PlayType playType;

        public int theme = 0;
        public int playerIcon = 0;
        public int bootAmount = 1000;

        [SerializeField] private int defaultCoins;

        private int _login;
        private string _userID;
        private string _userName;
        private int _coins;
        private string _userImage;
        private int _sound;
        private int _vibration;
        private int _totalPlayed;
        private int _totalWin;

        public int login
        {
            get
            {
                return _login;
            }
            set
            {
                _login = value;
                PlayerPrefs.SetInt("login", _login);
                PlayerPrefs.Save();
            }
        }

        public string userID
        {
            get
            {
                return _userID;
            }
            set
            {
                _userID = value;
                PlayerPrefs.SetString("userid", _userID);
                PlayerPrefs.Save();
            }
        }

        public string userName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                PlayerPrefs.SetString("username", _userName);
                PlayerPrefs.Save();
            }
        }

        public int coins
        {
            get
            {
                return _coins;
            }
            set
            {
                _coins = value;
                PlayerPrefs.SetInt("Coins", _coins);
                PlayerPrefs.Save();

                if (HomeScreen.Instance != null)
                {
                    HomeScreen.Instance.playerCoins.text = _coins.ToString();
                }
            }
        }

        public string userImage
        {
            get
            {
                return _userImage;
            }
            set
            {
                _userImage = value;
                PlayerPrefs.SetString("userimage", _userImage);
                PlayerPrefs.Save();
            }
        }

        public int sound
        {
            get
            {
                return _sound;
            }
            set
            {
                _sound = value;
                PlayerPrefs.SetInt("sound", _sound);
                PlayerPrefs.Save();
            }
        }

        public int vibration
        {
            get
            {
                return _vibration;
            }
            set
            {
                _vibration = value;
                PlayerPrefs.SetInt("vibration", _vibration);
                PlayerPrefs.Save();
            }
        }

        public int totalPlayed
        {
            get
            {
                return _totalPlayed;
            }
            set
            {
                _totalPlayed = value;
                PlayerPrefs.SetInt("totalplayed", _totalPlayed);
                PlayerPrefs.Save();
            }
        }

        public int totalWin
        {
            get
            {
                return _totalWin;
            }
            set
            {
                _totalWin = value;
                PlayerPrefs.SetInt("totalwin", _totalWin);
                PlayerPrefs.Save();
            }
        }

        void Awake()
        {
            Instance = this;

            if (!PlayerPrefs.HasKey("login"))
                login = 0;
            else
                login = PlayerPrefs.GetInt("login");

            if (!PlayerPrefs.HasKey("userid"))
                userID = SystemInfo.deviceUniqueIdentifier;
            else
                userID = PlayerPrefs.GetString("userid");

            if (!PlayerPrefs.HasKey("username"))
                userName = "Guest_" + Random.Range(100, 1000);
            else
                userName = PlayerPrefs.GetString("username");

            if (!PlayerPrefs.HasKey("Coins"))
                coins = defaultCoins;
            else
                coins = PlayerPrefs.GetInt("Coins");

            if (!PlayerPrefs.HasKey("userimage"))
                userImage = "0";
            else
                userImage = PlayerPrefs.GetString("userimage");

            if (!PlayerPrefs.HasKey("sound"))
                sound = 1;
            else
                sound = PlayerPrefs.GetInt("sound");

            if (!PlayerPrefs.HasKey("vibration"))
                vibration = 1;
            else
                vibration = PlayerPrefs.GetInt("vibration");

            if (!PlayerPrefs.HasKey("totalplayed"))
                totalPlayed = 0;
            else
                totalPlayed = PlayerPrefs.GetInt("totalplayed");

            if (!PlayerPrefs.HasKey("totalwin"))
                totalWin = 0;
            else
                totalWin = PlayerPrefs.GetInt("totalwin");
        }

        public void UpdateCoins(CoinAction action, int amount)
        {
            if (action == CoinAction.Add)
                coins += amount;
            else if (action == CoinAction.Minuse)
            {
                coins -= amount;
                if (coins <= 0)
                    coins = 0;
            }
        }
    }

    public enum CoinAction
    {
        Add,
        Minuse
    }

    public enum PlayType
    {
        Play = 0,
        PlayOnline = 1,
        PlayPrivate = 2
    }
}
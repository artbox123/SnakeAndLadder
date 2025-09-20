using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArtboxGames
{
    public class PlayerWaiting : GameManager
    {
        public static PlayerWaiting Instance;

        [SerializeField] private GameObject waitingMsg;
        [SerializeField] private GameObject waitingForStart;
        [SerializeField] private Transform parent;
        [SerializeField] private PlayerProfile playerPrefab;
        [SerializeField] private PlayerProfile mainPlayer;
        [SerializeField] private GameObject startButton;
        [SerializeField] private TextMeshProUGUI roomCode;
        [SerializeField] private GameObject share;

        private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
        private ServerCode serverCode;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            serverCode = ServerCode.Instance;

            waitingForStart.SetActive(false);
            players.Add(PlayerInfo.Instance.userID, mainPlayer.gameObject);
            if (PlayerInfo.Instance.playType == PlayType.PlayPrivate)
            {
                roomCode.text = "Room Code : " + serverCode.roomCode;
                share.SetActive(true);
            }
            else
            {
                roomCode.text = "";
                share.SetActive(false);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            SetPlayerProfile(mainPlayer, PlayerInfo.Instance.userName, PlayerInfo.Instance.userImage);
        }

        private void SetPlayerProfile(PlayerProfile player, string playerName, string playerImage)
        {
            player.playerName.text = playerName;
            StartCoroutine(LoadImageFromPath(playerImage, player.playerImage));
        }

        public void Close()
        {
            serverCode.SendDisconnect();
            serverCode.joinedPlayer.Clear();
            players.Clear();

            for (int i = 0; i < parent.childCount; i++)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        public void GeneratePlayer()
        {
            if (serverCode.joinedPlayer.Count >= 2)
            {
                waitingMsg.SetActive(false);
                if (PlayerInfo.Instance.playType == PlayType.PlayPrivate && serverCode.isAdmin)
                    startButton.SetActive(true);
                else
                    waitingForStart.SetActive(true);
            }
            foreach (PlayerData data in serverCode.joinedPlayer)
            {
                if (!players.ContainsKey(data.playerID))
                {
                    PlayerProfile player = Instantiate(playerPrefab, parent, false);
                    player.name = data.playerID;
                    if (data.playerImage.Length > 1)
                    {
                        Image playerImg = player.transform.Find("Background/ProfileImg").GetComponent<Image>();
                        StartCoroutine(LoadImageFromPath(data.playerImage, playerImg));
                    }
                    player.playerName.text = data.playerName;
                    players.Add(data.playerID, player.gameObject);
                }
            }
        }

        public void RemovePlayer(string playerId)
        {
            if (players.ContainsKey(playerId))
            {
                GameObject player;
                players.TryGetValue(playerId, out player);
                players.Remove(playerId);
                Destroy(player);

                if (serverCode.joinedPlayer.Count < 2)
                {
                    waitingMsg.SetActive(true);
                    if (PlayerInfo.Instance.playType == PlayType.PlayPrivate)
                        startButton.SetActive(false);
                }
            }
        }

        public void StartParty()
        {
            serverCode.SendStartParty(PlayerInfo.Instance.userID);
        }

        public void Share()
        {
            NativeShare share = new NativeShare();
            share.SetSubject("Share Room Code").SetTitle("Share").SetText("I want to play Snake And Ladder with you! Please install from Play Store : https://play.google.com/store/apps/details?id=" + Application.identifier + " Start game and go to Private Play and enter Room Code : " + serverCode.roomCode).Share();
        }
    }
}
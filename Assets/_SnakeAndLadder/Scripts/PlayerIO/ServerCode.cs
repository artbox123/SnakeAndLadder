using PlayerIOClient;
using System.Collections.Generic;
using UnityEngine;

namespace ArtboxGames
{
    public class ServerCode : GameManager
    {
        public static ServerCode Instance;

        public static Client player;
        public static Connection piocon;
        public List<PlayerData> joinedPlayer = new List<PlayerData>();
        public bool isAdmin = false;
        public bool botActivated = false;
        public string roomCode;

        private const string GameID = "snake-and-ladder-fvltxh8mkiohwgxrt0f7g";
        private const string password = "12345678";

        private const string roomType = "SnakeAndLadder";

        private List<Message> msgList = new List<Message>(); //  Messsage queue implementation    

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (player == null)
                Authentication();
        }

        void handlemessage(object sender, Message m)
        {
            msgList.Add(m);
        }

        void FixedUpdate()
        {
            // process message queue
            foreach (Message m in msgList)
            {
                switch (m.Type)
                {
                    case "JoinPlayer":
                        string playerId = m.GetString(0);
                        string playerName = m.GetString(1);
                        string playerImage = m.GetString(2);
                        int playerIcon = m.GetInt(3);
                        if (!joinedPlayer.Exists(x => x.playerID == playerId))
                        {
                            PlayerData jPlayer = new PlayerData();
                            jPlayer.playerID = playerId;
                            jPlayer.playerName = playerName;
                            jPlayer.playerImage = playerImage;
                            jPlayer.playerIcon = playerIcon;
                            joinedPlayer.Add(jPlayer);

                            if (PlayerWaiting.Instance != null)
                            {
                                PlayerWaiting.Instance.GeneratePlayer();
                            }
                            if (playerId.StartsWith("bot_"))
                            {
                                botActivated = true;
                            }
                        }
                        break;

                    case "PlayerTurn":
                        string playerId5 = m.GetString(0);
                        int playerValue = m.GetInt(1);
                        int diceValue = m.GetInt(2);
                        if (PlayerInfo.Instance.userID != playerId5)
                            StartCoroutine(GameController.Instance.RollDiceAnim(playerId5, playerValue, diceValue));
                        break;

                    case "PlayerLeft":
                        // remove characters from the scene when they leave                    
                        string playerId1 = m.GetString(0).Remove(0, 6);
                        //Debug.Log("player left : " + joinedPlayer.Count + "\t : " + playerId1);
                        if (joinedPlayer.Exists(x => x.playerID == playerId1))
                        {
                            joinedPlayer.Remove(joinedPlayer.Find(x => x.playerID == playerId1));

                            if (PlayerWaiting.Instance != null)
                            {
                                PlayerWaiting.Instance.RemovePlayer(playerId1);
                            }

                            if (GameController.Instance != null)
                            {
                                GameController.Instance.RemovePlayer(playerId1);
                            }
                        }
                        break;

                    case "DisconectPlayer":
                        // remove characters from the scene when they leave                    
                        string playerId2 = m.GetString(0).Remove(0, 6);
                        //Debug.Log("DisconectPlayer" + joinedPlayer.Count + "\t : " + playerId2);
                        if (joinedPlayer.Exists(x => x.playerID == playerId2))
                        {
                            joinedPlayer.Remove(joinedPlayer.Find(x => x.playerID == playerId2));

                            if (PlayerWaiting.Instance != null)
                            {
                                PlayerWaiting.Instance.RemovePlayer(playerId2);
                            }
                            if (GameController.Instance != null)
                            {
                                GameController.Instance.RemovePlayer(playerId2);
                            }
                        }
                        break;

                    case "GameStart":
                        LoadScene("Game", DoNotDestroy.Instance.loadingPanel, 1);
                        break;

                    case "Timer":
                        int time = m.GetInt(0);
                        //Debug.Log("Timer : " + time);
                        break;

                    case "Chat":
                        string playerId3 = m.GetString(0);
                        string playerName3 = m.GetString(1);
                        string message = m.GetString(2);
                        if (GameScreen.Instance != null && PlayerInfo.Instance.userID != playerId3)
                        {
                            //GameScreen.Instance.ShowPlayerMessage(playerId, playerName, message);
                        }
                        break;

                    case "NotAllowToJoin":
                        string playerId4 = m.GetString(0);
                        //Debug.Log("===== NotAllowToJoin : " + playerId4);
                        if (playerId4 == player.ConnectUserId)
                        {
                            bool playPrivate = PlayerInfo.Instance.playType == PlayType.PlayPrivate ? true : false;
                            if (playPrivate)
                            {
                                joinedPlayer.Clear();
                                piocon.Disconnect();
                                HomeScreen.Instance.playerWaiting.SetActive(false);
                                ShowMessageBox("Room already started, please create OR join another room");
                            }
                            else
                                CreateRoom(playPrivate);
                        }
                        break;
                }
            }
            // clear message queue after it's been processed
            msgList.Clear();
        }

        // Create new room
        public void CreateNewRoom(string roomName, bool playPrivate)
        {
            //Debug.Log("CreateNewRoom");
            if (player == null)
            {
                ShowMessageBox("Unable to connect with server, please try again");
                Authentication();
                return;
            }

            player.Multiplayer.CreateJoinRoom(
                roomName,             //Room id. If set to null a random roomid is used
                roomType,                                   //The room type started on the server
                true,                                         //Should the room be visible in the lobby?
                new Dictionary<string, string> {
                { "maxPlayers", "4" },
                { "currentPlayers", "0" },
                { "playPrivate", playPrivate.ToString() }
                },
                new Dictionary<string, string> {
                { "DeviceId" , SystemInfo.deviceUniqueIdentifier }
                },
                delegate (Connection connection)
                {
                //Debug.Log("room created successfull...");
                piocon = connection;
                    piocon.OnMessage += handlemessage;
                    isAdmin = true;
                    PlayerInfo player = PlayerInfo.Instance;
                    DoNotDestroy.Instance.loadingPanel.SetActive(false);
                    HomeScreen.Instance.createOrJoin.SetActive(false);
                    HomeScreen.Instance.playerWaiting.SetActive(true);
                    SendJoinPlayer(player.userID, player.userName, player.userImage, player.playerIcon);
                },
                delegate (PlayerIOError error)
                {
                    Debug.Log("Error CreateOrJoin Room: " + error.Message);
                    ShowMessageBox(error.Message);
                }
            );
        }

        // Joining room
        public void JoinRoom(string roomID, bool playPrivate)
        {
            if (player == null)
            {
                ShowMessageBox("Unable to connect with server, please try again");
                Authentication();
                return;
            }

            player.Multiplayer.JoinRoom(
                roomID,
                new Dictionary<string, string> {
                { "DeviceId" , SystemInfo.deviceUniqueIdentifier },
                { "maxPlayers", "4" },
                { "currentPlayers", "0" },
                { "playPrivate", playPrivate.ToString() }
                },
                delegate (Connection connection)
                {
                //Debug.Log("room joined successfull...");
                piocon = connection;
                    piocon.OnMessage += handlemessage;
                    isAdmin = false;
                    PlayerInfo player = PlayerInfo.Instance;
                    DoNotDestroy.Instance.loadingPanel.SetActive(false);
                    HomeScreen.Instance.createOrJoin.SetActive(false);
                    HomeScreen.Instance.playerWaiting.SetActive(true);
                    SendJoinPlayer(player.userID, player.userName, player.userImage, player.playerIcon);
                },
                delegate (PlayerIOError error)
                {
                    Debug.Log("Error Joining Room: " + error.ToString());
                    ShowMessageBox(error.Message);
                }
            );
        }

        public void CreateRoom(bool playPrivate)
        {
            PollRoomList(playPrivate);
        }

        // allready created room list function
        private void PollRoomList(bool playPrivate)
        {
            //Debug.Log("PollRoomList");
            if (player != null)
            {
                player.Multiplayer.ListRooms(roomType,
                    new Dictionary<string, string> {
                    { "playPrivate", playPrivate.ToString() }
                    }
                    , 20, 0, OnRoomList, delegate (PlayerIOError error)
                    {
                        Debug.Log("Error PollRoomList : " + error.ToString());
                        ShowMessageBox(error.Message);
                    });
            }
            else
            {
                ShowMessageBox("Unable to connect with server, please try again");
                Authentication();
            }
        }

        // room information
        void OnRoomList(RoomInfo[] rooms)
        {
            int i = 0;
            //Debug.Log("Room count : " + rooms.Length.ToString());
            bool playPrivate = PlayerInfo.Instance.playType == PlayType.PlayPrivate ? true : false;
            if (rooms.Length == 0)
            {
                CreateNewRoom(GenerateRoomId(10), playPrivate);
            }
            else
            {
                JoinRoom(rooms[i].Id, playPrivate);
            }
        }

        public void CreateNewUser(string name)
        {
            PlayerIO.Authenticate(
                GameID,                                 //Your game id
                "public",                               //Your SimpleUsers connection id
                new Dictionary<string, string> {        //Authentication arguments
                {"register", "true"},               //Register a user
                {"username", name},                 //Username - required
                {"password", password }
                },
                null,                                   //PlayerInsight segments
                delegate (Client client)
                {
                //Success!
                //Debug.Log("user registered success");
                player = client;
                },
                delegate (PlayerIOError error)
                {
                //Error registering.
                //Check error.Message to find out in what way it failed,
                //if any registration data was missing or invalid, etc.
                Debug.Log("ERROR_NewUser Reg. : " + error.Message);
                //MainScreen.Instance.ShowToastMsg(error.Message, Color.red);
            }
            );
        }

        public void Authentication(string deviceID = null)
        {
            deviceID = SystemInfo.deviceUniqueIdentifier;
            PlayerIO.Authenticate(
                GameID,                                 //Your game id
                "public",                               //Your SimpleUsers connection id
                new Dictionary<string, string> {        //Authentication arguments
                {"username", deviceID},             //Username - either this or email, or both                
                {"password", password}              //Password - required
                },
                null,                                   //PlayerInsight segments
                delegate (Client client)
                {
                //Success!
                //Debug.Log("authentication success : " + client.ConnectUserId);
                player = client;
                },
                delegate (PlayerIOError error)
                {
                //Error authenticating.
                Debug.Log("ERROR_Authentication : " + error.Message);
                    if (error.Message.StartsWith("UnknownUser"))
                    {
                        CreateNewUser(deviceID);
                    }
                }
            );
        }

        private void SendJoinPlayer(string playerId, string playerName, string playerImage, int playerIcon)
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("JoinPlayer", playerId, playerName, playerImage, playerIcon);
        }

        public void SendPlayerTurn(string playerId, int playerValue, int dicevalue)
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("PlayerTurn", playerId, playerValue, dicevalue);
        }

        public void SendDisconnect()
        {
            if (piocon != null && piocon.Connected)
            {
                piocon.Send("DisconectPlayer");
                piocon.Disconnect();
            }
        }

        public void SendStartParty(string playerId)
        {
            if (piocon != null && piocon.Connected)
            {
                piocon.Send("StartParty", playerId);
            }
        }

        public void SendWinner(string playerId)
        {
            if (piocon != null && piocon.Connected)
            {
                piocon.Send("Winner", playerId);
            }
        }

        public void LeaveRoom()
        {
            if (piocon != null && piocon.Connected)
            {
                piocon.Disconnect();
            }
        }

        public void SendPlayerMessage(string playerId, string playerName, string message)
        {
            if (piocon != null && piocon.Connected)
            {
                piocon.Send("Chat", playerId, playerName, message);
            }
        }
    }

    public class PlayerData
    {
        public string playerID { get; set; }
        public string playerName { get; set; }
        public string playerImage { get; set; }
        public int playerIcon { get; set; }
    }
}
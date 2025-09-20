using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;

namespace ArtboxSnakeAndLadder
{
    public class Player : BasePlayer
    {
    }

    public class PlayerData
    {
        public string playerID { get; set; }
        public string playerName { get; set; }
        public string playerImage { get; set; }
        public int playerIcon { get; set; }
    }

    [RoomType("SnakeAndLadder")]
    public class GameCode : Game<Player>
    {
        private string[] botNameList = new string[] { "Vaidik", "Rishik" ,"Peter","David","Michael","John","Priya","Kevin",
            "Raj Shah","Krunal Roy","Pankaj","Aryan","Grave","Addition","Rajesh","Divya","Kajal","Rohit","Bhuvan","Avani",
            "Nikita","Archana","Mitul","Pavan","Nirmal Vijay","Sweta","Yogesh","Deelip","Tushar","Alkesh","Robins","Dinesh",
            "Adler","Bardolf","Josef","Newton","Charles William","Stephen","Niels Bohr","Max Born","Robert","Brian Cox","Thomas","Henry",
            "Bhavin","Aadarsh","Pooja","Radhika","Kalpana","Bahubali","Chirag","Divya","Khushi","Rohan","Shreya","Dhruvi"};
        private List<string> botNames = new List<string>();

        private List<PlayerData> joinedPlayer = new List<PlayerData>();
        private Timer timer;
        private Timer waitTimer;
        private List<string> playerIcons = new List<string>();
        private bool isRoomFull = false;
        Random random = new Random();

        // This method is called when an instance of your the game is created
        public override void GameStarted() {
            // anything you write to the Console will show up in the 
            // output window of the development server
            Console.WriteLine("Game is started: " + RoomId);
            if (RoomData["playPrivate"] == "False")
                StartTimer();
        }

        // This method is called when the last player leaves the room, and it's closed down.
        public override void GameClosed() {
            Console.WriteLine("RoomId: " + RoomId);
        }

        // This method is called whenever a player joins the game
        public override void UserJoined(Player player) {
            foreach (Player pl in Players) {
                if (pl.ConnectUserId != player.ConnectUserId) {

                }
            }
        }

        // This method is called when a player leaves the game
        public override void UserLeft(Player player) {
            Broadcast("PlayerLeft", player.ConnectUserId);
            string playerid = player.ConnectUserId;
            playerid = playerid.Remove(0, 6);
            if (joinedPlayer.Exists(x => x.playerID == playerid)) {
                joinedPlayer.Remove(joinedPlayer.Find(x => x.playerID == playerid));
            }
        }

        //This method is called before a user joins a room.
        //If you return false, the user is not allowed to join.
        public override bool AllowUserJoin(Player player) {
            if (PlayerCount >= 4) {
                Visible = false;
                isRoomFull = true;
            }

            if (!isRoomFull)
                return true; // allow joining
            else {
                player.Send("NotAllowToJoin", player.ConnectUserId);
                return false;
            }
        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message message) {
            switch (message.Type) {
                // called when a player clicks on the ground
                case "JoinPlayer":
                    string playerId = message.GetString(0);
                    string playerName = message.GetString(1);
                    string playerImage = message.GetString(2);
                    int playerIcon = message.GetInt(3);

                    if (!joinedPlayer.Exists(x => x.playerID == playerId)) {
                        PlayerData jPlayer = new PlayerData();
                        jPlayer.playerID = playerId;
                        jPlayer.playerName = playerName;
                        jPlayer.playerImage = playerImage;
                        jPlayer.playerIcon = playerIcon;
                        joinedPlayer.Add(jPlayer);

                        foreach (PlayerData pData in joinedPlayer) {
                            Broadcast("JoinPlayer", pData.playerID, pData.playerName, pData.playerImage, pData.playerIcon);
                        }                        
                        if (!playerIcons.Contains(jPlayer.playerIcon.ToString()))
                            playerIcons.Add(jPlayer.playerIcon.ToString());
                 
                        if (joinedPlayer.Count >= 4) {
                            Visible = false;
                        }
                    }
                    break;

                case "PlayerTurn":
                    string playerId1 = message.GetString(0);
                    int playerValue = message.GetInt(1);
                    int diceValue = message.GetInt(2);
                    Broadcast("PlayerTurn", playerId1, playerValue, diceValue);
                    break;

                case "StartParty":
                    Visible = false;
                    isRoomFull = true;
                    string playerId2 = message.GetString(0);
                    Broadcast("GameStart", true);
                    break;

                case "Chat":

                    break;

                case "DisconectPlayer":
                    Broadcast("DisconectPlayer", player.ConnectUserId);
                    string playerid = player.ConnectUserId;
                    playerid = playerid.Remove(0, 6);
                    if (joinedPlayer.Exists(x => x.playerID == playerid)) {
                        joinedPlayer.Remove(joinedPlayer.Find(x => x.playerID == playerid));
                    }
                    break;
            }
        }

        private void StartTimer() {
            //Console.WriteLine("Game is started");
            int time = 15;
            timer = AddTimer(delegate {
                time--;
                //Console.WriteLine("timer:" + time);
                int playerLimit = int.Parse(RoomData["maxPlayers"]);
                int players = joinedPlayer.Count;
                Broadcast("Timer", time);

                if (players == playerLimit) {
                    Visible = false;
                    WaitBeforeStart(2000);
                    timer.Stop();
                }
                if (time == 2) {
                    Visible = false;
                }
                if (time == 0) {                    
                    if (joinedPlayer.Count == 1) {
                        GenerateBotPlayer();
                    }
                    else {
                        WaitBeforeStart(1000);
                    }
                    timer.Stop();
                }
            },
            1000);
        }

        private void WaitBeforeStart(int interval = 5000) {
            waitTimer = AddTimer(delegate {
                Broadcast("GameStart", true);
                waitTimer.Stop();
            },
            interval);
        }

        private void GenerateBotPlayer() {
            int noOfPlayer = random.Next(1, 3);
            for (int i = 0; i < noOfPlayer; i++) {
                PlayerData jPlayer = new PlayerData();
                string botName = PlayerName();
                jPlayer.playerID = "bot_" + botName;
                jPlayer.playerName = botName;
                jPlayer.playerImage = "0";
                jPlayer.playerIcon = 4;
                joinedPlayer.Add(jPlayer);

                Broadcast("JoinPlayer", jPlayer.playerID, jPlayer.playerName, jPlayer.playerImage, jPlayer.playerIcon);
            }
            WaitBeforeStart(2000);
        }

        private int PlayerIcon() {
            int playerIcon = random.Next(0, 4);
            while (playerIcons.Contains(playerIcon.ToString())) {
                playerIcon = random.Next(0, 4);
            }
            playerIcons.Add(playerIcon.ToString());
            return playerIcon;
        }

        private string PlayerName() {
            string playerName = botNameList[random.Next(0, botNameList.Length)];
            while (playerIcons.Contains(playerName.ToString())) {
                playerName = botNameList[random.Next(0, botNameList.Length)];
            }
            botNames.Add(playerName.ToString());
            return playerName;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class GameController : GameManager
    {
        public static GameController Instance;

        [SerializeField] private List<Square> squares;
        [SerializeField] private Sprite[] lightCellBg;
        [Space(5)]
        [SerializeField] private Transform boardPlayers;
        [SerializeField] private Transform mainPlayer;
        [SerializeField] private Transform oppPlayer;
        [Space(5)]
        [SerializeField] private GameObject boardPlayerPrefab;
        [SerializeField] private GameObject dicePlayerPrefab;
        [SerializeField] private GameObject opp_dicePlayerPrefab;

        [SerializeField] private List<DicePlayer> dicePlayerList = new List<DicePlayer>();
        [SerializeField] private List<Player> boardPlayerList = new List<Player>();

        [SerializeField] private Image dice;
        [SerializeField] private GameObject arrow;

        private int turnIndex = 0;
        private string[] turnPlayers;
        private List<string> winners = new List<string>();
        private List<string> playerIcons;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            playerIcons = new List<string>() { "0", "1", "2", "3" };
            AddSquareOnList();
            StartCoroutine(GeneratePlayer());
            PlayerInfo.Instance.totalPlayed++;
        }

        private void AddSquareOnList()
        {
            Transform board = transform.Find("Board");
            for (int i = 0; i < board.childCount; i++)
            {
                Square square = board.GetChild(i).GetComponent<Square>();
                square.number = (i + 1);
                if (i % 2 == 1)
                    square.background.sprite = lightCellBg[PlayerInfo.Instance.theme];
                squares.Add(square);
            }
        }

        private IEnumerator GeneratePlayer()
        {
            int myIcon = ServerCode.Instance.joinedPlayer.Find(x => x.playerID == PlayerInfo.Instance.userID).playerIcon;
            playerIcons.Remove(myIcon.ToString());
            for (int i = 0; i < ServerCode.Instance.joinedPlayer.Count; i++)
            {
                PlayerData playerData = ServerCode.Instance.joinedPlayer[i];
                Sprite playerIcon = SetPlayerIcon(playerData.playerID, playerData.playerIcon);
                Player boardPlayer = Instantiate(boardPlayerPrefab, boardPlayers, false).GetComponent<Player>();
                boardPlayer.name = playerData.playerID;
                boardPlayer.playerID = playerData.playerID;
                boardPlayer.playerValue = 1;
                boardPlayer.playerImage.sprite = playerIcon;
                boardPlayerList.Add(boardPlayer);

                DicePlayer dicePlayer = null;
                if (playerData.playerID == PlayerInfo.Instance.userID)
                {
                    dicePlayer = Instantiate(dicePlayerPrefab, mainPlayer, false).GetComponent<DicePlayer>();
                }
                else
                {
                    if (ServerCode.Instance.joinedPlayer.Count == 2)
                        dicePlayer = Instantiate(dicePlayerPrefab, oppPlayer, false).GetComponent<DicePlayer>();
                    else
                        dicePlayer = Instantiate(opp_dicePlayerPrefab, oppPlayer, false).GetComponent<DicePlayer>();
                }
                dicePlayer.name = playerData.playerID;
                dicePlayer.playerID = playerData.playerID;
                dicePlayer.playerName.text = playerData.playerName;
                dicePlayer.playerImage.sprite = playerIcon;
                dicePlayerList.Add(dicePlayer);
                yield return new WaitForEndOfFrame();
            }
            SetInitialTurn(ServerCode.Instance.joinedPlayer[0].playerID);
            boardPlayers.GetComponent<GridLayoutGroup>().enabled = false;

            if (ServerCode.Instance.joinedPlayer.Count == 1)
            {
                ShowWinGame();
            }
        }

        private Sprite SetPlayerIcon(string playerId, int playerIcon)
        {
            if (playerId == PlayerInfo.Instance.userID)
            {
                return GetPlayerImage(playerIcon);
            }

            if (playerId.StartsWith("Computer"))
                return GetPlayerImage(4);

            int icon = 0;
            if (playerIcons.Contains(playerIcon.ToString()))
            {
                playerIcons.Remove(playerIcon.ToString());
                return GetPlayerImage(playerIcon);
            }
            else
            {
                for (int i = 0; i < playerIcons.Count; i++)
                {
                    if (playerIcons[i] != playerIcon.ToString())
                    {
                        icon = int.Parse(playerIcons[i]);
                        playerIcons.Remove(icon.ToString());
                        break;
                    }
                }
                return GetPlayerImage(icon);
            }
        }

        private void SetInitialTurn(string playerId)
        {
            turnPlayers = new string[dicePlayerList.Count];

            for (int i = 0; i < dicePlayerList.Count; i++)
            {
                turnPlayers[i] = dicePlayerList[i].playerID;
            }

            if (playerId == PlayerInfo.Instance.userID)
            {
                arrow.SetActive(true);
                dice.GetComponent<Button>().enabled = true;
            }
            dicePlayerList.Find(x => x.playerID == playerId).turn.SetActive(true);
        }

        public void RollDice()
        {
            int diceValue = Random.Range(1, 7);
            string playerId = PlayerInfo.Instance.userID;
            int myValue = boardPlayerList.Find(x => x.playerID == playerId).playerValue;
            StartCoroutine(RollDiceAnim(playerId, myValue, diceValue, 0f));

            if (PlayerInfo.Instance.playType != PlayType.Play && !ServerCode.Instance.botActivated)
                ServerCode.Instance.SendPlayerTurn(playerId, myValue, diceValue);
        }

        public IEnumerator RollDiceAnim(string playerId, int playerValue, int diceValue, float wait = 0)
        {
            yield return new WaitForSeconds(wait);
            PlaySound(2);
            dice.transform.Find("ChildDice").GetComponent<Image>().enabled = false;
            dice.GetComponent<Animator>().enabled = true;
            dice.GetComponent<Animator>().speed = 1;
            dice.GetComponent<Animator>().Play("dice_rotate");
            yield return new WaitForSeconds(0.4f);
            dice.transform.Find("ChildDice").GetComponent<Image>().enabled = true;
            dice.transform.Find("ChildDice").GetComponent<Image>().sprite = GetDiceImage(diceValue);

            Player player = boardPlayerList.Find(x => x.playerID == playerId);

            if ((player.playerValue + diceValue) <= 100)
            {
                MovePlayerWithDice(player, playerValue, diceValue);
            }
            else
            {
                GiveTurn(turnIndex);
            }
        }

        private void MovePlayerWithDice(Player player, int playerValue, int diceValue)
        {
            List<Square> ListofSquare = GetDestinationSquare(playerValue, (playerValue + diceValue));
            ListofSquare = ListofSquare.OrderBy(x => x.number).ToList();
            Vector3[] SquarePOS = new Vector3[ListofSquare.Count + 1];
            SquarePOS[0] = GetSquare(player.playerValue - 1).transform.position;
            for (int i = 1; i <= ListofSquare.Count; i++)
            {
                SquarePOS[i] = ListofSquare[i - 1].transform.position;
            }
            player.playerValue += diceValue;
            StartCoroutine(MultipleLerp(SquarePOS, 3.5f, player));
        }

        private List<Square> GetDestinationSquare(int PlayerStartVal, int PlayerValue)
        {
            List<Square> NumberOfSquare = new List<Square>();
            for (int RequreSqure = PlayerStartVal; RequreSqure < PlayerValue; RequreSqure++)
            {
                var square = GetSquare(RequreSqure);
                if (square == null)
                {
                    break;
                }
                if (square != null)
                {
                    NumberOfSquare.Add(square);
                }
            }
            return NumberOfSquare;
        }

        private IEnumerator MultipleLerp(Vector3[] positions, float speed, Player player)
        {
            for (int i = 0; i < positions.Length - 1; i++)
            {
                Vector3 startPos = positions[i];
                Vector3 endPos = positions[i + 1];
                float timer = 0f;
                player.GetComponent<Animation>().Play();
                while (timer <= 1f)
                {
                    timer += Time.deltaTime * speed;
                    Vector3 newPos = Vector3.Lerp(startPos, endPos, timer);
                    player.transform.position = newPos;
                    yield return null;
                }
                yield return new WaitForSeconds(0.05f);
            }

            if (player.playerValue >= 100)
            {
                if (!winners.Contains(player.playerID))
                {
                    winners.Add(player.playerID);
                    int index = System.Array.IndexOf(turnPlayers, player.playerID);
                    turnPlayers[index] = "";
                }

                if (winners.Count == boardPlayerList.Count - 1 || ServerCode.Instance.joinedPlayer.Count == 1
                    || player.playerID == PlayerInfo.Instance.userID)
                {
                    yield return new WaitForSeconds(0.5f);
                    if (winners.Contains(PlayerInfo.Instance.userID))
                    {
                        ShowWinGame();
                    }
                    else
                    {
                        GameScreen.Instance.gameLose.SetActive(true);
                        Vibration.VibratePop();
                        PlaySound(6);

                        if (AdsManager.Instance != null)
                            AdsManager.Instance.ShowInterstitial();
                    }
                }
                else
                {
                    GiveTurn(turnIndex);
                }
            }
            else
            {
                Square fromSquare = GetSquare(player.playerValue - 1);
                if (fromSquare.from == player.playerValue)
                {
                    yield return new WaitForSeconds(0.2f);
                    if (fromSquare.to > player.playerValue)
                        PlaySound(3);
                    else
                        PlaySound(4);
                    Square toSquare = GetSquare(fromSquare.to - 1);
                    StartCoroutine(MovePlayer(fromSquare.transform.position, toSquare.transform.position, 0.5f, player));
                    player.playerValue = fromSquare.to;
                }
                else
                {
                    GiveTurn(turnIndex);
                }
            }
        }

        //set player position based on to value
        public IEnumerator MovePlayer(Vector3 from, Vector3 to, float time, Player player)
        {
            var i = 0.0f;
            var rate = 1.0 / time;
            while (i < 1.0)
            {
                i += (float)(Time.deltaTime * rate);
                if (player != null)
                {
                    player.transform.position = Vector3.Lerp(from, to, i);
                    if (player.transform.position == to)
                    {
                        break;
                    }
                }
                yield return null;
            }
            GiveTurn(turnIndex);
        }

        private Square GetSquare(int index)
        {
            if (index <= 99)
            {
                return squares[index];
            }
            else
            {
                return null;
            }
        }

        private int NextIndex(int lastIndex)
        {
            for (int i = 0; i < turnPlayers.Length; i++)
            {
                if (lastIndex == turnPlayers.Length - 1)
                {
                    lastIndex = 0;
                }
                else
                {
                    lastIndex++;
                }
                if (!string.IsNullOrEmpty(turnPlayers[lastIndex]))
                {
                    break;
                }
            }
            turnIndex = lastIndex;
            return lastIndex;
        }

        private void GiveTurn(int lastPlayerIndex)
        {
            int nextIndex = NextIndex(lastPlayerIndex);

            if (lastPlayerIndex < dicePlayerList.Count)
            {
                DicePlayer lastDicePlayer = dicePlayerList[lastPlayerIndex];
                lastDicePlayer.turn.SetActive(false);
            }

            DicePlayer turnDicePlayer = dicePlayerList[nextIndex];
            turnDicePlayer.turn.SetActive(true);

            // if turn player is me
            if (turnDicePlayer.playerID == PlayerInfo.Instance.userID)
            {
                arrow.SetActive(true);
                dice.GetComponent<Button>().enabled = true;
                Vibration.Vibrate(100);
            }
            else
            {
                if (PlayerInfo.Instance.playType == PlayType.Play
                    || (PlayerInfo.Instance.playType == PlayType.PlayOnline && ServerCode.Instance.botActivated))
                {
                    int diceValue = Random.Range(1, 7);
                    var playerValue = boardPlayerList.Find(x => x.playerID == turnDicePlayer.playerID).playerValue;
                    StartCoroutine(RollDiceAnim(turnDicePlayer.playerID, playerValue, diceValue, 0.5f));
                }
            }
        }

        public void RemovePlayer(string playerId)
        {
            if (dicePlayerList.Exists(x => x.playerID == playerId) && !winners.Contains(playerId))
            {
                DicePlayer dicePlayer = dicePlayerList.Find(x => x.playerID == playerId);
                Player boardPlayer = boardPlayerList.Find(x => x.playerID == playerId);
                boardPlayerList.Remove(boardPlayer);
                Destroy(boardPlayer.gameObject);
                dicePlayer.group.alpha = 0.5f;
                dicePlayer.turn.SetActive(false);

                if (ServerCode.Instance.joinedPlayer.Count == 1)
                {
                    ShowWinGame();
                }
                else
                {
                    int index = System.Array.IndexOf(turnPlayers, playerId);
                    turnPlayers[index] = "";
                    if (index == turnIndex)
                    {
                        GiveTurn(turnIndex);
                    }
                }
            }
        }

        private void ShowWinGame()
        {
            GameScreen.Instance.gameWin.SetActive(true);

            if (AdsManager.Instance != null)
                AdsManager.Instance.ShowInterstitial();
        }
    }
}
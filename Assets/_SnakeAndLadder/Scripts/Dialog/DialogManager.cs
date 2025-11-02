using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace ArtboxGames
{
    public class DialogManager : GameManager
    {
        [Header("Vibration & Sound")]
        [SerializeField] private GameObject vibrationOff;
        [SerializeField] private GameObject soundOff;

        private void Start()
        {
            SetVibration(PlayerInfo.Instance.vibration);
            SetSound(PlayerInfo.Instance.sound);
        }

        public void QuitYes(string sceneName)
        {
            if (SceneManager.GetActiveScene().name == "Game")
            {
                LoadScene(sceneName, DoNotDestroy.Instance.loadingPanel);
                DisconnectPlayer();
            }
            else
            {
                Application.Quit();
            }
        }   

        public void Restart()
        {
            if (PlayerInfo.Instance.playType == PlayType.Play)
            {
                LoadScene("Game", DoNotDestroy.Instance.loadingPanel);
            }
        }

        public void CreateRoom()
        {
            if (!CheckBalance(3000))
                return;
            PlayerInfo.Instance.playType = PlayType.PlayPrivate;
            PlayerInfo.Instance.bootAmount = 3000;
            DoNotDestroy.Instance.loadingPanel.SetActive(true);
            ServerCode.Instance.CreateNewRoom(GenerateRoomId(), true);
        }

        public void JoinRoom(TMP_InputField roomCode)
        {
            if (!CheckBalance(3000))
                return;
            PlayerInfo.Instance.playType = PlayType.PlayPrivate;
            PlayerInfo.Instance.bootAmount = 3000;
            ServerCode.Instance.roomCode = roomCode.text;
            if (string.IsNullOrEmpty(roomCode.text))
                ShowMessageBox("Please enter room code");
            else
            {
                DoNotDestroy.Instance.loadingPanel.SetActive(true);
                ServerCode.Instance.JoinRoom(roomCode.text, true);
            }
        }

        public void SetVibration(int status)
        {
            PlayerInfo.Instance.vibration = status;
            if (status == 1)
            {
                vibrationOff.SetActive(false);
            }
            else
            {
                vibrationOff.SetActive(true);
            }
        }

        public void SetSound(int status)
        {
            PlayerInfo.Instance.sound = status;
            if (status == 1)
            {
                soundOff.SetActive(false);
            }
            else
            {
                soundOff.SetActive(true);
            }
            SoundManager.Instance.PlayBackground();
        }

        public void WatchVideo()
        {
            if (!AdsManager.Instance.ShowRewardVideo())
            {
                ShowMessageBox("Rewarded video is not ready at the moment! Please try again later!");
            }
        }
    }
}   
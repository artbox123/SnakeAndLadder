using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

namespace ArtboxGames
{
    public class GameManager : MonoBehaviour
    {
        private string characters = "123456789abcdefghijkmnopqrstuvwxyz";

        public IEnumerator LoadAsyncScene(string sceneName, GameObject loading = null, float waitTime = 0, Image fillImage = null, TextMeshProUGUI fillPercent = null)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.
            if (loading != null)
                loading.SetActive(true);
            yield return new WaitForSeconds(waitTime);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 1f);
                if (fillImage != null)
                {
                    fillImage.fillAmount = progress;
                    if (fillPercent != null)
                        fillPercent.text = "Loading... " + (asyncLoad.progress * 100f).ToString("00") + "%";
                }
                yield return null;
            }
            if (asyncLoad.isDone)
            {
                if (fillImage != null)
                {
                    fillImage.fillAmount = 1f;
                }
            }
        }

        public void LoadScene(string sceneName, GameObject loading = null, float waitTime = 0, Image fillImage = null, TextMeshProUGUI fillPercent = null)
        {
            StartCoroutine(LoadAsyncScene(sceneName, loading, waitTime, fillImage, fillPercent));
        }

        public IEnumerator CheckInternet(System.Action<bool> action)
        {
            UnityWebRequest request = new UnityWebRequest("http://google.com");
            yield return request.SendWebRequest();
            if (request.error != null)
            {
                action(false);
            }
            else
            {
                action(true);
            }
        }

        // set screen ui with screen current resolution
        public void SetScreen()
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

        public IEnumerator LoadImageFromPath(string path, Image _userImage)
        {
            if (path.StartsWith("https"))
            {
                int h_w = 90;
                if (path.StartsWith("https://graph"))
                    h_w = 200;

                WWW url = new WWW(path);
                Texture2D textFb2 = new Texture2D(h_w, h_w, TextureFormat.RGBA32, false); //TextureFormat must be DXT5
                    
                yield return url;
                Debug.Log("GetUserImage : " + url);

                float width = (float)textFb2.width;
                float height = (float)textFb2.height;
                Rect rect = new Rect(0, 0, width, height);

                if (url.error == null)
                {
                    _userImage.sprite = Sprite.Create(textFb2, rect, new Vector2(0, 0), 1);
                    url.LoadImageIntoTexture(textFb2);
                }
            }
        }

        public Sprite GetPlayerImage(int index)
        {
            return DoNotDestroy.Instance.playerImages[index];
        }

        public Sprite GetDiceImage(int index)
        {
            return DoNotDestroy.Instance.diceImages[index - 1];
        }

        public void InitializeMessageBox(GameObject m_loadingPanel, GameObject m_messageBox, TextMeshProUGUI m_message)
        {
            DoNotDestroy.Instance.loadingPanel = m_loadingPanel;
            DoNotDestroy.Instance.messageBox = m_messageBox;
            DoNotDestroy.Instance.message = m_message;
        }

        public void ShowMessageBox(string msg)
        {
            DoNotDestroy.Instance.messageBox.SetActive(true);
            DoNotDestroy.Instance.loadingPanel.SetActive(false);
            DoNotDestroy.Instance.message.text = msg;
        }

        public void Logout()
        {
            PlayerInfo.Instance.userName = "";
            PlayerInfo.Instance.userImage = "0";
            LoadScene("Login", DoNotDestroy.Instance.loadingPanel, 1f);
        }

        public void DisconnectPlayer()
        {
            ServerCode.Instance.SendDisconnect();
            ServerCode.Instance.joinedPlayer.Clear();
            ServerCode.Instance.isAdmin = false;
            ServerCode.Instance.botActivated = false;
            ServerCode.Instance.roomCode = "";
        }

        public void PlayButtonSound(bool showAds = true)
        {
            SoundManager.Instance.PlayButtonSound();
        }

        public void PlaySound(int index)
        {
            SoundManager.Instance.PlaySound(index);
        }

        public bool CheckBalance(int bootAmount)
        {
            if (PlayerInfo.Instance.coins >= bootAmount)
                return true;
            else
            {
                HomeScreen.Instance.WatchVideo.SetActive(true);
                return false;
            }
        }

        public string GenerateRoomId(int length = 6)
        {
            string roomId = "";
            for (int i = 0; i < length; i++)
            {
                int a = Random.Range(0, characters.Length);
                roomId = roomId + characters[a];
            }
            //Debug.Log("=== room id : " + roomId);
            ServerCode.Instance.roomCode = roomId;
            return roomId;
        }
    }
}
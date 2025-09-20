using UnityEngine;

namespace ArtboxGames
{
    public class ThemeSelection : MonoBehaviour
    {
        [SerializeField] private GameObject[] themeSelection;

        // Start is called before the first frame update
        void Start()
        {
            SetTheme(PlayerInfo.Instance.theme);
        }

        public void SetTheme(int themeIndex)
        {
            foreach (GameObject obj in themeSelection)
            {
                obj.SetActive(false);
            }
            PlayerInfo.Instance.theme = themeIndex;
            themeSelection[themeIndex].SetActive(true);
        }
    }
}
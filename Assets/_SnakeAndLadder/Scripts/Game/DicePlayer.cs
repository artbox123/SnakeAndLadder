using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArtboxGames
{
    public class DicePlayer : MonoBehaviour
    {
        public string playerID;
        public Image playerImage;
        public TextMeshProUGUI playerName;
        public GameObject turn;
        public CanvasGroup group;
    }
}
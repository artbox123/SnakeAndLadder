using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.Events;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArtboxGames
{
    public class FortuneWheelManager : GameManager
    {
        [Header("Game Objects for some elements")]
        [SerializeField] private GameObject Circle;                   // Rotatable GameObject on scene with reward objects
        [SerializeField] private GameObject EarnCoinPanel;                 // Pop-up text with wasted or rewarded coins amount
        [SerializeField] private Button Spin;
        [SerializeField] private TextMeshProUGUI time;
        [SerializeField] private GameObject[] explosion;

        private bool _isStarted;                    // Flag that the wheel is spinning

        [Header("Params for each sector")]
        [SerializeField] private FortuneWheelSector[] Sectors;        // All sectors objects

        private float _finalAngle;                  // The final angle is needed to calculate the reward
        private float _startAngle;                  // The first time start angle equals 0 but the next time it equals the last final angle
        private float _currentLerpRotationTime;     // Needed for spinning animation

        // Key name for storing in PlayerPrefs
        private const string LAST_TURN_TIME = "LastTurnTime";

        private FortuneWheelSector _finalSector;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            InvokeRepeating("CheckSpinTime", 0, 1);
        }

        private void Awake()
        {
            // Show sector reward value in text object if it's set
            foreach (var sector in Sectors)
            {
                if (sector.ValueTextObject != null)
                    sector.ValueTextObject.GetComponent<Text>().text = sector.RewardValue.ToString();
            }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        void OnEnable()
        {
            EarnCoinPanel.gameObject.SetActive(false);
            explosion[0].SetActive(false);
            explosion[1].SetActive(false);
        }

        private void TurnWheelForFree() { TurnWheel(true); }

        private void TurnWheel(bool isFree)
        {
            //Debug.Log("turn wheel");

            _currentLerpRotationTime = 0f;

            // All sectors angles
            int[] sectorsAngles = new int[Sectors.Length];

            // Fill the necessary angles (for example if we want to have 12 sectors we need to fill the angles with 30 degrees step)
            // It's recommended to use the EVEN sectors count (2, 4, 6, 8, 10, 12, etc)
            for (int i = 1; i <= Sectors.Length; i++)
            {
                sectorsAngles[i - 1] = 360 / Sectors.Length * i;
            }

            //int cumulativeProbability = Sectors.Sum(sector => sector.Probability);

            double rndNumber = UnityEngine.Random.Range(1, Sectors.Sum(sector => sector.Probability));

            // Calculate the propability of each sector with respect to other sectors
            int cumulativeProbability = 0;
            // Random final sector accordingly to probability
            int randomFinalAngle = sectorsAngles[0];
            _finalSector = Sectors[0];

            for (int i = 0; i < Sectors.Length; i++)
            {
                cumulativeProbability += Sectors[i].Probability;

                if (rndNumber <= cumulativeProbability)
                {
                    // Choose final sector
                    randomFinalAngle = sectorsAngles[i];
                    _finalSector = Sectors[i];
                    break;
                }
            }

            int fullTurnovers = 5;

            // Set up how many turnovers our wheel should make before stop
            _finalAngle = fullTurnovers * 360 + randomFinalAngle;

            // Stop the wheel
            _isStarted = true;
            Spin.interactable = false;
        }

        public void TurnWheelButtonClick()
        {
            TurnWheelForFree();
        }

        private void Update()
        {
            if (!_isStarted)
                return;

            // Animation time
            float maxLerpRotationTime = 4f;

            // increment animation timer once per frame
            _currentLerpRotationTime += Time.deltaTime;

            // If the end of animation
            if (_currentLerpRotationTime > maxLerpRotationTime || Circle.transform.eulerAngles.z == _finalAngle)
            {
                _currentLerpRotationTime = maxLerpRotationTime;
                _isStarted = false;
                _startAngle = _finalAngle % 360;

                //GiveAwardByAngle ();
                _finalSector.RewardCallback.Invoke();
            }
            else
            {
                // Calculate current position using linear interpolation
                float t = _currentLerpRotationTime / maxLerpRotationTime;

                // This formulae allows to speed up at start and speed down at the end of rotation.
                // Try to change this values to customize the speed
                t = t * t * t * (t * (6f * t - 15f) + 10f);

                float angle = Mathf.Lerp(_startAngle, _finalAngle, t);
                Circle.transform.eulerAngles = new Vector3(0, 0, -angle);
            }
        }

        /// <summary>
        /// Sample callback for giving reward (in editor each sector have Reward Callback field pointed to this method)
        /// </summary>
        /// <param name="awardCoins">Coins for user</param>
        public void RewardCoins(int awardCoins)
        {
            EarnCoinPanel.transform.Find("Coins").GetComponent<TextMeshProUGUI>().text = String.Format("{0}", awardCoins);
            EarnCoinPanel.gameObject.SetActive(true);

            PlayerInfo.Instance.UpdateCoins(CoinAction.Add, awardCoins);
            PlayerPrefs.SetString(LAST_TURN_TIME, DateTime.UtcNow.ToString());
            PlayerPrefs.Save();
            InvokeRepeating("CheckSpinTime", 0, 1);
            explosion[0].SetActive(true);
            explosion[1].SetActive(true);

            if (AdsManager.Instance != null)
                AdsManager.Instance.ShowInterstitialAd();
        }

        private void CheckSpinTime()
        {
            if (!PlayerPrefs.HasKey(LAST_TURN_TIME))
            {
                Spin.interactable = true;
                CancelInvoke("CheckSpinTime");
            }
            else
            {
                DateTime currentDate = DateTime.UtcNow;
                DateTime lastDate = Convert.ToDateTime(PlayerPrefs.GetString(LAST_TURN_TIME));

                TimeSpan diff = (currentDate - lastDate);
                if (diff.Days >= 1)
                {
                    Spin.interactable = true;
                    CancelInvoke("CheckSpinTime");
                    time.text = "Ready to Spin!";
                }
                else
                {
                    Spin.interactable = false;
                    time.text = "Next Spin : " + (23 - diff.Hours).ToString("00") + ":" + (59 - diff.Minutes).ToString("00") + ":" + (59 - diff.Seconds).ToString("00");
                }
            }
        }
    }

    /**
     * One sector on the wheel
     */
    [Serializable]
    public class FortuneWheelSector : System.Object
    {
        [Tooltip("Text object where value will be placed (not required)")]
        public GameObject ValueTextObject;

        [Tooltip("Value of reward")]
        public string RewardValue = "100";

        [Tooltip("Chance that this sector will be randomly selected")]
        [RangeAttribute(0, 100)]
        public int Probability = 100;

        [Tooltip("Method that will be invoked if this sector will be randomly selected")]
        public UnityEvent RewardCallback;
    }

    /**
     * Draw custom button in inspector
     */
#if UNITY_EDITOR
    [CustomEditor(typeof(FortuneWheelManager))]
    public class FortuneWheelManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FortuneWheelManager myScript = (FortuneWheelManager)target;
            if (GUILayout.Button("Reset Timer"))
            {
                //myScript.ResetTimer();
            }
        }
    }
#endif
}
using UnityEngine;

namespace ArtboxGames
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        [SerializeField] private AudioSource background;
        [SerializeField] private AudioSource sounds;
        [Space(5)]
        [SerializeField] private AudioClip[] soundClips;

        private void Awake()
        {
            Instance = this;
        }

        public void PlayBackground()
        {
            if (PlayerInfo.Instance.sound == 1)
                background.Play();
            else
            {
                background.Pause();
            }
        }

        public void PlayButtonSound()
        {
            if (PlayerInfo.Instance.sound == 1)
                sounds.PlayOneShot(soundClips[0]);
        }

        public void PlaySound(int index)
        {
            if (PlayerInfo.Instance.sound == 1)
                sounds.PlayOneShot(soundClips[index]);
        }
    }
}
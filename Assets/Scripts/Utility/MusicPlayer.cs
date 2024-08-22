using Gameplay;
using UnityEngine;

namespace Utility
{
    public class MusicPlayer : MonoBehaviour
    {
        // 0 - Intro + Win Loop
        // 1 - Level Loop 1
        // 2 - Level Loop 2
        // 3 - Level Loop 3
        // 4 - Level Loop 4
        // 5 - Level Loop 5
        // 6 - Losing Spree Minor Loop
        // 7 - Ending Loss Loop

        public AudioClip[] audioClips;
        public AudioSource audioSource;

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
            }

            Events.OnVolumeChanged.AddListener(ChangeVolume);
            PlayClip(0);
        }

        private void Update()
        {
            if (!audioSource.isPlaying)
            {
                PlayNextClip();
            }
        }

        private void PlayClip(int index)
        {
            audioSource.clip = audioClips[index];
            audioSource.Play();
        }

        private void PlayNextClip()
        {
            if (GameData.gameState == GameState.MainMenu)
            {
                PlayClip(1);
                return;
            }
            
            if (GameData.gameState == GameState.Win)
            {
                PlayClip(1);
                return;
            }
            
            if (GameData.gameState == GameState.Lose)
            {
                PlayClip(7);
                return;
            }
            
            var aqi = GameManager.Instance.AQI;
            var gdp = GameManager.Instance.GDP;
            
            if (aqi > 90)
            {
                PlayClip(6);
                return;
            }
            
            if (gdp > 100)
            {
                PlayClip(5);
                return;
            }
            
            if (gdp > 60)
            {
                PlayClip(4);
                return;
            }
            
            if (gdp > 30)
            {
                PlayClip(3);
                return;
            }
            
            if (gdp > 15)
            {
                PlayClip(2);
                return;
            }
            PlayClip(1);
        }

        private void ChangeVolume(float newVolume)
        {
            audioSource.volume = newVolume;
        }
    }
}
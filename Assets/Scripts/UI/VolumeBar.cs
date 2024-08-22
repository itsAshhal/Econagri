using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class VolumeBar : MonoBehaviour
    {
        
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Image volumeFill;

        private void Start()
        {
            var currentVolume = AudioPlayer.instance.volume;
            volumeSlider.value = currentVolume;
            volumeFill.fillAmount = currentVolume;
        }

        public void OnValueChange()
        {
            var value = volumeSlider.value;
            volumeFill.fillAmount = value;
            Events.OnVolumeChanged.Invoke(volumeSlider.value);
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MenakSopal.Audio
{
    /// <summary>
    /// Connects UI Sliders for Music, SFX, and Master volume to the AudioSystem.
    /// Attach this component to your Settings Panel and assign your UI Sliders in the Inspector.
    /// </summary>
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("UI Sliders")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;

        private Coroutine initCoroutine;

        private void OnEnable()
        {
            RefreshSliders();
        }

        private void Start()
        {
            RefreshSliders();
        }

        public void RefreshSliders()
        {
            if (initCoroutine != null) StopCoroutine(initCoroutine);
            initCoroutine = StartCoroutine(DeferredInitialize());
        }

        private IEnumerator DeferredInitialize()
        {
            // Wait until AudioSystem singleton is ready
            while (AudioSystem.Instance == null)
            {
                yield return null;
            }

            // Bind each slider using min..max range mapping
            BindSlider(masterVolumeSlider, AudioSystem.Instance.masterVolume, (val) => AudioSystem.Instance.SetMasterVolume(val));
            BindSlider(musicVolumeSlider, AudioSystem.Instance.musicVolume, (val) => AudioSystem.Instance.SetMusicVolume(val));
            BindSlider(sfxVolumeSlider, AudioSystem.Instance.sfxVolume, (val) => AudioSystem.Instance.SetSFXVolume(val));
        }

        private void BindSlider(Slider slider, float volume01, System.Action<float> onVolumeChanged)
        {
            if (slider == null) return;

            slider.onValueChanged.RemoveAllListeners();

            // Map 0..1 volume to slider's min..max range
            float mappedValue = Mathf.Lerp(slider.minValue, slider.maxValue, Mathf.Clamp01(volume01));
            slider.value = mappedValue;

            slider.onValueChanged.AddListener((val) =>
            {
                float range = slider.maxValue - slider.minValue;
                float normalizedVol = range > 0.0001f ? (val - slider.minValue) / range : val;
                onVolumeChanged?.Invoke(normalizedVol);
            });
        }
    }
}

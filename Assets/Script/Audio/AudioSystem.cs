using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MenakSopal.Audio
{
    public class AudioSystem : MonoBehaviour
    {
        public static AudioSystem Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SoundLibrary library;

        [Header("Channels")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSourcePrefab;
        [SerializeField] private int initialSfxPoolSize = 10;

        [Header("Volume Settings")]
        [Tooltip("Default volume used when no saved preference exists (0.0 to 1.0)")]
        [Range(0f, 1f)] public float defaultMasterVolume = 1f;
        [Range(0f, 1f)] public float defaultMusicVolume = 1f;
        [Range(0f, 1f)] public float defaultSFXVolume = 1f;

        [HideInInspector] public float masterVolume = 1f;
        [HideInInspector] public float musicVolume = 1f;
        [HideInInspector] public float sfxVolume = 1f;

        private List<AudioSource> sfxPool = new List<AudioSource>();
        private Coroutine musicFadeCoroutine;

        private const string MASTER_VOL_KEY = "Audio_MasterVolume";
        private const string MUSIC_VOL_KEY = "Audio_MusicVolume";
        private const string SFX_VOL_KEY = "Audio_SFXVolume";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadVolumeSettings();
            InitializePool();
        }

        private void LoadVolumeSettings()
        {
            float fallbackMaster = defaultMasterVolume > 0.001f ? defaultMasterVolume : 1f;
            float fallbackMusic = defaultMusicVolume > 0.001f ? defaultMusicVolume : 0.5f;
            float fallbackSFX = defaultSFXVolume > 0.001f ? defaultSFXVolume : 0.8f;

            masterVolume = PlayerPrefs.GetFloat(MASTER_VOL_KEY, fallbackMaster);
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, fallbackMusic);
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, fallbackSFX);
        }

        public void ResetToDefaultVolumes()
        {
            PlayerPrefs.DeleteKey(MASTER_VOL_KEY);
            PlayerPrefs.DeleteKey(MUSIC_VOL_KEY);
            PlayerPrefs.DeleteKey(SFX_VOL_KEY);

            masterVolume = defaultMasterVolume > 0.001f ? defaultMasterVolume : 1f;
            musicVolume = defaultMusicVolume > 0.001f ? defaultMusicVolume : 0.5f;
            sfxVolume = defaultSFXVolume > 0.001f ? defaultSFXVolume : 0.8f;

            UpdateMusicVolume();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MASTER_VOL_KEY, masterVolume);
            PlayerPrefs.Save();
            UpdateMusicVolume();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
            PlayerPrefs.Save();
            UpdateMusicVolume();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
            PlayerPrefs.Save();
        }

        public float GetEffectiveMusicVolume() => musicVolume * masterVolume;
        public float GetEffectiveSFXVolume() => sfxVolume * masterVolume;

        private void UpdateMusicVolume()
        {
            if (musicSource != null)
            {
                musicSource.volume = GetEffectiveMusicVolume();
            }
        }


        private void InitializePool()
        {
            // Auto-create or instantiate music source if it's a prefab
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            else if (!musicSource.gameObject.scene.IsValid())
            {
                // If it's a prefab, we must instantiate it to use it in the scene
                AudioSource instance = Instantiate(musicSource, transform);
                instance.name = "MusicSource_Instance";
                musicSource = instance;
            }

            musicSource.gameObject.SetActive(true);
            musicSource.enabled = true;

            // Safety check: Don't pool the music source or the system itself
            if (sfxSourcePrefab != null && (sfxSourcePrefab == musicSource || sfxSourcePrefab.gameObject == gameObject))
            {
                Debug.LogError("[AudioSystem] SfxSourcePrefab cannot be the same as MusicSource or the AudioSystem itself! Resetting to default.");
                sfxSourcePrefab = null;
            }

            // Ensure we have a prefab to pool
            if (sfxSourcePrefab == null)
            {
                GameObject prefabGO = new GameObject("DefaultSFXSource");
                prefabGO.transform.SetParent(transform);
                sfxSourcePrefab = prefabGO.AddComponent<AudioSource>();
                sfxSourcePrefab.playOnAwake = false;
                prefabGO.SetActive(false);
            }

            for (int i = 0; i < initialSfxPoolSize; i++)
            {
                CreateNewPoolSource();
            }
        }

        private AudioSource CreateNewPoolSource()
        {
            AudioSource newSource = Instantiate(sfxSourcePrefab, transform);
            newSource.gameObject.SetActive(false);
            sfxPool.Add(newSource);
            return newSource;
        }

        private AudioSource GetAvailableSfxSource()
        {
            foreach (var source in sfxPool)
            {
                if (source != null && !source.gameObject.activeInHierarchy)
                {
                    source.gameObject.SetActive(true);
                    source.enabled = true; // Ensure component is enabled
                    return source;
                }
            }

            // Expand pool if needed
            var newSource = CreateNewPoolSource();
            newSource.gameObject.SetActive(true);
            newSource.enabled = true;
            return newSource;
        }

        // SFX Playback
        public void PlaySFX(string soundName)
        {
            if (library == null) return;
            var group = library.GetSoundGroup(soundName);
            if (group == null)
            {
                Debug.LogWarning($"[AudioSystem] Sound '{soundName}' not found in library.");
                return;
            }

            AudioClip clip = group.GetRandomClip();
            if (clip == null) return;

            AudioSource source = GetAvailableSfxSource();

            float baseVol = group.volume + Random.Range(-group.volumeVariance, group.volumeVariance);
            float finalVolume = Mathf.Clamp01(baseVol * GetEffectiveSFXVolume());
            float finalPitch = group.pitch + Random.Range(-group.pitchVariance, group.pitchVariance);

            source.pitch = finalPitch;
            source.PlayOneShot(clip, finalVolume);
            
            float duration = clip.length / Mathf.Max(0.1f, Mathf.Abs(finalPitch));
            StartCoroutine(ReturnToPool(source, duration));
        }

        private IEnumerator ReturnToPool(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.Stop();
            source.gameObject.SetActive(false);
        }

        // Music Playback
        public void PlayMusic(string soundName, float fadeDuration = 1.0f)
        {
            if (library == null) return;
            var group = library.GetSoundGroup(soundName);
            if (group == null)
            {
                Debug.LogWarning($"[AudioSystem] Music '{soundName}' not found in library.");
                return;
            }

            AudioClip clip = group.GetRandomClip();
            if (clip != null)
            {
                PlayMusic(clip, fadeDuration);
            }
        }

        public void PlayMusic(AudioClip clip, float fadeDuration = 1.0f)
        {
            if (musicSource == null || clip == null) return;

            // Defensive: Always ensure musicSource is enabled and active before checking isPlaying
            musicSource.gameObject.SetActive(true);
            musicSource.enabled = true;

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(CrossFadeMusic(clip, fadeDuration));
        }

        private IEnumerator CrossFadeMusic(AudioClip newClip, float duration)
        {
            float startVolume = musicSource.volume;
            float targetVolume = GetEffectiveMusicVolume();

            // Fade Out
            if (musicSource.isPlaying)
            {
                for (float t = 0; t < duration; t += Time.deltaTime)
                {
                    musicSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
                    yield return null;
                }
            }

            musicSource.Stop();
            musicSource.clip = newClip;

            // CRITICAL: Ensure it is active AND enabled before Play()
            musicSource.gameObject.SetActive(true);
            musicSource.enabled = true;

            musicSource.Play();

            // Fade In
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, targetVolume, t / duration);
                yield return null;
            }
            musicSource.volume = targetVolume;
        }
    }
}

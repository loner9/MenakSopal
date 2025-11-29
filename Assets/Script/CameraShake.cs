using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Camera shake system that can be called from anywhere.
/// Works with Cinemachine Virtual Camera.
/// Usage: CameraShake.Instance.Shake(duration, intensity);
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Cinemachine Setup")]
    [Tooltip("The Cinemachine Virtual Camera to shake")]
    public CinemachineCamera virtualCamera;

    [Header("Default Shake Settings")]
    [Tooltip("Default shake duration in seconds")]
    public float defaultDuration = 0.3f;
    [Tooltip("Default shake intensity")]
    public float defaultIntensity = 3.0f;

    [Header("Shake Presets - Tuned for 2D Top-Down")]
    [Tooltip("For 2D cameras, intensity needs to be much higher than 3D")]
    public ShakePreset lightShake = new ShakePreset("Light", 0.2f, 2.0f);
    public ShakePreset mediumShake = new ShakePreset("Medium", 0.3f, 4.0f);
    public ShakePreset heavyShake = new ShakePreset("Heavy", 0.5f, 7.0f);
    public ShakePreset explosionShake = new ShakePreset("Explosion", 0.8f, 12.0f);

    [Header("Debug")]
    public bool enableDebugLogs = false;

    // Events
    /// <summary>
    /// Invoked when a shake starts
    /// </summary>
    public System.Action OnShakeStarted;

    /// <summary>
    /// Invoked when a shake completes (including fade out)
    /// </summary>
    public System.Action OnShakeCompleted;

    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine currentShakeCoroutine;
    private System.Action currentShakeCallback;

    #region Initialization

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitializeCinemachine();
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene changes
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[CameraShake] Scene loaded: {scene.name}, re-initializing camera...");

        // Clear current references
        virtualCamera = null;
        noise = null;

        // Re-initialize for new scene
        InitializeCinemachine();
    }

    private void InitializeCinemachine()
    {
        Debug.Log("[CameraShake] Starting initialization...");

        // Try to find virtual camera if not assigned
        if (virtualCamera == null)
        {
            Debug.Log("[CameraShake] Virtual camera not assigned, searching for CinemachineCamera in scene...");
            virtualCamera = FindObjectOfType<CinemachineCamera>();

            if (virtualCamera == null)
            {
                Debug.LogError("[CameraShake] ❌ No CinemachineCamera found in scene!");
                Debug.LogError("[CameraShake] Please do one of the following:");
                Debug.LogError("[CameraShake]   1. Assign a CinemachineCamera in the Inspector, OR");
                Debug.LogError("[CameraShake]   2. Add a CinemachineCamera to your scene");
                return;
            }
            else
            {
                Debug.Log($"[CameraShake] ✅ Auto-found CinemachineCamera: {virtualCamera.name}");
            }
        }
        else
        {
            Debug.Log($"[CameraShake] Using assigned virtual camera: {virtualCamera.name}");
        }

        // Get or add the noise component
        Debug.Log("[CameraShake] Looking for CinemachineBasicMultiChannelPerlin component...");
        noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogWarning("[CameraShake] CinemachineBasicMultiChannelPerlin not found. Adding it now...");
            noise = virtualCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();

            if (noise != null)
            {
                Debug.Log("[CameraShake] ✅ Successfully added CinemachineBasicMultiChannelPerlin");
            }
            else
            {
                Debug.LogError("[CameraShake] ❌ Failed to add CinemachineBasicMultiChannelPerlin!");
                Debug.LogError("[CameraShake] Manual setup required:");
                Debug.LogError("[CameraShake]   1. Select your CinemachineCamera in the hierarchy");
                Debug.LogError("[CameraShake]   2. Add Component → Cinemachine → CinemachineBasicMultiChannelPerlin");
                return;
            }
        }
        else
        {
            Debug.Log("[CameraShake] ✅ Found existing CinemachineBasicMultiChannelPerlin");
        }

        // CRITICAL: Assign a noise profile if one doesn't exist
        if (noise.NoiseProfile == null)
        {
            Debug.LogWarning("[CameraShake] No noise profile assigned. Loading default...");

            // Try to load Unity's built-in noise profiles
            var noiseProfile = Resources.Load<NoiseSettings>("CM_Handheld_tele2");

            if (noiseProfile == null)
            {
                // Fallback: try other built-in profiles
                noiseProfile = Resources.Load<NoiseSettings>("6D Shake");
            }

            if (noiseProfile != null)
            {
                noise.NoiseProfile = noiseProfile;
                Debug.Log($"[CameraShake] ✅ Assigned noise profile: {noiseProfile.name}");
            }
            else
            {
                Debug.LogError("[CameraShake] ❌ Could not find any noise profile!");
                Debug.LogError("[CameraShake] MANUAL FIX REQUIRED:");
                Debug.LogError("[CameraShake]   1. Select your CinemachineCamera in hierarchy");
                Debug.LogError("[CameraShake]   2. Find CinemachineBasicMultiChannelPerlin component");
                Debug.LogError("[CameraShake]   3. Assign a Noise Profile (use '6D Shake' or any other)");
                Debug.LogError("[CameraShake]   Noise profiles are usually in: Packages/Cinemachine/Runtime/Presets/Noise");
            }
        }
        else
        {
            Debug.Log($"[CameraShake] ✅ Noise profile already assigned: {noise.NoiseProfile.name}");
        }

        // Ensure noise component is ready
        if (noise != null)
        {
            // Ensure shake is off initially
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 1f;

            Debug.Log($"[CameraShake] ✅ Initialization complete!");
            Debug.Log($"[CameraShake] Amplitude: {noise.AmplitudeGain}, Frequency: {noise.FrequencyGain}");
        }
        else
        {
            Debug.LogError("[CameraShake] ❌ Initialization failed - noise component is still null!");
        }
    }

    #endregion

    #region Public Shake Methods

    /// <summary>
    /// Shake the camera with custom duration and intensity
    /// </summary>
    /// <param name="duration">How long to shake in seconds</param>
    /// <param name="intensity">Shake intensity (amplitude)</param>
    /// <param name="frequency">Shake frequency (speed). Default is 1.0f</param>
    /// <param name="onComplete">Optional callback to invoke when shake completes</param>
    public void Shake(float duration, float intensity, float frequency = 1.0f, System.Action onComplete = null)
    {
        // Lazy initialization fallback
        if (noise == null)
        {
            Debug.LogWarning("[CameraShake] Noise not initialized, attempting to initialize now...");
            InitializeCinemachine();

            if (noise == null)
            {
                Debug.LogError("[CameraShake] ❌ Cannot shake - noise component not initialized!");
                Debug.LogError("[CameraShake] Troubleshooting steps:");
                Debug.LogError("[CameraShake]   1. Make sure you have a CinemachineCamera in your scene");
                Debug.LogError("[CameraShake]   2. Assign it to the CameraShake component's 'Virtual Camera' field");
                Debug.LogError("[CameraShake]   3. Or manually add CinemachineBasicMultiChannelPerlin to your camera");

                // Invoke callback even if failed (so caller doesn't wait forever)
                onComplete?.Invoke();
                return;
            }
        }

        // Stop any existing shake
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        // Store callback
        currentShakeCallback = onComplete;

        currentShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity, frequency));

        if (enableDebugLogs)
        {
            Debug.Log($"[CameraShake] Shaking with duration: {duration}s, intensity: {intensity}, frequency: {frequency}");
        }
    }

    /// <summary>
    /// Shake the camera with default settings
    /// </summary>
    /// <param name="onComplete">Optional callback to invoke when shake completes</param>
    public void Shake(System.Action onComplete = null)
    {
        Shake(defaultDuration, defaultIntensity, 1.0f, onComplete);
    }

    /// <summary>
    /// Light camera shake (good for small impacts)
    /// </summary>
    /// <param name="onComplete">Optional callback to invoke when shake completes</param>
    public void ShakeLight(System.Action onComplete = null)
    {
        Shake(lightShake.duration, lightShake.intensity, 1.0f, onComplete);
    }

    /// <summary>
    /// Medium camera shake (good for player damage, hits)
    /// </summary>
    /// <param name="onComplete">Optional callback to invoke when shake completes</param>
    public void ShakeMedium(System.Action onComplete = null)
    {
        Shake(mediumShake.duration, mediumShake.intensity, 1.0f, onComplete);
    }

    /// <summary>
    /// Heavy camera shake (good for large impacts, boss attacks)
    /// </summary>
    /// <param name="onComplete">Optional callback to invoke when shake completes</param>
    public void ShakeHeavy(System.Action onComplete = null)
    {
        Shake(heavyShake.duration, heavyShake.intensity, 1.0f, onComplete);
    }

    /// <summary>
    /// Explosion shake (good for explosions, building collapses)
    /// </summary>
    /// <param name="onComplete">Optional callback to invoke when shake completes</param>
    public void ShakeExplosion(System.Action onComplete = null)
    {
        Shake(explosionShake.duration, explosionShake.intensity, 1.0f, onComplete);
    }

    /// <summary>
    /// Stop any ongoing shake immediately
    /// </summary>
    public void StopShake()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            currentShakeCoroutine = null;
        }

        if (noise != null)
        {
            // Set amplitude to 0 to stop the shake
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 1f;
        }

        // Invoke callbacks even when manually stopped
        try
        {
            currentShakeCallback?.Invoke();
            OnShakeCompleted?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CameraShake] Error in shake completion callback: {e.Message}");
        }
        finally
        {
            currentShakeCallback = null;
        }

        if (enableDebugLogs)
        {
            Debug.Log("[CameraShake] Shake stopped manually, amplitude reset to 0");
        }
    }

    #endregion

    #region Coroutines

    private IEnumerator ShakeCoroutine(float duration, float intensity, float frequency)
    {
        if (enableDebugLogs)
            Debug.Log($"[CameraShake] Coroutine started - Duration: {duration}s, Intensity: {intensity}");

        // Invoke started event
        try
        {
            OnShakeStarted?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CameraShake] Error in OnShakeStarted event: {e.Message}");
        }

        // Set shake parameters
        noise.AmplitudeGain = intensity;
        noise.FrequencyGain = frequency;

        if (enableDebugLogs)
            Debug.Log($"[CameraShake] Amplitude set to {intensity}, waiting {duration}s...");

        // Wait for duration
        yield return new WaitForSeconds(duration);

        if (enableDebugLogs)
            Debug.Log("[CameraShake] Duration complete, fading out...");

        // Smoothly fade out the shake
        float fadeTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            // Fade to 0, not to originalAmplitude (which might not be 0)
            noise.AmplitudeGain = Mathf.Lerp(intensity, 0f, t);
            yield return null;
        }

        // Ensure shake is completely stopped (amplitude = 0)
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 1f; // Reset to default frequency

        if (enableDebugLogs)
            Debug.Log("[CameraShake] Shake stopped, amplitude reset to 0");

        // Invoke completion callbacks
        try
        {
            currentShakeCallback?.Invoke();
            OnShakeCompleted?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CameraShake] Error in shake completion callback: {e.Message}");
        }
        finally
        {
            currentShakeCallback = null;
        }

        currentShakeCoroutine = null;
    }

    #endregion

    #region Context Menu Debug

    [ContextMenu("Test Shake - Default")]
    private void TestShakeDefault()
    {
        Shake();
    }

    [ContextMenu("Test Shake - Light")]
    private void TestShakeLight()
    {
        ShakeLight();
    }

    [ContextMenu("Test Shake - Medium")]
    private void TestShakeMedium()
    {
        ShakeMedium();
    }

    [ContextMenu("Test Shake - Heavy")]
    private void TestShakeHeavy()
    {
        ShakeHeavy();
    }

    [ContextMenu("Test Shake - Explosion")]
    private void TestShakeExplosion()
    {
        ShakeExplosion();
    }

    #endregion
}

/// <summary>
/// Preset configuration for different shake types
/// </summary>
[System.Serializable]
public class ShakePreset
{
    public string name;
    public float duration;
    public float intensity;

    public ShakePreset(string name, float duration, float intensity)
    {
        this.name = name;
        this.duration = duration;
        this.intensity = intensity;
    }
}

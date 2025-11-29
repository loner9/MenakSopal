using UnityEngine;

/// <summary>
/// Controls 2D fog shader properties for top-down perspective.
/// Attach this to a sprite/quad with the AnimatedFog shader.
/// Can be called from anywhere to control fog intensity and appearance.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FogController : MonoBehaviour
{
    public static FogController Instance { get; private set; }

    [Header("Fog Appearance")]
    [SerializeField, Range(0f, 1f)]
    [Tooltip("Overall fog density (0 = invisible, 1 = opaque)")]
    private float fogDensity = 0.5f;

    [SerializeField]
    [Tooltip("Fog color (use grays/blues for eerie effect)")]
    private Color fogColor = new Color(0.7f, 0.7f, 0.8f, 1f);

    [Header("Animation Settings")]
    [SerializeField]
    [Tooltip("How fast the fog moves/scrolls")]
    private Vector2 scrollSpeed = new Vector2(0.02f, 0.01f);

    [SerializeField, Range(0.1f, 5f)]
    [Tooltip("Scale of fog noise pattern (lower = larger fog clouds)")]
    private float noiseScale = 1.5f;

    [Header("Layer Settings")]
    [SerializeField]
    [Tooltip("Use multiple layers for more depth (slower but more atmospheric)")]
    private bool useMultipleLayers = true;

    [SerializeField, Range(1, 3)]
    [Tooltip("Number of fog layers to blend (more = more depth)")]
    private int layerCount = 2;

    [Header("Debug")]
    [SerializeField]
    private bool enableDebugLogs = false;

    private SpriteRenderer spriteRenderer;
    private Material fogMaterial;
    private float currentDensity;
    private bool isFading = false;

    // Shader property IDs (cached for performance)
    private static readonly int DensityID = Shader.PropertyToID("_Density");
    private static readonly int ColorID = Shader.PropertyToID("_FogColor");
    private static readonly int ScrollSpeedID = Shader.PropertyToID("_ScrollSpeed");
    private static readonly int NoiseScaleID = Shader.PropertyToID("_NoiseScale");
    private static readonly int LayerCountID = Shader.PropertyToID("_LayerCount");
    private static readonly int TimeOffsetID = Shader.PropertyToID("_TimeOffset");

    #region Initialization

    private void Awake()
    {
        // Singleton pattern (optional - remove if you want multiple fog areas)
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[FogController] Multiple FogController instances found. Singleton behavior disabled.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        // Create material instance to avoid affecting other objects
        if (spriteRenderer.material != null)
        {
            fogMaterial = new Material(spriteRenderer.material);
            spriteRenderer.material = fogMaterial;
        }
        else
        {
            Debug.LogError("[FogController] No material assigned to SpriteRenderer! Please assign the AnimatedFog material.");
        }
    }

    private void Start()
    {
        // Initialize shader properties
        UpdateShaderProperties();
        currentDensity = fogDensity;
    }

    private void OnDestroy()
    {
        // Clean up material instance
        if (fogMaterial != null)
        {
            Destroy(fogMaterial);
        }
    }

    #endregion

    #region Update

    private void Update()
    {
        // Update time-based animation
        if (fogMaterial != null)
        {
            // You can add additional time-based effects here if needed
        }
    }

    #endregion

    #region Public Control Methods

    /// <summary>
    /// Set fog density instantly
    /// </summary>
    public void SetDensity(float density)
    {
        fogDensity = Mathf.Clamp01(density);
        currentDensity = fogDensity;
        UpdateDensity();

        if (enableDebugLogs)
            Debug.Log($"[FogController] Density set to {fogDensity}");
    }

    /// <summary>
    /// Fade fog in over time
    /// </summary>
    public void FadeIn(float targetDensity, float duration)
    {
        if (!isFading)
        {
            StartCoroutine(FadeCoroutine(currentDensity, targetDensity, duration));
        }
    }

    /// <summary>
    /// Fade fog out over time
    /// </summary>
    public void FadeOut(float duration)
    {
        if (!isFading)
        {
            StartCoroutine(FadeCoroutine(currentDensity, 0f, duration));
        }
    }

    /// <summary>
    /// Set fog color
    /// </summary>
    public void SetColor(Color color)
    {
        fogColor = color;
        UpdateColor();

        if (enableDebugLogs)
            Debug.Log($"[FogController] Color set to {fogColor}");
    }

    /// <summary>
    /// Set fog scroll speed
    /// </summary>
    public void SetScrollSpeed(Vector2 speed)
    {
        scrollSpeed = speed;
        UpdateScrollSpeed();

        if (enableDebugLogs)
            Debug.Log($"[FogController] Scroll speed set to {scrollSpeed}");
    }

    /// <summary>
    /// Set fog noise scale (size of fog clouds)
    /// </summary>
    public void SetNoiseScale(float scale)
    {
        noiseScale = Mathf.Clamp(scale, 0.1f, 5f);
        UpdateNoiseScale();

        if (enableDebugLogs)
            Debug.Log($"[FogController] Noise scale set to {noiseScale}");
    }

    /// <summary>
    /// Enable/disable the fog
    /// </summary>
    public void SetActive(bool active)
    {
        spriteRenderer.enabled = active;

        if (enableDebugLogs)
            Debug.Log($"[FogController] Fog {(active ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Reset fog to default settings
    /// </summary>
    public void ResetToDefault()
    {
        fogDensity = 0.5f;
        fogColor = new Color(0.7f, 0.7f, 0.8f, 1f);
        scrollSpeed = new Vector2(0.02f, 0.01f);
        noiseScale = 1.5f;
        UpdateShaderProperties();

        if (enableDebugLogs)
            Debug.Log("[FogController] Reset to default settings");
    }

    #endregion

    #region Shader Property Updates

    private void UpdateShaderProperties()
    {
        UpdateDensity();
        UpdateColor();
        UpdateScrollSpeed();
        UpdateNoiseScale();
        UpdateLayerCount();
    }

    private void UpdateDensity()
    {
        if (fogMaterial != null)
        {
            fogMaterial.SetFloat(DensityID, fogDensity);
        }
    }

    private void UpdateColor()
    {
        if (fogMaterial != null)
        {
            fogMaterial.SetColor(ColorID, fogColor);
        }
    }

    private void UpdateScrollSpeed()
    {
        if (fogMaterial != null)
        {
            fogMaterial.SetVector(ScrollSpeedID, scrollSpeed);
        }
    }

    private void UpdateNoiseScale()
    {
        if (fogMaterial != null)
        {
            fogMaterial.SetFloat(NoiseScaleID, noiseScale);
        }
    }

    private void UpdateLayerCount()
    {
        if (fogMaterial != null && useMultipleLayers)
        {
            fogMaterial.SetInt(LayerCountID, layerCount);
        }
    }

    #endregion

    #region Coroutines

    private System.Collections.IEnumerator FadeCoroutine(float startDensity, float targetDensity, float duration)
    {
        isFading = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentDensity = Mathf.Lerp(startDensity, targetDensity, t);
            fogDensity = currentDensity;
            UpdateDensity();
            yield return null;
        }

        currentDensity = targetDensity;
        fogDensity = targetDensity;
        UpdateDensity();
        isFading = false;

        if (enableDebugLogs)
            Debug.Log($"[FogController] Fade complete. Final density: {fogDensity}");
    }

    #endregion

    #region Inspector Validation

    private void OnValidate()
    {
        // Update shader properties when values change in inspector
        if (Application.isPlaying && fogMaterial != null)
        {
            UpdateShaderProperties();
        }
    }

    #endregion

    #region Context Menu Debug

    [ContextMenu("Fade In (2s)")]
    private void TestFadeIn()
    {
        FadeIn(0.8f, 2f);
    }

    [ContextMenu("Fade Out (2s)")]
    private void TestFadeOut()
    {
        FadeOut(2f);
    }

    [ContextMenu("Set Eerie Blue")]
    private void SetEerieBlue()
    {
        SetColor(new Color(0.6f, 0.65f, 0.8f, 1f));
    }

    [ContextMenu("Set Dark Gray")]
    private void SetDarkGray()
    {
        SetColor(new Color(0.3f, 0.3f, 0.35f, 1f));
    }

    #endregion
}

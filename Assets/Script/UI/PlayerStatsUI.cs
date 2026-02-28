using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Links the Health and Stamina Sliders to PlayerStats values each frame.
/// Each slider can be independently shown or hidden based on a story flag:
///   - Flag added   → slider fades IN
///   - Flag removed → slider fades OUT
///
/// Setup:
///   1. Assign the Slider references in the Inspector.
///   2. Add a CanvasGroup component to each Slider GameObject (same GO as the Slider).
///   3. Assign those CanvasGroups to the matching fields below.
///   4. Fill in the flag name strings. Leave a flag field empty to keep that slider always visible.
///   5. Assign your PlayerStats ScriptableObject (or leave blank — it will try to find it via Player).
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  SLIDERS
    // ─────────────────────────────────────────────
    [Header("Sliders")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    // ─────────────────────────────────────────────
    //  PLAYER STATS SOURCE
    // ─────────────────────────────────────────────
    [Header("Stats Source")]
    [Tooltip("Drag your PlayerStats ScriptableObject here. " +
             "If left empty the script will find it via the Player component.")]
    [SerializeField] private PlayerStats playerStats;

    // ─────────────────────────────────────────────
    //  CANVAS GROUPS  (one per slider GameObject)
    // ─────────────────────────────────────────────
    [Header("Canvas Groups (for fade)")]
    [Tooltip("CanvasGroup on the same GameObject as the Health Slider.")]
    [SerializeField] private CanvasGroup healthCanvasGroup;
    [Tooltip("CanvasGroup on the same GameObject as the Stamina Slider.")]
    [SerializeField] private CanvasGroup staminaCanvasGroup;

    // ─────────────────────────────────────────────
    //  FLAG SETTINGS
    // ─────────────────────────────────────────────
    [Header("Visibility Flags")]
    [Tooltip("Health slider shows when this flag exists. Leave empty = always visible.")]
    [SerializeField] private string healthVisibilityFlag = "";
    [Tooltip("Stamina slider shows when this flag exists. Leave empty = always visible.")]
    [SerializeField] private string staminaVisibilityFlag = "";

    // ─────────────────────────────────────────────
    //  FADE SETTINGS
    // ─────────────────────────────────────────────
    [Header("Fade Settings")]
    [Tooltip("How fast the sliders fade in/out (higher = faster).")]
    [SerializeField] private float fadeSpeed = 4f;

    // ─────────────────────────────────────────────
    //  RUNTIME STATE
    // ─────────────────────────────────────────────
    private float healthTargetAlpha   = 1f;
    private float staminaTargetAlpha  = 1f;

    // ─────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────
    private void Awake()
    {
        // Auto-find PlayerStats via Player component if not assigned
        if (playerStats == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                playerStats = player.Stats;
                Debug.Log("[PlayerStatsUI] PlayerStats located via Player component.");
            }
            else
            {
                Debug.LogWarning("[PlayerStatsUI] No PlayerStats assigned and no Player found in scene!");
            }
        }

        // Validate sliders
        if (healthSlider  == null) Debug.LogWarning("[PlayerStatsUI] Health Slider is not assigned!");
        if (staminaSlider == null) Debug.LogWarning("[PlayerStatsUI] Stamina Slider is not assigned!");

        // Auto-add CanvasGroups if the user forgot to assign them
        healthCanvasGroup  = EnsureCanvasGroup(healthSlider,  healthCanvasGroup,  "Health");
        staminaCanvasGroup = EnsureCanvasGroup(staminaSlider, staminaCanvasGroup, "Stamina");

        // Set initial slider ranges to match PlayerStats
        if (playerStats != null)
        {
            if (healthSlider  != null) { healthSlider.minValue  = 0; healthSlider.maxValue  = playerStats.maxHealth;  }
            if (staminaSlider != null) { staminaSlider.minValue = 0; staminaSlider.maxValue = playerStats.maxStamina; }
        }
    }

    private void Start()
    {
        SetupFlagWatchers();
    }

    private void Update()
    {
        UpdateSliderValues();
        UpdateFades();
    }

    // ─────────────────────────────────────────────
    //  FLAG WATCHERS
    // ─────────────────────────────────────────────
    private void SetupFlagWatchers()
    {
        // ── Health slider visibility ──
        if (!string.IsNullOrEmpty(healthVisibilityFlag))
        {
            // Initially hidden until the flag arrives
            SetAlphaImmediate(healthCanvasGroup, 0f);
            healthTargetAlpha = 0f;

            // WatchFlag fires (true) when added, (false) when removed.
            // triggerIfExists = true → if the flag already exists at Start(), fire immediately.
            FlagMonitorSystem.WatchFlag(healthVisibilityFlag, (isAdded) =>
            {
                healthTargetAlpha = isAdded ? 1f : 0f;
                Debug.Log($"[PlayerStatsUI] Health slider → {(isAdded ? "SHOW" : "HIDE")} (flag: {healthVisibilityFlag})");
            }, triggerIfExists: true);
        }
        else
        {
            // No flag configured → always visible
            SetAlphaImmediate(healthCanvasGroup, 1f);
            healthTargetAlpha = 1f;
        }

        // ── Stamina slider visibility ──
        if (!string.IsNullOrEmpty(staminaVisibilityFlag))
        {
            SetAlphaImmediate(staminaCanvasGroup, 0f);
            staminaTargetAlpha = 0f;

            FlagMonitorSystem.WatchFlag(staminaVisibilityFlag, (isAdded) =>
            {
                staminaTargetAlpha = isAdded ? 1f : 0f;
                Debug.Log($"[PlayerStatsUI] Stamina slider → {(isAdded ? "SHOW" : "HIDE")} (flag: {staminaVisibilityFlag})");
            }, triggerIfExists: true);
        }
        else
        {
            SetAlphaImmediate(staminaCanvasGroup, 1f);
            staminaTargetAlpha = 1f;
        }
    }

    // ─────────────────────────────────────────────
    //  VALUE SYNC  (polls every frame — stamina changes every frame anyway)
    // ─────────────────────────────────────────────
    private void UpdateSliderValues()
    {
        if (playerStats == null) return;

        if (healthSlider != null)
        {
            healthSlider.value = playerStats.health;
        }

        if (staminaSlider != null)
        {
            staminaSlider.value = playerStats.stamina;
        }
    }

    // ─────────────────────────────────────────────
    //  SMOOTH FADE
    // ─────────────────────────────────────────────
    private void UpdateFades()
    {
        FadeCanvasGroup(healthCanvasGroup,  healthTargetAlpha);
        FadeCanvasGroup(staminaCanvasGroup, staminaTargetAlpha);
    }

    private void FadeCanvasGroup(CanvasGroup cg, float targetAlpha)
    {
        if (cg == null) return;

        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // When fully invisible: block raycasts and disable interactivity to be safe
        bool isVisible = cg.alpha > 0.01f;
        cg.interactable    = isVisible;
        cg.blocksRaycasts  = isVisible;
    }

    // ─────────────────────────────────────────────
    //  PUBLIC API  (call these from other scripts
    //              if you need to override visibility manually)
    // ─────────────────────────────────────────────

    /// <summary>Show the health bar instantly (no fade).</summary>
    public void ShowHealthInstant()  => SetAlphaImmediate(healthCanvasGroup,  1f);

    /// <summary>Hide the health bar instantly (no fade).</summary>
    public void HideHealthInstant()  => SetAlphaImmediate(healthCanvasGroup,  0f);

    /// <summary>Show the stamina bar instantly (no fade).</summary>
    public void ShowStaminaInstant() => SetAlphaImmediate(staminaCanvasGroup, 1f);

    /// <summary>Hide the stamina bar instantly (no fade).</summary>
    public void HideStaminaInstant() => SetAlphaImmediate(staminaCanvasGroup, 0f);

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────
    private void SetAlphaImmediate(CanvasGroup cg, float alpha)
    {
        if (cg == null) return;
        cg.alpha           = alpha;
        cg.interactable    = alpha > 0.01f;
        cg.blocksRaycasts  = alpha > 0.01f;
    }

    /// <summary>
    /// If the Slider has no CanvasGroup assigned, try to grab or add one automatically.
    /// </summary>
    private CanvasGroup EnsureCanvasGroup(Slider slider, CanvasGroup existing, string label)
    {
        if (existing != null) return existing;
        if (slider   == null) return null;

        CanvasGroup cg = slider.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = slider.gameObject.AddComponent<CanvasGroup>();
            Debug.Log($"[PlayerStatsUI] Auto-added CanvasGroup to {label} Slider GameObject.");
        }
        return cg;
    }
}

using System.Collections.Generic;
using MenakSopal.Cutscenes;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the minimap HUD panel:
///   • Shows/hides the whole panel based on a story flag (optional).
///   • Draws blips (small icons) on the minimap for registered world-space targets
///     such as enemies, NPCs, or quest markers.
///   • Rotates the map or keeps it north-up — configurable in the Inspector.
///
/// HOW IT WORKS:
///   Each "blip" is an Image child of the minimap RawImage.
///   Every frame we convert each tracked world position to a minimap UV coordinate,
///   then set the blip's anchored position inside the RawImage's rect.
///
/// Setup:
///   1. Add this component to the parent GameObject that contains your minimap.
///   2. Assign minimapPanel (the root panel CanvasGroup for fade/show-hide).
///   3. Assign minimapImage (the RawImage displaying the RenderTexture).
///   4. Assign minimapCamera (your MinimapCamera component).
///   5. Assign blipPrefab (a small UI Image, e.g. 8×8 circle).
///   6. (Optional) Fill in visibilityFlag to gate the minimap behind a story flag.
///   7. Call MinimapUI.Instance.RegisterBlip / UnregisterBlip from other scripts.
/// </summary>
public class MinimapUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  SINGLETON
    // ─────────────────────────────────────────────
    public static MinimapUI Instance { get; private set; }

    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────
    [Header("References")]
    [Tooltip("CanvasGroup on the whole minimap panel (for fade in/out).")]
    [SerializeField] private CanvasGroup minimapPanel;

    [Tooltip("The RawImage that shows the minimap RenderTexture.")]
    [SerializeField] private RawImage minimapImage;

    [Tooltip("The MinimapCamera component (needs its orthographicSize).")]
    [SerializeField] private MinimapCamera minimapCamera;

    [Header("Blips")]
    [Tooltip("Prefab for map blips — should be a small Image (UI).")]
    [SerializeField] private GameObject blipPrefab;

    [Header("Visibility Flag")]
    [Tooltip("Minimap shows when this flag is active. Leave empty = always visible.")]
    [SerializeField] private string visibilityFlag = "";

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 4f;

    [Header("Map Rotation")]
    [Tooltip("If true the map rotates so the player always points up. " +
             "If false the map is always north-up.")]
    [SerializeField] private bool rotateWithPlayer = false;

    // ─────────────────────────────────────────────
    //  RUNTIME
    // ─────────────────────────────────────────────
    private float targetAlpha = 1f;

    // Maps a tracked transform → its blip Image on the minimap
    private Dictionary<Transform, Image> blips = new Dictionary<Transform, Image>();

    // Cached reference to the player transform (for rotation mode)
    private Transform playerTransform;

    // The orthographic camera component used for position mapping
    private Camera minimapCam;

    // ─────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache camera
        if (minimapCamera != null)
            minimapCam = minimapCamera.GetComponent<Camera>();

        // Auto-find player
        Player p = FindObjectOfType<Player>();
        if (p != null) playerTransform = p.transform;
    }

    private void Start()
    {
        SetupVisibilityFlag();
    }

    private void OnEnable()
    {
        CutsceneEvents.OnCutsceneStarted += OnCutsceneStarted;
        CutsceneEvents.OnCutsceneCompleted += OnCutsceneEnded;
        CutsceneEvents.OnCutsceneSkipped += OnCutsceneEnded;
    }

    private void OnDisable()
    {
        CutsceneEvents.OnCutsceneStarted -= OnCutsceneStarted;
        CutsceneEvents.OnCutsceneCompleted -= OnCutsceneEnded;
        CutsceneEvents.OnCutsceneSkipped -= OnCutsceneEnded;
    }

    // Called when any cutscene begins — hide the minimap
    private void OnCutsceneStarted(CutsceneData _)
    {
        targetAlpha = 0f;
        Debug.Log("[MinimapUI] Hiding minimap for cutscene.");
    }

    // Called when a cutscene ends (complete or skipped) — restore the minimap
    private void OnCutsceneEnded(CutsceneData _)
    {
        // Only restore if the flag-based system also wants it visible
        bool flagWantsVisible = string.IsNullOrEmpty(visibilityFlag)
                                || FlagMonitorSystem.HasFlag(visibilityFlag);
        targetAlpha = flagWantsVisible ? 1f : 0f;
        Debug.Log($"[MinimapUI] Restoring minimap after cutscene (visible: {flagWantsVisible}).");
    }

    private void LateUpdate()
    {
        UpdateBlips();
        UpdateFade();
        UpdateRotation();
    }

    // ─────────────────────────────────────────────
    //  FLAG-BASED VISIBILITY
    // ─────────────────────────────────────────────
    private void SetupVisibilityFlag()
    {
        if (!string.IsNullOrEmpty(visibilityFlag))
        {
            // Start hidden
            SetAlphaImmediate(0f);
            targetAlpha = 0f;

            FlagMonitorSystem.WatchFlag(visibilityFlag, (isAdded) =>
            {
                targetAlpha = isAdded ? 1f : 0f;
                Debug.Log($"[MinimapUI] Minimap → {(isAdded ? "SHOW" : "HIDE")} (flag: {visibilityFlag})");
            }, triggerIfExists: true);
        }
        else
        {
            SetAlphaImmediate(1f);
            targetAlpha = 1f;
        }
    }

    // ─────────────────────────────────────────────
    //  BLIP MANAGEMENT
    // ─────────────────────────────────────────────

    /// <summary>
    /// Register a world-space transform to appear as a blip on the minimap.
    /// </summary>
    /// <param name="worldTarget">The transform to track.</param>
    /// <param name="blipColor">Color to tint the blip image.</param>
    public void RegisterBlip(Transform worldTarget, Color blipColor)
    {
        if (worldTarget == null || blipPrefab == null || minimapImage == null) return;
        if (blips.ContainsKey(worldTarget)) return; // already registered

        GameObject blipGO = Instantiate(blipPrefab, minimapImage.transform);
        Image blipImage = blipGO.GetComponent<Image>();
        if (blipImage != null) blipImage.color = blipColor;

        blips[worldTarget] = blipImage;
    }

    /// <summary>
    /// Remove a previously registered blip.
    /// </summary>
    public void UnregisterBlip(Transform worldTarget)
    {
        if (worldTarget == null) return;
        if (!blips.ContainsKey(worldTarget)) return;

        Image blipImage = blips[worldTarget];
        if (blipImage != null) Destroy(blipImage.gameObject);

        blips.Remove(worldTarget);
    }

    // ─────────────────────────────────────────────
    //  BLIP POSITIONING
    // ─────────────────────────────────────────────
    private void UpdateBlips()
    {
        if (minimapCam == null || minimapImage == null) return;

        Rect rect = minimapImage.rectTransform.rect;

        // Build a list of stale entries to remove (destroyed objects)
        List<Transform> toRemove = null;

        foreach (var kvp in blips)
        {
            Transform tracked = kvp.Key;
            Image blip = kvp.Value;

            // Clean up if the tracked object was destroyed
            if (tracked == null || blip == null)
            {
                if (toRemove == null) toRemove = new List<Transform>();
                toRemove.Add(tracked);
                continue;
            }

            // Convert world position → viewport position (0..1, 0..1)
            Vector3 viewportPos = minimapCam.WorldToViewportPoint(tracked.position);

            // Map viewport [0,1] to the RawImage rect (centred at 0,0 in anchor space)
            float x = (viewportPos.x - 0.5f) * rect.width;
            float y = (viewportPos.y - 0.5f) * rect.height;

            blip.rectTransform.anchoredPosition = new Vector2(x, y);

            // Hide blip when target is behind the camera or outside view
            bool inView = viewportPos.z > 0f
                          && viewportPos.x >= 0f && viewportPos.x <= 1f
                          && viewportPos.y >= 0f && viewportPos.y <= 1f;
            blip.gameObject.SetActive(inView);
        }

        // Remove destroyed entries
        if (toRemove != null)
        {
            foreach (var t in toRemove)
                blips.Remove(t);
        }
    }

    // ─────────────────────────────────────────────
    //  MAP ROTATION  (player-up mode)
    // ─────────────────────────────────────────────
    private void UpdateRotation()
    {
        if (!rotateWithPlayer || minimapImage == null || playerTransform == null) return;

        // We read the Rigidbody2D velocity as a facing proxy, but a simpler
        // approach is to track lastCardinalDirection. For now we use the
        // player's facing from its scale (flip) or just keep north-up.
        // Rotate the RawImage opposite to player angle so the player blip points up.
        // This requires the player to have a rotation — for top-down 2D most
        // players don't rotate, so this is left as an extension point.
    }

    // ─────────────────────────────────────────────
    //  SMOOTH FADE
    // ─────────────────────────────────────────────
    private void UpdateFade()
    {
        if (minimapPanel == null) return;

        minimapPanel.alpha = Mathf.MoveTowards(
            minimapPanel.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        bool visible = minimapPanel.alpha > 0.01f;
        minimapPanel.interactable = visible;
        minimapPanel.blocksRaycasts = visible;
    }

    private void SetAlphaImmediate(float alpha)
    {
        if (minimapPanel == null) return;
        minimapPanel.alpha = alpha;
        minimapPanel.interactable = alpha > 0.01f;
        minimapPanel.blocksRaycasts = alpha > 0.01f;
    }

    // ─────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────

    /// <summary>Show the minimap panel instantly.</summary>
    public void ShowInstant() => SetAlphaImmediate(1f);

    /// <summary>Hide the minimap panel instantly.</summary>
    public void HideInstant() => SetAlphaImmediate(0f);
}

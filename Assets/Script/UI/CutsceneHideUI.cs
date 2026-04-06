using MenakSopal.Cutscenes;
using UnityEngine;

/// <summary>
/// Attach this to any GameObject in your gameplay Canvas.
/// Drag the UI GameObjects you want hidden during cutscenes into the
/// 'elementsToHide' list. They will be deactivated when a cutscene starts
/// and re-activated when it ends or is skipped.
///
/// Setup:
///   1. Add this component to a persistent UI root (e.g. your HUD panel).
///   2. Drag any number of UI buttons or panels into the "Elements To Hide" list.
///   3. Done — no further configuration needed.
/// </summary>
public class CutsceneHideUI : MonoBehaviour
{
    [Header("UI Elements to Hide During Cutscenes")]
    [Tooltip("Drag the buttons, panels, or any GameObjects you want hidden during cutscenes.")]
    [SerializeField] private GameObject[] elementsToHide;

    // ─────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────

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

    // ─────────────────────────────────────────────
    //  EVENT HANDLERS
    // ─────────────────────────────────────────────

    private void OnCutsceneStarted(CutsceneData _) => SetVisibility(false);

    private void OnCutsceneEnded(CutsceneData _) => SetVisibility(true);

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────

    private void SetVisibility(bool visible)
    {
        if (elementsToHide == null) return;

        foreach (GameObject element in elementsToHide)
        {
            if (element != null)
                element.SetActive(visible);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Simulate Cutscene Start")]
    private void DebugHide() => SetVisibility(false);

    [ContextMenu("Debug: Simulate Cutscene End")]
    private void DebugShow() => SetVisibility(true);
#endif
}

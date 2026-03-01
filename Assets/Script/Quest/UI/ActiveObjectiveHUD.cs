using MenakSopal.Cutscenes;
using TMPro;
using UnityEngine;

/// <summary>
/// Gameplay HUD widget that shows the current active objective so the player
/// doesn't have to open the quest journal.
///
/// What it does:
///   • Finds the first active quest that still has at least one pending objective.
///   • Displays that objective's text (with "3/5" progress where relevant).
///   • Refreshes automatically via QuestEvents — no per-frame polling.
///   • Auto-hides during cutscenes via CutsceneEvents.
///   • Optionally fades in/out behind a story flag (same pattern as PlayerStatsUI).
///
/// Setup:
///   1. Add this component to a UI GameObject inside your gameplay Canvas.
///   2. Assign questTitleText (optional) and objectiveText in the Inspector.
///   3. Add a CanvasGroup on the same GameObject and assign it to 'canvasGroup'.
///   4. Fill 'visibilityFlag' if you want the widget to be hidden until a flag fires.
///      Leave it empty to keep the widget always visible.
/// </summary>
public class ActiveObjectiveHUD : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────

    [Header("UI References")]
    [Tooltip("(Optional) Displays the quest title above the objective.")]
    [SerializeField] private TextMeshProUGUI questTitleText;

    [Tooltip("Displays the current objective text (with progress if applicable).")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Canvas Group (for fade)")]
    [Tooltip("CanvasGroup on this GameObject — used for fade in/out.")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visibility Flag")]
    [Tooltip("Widget shows only while this flag is active. Leave empty = always visible.")]
    [SerializeField] private string visibilityFlag = "";

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 4f;

    [Header("Fallback Text")]
    [Tooltip("Shown when there are no active objectives.")]
    [SerializeField] private string noObjectiveText = "";

    // ─────────────────────────────────────────────
    //  RUNTIME STATE
    // ─────────────────────────────────────────────

    private float targetAlpha = 1f;
    private bool flagAllowsShow = true;  // true when no flag required OR flag is present
    private bool isCutscenePlaying = false;

    // ─────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────

    private void Awake()
    {
        // Auto-add a CanvasGroup if the user forgot to assign one
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                Debug.Log("[ActiveObjectiveHUD] Auto-added CanvasGroup.");
            }
        }

        if (objectiveText == null)
            Debug.LogWarning("[ActiveObjectiveHUD] objectiveText is not assigned!");
    }

    private void Start()
    {
        SetupFlagWatcher();
        SubscribeToQuestEvents();
        SubscribeToCutsceneEvents();

        // Show whatever is current right now
        RefreshDisplay();
    }

    private void OnDestroy()
    {
        UnsubscribeFromQuestEvents();
        UnsubscribeFromCutsceneEvents();
    }

    private void Update()
    {
        FadeCanvasGroup();
    }

    // ─────────────────────────────────────────────
    //  FLAG WATCHER
    // ─────────────────────────────────────────────

    private void SetupFlagWatcher()
    {
        if (string.IsNullOrEmpty(visibilityFlag))
        {
            // No flag configured → always visible
            flagAllowsShow = true;
            SetAlphaImmediate(1f);
        }
        else
        {
            // Start hidden; flag will reveal the widget
            flagAllowsShow = false;
            SetAlphaImmediate(0f);

            FlagMonitorSystem.WatchFlag(visibilityFlag, (isAdded) =>
            {
                flagAllowsShow = isAdded;
                UpdateTargetAlpha();
                Debug.Log($"[ActiveObjectiveHUD] Visibility flag '{visibilityFlag}' → {(isAdded ? "SHOW" : "HIDE")}");
            }, triggerIfExists: true);
        }
    }

    // ─────────────────────────────────────────────
    //  QUEST EVENT SUBSCRIPTIONS
    // ─────────────────────────────────────────────

    private void SubscribeToQuestEvents()
    {
        QuestEvents.OnQuestStarted += OnQuestChanged;
        QuestEvents.OnQuestCompleted += OnQuestChanged;
        QuestEvents.OnQuestFailed += OnQuestChanged;
        QuestEvents.OnQuestAbandoned += OnQuestChanged;
        QuestEvents.OnObjectiveCompleted += OnObjectiveChanged;
        QuestEvents.OnObjectiveProgressUpdated += OnObjectiveProgressChanged;
    }

    private void UnsubscribeFromQuestEvents()
    {
        QuestEvents.OnQuestStarted -= OnQuestChanged;
        QuestEvents.OnQuestCompleted -= OnQuestChanged;
        QuestEvents.OnQuestFailed -= OnQuestChanged;
        QuestEvents.OnQuestAbandoned -= OnQuestChanged;
        QuestEvents.OnObjectiveCompleted -= OnObjectiveChanged;
        QuestEvents.OnObjectiveProgressUpdated -= OnObjectiveProgressChanged;
    }

    // ─────────────────────────────────────────────
    //  CUTSCENE EVENT SUBSCRIPTIONS
    // ─────────────────────────────────────────────

    private void SubscribeToCutsceneEvents()
    {
        CutsceneEvents.OnCutsceneStarted += OnCutsceneStarted;
        CutsceneEvents.OnCutsceneCompleted += OnCutsceneEnded;
        CutsceneEvents.OnCutsceneSkipped += OnCutsceneEnded;
    }

    private void UnsubscribeFromCutsceneEvents()
    {
        CutsceneEvents.OnCutsceneStarted -= OnCutsceneStarted;
        CutsceneEvents.OnCutsceneCompleted -= OnCutsceneEnded;
        CutsceneEvents.OnCutsceneSkipped -= OnCutsceneEnded;
    }

    // ─────────────────────────────────────────────
    //  EVENT CALLBACKS
    // ─────────────────────────────────────────────

    private void OnQuestChanged(QuestData _) => RefreshDisplay();

    private void OnObjectiveChanged(QuestData _, QuestObjective __) => RefreshDisplay();

    private void OnObjectiveProgressChanged(QuestData _, QuestObjective __, int ___) => RefreshDisplay();

    private void OnCutsceneStarted(CutsceneData _)
    {
        isCutscenePlaying = true;
        UpdateTargetAlpha();
    }

    private void OnCutsceneEnded(CutsceneData _)
    {
        isCutscenePlaying = false;
        UpdateTargetAlpha();
    }

    // ─────────────────────────────────────────────
    //  CORE LOGIC
    // ─────────────────────────────────────────────

    /// <summary>
    /// Finds the first active quest with at least one pending (non-completed)
    /// objective and displays that objective's text.
    /// </summary>
    private void RefreshDisplay()
    {
        if (QuestManager.Instance == null)
        {
            SetTexts("", noObjectiveText);
            UpdateTargetAlpha();
            return;
        }

        QuestObjective pendingObjective = null;
        QuestData sourceQuest = null;

        // Walk through active quests in order — first pending objective wins
        foreach (QuestData quest in QuestManager.Instance.ActiveQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (!objective.isCompleted)
                {
                    pendingObjective = objective;
                    sourceQuest = quest;
                    break;
                }
            }

            if (pendingObjective != null) break;
        }

        if (pendingObjective != null)
        {
            string title = sourceQuest != null ? sourceQuest.questTitle : "";
            string objText = pendingObjective.GetProgressText();
            SetTexts(title, objText);
        }
        else
        {
            // Nothing pending — show fallback (or hide if fallback is empty)
            SetTexts("", noObjectiveText);
        }

        UpdateTargetAlpha();
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────

    private void SetTexts(string title, string objective)
    {
        if (questTitleText != null)
            questTitleText.text = title;

        if (objectiveText != null)
            objectiveText.text = objective;
    }

    /// <summary>
    /// Decides whether the widget should be visible.
    /// It is visible only when:
    ///   - The flag allows it (or no flag is configured), AND
    ///   - There is something to display (objective found OR noObjectiveText is set).
    /// </summary>
    private void UpdateTargetAlpha()
    {
        bool hasContent = (objectiveText != null && !string.IsNullOrEmpty(objectiveText.text));
        targetAlpha = (flagAllowsShow && hasContent && !isCutscenePlaying) ? 1f : 0f;
    }

    private void FadeCanvasGroup()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        bool isVisible = canvasGroup.alpha > 0.01f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }

    private void SetAlphaImmediate(float alpha)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = alpha;
        canvasGroup.interactable = alpha > 0.01f;
        canvasGroup.blocksRaycasts = alpha > 0.01f;
    }

#if UNITY_EDITOR
    // ─────────────────────────────────────────────
    //  RUNTIME DEBUG (Editor only)
    // ─────────────────────────────────────────────

    [ContextMenu("Debug: Force Refresh Display")]
    private void DebugRefresh() => RefreshDisplay();
#endif
}

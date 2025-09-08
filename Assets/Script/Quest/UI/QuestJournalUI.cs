using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class QuestJournalUI : MonoBehaviour
{
    [Header("Essential Journal UI")]
    [Tooltip("REQUIRED: Main journal panel GameObject")]
    public GameObject journalPanel;
    [Tooltip("REQUIRED: Button to close the journal")]
    public Button closeJournalButton;
    [Tooltip("REQUIRED: Container where quest entries will be spawned")]
    public Transform questListContainer;
    [Tooltip("REQUIRED: Prefab for individual quest list entries")]
    public GameObject questEntryPrefab;

    [Header("Optional UI Elements")]
    [Tooltip("Optional: Button to open journal from UI")]
    public Button openJournalButton;
    [Tooltip("Optional: Canvas containing the journal (for show/hide)")]
    public GameObject journalCanvas;
    [Tooltip("Optional: ScrollRect for quest list scrolling")]
    public ScrollRect questScrollRect;

    [Header("Optional Adventure Book Styling")]
    [Tooltip("Optional: Book background image for theming")]
    public Image journalBookImage;
    [Tooltip("Optional: Adventure book frame sprite")]
    public Sprite adventureBookFrame;

    [Header("Optional Tab System")]
    [Tooltip("Optional: Tab button for active quests")]
    public Button activeTabButton;
    [Tooltip("Optional: Tab button for completed quests")]
    public Button completedTabButton;
    [Tooltip("Optional: Tab button for failed quests")]
    public Button failedTabButton;
    [Tooltip("Tab colors (only used if tab buttons are assigned)")]
    public Color selectedTabColor = Color.black;
    public Color unselectedTabColor = Color.white;

    [Header("Optional Quest Details Panel")]
    [Tooltip("Optional: Panel showing detailed quest information")]
    public GameObject questDetailsPanel;
    [Tooltip("Optional: Text showing quest title in details")]
    public TextMeshProUGUI questTitleText;
    [Tooltip("Optional: Text showing quest description")]
    public TextMeshProUGUI questDescriptionText;
    [Tooltip("Optional: Text showing quest type (Main, Side, etc.)")]
    public TextMeshProUGUI questTypeText;
    [Tooltip("Optional: Image showing quest icon")]
    public Image questIconImage;
    [Tooltip("Optional: Container for objective list")]
    public Transform objectiveListContainer;
    [Tooltip("Optional: Prefab for individual objectives")]
    public GameObject objectiveEntryPrefab;
    [Tooltip("Optional: Button to abandon selected quest")]
    public Button abandonQuestButton;

    [Header("Animations")]
    public float bookOpenSpeed = 0.5f;
    public bool enableBookAnimations = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bookOpenSound;
    public AudioClip bookCloseSound;
    public AudioClip pageFlipSound;
    public AudioClip questCompleteSound;

    [Header("Input")]
    public KeyCode openJournalKey = KeyCode.J;

    // Private variables
    private bool isJournalOpen = false;
    private QuestData selectedQuest;
    private QuestTabType currentTab = QuestTabType.Active;
    private List<QuestEntryUI> questEntryUIs = new List<QuestEntryUI>();
    private QuestManager questManager;

    public enum QuestTabType
    {
        Active,
        Completed,
        Failed
    }

    // Events
    public System.Action<QuestData> OnQuestSelected;
    public System.Action OnJournalOpened;
    public System.Action OnJournalClosed;

    private void Start()
    {
        InitializeJournal();
        SetupEventListeners();
        SetupAdventureBookStyling();
    }

    private void Update()
    {
        if (Input.GetKeyDown(openJournalKey))
        {
            ToggleJournal();
        }
    }

    private void InitializeJournal()
    {
        questManager = QuestManager.Instance;

        if (questManager == null)
        {
            Debug.LogWarning("QuestJournalUI: QuestManager not found!");
            return;
        }

        // Validate essential components
        if (journalPanel == null)
        {
            Debug.LogError("QuestJournalUI: journalPanel is required but not assigned!");
            return;
        }

        if (closeJournalButton == null)
        {
            Debug.LogError("QuestJournalUI: closeJournalButton is required but not assigned!");
            return;
        }

        if (questListContainer == null)
        {
            Debug.LogError("QuestJournalUI: questListContainer is required but not assigned!");
            return;
        }

        if (questEntryPrefab == null)
        {
            Debug.LogError("QuestJournalUI: questEntryPrefab is required but not assigned!");
            return;
        }

        // Hide journal initially
        if (journalCanvas != null)
            journalCanvas.SetActive(false);
        if (journalPanel != null)
            journalPanel.SetActive(false);

        // Initialize quest details panel
        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(false);

        // Set default tab
        SwitchToTab(QuestTabType.Active);
    }

    private void SetupEventListeners()
    {
        // Button listeners
        if (openJournalButton != null)
        {
            openJournalButton.onClick.AddListener(OpenJournal);
            Debug.Log("[QuestJournal] Open journal button listener added successfully," + openJournalButton.name);
        }
        else
        {
            Debug.LogWarning("[QuestJournal] openJournalButton is NULL - cannot add listener!");
        }

        if (closeJournalButton != null)
            closeJournalButton.onClick.AddListener(CloseJournal);

        if (activeTabButton != null)
            activeTabButton.onClick.AddListener(() => SwitchToTab(QuestTabType.Active));

        if (completedTabButton != null)
            completedTabButton.onClick.AddListener(() => SwitchToTab(QuestTabType.Completed));

        if (failedTabButton != null)
            failedTabButton.onClick.AddListener(() => SwitchToTab(QuestTabType.Failed));

        if (abandonQuestButton != null)
            abandonQuestButton.onClick.AddListener(AbandonSelectedQuest);

        // Quest Manager events
        if (questManager != null)
        {
            questManager.OnQuestStarted += OnQuestStarted;
            questManager.OnQuestCompleted += OnQuestCompleted;
            questManager.OnQuestFailed += OnQuestFailed;
            questManager.OnObjectiveCompleted += OnObjectiveCompleted;
            questManager.OnObjectiveUpdated += OnObjectiveUpdated;
        }
    }

    private void SetupAdventureBookStyling()
    {
        // Apply Adventure Book frame if provided
        if (journalBookImage != null && adventureBookFrame != null)
        {
            journalBookImage.sprite = adventureBookFrame;
            journalBookImage.type = Image.Type.Sliced;
        }

        // Style quest title text
        if (questTitleText != null)
        {
            questTitleText.color = Color.black; // Black color
            questTitleText.fontStyle = FontStyles.Bold;
        }

        // Style quest description text
        if (questDescriptionText != null)
        {
            questDescriptionText.color = new Color(0.18f, 0.11f, 0.08f, 1f); // Dark brown
        }
    }

    public void ToggleJournal()
    {
        Debug.Log($"[QuestJournal] ToggleJournal() called! Current state: {(isJournalOpen ? "OPEN" : "CLOSED")}");

        if (isJournalOpen)
            CloseJournal();
        else
            OpenJournal();
    }

    public void OpenJournal()
    {
        Debug.Log("[QuestJournal] OpenJournal() called!");

        if (DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.PauseTime();
        }

        if (isJournalOpen)
        {
            Debug.Log("[QuestJournal] Journal is already open, returning");
            return;
        }

        Debug.Log("[QuestJournal] Opening journal...");
        isJournalOpen = true;

        // Play open sound
        PlayAudioClip(bookOpenSound);

        // Show journal with animation
        if (enableBookAnimations)
        {
            StartCoroutine(OpenBookAnimation());
        }
        else
        {
            if (journalCanvas != null)
                journalCanvas.SetActive(true);
            if (journalPanel != null)
                journalPanel.SetActive(true);
            RefreshQuestList();
        }

        OnJournalOpened?.Invoke();
    }

    public void CloseJournal()
    {
        if (!isJournalOpen) return;

        if (DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.ResumeTime();
        }

        isJournalOpen = false;

        // Play close sound
        PlayAudioClip(bookCloseSound);

        // Hide journal with animation
        if (enableBookAnimations)
        {
            StartCoroutine(CloseBookAnimation());
        }
        else
        {
            if (journalPanel != null)
                journalPanel.SetActive(false);
            if (journalCanvas != null)
                journalCanvas.SetActive(false);
        }

        OnJournalClosed?.Invoke();
    }

    private System.Collections.IEnumerator OpenBookAnimation()
    {
        // Show canvas
        if (journalCanvas != null)
            journalCanvas.SetActive(true);
        if (journalPanel != null)
            journalPanel.SetActive(true);

        // Animate book opening
        if (journalBookImage != null)
        {
            Transform bookTransform = journalBookImage.transform;

            // Start closed
            bookTransform.localScale = new Vector3(0f, 1f, 1f);
            bookTransform.localRotation = Quaternion.Euler(0, 0, -5f);

            // Animate to open
            float elapsedTime = 0f;
            Vector3 targetScale = Vector3.one;
            Quaternion targetRotation = Quaternion.identity;

            while (elapsedTime < bookOpenSpeed)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / bookOpenSpeed;

                // Ease out animation
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

                bookTransform.localScale = Vector3.Lerp(new Vector3(0f, 1f, 1f), targetScale, easedProgress);
                bookTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, -5f), targetRotation, easedProgress);

                yield return null;
            }

            bookTransform.localScale = targetScale;
            bookTransform.localRotation = targetRotation;
        }

        // Refresh quest list after animation
        yield return new WaitForSeconds(0.2f);
        RefreshQuestList();
    }

    private System.Collections.IEnumerator CloseBookAnimation()
    {
        if (journalBookImage != null)
        {
            Transform bookTransform = journalBookImage.transform;

            float elapsedTime = 0f;
            Vector3 startScale = Vector3.one;
            Quaternion startRotation = Quaternion.identity;

            while (elapsedTime < bookOpenSpeed * 0.7f) // Faster close
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (bookOpenSpeed * 0.7f);

                bookTransform.localScale = Vector3.Lerp(startScale, new Vector3(0f, 1f, 1f), progress);
                bookTransform.localRotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 0, 5f), progress);

                yield return null;
            }
        }

        // Hide UI
        if (journalPanel != null)
            journalPanel.SetActive(false);
        if (journalCanvas != null)
            journalCanvas.SetActive(false);
    }

    public void SwitchToTab(QuestTabType tabType)
    {
        currentTab = tabType;

        // Update tab button colors
        UpdateTabButtons();

        // Play page flip sound
        PlayAudioClip(pageFlipSound);

        // Refresh quest list for new tab
        RefreshQuestList();

        // Clear quest details when switching tabs
        ClearQuestDetails();
    }

    private void UpdateTabButtons()
    {
        // Only update tab buttons if they exist
        if (HasTabButtons())
        {
            // Reset all tab colors
            SetTabButtonColor(activeTabButton, unselectedTabColor);
            SetTabButtonColor(completedTabButton, unselectedTabColor);
            SetTabButtonColor(failedTabButton, unselectedTabColor);

            // Highlight current tab
            switch (currentTab)
            {
                case QuestTabType.Active:
                    SetTabButtonColor(activeTabButton, selectedTabColor);
                    break;
                case QuestTabType.Completed:
                    SetTabButtonColor(completedTabButton, selectedTabColor);
                    break;
                case QuestTabType.Failed:
                    SetTabButtonColor(failedTabButton, selectedTabColor);
                    break;
            }
        }
    }

    private bool HasTabButtons()
    {
        return activeTabButton != null || completedTabButton != null || failedTabButton != null;
    }

    private void SetTabButtonColor(Button button, Color color)
    {
        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = color;
            button.colors = colors;
        }
    }

    public void RefreshQuestList()
    {
        if (questManager == null)
        {
            Debug.LogError("[QuestJournal] QuestManager is null!");
            return;
        }

        // Clear existing quest entries
        ClearQuestEntries();

        // Get quests for current tab
        List<QuestData> questsToShow = GetQuestsForCurrentTab();

        Debug.Log($"[QuestJournal] Current tab: {currentTab}, Found {questsToShow.Count} quests to show");
        Debug.Log($"[QuestJournal] QuestManager has - Active: {questManager.ActiveQuestCount}, Available: {questManager.availableQuests.Count}");

        // Create quest entry UIs
        foreach (var quest in questsToShow)
        {
            Debug.Log($"[QuestJournal] Creating entry for quest: {quest.questTitle} ({quest.questID}) - Status: {quest.status}");
            CreateQuestEntry(quest);
        }

        // Reset scroll position
        if (questScrollRect != null)
        {
            questScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private List<QuestData> GetQuestsForCurrentTab()
    {
        // If no tab system, show all active quests by default
        if (!HasTabButtons())
        {
            return questManager.ActiveQuests;
        }

        // Use tab system if available
        switch (currentTab)
        {
            case QuestTabType.Active:
                return questManager.ActiveQuests;
            case QuestTabType.Completed:
                return questManager.CompletedQuests;
            case QuestTabType.Failed:
                return questManager.FailedQuests;
            default:
                return questManager.ActiveQuests; // Default to active
        }
    }

    private void CreateQuestEntry(QuestData quest)
    {
        if (questEntryPrefab == null || questListContainer == null) return;

        GameObject entryGO = Instantiate(questEntryPrefab, questListContainer);
        RectTransform rt = entryGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(294.464f, 90);
        QuestEntryUI entryUI = entryGO.GetComponent<QuestEntryUI>();

        if (entryUI == null)
        {
            entryUI = entryGO.AddComponent<QuestEntryUI>();
        }

        entryUI.Initialize(quest, this);
        questEntryUIs.Add(entryUI);
    }

    private void ClearQuestEntries()
    {
        foreach (var entryUI in questEntryUIs)
        {
            if (entryUI != null && entryUI.gameObject != null)
            {
                Destroy(entryUI.gameObject);
            }
        }
        questEntryUIs.Clear();
    }

    public void SelectQuest(QuestData quest)
    {
        selectedQuest = quest;
        DisplayQuestDetails(quest);
        OnQuestSelected?.Invoke(quest);
    }

    private void DisplayQuestDetails(QuestData quest)
    {
        if (quest == null || questDetailsPanel == null) return;

        questDetailsPanel.SetActive(true);

        // Update quest info
        if (questTitleText != null)
            questTitleText.text = quest.questTitle.ToUpper();

        if (questDescriptionText != null)
            questDescriptionText.text = quest.questDescription;

        if (questTypeText != null)
            questTypeText.text = quest.questType.ToString().ToUpper();

        if (questIconImage != null && quest.questIcon != null)
        {
            questIconImage.sprite = quest.questIcon;
            questIconImage.gameObject.SetActive(true);
        }
        else if (questIconImage != null)
        {
            questIconImage.gameObject.SetActive(false);
        }

        // Update abandon button
        if (abandonQuestButton != null)
        {
            abandonQuestButton.gameObject.SetActive(quest.status == QuestStatus.Active && quest.canAbandon);
        }

        // Display objectives
        DisplayObjectives(quest);
    }

    private void DisplayObjectives(QuestData quest)
    {
        if (objectiveListContainer == null || objectiveEntryPrefab == null) return;

        // Clear existing objectives
        foreach (Transform child in objectiveListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create objective entries
        if (quest.objectives != null)
        {
            foreach (var objective in quest.objectives)
            {
                CreateObjectiveEntry(objective);
            }
        }
    }

    private void CreateObjectiveEntry(QuestObjective objective)
    {
        GameObject entryGO = Instantiate(objectiveEntryPrefab, objectiveListContainer);
        QuestObjectiveUI objectiveUI = entryGO.GetComponent<QuestObjectiveUI>();

        if (objectiveUI == null)
        {
            objectiveUI = entryGO.AddComponent<QuestObjectiveUI>();
        }

        objectiveUI.Initialize(objective);
    }

    private void ClearQuestDetails()
    {
        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(false);

        selectedQuest = null;
    }

    private void AbandonSelectedQuest()
    {
        if (selectedQuest != null && questManager != null)
        {
            questManager.AbandonQuest(selectedQuest.questID);
            RefreshQuestList();
            ClearQuestDetails();
        }
    }

    #region Quest Event Handlers

    private void OnQuestStarted(QuestData quest)
    {
        if (currentTab == QuestTabType.Active && isJournalOpen)
        {
            RefreshQuestList();
        }
    }

    private void OnQuestCompleted(QuestData quest)
    {
        // Play completion sound
        PlayAudioClip(questCompleteSound);

        if (isJournalOpen)
        {
            RefreshQuestList();

            // Clear details if this quest was selected
            if (selectedQuest == quest)
            {
                ClearQuestDetails();
            }
        }
    }

    private void OnQuestFailed(QuestData quest)
    {
        if (isJournalOpen)
        {
            RefreshQuestList();

            // Clear details if this quest was selected
            if (selectedQuest == quest)
            {
                ClearQuestDetails();
            }
        }
    }

    private void OnObjectiveCompleted(QuestData quest, QuestObjective objective)
    {
        // Refresh if this quest is currently displayed
        if (selectedQuest == quest)
        {
            DisplayQuestDetails(quest);
        }
    }

    private void OnObjectiveUpdated(QuestData quest, QuestObjective objective)
    {
        // Refresh if this quest is currently displayed
        if (selectedQuest == quest)
        {
            DisplayQuestDetails(quest);
        }
    }

    #endregion

    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (questManager != null)
        {
            questManager.OnQuestStarted -= OnQuestStarted;
            questManager.OnQuestCompleted -= OnQuestCompleted;
            questManager.OnQuestFailed -= OnQuestFailed;
            questManager.OnObjectiveCompleted -= OnObjectiveCompleted;
            questManager.OnObjectiveUpdated -= OnObjectiveUpdated;
        }
    }
}
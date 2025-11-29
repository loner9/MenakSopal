using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MonologueSystem : MonoBehaviour
{
    [Header("UI Components - Uses same UI as dialogue system")]
    [Tooltip("Leave empty to auto-find NPCInteractionSystem's dialogue UI")]
    public GameObject monologuePanel;
    public GameObject monologueCanvas;
    public TextMeshProUGUI monologueText;
    public TextMeshProUGUI speakerNameText; // Set to "Menak Sopal" during monologue
    public Image backgroundImage;
    public Button continueButton;
    public Button endButton;
    public Transform choiceContainer; // Hide this during monologue
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip defaultMonologueStartSound;
    public AudioClip defaultTypewriterSound;
    
    [Header("Animation Settings")]
    public float typewriterSpeed = 0.05f;
    public bool enableTypewriterEffect = true;
    
    [Header("Input")]
    public KeyCode continueKey = KeyCode.Space;
    public KeyCode skipKey = KeyCode.Escape;
    
    // Private variables
    private bool isInMonologue = false;
    private bool isTyping = false;
    private MonologueEntry currentMonologue;
    private Coroutine typewriterCoroutine;
    private HashSet<string> queuedFlagsToAdd = new HashSet<string>();
    private HashSet<string> queuedFlagsToRemove = new HashSet<string>();
    private List<QuestObjectiveAction> queuedObjectiveActions = new List<QuestObjectiveAction>();
    
    // References
    private NPCInteractionSystem npcInteractionSystem;
    private QuestManager questManager;
    private DayNightCycle dayNightCycle;
    private PlayerMovements playerMovements;
    
    // Singleton
    public static MonologueSystem Instance { get; private set; }
    
    // Events
    public System.Action<MonologueEntry> OnMonologueStarted;
    public System.Action<MonologueEntry> OnMonologueEnded;
    
    private struct QuestObjectiveAction
    {
        public string questID;
        public string objectiveID;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeMonologueSystem();
        SetupEventListeners();
    }
    
    void Update()
    {
        if (isInMonologue)
        {
            if (Input.GetKeyDown(continueKey) || Input.GetKeyDown(KeyCode.Return))
            {
                if (isTyping)
                {
                    SkipTypewriter();
                }
                else
                {
                    EndMonologue();
                }
            }
            
            if (Input.GetKeyDown(skipKey))
            {
                EndMonologue();
            }
        }
    }
    
    void InitializeMonologueSystem()
    {
        // Find references
        npcInteractionSystem = FindObjectOfType<NPCInteractionSystem>();
        questManager = QuestManager.Instance;
        dayNightCycle = DayNightCycle.Instance;
        
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerMovements = playerGO.GetComponent<PlayerMovements>();
        }
        
        // Auto-find UI components from NPCInteractionSystem if not assigned
        if (npcInteractionSystem != null)
        {
            if (monologuePanel == null)
                monologuePanel = npcInteractionSystem.dialoguePanel;
            if (monologueCanvas == null)
                monologueCanvas = npcInteractionSystem.dialogueCanvas;
            if (monologueText == null)
                monologueText = npcInteractionSystem.dialogueText;
            if (speakerNameText == null)
                speakerNameText = npcInteractionSystem.speakerNameText;
            if (continueButton == null)
                continueButton = npcInteractionSystem.continueButton;
            if (endButton == null)
                endButton = npcInteractionSystem.endButton;
            if (choiceContainer == null)
                choiceContainer = npcInteractionSystem.choiceContainer;
        }
        
        // Validate UI components
        if (monologuePanel == null)
        {
            Debug.LogError("[MonologueSystem] monologuePanel is required!");
        }
        
        if (monologueText == null)
        {
            Debug.LogError("[MonologueSystem] monologueText is required!");
        }
        
        // Hide monologue UI initially
        if (monologueCanvas != null)
            monologueCanvas.SetActive(false);
        if (monologuePanel != null)
            monologuePanel.SetActive(false);
    }
    
    void SetupEventListeners()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(() => {
                if (isTyping)
                {
                    SkipTypewriter();
                }
                else
                {
                    EndMonologue();
                }
            });
        }
        
        if (endButton != null)
        {
            endButton.onClick.AddListener(() => {
                EndMonologue();
            });
        }
    }
    
    /// <summary>
    /// Show a monologue entry. Can be called from other systems like FlagMonitorSystem.
    /// </summary>
    public void ShowMonologue(MonologueEntry monologue)
    {
        if (monologue == null)
        {
            Debug.LogWarning("[MonologueSystem] Attempted to show null monologue");
            return;
        }
        
        if (isInMonologue)
        {
            Debug.LogWarning("[MonologueSystem] Already in monologue, ignoring new request");
            return;
        }
        
        StartCoroutine(ShowMonologueCoroutine(monologue));
    }
    
    /// <summary>
    /// Show a monologue by ID from a MonologueData asset
    /// </summary>
    public void ShowMonologue(string monologueDataID, string monologueEntryIndex = "")
    {
        // Load monologue data from Resources
        MonologueData data = Resources.Load<MonologueData>($"Monologues/{monologueDataID}");
        if (data == null)
        {
            Debug.LogWarning($"[MonologueSystem] MonologueData not found: {monologueDataID}");
            return;
        }
        
        // Get current game flags
        List<string> gameFlags = GetGameFlags();
        
        // Get available monologues
        MonologueEntry[] availableMonologues = data.GetAvailableMonologues(gameFlags);
        if (availableMonologues.Length == 0)
        {
            Debug.LogWarning($"[MonologueSystem] No available monologues for: {monologueDataID}");
            return;
        }
        
        // Sort by priority (higher first)
        var sortedMonologues = availableMonologues.OrderByDescending(m => m.priority).ToArray();
        
        // Show the highest priority monologue
        ShowMonologue(sortedMonologues[0]);
    }
    
    /// <summary>
    /// Quick way to show a simple text monologue (useful for FlagMonitorSystem callbacks)
    /// Uses the NPCInteractionSystem to display monologue like a dialogue
    /// </summary>
    public void ShowSimpleMonologue(string text, string[] flagsToAdd = null, string objectiveToComplete = "", string questForObjective = "")
    {
        if (npcInteractionSystem == null)
        {
            Debug.LogError("[MonologueSystem] NPCInteractionSystem not found! Cannot show monologue.");
            return;
        }

        // Create a temporary dialogue data for the monologue
        DialogueData tempDialogue = ScriptableObject.CreateInstance<DialogueData>();
        tempDialogue.npcName = "Menak Sopal";
        tempDialogue.dialogueEntries = new DialogueEntry[]
        {
            new DialogueEntry
            {
                dialogueText = text,
                speakerName = "Menak Sopal",
                flagsToAdd = flagsToAdd ?? new string[0],
                pauseAfterDialogue = 0f
            }
        };

        // Create a temporary NPC object for the monologue
        GameObject tempNPCObj = new GameObject("TempMonologueNPC");
        tempNPCObj.SetActive(false); // Keep it hidden
        NPC tempNPC = tempNPCObj.AddComponent<NPC>();
        tempNPC.npcName = "Menak Sopal";
        tempNPC.dialogueData = tempDialogue;

        // Queue objectives and additional flags to be added after dialogue ends
        if (!string.IsNullOrEmpty(objectiveToComplete) && !string.IsNullOrEmpty(questForObjective))
        {
            queuedObjectiveActions.Add(new QuestObjectiveAction
            {
                questID = questForObjective,
                objectiveID = objectiveToComplete
            });
        }

        // Store the monologue entry to pass to the event
        MonologueEntry tempMonologueEntry = new MonologueEntry
        {
            monologueText = text,
            flagsToAdd = flagsToAdd ?? new string[0],
            objectiveToComplete = objectiveToComplete,
            questForObjective = questForObjective
        };

        // Subscribe to dialogue end event to clean up and process queued actions
        System.Action<NPC> onDialogueEndHandler = null;
        onDialogueEndHandler = (npc) =>
        {
            // Process queued objectives
            foreach (var objectiveAction in queuedObjectiveActions)
            {
                if (questManager != null)
                {
                    questManager.CompleteObjective(objectiveAction.questID, objectiveAction.objectiveID);
                }
            }
            queuedObjectiveActions.Clear();

            // Invoke the OnMonologueEnded event so other systems can react
            OnMonologueEnded?.Invoke(tempMonologueEntry);
            Debug.Log("[MonologueSystem] OnMonologueEnded event invoked");

            // Cleanup temporary objects
            if (tempNPCObj != null)
                Destroy(tempNPCObj);
            if (tempDialogue != null)
                Destroy(tempDialogue);

            // Unsubscribe
            npcInteractionSystem.OnDialogueEnd -= onDialogueEndHandler;
        };

        npcInteractionSystem.OnDialogueEnd += onDialogueEndHandler;

        // Start the dialogue (which will display like a monologue)
        npcInteractionSystem.StartDialogue(tempNPC);
    }
    
    private IEnumerator ShowMonologueCoroutine(MonologueEntry monologue)
    {
        Debug.Log("[MonologueSystem] Starting ShowMonologueCoroutine...");

        currentMonologue = monologue;
        isInMonologue = true;

        // Clear any previously queued flags and objectives
        queuedFlagsToAdd.Clear();
        queuedFlagsToRemove.Clear();
        queuedObjectiveActions.Clear();

        // Queue flags and objectives for later processing
        QueueMonologueConsequences(monologue);

        // Pause game time and disable player movement
        if (dayNightCycle != null)
        {
            dayNightCycle.PauseTime();
            Debug.Log("[MonologueSystem] Time paused");
        }

        if (playerMovements != null)
        {
            playerMovements.enabled = false;
            Debug.Log("[MonologueSystem] Player movement disabled");
        }

        // Show monologue canvas/panel
        if (monologueCanvas != null)
        {
            monologueCanvas.SetActive(true);
            Debug.Log($"[MonologueSystem] Monologue canvas activated: {monologueCanvas.name}");
        }
        else
        {
            Debug.LogError("[MonologueSystem] monologueCanvas is NULL!");
        }

        if (monologuePanel != null)
        {
            monologuePanel.SetActive(true);
            Debug.Log($"[MonologueSystem] Monologue panel activated: {monologuePanel.name}");
        }
        else
        {
            Debug.LogError("[MonologueSystem] monologuePanel is NULL!");
        }

        // Setup UI
        SetupMonologueUI(monologue);

        // Play start sound
        PlayAudioClip(defaultMonologueStartSound);

        // Show text with typewriter effect
        if (enableTypewriterEffect)
        {
            Debug.Log("[MonologueSystem] Starting typewriter effect...");
            yield return StartCoroutine(TypewriterEffect(monologue.monologueText));
        }
        else
        {
            monologueText.text = monologue.monologueText;
            Debug.Log("[MonologueSystem] Text set directly (no typewriter)");
        }

        Debug.Log("[MonologueSystem] Monologue display complete");
        OnMonologueStarted?.Invoke(monologue);
    }
    
    private void SetupMonologueUI(MonologueEntry monologue)
    {
        Debug.Log("[MonologueSystem] Setting up monologue UI...");

        // Set speaker name to "Menak Sopal" for monologues
        if (speakerNameText != null)
        {
            // Ensure parent hierarchy is active
            Transform parent = speakerNameText.transform.parent;
            while (parent != null && parent != monologuePanel.transform)
            {
                if (!parent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[MonologueSystem] Activating parent: {parent.name}");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }

            speakerNameText.text = "Menak Sopal";
            speakerNameText.gameObject.SetActive(true);
            Debug.Log($"[MonologueSystem] Speaker name set to 'Menak Sopal' - Active: {speakerNameText.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("[MonologueSystem] speakerNameText is NULL!");
        }

        if (monologueText != null)
        {
            // Ensure parent hierarchy is active
            Transform parent = monologueText.transform.parent;
            while (parent != null && parent != monologuePanel.transform)
            {
                if (!parent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[MonologueSystem] Activating parent: {parent.name}");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }

            monologueText.text = "";
            monologueText.color = monologue.textColor;
            monologueText.gameObject.SetActive(true);
            Debug.Log($"[MonologueSystem] Monologue text component initialized - Active: {monologueText.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("[MonologueSystem] monologueText is NULL!");
        }

        if (backgroundImage != null)
        {
            // Ensure parent hierarchy is active
            Transform parent = backgroundImage.transform.parent;
            while (parent != null && parent != monologuePanel.transform)
            {
                if (!parent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[MonologueSystem] Activating parent: {parent.name}");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }

            backgroundImage.color = monologue.backgroundColor;
            backgroundImage.gameObject.SetActive(true);
            Debug.Log($"[MonologueSystem] Background image set - Active: {backgroundImage.gameObject.activeInHierarchy}");
        }

        // Hide choice container if it exists (it might be blocking the text)
        if (choiceContainer != null)
        {
            choiceContainer.gameObject.SetActive(false);
            Debug.Log("[MonologueSystem] Choice container hidden");
        }

        // Setup buttons for monologue mode
        // Hide continue button during typing, show end button
        UpdateMonologueButtons(true); // isTyping = true initially
    }
    
    private void UpdateMonologueButtons(bool isTyping)
    {
        if (continueButton != null)
        {
            // Show continue button only when not typing (to skip or continue)
            continueButton.gameObject.SetActive(!isTyping);
        }
        
        if (endButton != null)
        {
            // Always show end button during monologue
            endButton.gameObject.SetActive(true);
        }
    }
    
    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        monologueText.text = "";

        Debug.Log($"[MonologueSystem] TypewriterEffect - Text length: {text.Length}, Font size: {monologueText.fontSize}, Color: {monologueText.color}, Alpha: {monologueText.color.a}");

        for (int i = 0; i <= text.Length; i++)
        {
            monologueText.text = text.Substring(0, i);

            // Log first few characters to verify text is being set
            if (i <= 10 || i == text.Length)
            {
                Debug.Log($"[MonologueSystem] Typewriter progress {i}/{text.Length}: '{monologueText.text}'");
            }

            // Play typewriter sound occasionally
            if (i % 3 == 0)
            {
                PlayAudioClip(defaultTypewriterSound);
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        Debug.Log($"[MonologueSystem] Typewriter complete. Final text: '{monologueText.text}'");
        UpdateMonologueButtons(false); // Update buttons when typing is finished
    }
    
    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        isTyping = false;
        monologueText.text = currentMonologue.monologueText;
        UpdateMonologueButtons(false); // Update buttons when typewriter is skipped
    }
    
    public void EndMonologue()
    {
        if (!isInMonologue) return;
        
        // Process queued consequences
        ProcessQueuedConsequences();
        
        // Hide UI and restore button states
        if (monologuePanel != null)
            monologuePanel.SetActive(false);
        if (monologueCanvas != null)
            monologueCanvas.SetActive(false);
        
        // Hide monologue buttons (they'll be restored when dialogue system is used next)
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
        if (endButton != null)
            endButton.gameObject.SetActive(false);
        
        // Resume game time and enable player movement
        if (dayNightCycle != null)
        {
            dayNightCycle.ResumeTime();
        }
        
        if (playerMovements != null)
        {
            playerMovements.enabled = true;
        }
        
        OnMonologueEnded?.Invoke(currentMonologue);
        
        isInMonologue = false;
        currentMonologue = null;
    }
    
    private void QueueMonologueConsequences(MonologueEntry monologue)
    {
        // Queue flags to add
        if (monologue.flagsToAdd != null)
        {
            foreach (string flag in monologue.flagsToAdd)
            {
                queuedFlagsToAdd.Add(flag);
            }
        }
        
        // Queue flags to remove
        if (monologue.flagsToRemove != null)
        {
            foreach (string flag in monologue.flagsToRemove)
            {
                queuedFlagsToRemove.Add(flag);
            }
        }
        
        // Queue objective completion
        if (!string.IsNullOrEmpty(monologue.objectiveToComplete) && !string.IsNullOrEmpty(monologue.questForObjective))
        {
            queuedObjectiveActions.Add(new QuestObjectiveAction
            {
                questID = monologue.questForObjective,
                objectiveID = monologue.objectiveToComplete
            });
        }
    }
    
    private void ProcessQueuedConsequences()
    {
        // Add queued flags
        foreach (string flag in queuedFlagsToAdd)
        {
            if (npcInteractionSystem != null)
            {
                npcInteractionSystem.AddGameFlag(flag);
                Debug.Log($"[MonologueSystem] Added flag: {flag}");
            }
        }
        
        // Remove queued flags
        foreach (string flag in queuedFlagsToRemove)
        {
            if (npcInteractionSystem != null)
            {
                npcInteractionSystem.RemoveGameFlag(flag);
                Debug.Log($"[MonologueSystem] Removed flag: {flag}");
            }
        }
        
        // Complete queued objectives
        foreach (var objectiveAction in queuedObjectiveActions)
        {
            if (questManager != null)
            {
                bool completed = questManager.CompleteObjective(objectiveAction.questID, objectiveAction.objectiveID);
                Debug.Log($"[MonologueSystem] Completed objective {objectiveAction.objectiveID} in quest {objectiveAction.questID}: {completed}");
            }
        }
        
        // Clear queues
        queuedFlagsToAdd.Clear();
        queuedFlagsToRemove.Clear();
        queuedObjectiveActions.Clear();
    }
    
    private List<string> GetGameFlags()
    {
        if (npcInteractionSystem != null)
        {
            return npcInteractionSystem.GetGameFlags();
        }
        return new List<string>();
    }
    
    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public bool IsInMonologue()
    {
        return isInMonologue;
    }
}
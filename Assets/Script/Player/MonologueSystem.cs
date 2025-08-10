using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MonologueSystem : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject monologuePanel;
    public GameObject monologueCanvas;
    public TextMeshProUGUI monologueText;
    public Image backgroundImage;
    public Button continueButton;
    
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
    /// </summary>
    public void ShowSimpleMonologue(string text, string[] flagsToAdd = null, string objectiveToComplete = "", string questForObjective = "")
    {
        MonologueEntry simpleMonologue = new MonologueEntry
        {
            monologueText = text,
            flagsToAdd = flagsToAdd ?? new string[0],
            objectiveToComplete = objectiveToComplete,
            questForObjective = questForObjective,
            isRepeatable = false,
            priority = 0
        };
        
        ShowMonologue(simpleMonologue);
    }
    
    private IEnumerator ShowMonologueCoroutine(MonologueEntry monologue)
    {
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
        }
        
        if (playerMovements != null)
        {
            playerMovements.enabled = false;
        }
        
        // Setup UI
        SetupMonologueUI(monologue);
        
        // Show monologue canvas/panel
        if (monologueCanvas != null)
            monologueCanvas.SetActive(true);
        if (monologuePanel != null)
            monologuePanel.SetActive(true);
        
        // Play start sound
        PlayAudioClip(defaultMonologueStartSound);
        
        // Show text with typewriter effect
        if (enableTypewriterEffect)
        {
            yield return StartCoroutine(TypewriterEffect(monologue.monologueText));
        }
        else
        {
            monologueText.text = monologue.monologueText;
        }
        
        OnMonologueStarted?.Invoke(monologue);
    }
    
    private void SetupMonologueUI(MonologueEntry monologue)
    {
        if (monologueText != null)
        {
            monologueText.text = "";
            monologueText.color = monologue.textColor;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = monologue.backgroundColor;
        }
    }
    
    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        monologueText.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            monologueText.text = text.Substring(0, i);
            
            // Play typewriter sound occasionally
            if (i % 3 == 0)
            {
                PlayAudioClip(defaultTypewriterSound);
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
    
    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        isTyping = false;
        monologueText.text = currentMonologue.monologueText;
    }
    
    public void EndMonologue()
    {
        if (!isInMonologue) return;
        
        // Process queued consequences
        ProcessQueuedConsequences();
        
        // Hide UI
        if (monologuePanel != null)
            monologuePanel.SetActive(false);
        if (monologueCanvas != null)
            monologueCanvas.SetActive(false);
        
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
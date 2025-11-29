using UnityEngine;

public class ForceDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    [Tooltip("Name of the DialogueData asset in Resources/Dialogues/")]
    public string dialogueAssetName;
    
    [Tooltip("Specific dialogue entry index to show (-1 for auto-select based on flags)")]
    public int dialogueEntryIndex = -1;
    
    [Header("Trigger Conditions")]
    [Tooltip("Required flags for this dialogue to trigger")]
    public string[] requiredFlags;
    
    [Tooltip("Flags that must NOT be present for this dialogue to trigger")]
    public string[] forbiddenFlags;
    
    [Header("Trigger Settings")]
    [Tooltip("Only trigger once?")]
    public bool triggerOnce = true;
    
    [Tooltip("Player tag to check for")]
    public string playerTag = "Player";
    
    [Tooltip("Auto-hide this trigger after activation?")]
    public bool hideAfterTrigger = true;
    
    [Header("Audio")]
    [Tooltip("Sound to play when dialogue triggers")]
    public AudioClip triggerSound;
    
    // Private variables
    private bool hasTriggered = false;
    private NPCInteractionSystem npcInteractionSystem;
    private AudioSource audioSource;
    public static ForceDialogueTrigger Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        npcInteractionSystem = FindObjectOfType<NPCInteractionSystem>();
        audioSource = GetComponent<AudioSource>();
        
        if (npcInteractionSystem == null)
        {
            Debug.LogError("[ForceDialogueTrigger] NPCInteractionSystem not found!");
        }
        
        if (string.IsNullOrEmpty(dialogueAssetName))
        {
            Debug.LogError($"[ForceDialogueTrigger] dialogueAssetName is required! GameObject: {gameObject.name}");
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            TriggerDialogue();
        }
    }
    
    /// <summary>
    /// Trigger the dialogue (can also be called manually from other scripts)
    /// </summary>
    public void TriggerDialogue()
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        if (npcInteractionSystem == null)
        {
            Debug.LogWarning("[ForceDialogueTrigger] NPCInteractionSystem not found!");
            return;
        }
        
        // Check required flags
        if (!CheckRequiredFlags())
        {
            Debug.Log($"[ForceDialogueTrigger] Flag requirements not met for {dialogueAssetName}");
            return;
        }
        
        // Check forbidden flags
        if (!CheckForbiddenFlags())
        {
            Debug.Log($"[ForceDialogueTrigger] Forbidden flags present for {dialogueAssetName}");
            return;
        }
        
        // Load dialogue data
        DialogueData dialogueData = LoadDialogueData();
        if (dialogueData == null)
        {
            return;
        }
        
        // Play trigger sound
        if (triggerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(triggerSound);
        }
        
        // Force start the dialogue
        ForceStartDialogue(dialogueData);
        
        if (triggerOnce)
        {
            hasTriggered = true;
        }
        
        if (hideAfterTrigger)
        {
            gameObject.SetActive(false);
        }
    }
    
    private DialogueData LoadDialogueData()
    {
        // Try loading from different possible paths
        string[] possiblePaths = {
            $"Dialogues/{dialogueAssetName}",
            $"Dialogues/Story/{dialogueAssetName}",
            $"Dialogues/Village/{dialogueAssetName}",
            dialogueAssetName
        };
        
        foreach (string path in possiblePaths)
        {
            DialogueData data = Resources.Load<DialogueData>(path);
            if (data != null)
            {
                Debug.Log($"[ForceDialogueTrigger] Loaded dialogue from: {path}");
                return data;
            }
        }
        
        Debug.LogError($"[ForceDialogueTrigger] Could not find DialogueData: {dialogueAssetName}");
        return null;
    }
    
    private void ForceStartDialogue(DialogueData dialogueData)
    {
        if (npcInteractionSystem.IsInDialogue())
        {
            Debug.LogWarning("[ForceDialogueTrigger] Already in dialogue, skipping");
            return;
        }
        
        Debug.Log($"[ForceDialogueTrigger] Starting forced dialogue: {dialogueData.npcName}");
        
        // Force start dialogue using the interaction system's internal method
        npcInteractionSystem.StartForcedDialogue(dialogueData, dialogueEntryIndex);
    }
    
    private bool CheckRequiredFlags()
    {
        if (requiredFlags == null || requiredFlags.Length == 0)
        {
            return true;
        }
        
        var gameFlags = npcInteractionSystem.GetGameFlags();
        
        foreach (string flag in requiredFlags)
        {
            if (!gameFlags.Contains(flag))
            {
                return false;
            }
        }
        
        return true;
    }
    
    private bool CheckForbiddenFlags()
    {
        if (forbiddenFlags == null || forbiddenFlags.Length == 0)
        {
            return true;
        }
        
        var gameFlags = npcInteractionSystem.GetGameFlags();
        
        foreach (string flag in forbiddenFlags)
        {
            if (gameFlags.Contains(flag))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Reset the trigger so it can be used again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Check if this trigger can activate with current game state
    /// </summary>
    public bool CanTrigger()
    {
        if (triggerOnce && hasTriggered) return false;
        if (string.IsNullOrEmpty(dialogueAssetName)) return false;
        if (!CheckRequiredFlags()) return false;
        if (!CheckForbiddenFlags()) return false;
        
        return true;
    }
}
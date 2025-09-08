using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("NPC Target")]
    [Tooltip("The NPC whose dialogue should be triggered")]
    public NPC targetNPC;
    
    [Tooltip("Or find NPC by name if targetNPC is not assigned")]
    public string npcName;
    
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
    
    [Header("Optional: Force specific dialogue")]
    [Tooltip("Force a specific dialogue entry index (leave -1 for auto-select)")]
    public int forceDialogueIndex = -1;
    
    // Private variables
    private bool hasTriggered = false;
    private NPCInteractionSystem npcInteractionSystem;
    
    void Start()
    {
        npcInteractionSystem = FindObjectOfType<NPCInteractionSystem>();
        
        if (npcInteractionSystem == null)
        {
            Debug.LogError("[DialogueTrigger] NPCInteractionSystem not found!");
        }
        
        // Auto-find NPC by name if targetNPC is not assigned
        if (targetNPC == null && !string.IsNullOrEmpty(npcName))
        {
            GameObject npcObject = GameObject.Find(npcName);
            if (npcObject != null)
            {
                targetNPC = npcObject.GetComponent<NPC>();
            }
        }
        
        if (targetNPC == null)
        {
            Debug.LogError($"[DialogueTrigger] Target NPC not found! GameObject: {gameObject.name}");
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
        
        if (targetNPC == null || npcInteractionSystem == null)
        {
            Debug.LogWarning("[DialogueTrigger] Missing references!");
            return;
        }
        
        // Check if NPC is properly initialized
        if (targetNPC.StateMachine == null)
        {
            Debug.LogWarning($"[DialogueTrigger] NPC {targetNPC.npcName} StateMachine is null! Trying delayed trigger...");
            StartCoroutine(DelayedTrigger());
            return;
        }
        
        if (targetNPC.InteractionState == null)
        {
            Debug.LogWarning($"[DialogueTrigger] NPC {targetNPC.npcName} InteractionState is null! Trying delayed trigger...");
            StartCoroutine(DelayedTrigger());
            return;
        }
        
        // Check required flags
        if (!CheckRequiredFlags())
        {
            Debug.Log($"[DialogueTrigger] Flag requirements not met for {targetNPC.npcName}");
            return;
        }
        
        // Check forbidden flags
        if (!CheckForbiddenFlags())
        {
            Debug.Log($"[DialogueTrigger] Forbidden flags present for {targetNPC.npcName}");
            return;
        }
        
        // Trigger the dialogue
        Debug.Log($"[DialogueTrigger] Starting dialogue with {targetNPC.npcName}");
        
        if (forceDialogueIndex >= 0)
        {
            // Force specific dialogue index if specified
            StartSpecificDialogue();
        }
        else
        {
            // Normal dialogue interaction
            npcInteractionSystem.StartDialogue(targetNPC);
        }
        
        if (triggerOnce)
        {
            hasTriggered = true;
        }
        
        if (hideAfterTrigger)
        {
            gameObject.SetActive(false);
        }
    }
    
    private bool CheckRequiredFlags()
    {
        if (requiredFlags == null || requiredFlags.Length == 0)
        {
            return true; // No requirements
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
            return true; // No restrictions
        }
        
        var gameFlags = npcInteractionSystem.GetGameFlags();
        
        foreach (string flag in forbiddenFlags)
        {
            if (gameFlags.Contains(flag))
            {
                return false; // Forbidden flag is present
            }
        }
        
        return true;
    }
    
    private void StartSpecificDialogue()
    {
        // Get dialogue data for the NPC
        DialogueData dialogueData = npcInteractionSystem.GetDialogueForNPC(targetNPC);
        if (dialogueData == null || dialogueData.dialogueEntries == null || 
            forceDialogueIndex >= dialogueData.dialogueEntries.Length)
        {
            Debug.LogWarning($"[DialogueTrigger] Invalid dialogue index {forceDialogueIndex} for {targetNPC.npcName}");
            return;
        }
        
        // Start dialogue with specific entry
        npcInteractionSystem.StartDialogue(targetNPC);
        // Note: You might need to modify NPCInteractionSystem to support starting at specific index
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
        if (targetNPC == null || npcInteractionSystem == null) return false;
        if (!CheckRequiredFlags()) return false;
        if (!CheckForbiddenFlags()) return false;
        
        return true;
    }
    
    private System.Collections.IEnumerator DelayedTrigger()
    {
        // Wait for NPC to be properly initialized
        yield return new WaitForSeconds(0.5f);
        
        // Try triggering again
        if (targetNPC != null && targetNPC.StateMachine != null && targetNPC.InteractionState != null)
        {
            Debug.Log($"[DialogueTrigger] Delayed trigger successful for {targetNPC.npcName}");
            TriggerDialogue();
        }
        else
        {
            Debug.LogError($"[DialogueTrigger] NPC {targetNPC?.npcName ?? "Unknown"} still not properly initialized after delay!");
        }
    }
}
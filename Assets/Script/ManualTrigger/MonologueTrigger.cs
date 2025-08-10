using UnityEngine;

public class MonologueTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("What should trigger this monologue?")]
    public TriggerType triggerType = TriggerType.OnTriggerEnter;
    
    [Header("Monologue Configuration")]
    [Tooltip("ID of the MonologueData asset in Resources/Monologues/")]
    public string monologueDataID;
    
    [Tooltip("Or use simple text monologue")]
    public bool useSimpleMonologue = false;
    
    [TextArea(3, 6)]
    [Tooltip("Simple monologue text (only used if useSimpleMonologue is true)")]
    public string simpleMonologueText;
    
    [Tooltip("Flags to add after monologue (only for simple monologue)")]
    public string[] flagsToAdd;
    
    [Header("Trigger Conditions")]
    [Tooltip("Required flags for this monologue to trigger")]
    public string[] requiredFlags;
    
    [Tooltip("Only trigger once?")]
    public bool triggerOnce = true;
    
    [Tooltip("Player tag to check for")]
    public string playerTag = "Player";
    
    // Private variables
    private bool hasTriggered = false;
    private NPCInteractionSystem npcInteractionSystem;
    
    public enum TriggerType
    {
        OnTriggerEnter,
        OnStart,
        Manual
    }
    
    void Start()
    {
        npcInteractionSystem = FindObjectOfType<NPCInteractionSystem>();
        
        if (triggerType == TriggerType.OnStart)
        {
            TriggerMonologue();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerType == TriggerType.OnTriggerEnter && other.CompareTag(playerTag))
        {
            TriggerMonologue();
        }
    }
    
    /// <summary>
    /// Manually trigger the monologue (useful for other systems)
    /// </summary>
    public void TriggerMonologue()
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        // Check required flags
        if (!CheckRequiredFlags())
        {
            return;
        }
        
        if (MonologueSystem.Instance == null)
        {
            Debug.LogWarning("[MonologueTrigger] MonologueSystem.Instance is null!");
            return;
        }
        
        if (useSimpleMonologue)
        {
            MonologueSystem.Instance.ShowSimpleMonologue(simpleMonologueText, flagsToAdd);
        }
        else
        {
            MonologueSystem.Instance.ShowMonologue(monologueDataID);
        }
        
        if (triggerOnce)
        {
            hasTriggered = true;
        }
    }
    
    private bool CheckRequiredFlags()
    {
        if (requiredFlags == null || requiredFlags.Length == 0)
        {
            return true; // No requirements, always trigger
        }
        
        if (npcInteractionSystem == null)
        {
            Debug.LogWarning("[MonologueTrigger] NPCInteractionSystem not found!");
            return false;
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
    
    /// <summary>
    /// Reset the trigger so it can be used again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
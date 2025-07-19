using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bridge component that allows SimpleNPC to work with your existing NPCInteractionSystem.
/// This acts as a compatibility layer, translating between the new and old NPC interfaces.
/// 
/// Why we need this: Your NPCInteractionSystem expects certain properties and methods
/// that SimpleNPC doesn't have. Rather than rewriting your interaction system,
/// this bridge makes SimpleNPC "speak the same language" as your old NPC component.
/// </summary>
[RequireComponent(typeof(SimpleNPC))]
public class NPCInteractionBridge : MonoBehaviour
{
    [Header("Dialogue Integration")]
    [Tooltip("Dialogue data for this NPC - will be used by the interaction system")]
    public DialogueData dialogueData;
    
    [Header("Interaction Settings")]
    [Tooltip("Can this NPC be interacted with? Automatically managed by SimpleNPC state")]
    public bool canInteract = true;
    
    [Header("Compatibility")]
    [Tooltip("NPC name for dialogue system - auto-filled from schedule data")]
    public string npcName;
    
    // References
    private SimpleNPC simpleNPC;
    private EnhancedNPCInteractionSystem interactionSystem;
    
    // Bridge properties that mimic the old NPC component interface
    /// <summary>
    /// Bridge property: Makes SimpleNPC compatible with interaction system checks.
    /// The interaction system checks 'npc.canInteract' - this property provides that.
    /// </summary>
    public bool CanInteractProperty 
    { 
        get 
        { 
            // NPC can only be interacted with when idle and player is nearby
            return simpleNPC != null && simpleNPC.CanInteractWithPlayer() && canInteract;
        } 
    }
    
    /// <summary>
    /// Bridge property: Provides schedule data in the format the interaction system expects.
    /// This allows the dialogue system to fall back to schedule data if no DialogueData is assigned.
    /// </summary>
    public SimpleNPCScheduleData scheduleData 
    { 
        get 
        { 
            return simpleNPC != null ? simpleNPC.GetScheduleData() : null; 
        } 
    }
    
    void Awake()
    {
        // Get required components
        simpleNPC = GetComponent<SimpleNPC>();
        if (simpleNPC == null)
        {
            Debug.LogError($"NPCInteractionBridge: No SimpleNPC component found on {gameObject.name}");
            return;
        }
        
        // Find the interaction system
        interactionSystem = FindObjectOfType<EnhancedNPCInteractionSystem>();
        
        // Auto-fill NPC name from schedule data if not manually set
        if (string.IsNullOrEmpty(npcName) && scheduleData != null)
        {
            npcName = scheduleData.npcName;
        }
    }
    
    void Start()
    {
        // Subscribe to SimpleNPC state changes to update interaction availability
        if (simpleNPC != null)
        {
            // If SimpleNPC had events for state changes, we'd subscribe here
            // For now, we'll update in Update() method
        }
    }
    
    void Update()
    {
        // Update interaction availability based on SimpleNPC state
        // This ensures canInteract reflects whether the NPC is actually available for conversation
        UpdateInteractionAvailability();
    }
    
    /// <summary>
    /// Updates whether this NPC can be interacted with based on their current state.
    /// This is the key method that bridges the gap between SimpleNPC's state system
    /// and the interaction system's expectations.
    /// </summary>
    void UpdateInteractionAvailability()
    {
        if (simpleNPC == null) 
        {
            canInteract = false;
            return;
        }
        
        // NPCs can only be interacted with when they're in an idle state
        // (not walking to destinations, not at waypoints taking breaks)
        canInteract = simpleNPC.CanInteractWithPlayer();
    }
    
    /// <summary>
    /// Called by the interaction system when a dialogue starts.
    /// This bridges the call to SimpleNPC so it knows it's in an interaction.
    /// </summary>
    public void OnDialogueStart()
    {
        if (simpleNPC != null)
        {
            simpleNPC.StartInteraction();
        }
        
        Debug.Log($"NPCInteractionBridge: Started dialogue with {npcName}");
    }
    
    /// <summary>
    /// Called by the interaction system when a dialogue ends.
    /// This tells SimpleNPC to return to its scheduled behavior.
    /// </summary>
    public void OnDialogueEnd()
    {
        if (simpleNPC != null)
        {
            // SimpleNPC will automatically return to its previous state
            // when the interaction state ends
        }
        
        Debug.Log($"NPCInteractionBridge: Ended dialogue with {npcName}");
    }
    
    /// <summary>
    /// Bridge method that provides dialogue data in the format your interaction system expects.
    /// This method is called by a modified version of your NPCInteractionSystem.
    /// </summary>
    public DialogueData GetDialogueData()
    {
        // Priority 1: Use assigned DialogueData
        if (dialogueData != null)
        {
            return dialogueData;
        }
        
        // Priority 2: Try loading from Resources (your existing fallback)
        DialogueData resourceDialogue = Resources.Load<DialogueData>($"Dialogues/{npcName}");
        if (resourceDialogue != null)
        {
            return resourceDialogue;
        }
        
        // Priority 3: Create from schedule data (your existing fallback)
        if (scheduleData != null && scheduleData.dialogues != null && scheduleData.dialogues.Length > 0)
        {
            return CreateDialogueFromScheduleData();
        }
        
        return null;
    }
    
    /// <summary>
    /// Creates a temporary DialogueData from the schedule data.
    /// This maintains compatibility with your existing dialogue system.
    /// </summary>
    DialogueData CreateDialogueFromScheduleData()
    {
        DialogueData tempDialogue = ScriptableObject.CreateInstance<DialogueData>();
        tempDialogue.npcName = npcName;
        
        // Convert schedule dialogue strings to DialogueEntry array
        List<DialogueEntry> entries = new List<DialogueEntry>();
        foreach (string dialogue in scheduleData.dialogues)
        {
            DialogueEntry entry = new DialogueEntry
            {
                speakerName = npcName,
                dialogueText = dialogue,
                availableTimesOfDay = new TimeOfDay[] { 
                    TimeOfDay.Day, TimeOfDay.Night, TimeOfDay.Sunrise, TimeOfDay.Sunset 
                },
                isRepeatable = true,
                conversationBubbleSprite = null // Will use default
            };
            entries.Add(entry);
        }
        
        tempDialogue.dialogueEntries = entries.ToArray();
        
        // Use system default bubble if available
        if (interactionSystem != null)
        {
            tempDialogue.defaultConversationBubble = interactionSystem.defaultInteractionBubble;
        }
        
        return tempDialogue;
    }
    
    /// <summary>
    /// Helper method for external systems to check if this NPC is available for interaction.
    /// This provides a clean interface for other systems that need to know interaction status.
    /// </summary>
    public bool IsAvailableForInteraction()
    {
        return CanInteractProperty;
    }
    
    /// <summary>
    /// Bridge method to trigger interaction from external systems.
    /// This provides the same interface as the old NPC component.
    /// </summary>
    public void StartInteraction()
    {
        if (interactionSystem != null && CanInteractProperty)
        {
            // We need to pass a fake NPC component to the interaction system
            // This is handled in the modified interaction system
            interactionSystem.StartDialogueWithBridge(this);
        }
    }
    
    /// <summary>
    /// Provides access to the underlying SimpleNPC for systems that need it.
    /// This allows advanced integrations while maintaining compatibility.
    /// </summary>
    public SimpleNPC GetSimpleNPC()
    {
        return simpleNPC;
    }
    
    /// <summary>
    /// Debug method to show current interaction status.
    /// Useful for understanding why an NPC might not be interactable.
    /// </summary>
    public string GetInteractionStatus()
    {
        if (simpleNPC == null)
            return "No SimpleNPC component";
            
        if (!simpleNPC.CanInteractWithPlayer())
            return "NPC is busy (walking or at waypoint)";
            
        if (!canInteract)
            return "Interaction disabled";
            
        return "Available for interaction";
    }
    
    #region Debug and Gizmos
    
    void OnDrawGizmosSelected()
    {
        // Draw interaction status info
        if (simpleNPC != null)
        {
            Vector3 pos = transform.position + Vector3.up * 2f;
            
            #if UNITY_EDITOR
            string status = GetInteractionStatus();
            Color statusColor = CanInteractProperty ? Color.green : Color.red;
            
            UnityEditor.Handles.color = statusColor;
            UnityEditor.Handles.Label(pos, $"Interaction: {status}");
            #endif
        }
    }
    
    #endregion
}
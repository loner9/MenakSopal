using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [Header("Choice Content")]
    [TextArea(2, 3)]
    public string choiceText;
    
    [Header("Availability")]
    [Tooltip("Required flags for this choice to appear")]
    public string[] requiredFlags;
    [Tooltip("Times when this choice is available")]
    public TimeOfDay[] availableTimesOfDay;
    [Tooltip("Can this choice be selected multiple times?")]
    public bool isRepeatable = true;
    
    [Header("Consequences")]
    [Tooltip("Flags to add when this choice is selected")]
    public string[] flagsToAdd;
    [Tooltip("Flags to remove when this choice is selected")]
    public string[] flagsToRemove;
    
    [Header("Navigation")]
    [Tooltip("Index of the dialogue entry to show after this choice (-1 to end dialogue)")]
    public int targetDialogueIndex = -1;
    [Tooltip("Specific dialogue response to this choice")]
    public DialogueResponse response;
    
    [Header("Quest Integration")]
    [Tooltip("Quest to start when this choice is selected")]
    public string questToStart;
    [Tooltip("Quest to complete when this choice is selected")]
    public string questToComplete;
    [Tooltip("Quest objective to complete when this choice is selected")]
    public string objectiveToComplete;
    [Tooltip("Objective in which quest to complete")]
    public string questForObjective;
    
    [Header("Visual")]
    [Tooltip("Mark as important choice for special styling")]
    public bool isImportantChoice = false;
    [Tooltip("Color tint for this choice button")]
    public Color choiceColor = Color.white;
}

[System.Serializable]
public class DialogueResponse
{
    [Header("Response Content")]
    public string speakerName;
    [TextArea(3, 5)]
    public string responseText;
    
    [Header("Visual")]
    [Tooltip("Bubble sprite to show during this response")]
    public Sprite conversationBubbleSprite;
    [Tooltip("Pause duration after this response")]
    public float pauseAfterResponse = 0f;
    
    [Header("Navigation")]
    [Tooltip("Continue to next dialogue entry after this response?")]
    public bool continueToNext = true;
    [Tooltip("Index of dialogue entry to continue to (-1 to end dialogue)")]
    public int nextDialogueIndex = -1;
}

[System.Serializable]
public class DialogueEntry
{
    [Header("Dialogue Content")]
    public string speakerName;
    [TextArea(3, 5)]
    public string dialogueText;
    
    [Header("Availability")]
    public TimeOfDay[] availableTimesOfDay;
    public bool isRepeatable = true;
    public string[] requiredFlags; // For quest system integration
    
    [Header("Consequences")]
    [Tooltip("Flags to add when this dialogue is shown")]
    public string[] flagsToAdd;
    [Tooltip("Flags to remove when this dialogue is shown")]
    public string[] flagsToRemove;
    
    [Header("Choice System")]
    [Tooltip("Does this dialogue entry present choices to the player?")]
    public bool hasChoices = false;
    [Tooltip("Available choices for the player")]
    public DialogueChoice[] choices;
    
    [Header("Visual")]
    [Tooltip("Specific bubble sprite to show during this dialogue. If null, uses default conversation bubble.")]
    public Sprite conversationBubbleSprite;
    
    [Header("Advanced")]
    [Tooltip("Mark as important dialogue for special styling")]
    public bool isImportantDialogue = false;
    [Tooltip("Pause duration after this dialogue before allowing continue")]
    public float pauseAfterDialogue = 0f;
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "NPC/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("NPC Information")]
    public string npcName;
    [TextArea(2, 3)]
    public string dialogueDescription = "Description of this dialogue set...";
    
    [Header("Dialogue Sequences")]
    [Tooltip("Main dialogue entries that play in sequence")]
    public DialogueEntry[] dialogueEntries;
    
    [Tooltip("Optional greeting dialogues (played first if available)")]
    public DialogueEntry[] greetings;
    
    [Tooltip("Optional farewell dialogues (played at the end)")]
    public DialogueEntry[] farewells;
    
    [Header("Visual Settings")]
    [Tooltip("Default bubble sprite for this NPC's conversations if no specific bubble is set.")]
    public Sprite defaultConversationBubble;
    
    [Header("Dialogue Behavior")]
    [Tooltip("Should this dialogue loop back to the beginning after completion?")]
    public bool loopDialogue = false;
    
    [Tooltip("Can this dialogue be interrupted by player actions?")]
    public bool canBeInterrupted = true;
    
    [Header("Audio")]
    [Tooltip("Sound effect to play when this dialogue starts")]
    public AudioClip dialogueStartSound;
    
    [Tooltip("Sound effect to play for each character typed (typewriter effect)")]
    public AudioClip typewriterSound;
    
    /// <summary>
    /// Get all dialogue entries that are available at the current time and with current game flags
    /// </summary>
    public DialogueEntry[] GetAvailableDialogues(DialogueEntry[] entries, TimeOfDay currentTime, System.Collections.Generic.List<string> gameFlags)
    {
        if (entries == null) return new DialogueEntry[0];
        
        System.Collections.Generic.List<DialogueEntry> available = new System.Collections.Generic.List<DialogueEntry>();
        
        foreach (var entry in entries)
        {
            if (IsDialogueAvailable(entry, currentTime, gameFlags))
            {
                available.Add(entry);
            }
        }
        
        return available.ToArray();
    }
    
    /// <summary>
    /// Check if a specific dialogue entry is available based on time and flags
    /// </summary>
    public bool IsDialogueAvailable(DialogueEntry entry, TimeOfDay currentTime, System.Collections.Generic.List<string> gameFlags)
    {
        // Check time of day availability
        if (entry.availableTimesOfDay != null && entry.availableTimesOfDay.Length > 0)
        {
            bool timeMatches = false;
            foreach (var timeOfDay in entry.availableTimesOfDay)
            {
                if (currentTime == timeOfDay)
                {
                    timeMatches = true;
                    break;
                }
            }
            if (!timeMatches) return false;
        }
        
        // Check required flags
        if (entry.requiredFlags != null && entry.requiredFlags.Length > 0)
        {
            foreach (var flag in entry.requiredFlags)
            {
                if (gameFlags == null || !gameFlags.Contains(flag))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get the total number of available dialogue entries for current conditions
    /// </summary>
    public int GetAvailableDialogueCount(TimeOfDay currentTime, System.Collections.Generic.List<string> gameFlags)
    {
        return GetAvailableDialogues(dialogueEntries, currentTime, gameFlags).Length;
    }
    
    /// <summary>
    /// Get a random available dialogue entry
    /// </summary>
    public DialogueEntry GetRandomDialogue(TimeOfDay currentTime, System.Collections.Generic.List<string> gameFlags)
    {
        var available = GetAvailableDialogues(dialogueEntries, currentTime, gameFlags);
        if (available.Length == 0) return null;
        
        return available[Random.Range(0, available.Length)];
    }
    
    /// <summary>
    /// Get available choices for a dialogue entry
    /// </summary>
    public DialogueChoice[] GetAvailableChoices(DialogueEntry entry, TimeOfDay currentTime, System.Collections.Generic.List<string> gameFlags)
    {
        if (entry == null || !entry.hasChoices || entry.choices == null) 
            return new DialogueChoice[0];
        
        System.Collections.Generic.List<DialogueChoice> available = new System.Collections.Generic.List<DialogueChoice>();
        
        foreach (var choice in entry.choices)
        {
            if (IsChoiceAvailable(choice, currentTime, gameFlags))
            {
                available.Add(choice);
            }
        }
        
        return available.ToArray();
    }
    
    /// <summary>
    /// Check if a specific choice is available based on time and flags
    /// </summary>
    public bool IsChoiceAvailable(DialogueChoice choice, TimeOfDay currentTime, System.Collections.Generic.List<string> gameFlags)
    {
        if (choice == null) return false;
        
        // Check time of day availability
        if (choice.availableTimesOfDay != null && choice.availableTimesOfDay.Length > 0)
        {
            bool timeMatches = false;
            foreach (var timeOfDay in choice.availableTimesOfDay)
            {
                if (currentTime == timeOfDay)
                {
                    timeMatches = true;
                    break;
                }
            }
            if (!timeMatches) return false;
        }
        
        // Check required flags
        if (choice.requiredFlags != null && choice.requiredFlags.Length > 0)
        {
            foreach (var flag in choice.requiredFlags)
            {
                if (gameFlags == null || !gameFlags.Contains(flag))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get dialogue entry by index safely
    /// </summary>
    public DialogueEntry GetDialogueEntry(int index)
    {
        if (dialogueEntries == null || index < 0 || index >= dialogueEntries.Length)
            return null;
            
        return dialogueEntries[index];
    }
    
    /// <summary>
    /// Find the index of a specific dialogue entry
    /// </summary>
    public int GetDialogueEntryIndex(DialogueEntry entry)
    {
        if (dialogueEntries == null || entry == null) return -1;
        
        for (int i = 0; i < dialogueEntries.Length; i++)
        {
            if (dialogueEntries[i] == entry) return i;
        }
        
        return -1;
    }
    
    /// <summary>
    /// Validation method called by Unity editor
    /// </summary>
    private void OnValidate()
    {
        // Ensure NPC name is not empty
        if (string.IsNullOrEmpty(npcName))
        {
            npcName = name.Replace("_Dialogue", "").Replace("Dialogue", "");
        }
        
        // Ensure all dialogue entries have speaker names
        if (dialogueEntries != null)
        {
            foreach (var entry in dialogueEntries)
            {
                if (string.IsNullOrEmpty(entry.speakerName))
                {
                    entry.speakerName = npcName;
                }
            }
        }
        
        // Ensure greetings have speaker names
        if (greetings != null)
        {
            foreach (var entry in greetings)
            {
                if (string.IsNullOrEmpty(entry.speakerName))
                {
                    entry.speakerName = npcName;
                }
            }
        }
        
        // Ensure farewells have speaker names
        if (farewells != null)
        {
            foreach (var entry in farewells)
            {
                if (string.IsNullOrEmpty(entry.speakerName))
                {
                    entry.speakerName = npcName;
                }
            }
        }
    }
}
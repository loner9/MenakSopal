using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Monologue Data", menuName = "Game/Monologue Data", order = 3)]
public class MonologueData : ScriptableObject
{
    [Header("Monologue Information")]
    public string monologueID;
    [Tooltip("Brief description of this monologue set")]
    public string description;
    
    [Header("Monologue Entries")]
    public MonologueEntry[] monologueEntries;
    
    [Header("Audio")]
    public AudioClip monologueStartSound;
    public AudioClip typewriterSound;
    
    /// <summary>
    /// Get available monologue entries based on current game flags
    /// </summary>
    public MonologueEntry[] GetAvailableMonologues(List<string> gameFlags)
    {
        if (monologueEntries == null) return new MonologueEntry[0];
        
        List<MonologueEntry> available = new List<MonologueEntry>();
        
        foreach (var entry in monologueEntries)
        {
            if (IsMonologueAvailable(entry, gameFlags))
            {
                available.Add(entry);
            }
        }
        
        return available.ToArray();
    }
    
    /// <summary>
    /// Check if a specific monologue entry is available based on flags
    /// </summary>
    public bool IsMonologueAvailable(MonologueEntry entry, List<string> gameFlags)
    {
        // Check required flags - ALL must be present
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
        
        // Check forbidden flags - NONE should be present
        if (entry.forbiddenFlags != null && entry.forbiddenFlags.Length > 0)
        {
            foreach (var flag in entry.forbiddenFlags)
            {
                if (gameFlags != null && gameFlags.Contains(flag))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get a random available monologue entry
    /// </summary>
    public MonologueEntry GetRandomMonologue(List<string> gameFlags)
    {
        var available = GetAvailableMonologues(gameFlags);
        if (available.Length == 0) return null;
        
        return available[Random.Range(0, available.Length)];
    }
}

[System.Serializable]
public class MonologueEntry
{
    [Header("Content")]
    [TextArea(3, 6)]
    public string monologueText;
    
    [Header("Conditions")]
    [Tooltip("All of these flags must be present for this monologue to be available")]
    public string[] requiredFlags;
    
    [Tooltip("None of these flags should be present for this monologue to be available")]
    public string[] forbiddenFlags;
    
    [Header("Consequences")]
    [Tooltip("Flags to add after this monologue is shown (queued until monologue ends)")]
    public string[] flagsToAdd;
    
    [Tooltip("Flags to remove after this monologue is shown")]
    public string[] flagsToRemove;
    
    [Header("Quest Integration")]
    [Tooltip("Quest objective to complete when this monologue is shown")]
    public string objectiveToComplete;
    
    [Tooltip("Quest ID that contains the objective to complete")]
    public string questForObjective;
    
    [Header("Behavior")]
    [Tooltip("Can this monologue be shown multiple times?")]
    public bool isRepeatable = false;
    
    [Tooltip("Priority level - higher numbers show first")]
    public int priority = 0;
    
    [Header("Visual")]
    [Tooltip("Background color/tint for this monologue")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
    
    [Tooltip("Text color for this monologue")]
    public Color textColor = Color.white;
}
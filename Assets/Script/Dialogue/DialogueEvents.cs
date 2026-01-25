using UnityEngine;
using Ink.Runtime;
using System;
using System.Collections.Generic;

/// <summary>
/// Static event hub for dialogue system events.
/// Subscribe to these events to react to dialogue changes.
/// </summary>
public static class DialogueEvents
{
    // Dialogue lifecycle events
    public static event Action<string, NPC> OnDialogueStarted;      // storyId, npc
    public static event Action<string, NPC> OnDialogueEnded;        // storyId, npc
    
    // Line display events
    public static event Action<string, string> OnDialogueLineShown; // speaker, text
    public static event Action<string, string> OnSpeakerChanged;    // oldSpeaker, newSpeaker
    
    // Choice events
    public static event Action<List<Choice>> OnChoicesPresented;    // available choices
    public static event Action<Choice, int> OnChoiceSelected;       // choice, index
    
    // Tag events (for audio, bubbles, etc.)
    public static event Action<string, string> OnDialogueTagParsed; // tagType, tagValue
    
    // Flag sync events
    public static event Action<string[]> OnFlagsChangedFromDialogue; // flags that changed
    
    // Quest trigger events
    public static event Action<string> OnQuestStartedFromDialogue;   // questId
    public static event Action<string, string> OnObjectiveCompletedFromDialogue; // questId, objectiveId

    // Internal methods to invoke events (called by InkStoryManager)
    internal static void InvokeDialogueStarted(string storyId, NPC npc)
    {
        OnDialogueStarted?.Invoke(storyId, npc);
        Debug.Log($"[DialogueEvents] Dialogue started: {storyId} with {npc?.npcName ?? "null"}");
    }

    internal static void InvokeDialogueEnded(string storyId, NPC npc)
    {
        OnDialogueEnded?.Invoke(storyId, npc);
        Debug.Log($"[DialogueEvents] Dialogue ended: {storyId}");
    }

    internal static void InvokeLineShown(string speaker, string text)
    {
        OnDialogueLineShown?.Invoke(speaker, text);
    }

    internal static void InvokeSpeakerChanged(string oldSpeaker, string newSpeaker)
    {
        OnSpeakerChanged?.Invoke(oldSpeaker, newSpeaker);
    }

    internal static void InvokeChoicesPresented(List<Choice> choices)
    {
        OnChoicesPresented?.Invoke(choices);
        Debug.Log($"[DialogueEvents] Presenting {choices.Count} choices");
    }

    internal static void InvokeChoiceSelected(Choice choice, int index)
    {
        OnChoiceSelected?.Invoke(choice, index);
        Debug.Log($"[DialogueEvents] Choice selected: [{index}] {choice.text}");
    }

    internal static void InvokeTagParsed(string tagType, string tagValue)
    {
        OnDialogueTagParsed?.Invoke(tagType, tagValue);
    }

    internal static void InvokeFlagsChanged(string[] flags)
    {
        OnFlagsChangedFromDialogue?.Invoke(flags);
    }

    internal static void InvokeQuestStarted(string questId)
    {
        OnQuestStartedFromDialogue?.Invoke(questId);
        Debug.Log($"[DialogueEvents] Quest started from dialogue: {questId}");
    }

    internal static void InvokeObjectiveCompleted(string questId, string objectiveId)
    {
        OnObjectiveCompletedFromDialogue?.Invoke(questId, objectiveId);
        Debug.Log($"[DialogueEvents] Objective completed from dialogue: {questId}/{objectiveId}");
    }
}

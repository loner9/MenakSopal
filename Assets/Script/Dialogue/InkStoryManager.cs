using System;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

/// <summary>
/// Manages Ink stories for NPC dialogues.
/// Loads, runs, and coordinates Ink stories with the game state.
/// </summary>
public class InkStoryManager : MonoBehaviour
{
    public static InkStoryManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string inkStoriesPath = "Ink/Story";
    [SerializeField] private bool showDebugLogs = true;

    // Currently active story
    private Story currentStory;
    private string currentStoryId;
    private NPC currentNPC;

    // Story cache
    private Dictionary<string, TextAsset> storyCache = new Dictionary<string, TextAsset>();

    // State
    public bool IsDialogueActive => currentStory != null;
    public string CurrentSpeaker { get; private set; }

    private void Awake()
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

    /// <summary>
    /// Start a dialogue with an NPC using their Ink story
    /// </summary>
    public bool StartDialogue(string storyId, NPC npc = null)
    {
        if (IsDialogueActive)
        {
            Debug.LogWarning($"[InkStoryManager] Already in dialogue, ending current first");
            EndDialogue();
        }

        // Load story
        TextAsset storyJson = LoadStoryAsset(storyId);
        if (storyJson == null)
        {
            Debug.LogError($"[InkStoryManager] Could not load story: {storyId}");
            return false;
        }

        try
        {
            currentStory = new Story(storyJson.text);
            currentStoryId = storyId;
            currentNPC = npc;

            // Bind external functions
            BindExternalFunctions();

            // Sync variables from game state
            SyncVariablesFromGame();

            // Fire event
            DialogueEvents.InvokeDialogueStarted(storyId, npc);

            if (showDebugLogs)
                Debug.Log($"[InkStoryManager] Started dialogue: {storyId}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[InkStoryManager] Error starting story {storyId}: {e.Message}");
            currentStory = null;
            return false;
        }
    }

    /// <summary>
    /// Continue the story and get the next line
    /// </summary>
    public DialogueLine ContinueStory()
    {
        if (currentStory == null)
            return null;

        if (!currentStory.canContinue)
        {
            if (currentStory.currentChoices.Count == 0)
            {
                // Story ended
                EndDialogue();
                return null;
            }
            // Waiting for choice
            return null;
        }

        string text = currentStory.Continue();
        text = text.Trim();

        // Parse tags
        var tags = currentStory.currentTags;
        var line = ParseDialogueLine(text, tags);

        // Fire event
        DialogueEvents.InvokeLineShown(line.Speaker, line.Text);

        return line;
    }

    /// <summary>
    /// Get current choices if any
    /// </summary>
    public List<Choice> GetCurrentChoices()
    {
        if (currentStory == null)
            return new List<Choice>();

        var choices = currentStory.currentChoices;
        if (choices.Count > 0)
        {
            DialogueEvents.InvokeChoicesPresented(choices);
        }
        return choices;
    }

    /// <summary>
    /// Select a choice by index
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (currentStory == null || choiceIndex < 0 || choiceIndex >= currentStory.currentChoices.Count)
        {
            Debug.LogWarning($"[InkStoryManager] Invalid choice index: {choiceIndex}");
            return;
        }

        var choice = currentStory.currentChoices[choiceIndex];
        currentStory.ChooseChoiceIndex(choiceIndex);

        DialogueEvents.InvokeChoiceSelected(choice, choiceIndex);
    }

    /// <summary>
    /// End the current dialogue
    /// </summary>
    public void EndDialogue()
    {
        if (currentStory == null)
            return;

        // Sync variables back to game
        SyncVariablesToGame();

        string storyId = currentStoryId;
        NPC npc = currentNPC;

        currentStory = null;
        currentStoryId = null;
        currentNPC = null;
        CurrentSpeaker = null;

        DialogueEvents.InvokeDialogueEnded(storyId, npc);

        if (showDebugLogs)
            Debug.Log($"[InkStoryManager] Ended dialogue: {storyId}");
    }

    /// <summary>
    /// Check if story can continue
    /// </summary>
    public bool CanContinue => currentStory?.canContinue ?? false;

    /// <summary>
    /// Check if there are choices to display
    /// </summary>
    public bool HasChoices => currentStory?.currentChoices.Count > 0;

    #region Story Loading

    private TextAsset LoadStoryAsset(string storyId)
    {
        if (storyCache.TryGetValue(storyId, out TextAsset cached))
            return cached;

        string path = $"{inkStoriesPath}/{storyId}";
        TextAsset asset = Resources.Load<TextAsset>(path);

        if (asset != null)
            storyCache[storyId] = asset;

        return asset;
    }

    #endregion

    #region External Functions

    private void BindExternalFunctions()
    {
        // Quest functions
        currentStory.BindExternalFunction("startQuest", (string questId) =>
        {
            QuestManager.Instance?.StartQuest(questId);
            DialogueEvents.InvokeQuestStarted(questId);
        });

        currentStory.BindExternalFunction("completeQuest", (string questId) =>
        {
            QuestManager.Instance?.CompleteQuest(questId);
        });

        currentStory.BindExternalFunction("completeObjective", (string questId, string objectiveId) =>
        {
            QuestManager.Instance?.CompleteObjective(questId, objectiveId);
            DialogueEvents.InvokeObjectiveCompleted(questId, objectiveId);
        });

        // Flag functions - using centralized FlagManager
        currentStory.BindExternalFunction("addFlag", (string flagName) =>
        {
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.AddFlag(flagName);
            }
            else
            {
                // Fallback to legacy system
                var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
                interactionSystem?.AddGameFlag(flagName);
            }
            DialogueEvents.InvokeFlagsChanged(new[] { flagName });
        });

        currentStory.BindExternalFunction("removeFlag", (string flagName) =>
        {
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.RemoveFlag(flagName);
            }
            else
            {
                var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
                interactionSystem?.RemoveGameFlag(flagName);
            }
        });

        currentStory.BindExternalFunction("hasFlag", (string flagName) =>
        {
            if (FlagManager.Instance != null)
            {
                return FlagManager.Instance.HasFlag(flagName);
            }
            // Fallback to legacy system
            var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
            return interactionSystem?.HasGameFlag(flagName) ?? false;
        });
    }

    #endregion

    #region Variable Sync

    private void SyncVariablesFromGame()
    {
        // Sync flags from game state to Ink variables
        List<string> flags;
        if (FlagManager.Instance != null)
        {
            flags = FlagManager.Instance.GetAllFlags();
        }
        else
        {
            // Fallback to legacy system
            var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
            if (interactionSystem == null) return;
            flags = interactionSystem.GetGameFlags();
        }
        foreach (var flag in flags)
        {
            try
            {
                // Try to set Ink variable if it exists
                if (currentStory.variablesState.GlobalVariableExistsWithName(flag))
                {
                    currentStory.variablesState[flag] = true;
                }
            }
            catch { } // Ink variable doesn't exist - that's okay
        }

        // Sync time of day
        // TODO: Get from actual day/night system
        try
        {
            if (currentStory.variablesState.GlobalVariableExistsWithName("time_of_day"))
            {
                currentStory.variablesState["time_of_day"] = 1; // Default to day
            }
        }
        catch { }
    }

    private void SyncVariablesToGame()
    {
        // After dialogue ends, sync any changed Ink variables back to game state
        // Get all variables from Ink
        foreach (var variableName in currentStory.variablesState)
        {
            var value = currentStory.variablesState[variableName];
            if (value is bool boolValue && boolValue)
            {
                // If it's a true boolean, treat it as a flag
                if (FlagManager.Instance != null)
                {
                    if (!FlagManager.Instance.HasFlag(variableName))
                    {
                        FlagManager.Instance.AddFlag(variableName);
                    }
                }
                else
                {
                    // Fallback to legacy system
                    var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
                    if (interactionSystem != null && !interactionSystem.HasGameFlag(variableName))
                    {
                        interactionSystem.AddGameFlag(variableName);
                    }
                }
            }
        }
    }

    #endregion

    #region Tag Parsing

    private DialogueLine ParseDialogueLine(string text, List<string> tags)
    {
        var line = new DialogueLine
        {
            Text = text,
            Speaker = CurrentSpeaker
        };

        foreach (var tag in tags)
        {
            var parts = tag.Split(':');
            var tagType = parts[0].Trim().ToLower();
            var tagValue = parts.Length > 1 ? string.Join(":", parts, 1, parts.Length - 1).Trim() : "";

            switch (tagType)
            {
                case "speaker":
                    line.Speaker = tagValue;
                    if (CurrentSpeaker != tagValue)
                    {
                        string oldSpeaker = CurrentSpeaker;
                        CurrentSpeaker = tagValue;
                        DialogueEvents.InvokeSpeakerChanged(oldSpeaker, tagValue);
                    }
                    break;

                case "important":
                    line.IsImportant = true;
                    break;

                case "pause":
                    if (float.TryParse(tagValue, out float pause))
                        line.PauseAfter = pause;
                    break;

                case "bubble":
                    line.BubbleType = tagValue;
                    break;

                case "audio":
                    line.AudioClip = tagValue;
                    break;
            }

            DialogueEvents.InvokeTagParsed(tagType, tagValue);
        }

        return line;
    }

    #endregion
}

/// <summary>
/// Represents a single line of dialogue with metadata
/// </summary>
public class DialogueLine
{
    public string Text { get; set; }
    public string Speaker { get; set; }
    public bool IsImportant { get; set; }
    public float PauseAfter { get; set; }
    public string BubbleType { get; set; }
    public string AudioClip { get; set; }
}

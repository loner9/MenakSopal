using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum QuestType
{
    Main,
    Side,
    Daily,
    Collection,
    Delivery
}

public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    Failed,
    Abandoned
}

public enum ObjectiveType
{
    TalkToNPC,        // "Talk to the Blacksmith"
    CollectItems,     // "Collect 5 Iron Ore"  
    VisitLocation,    // "Visit the Ancient Cave"
    DefeatEnemies,    // "Defeat 3 Slimes"
    TimeDelay,        // "Wait until tomorrow"
    FlagCondition,    // "Complete the ritual"
    Custom            // Custom objective with manual completion
}

[System.Serializable]
public class QuestObjective
{
    [Header("Objective Info")]
    public string objectiveID;
    public string description;
    public ObjectiveType type;
    public bool isCompleted = false;
    public bool isOptional = false;

    [Header("Conditions")]
    public string[] requiredFlags;
    public string flagToSetOnComplete;
    public string[] flagsToRemoveOnComplete;

    [Header("Target Settings")]
    public string targetNPC;        // For TalkToNPC objectives
    public string targetItem;       // For CollectItems objectives
    public int targetAmount = 1;    // Amount needed for collection/defeat objectives
    public int currentAmount = 0;   // Current progress
    public string targetLocation;   // For VisitLocation objectives
    public float timeDelay = 0f;    // For TimeDelay objectives (in game hours)

    [Header("UI")]
    public bool showProgress = true; // Show "3/5" style progress

    public bool IsAvailable(List<string> gameFlags)
    {
        if (requiredFlags == null || requiredFlags.Length == 0)
            return true;

        return requiredFlags.All(flag => gameFlags.Contains(flag));
    }

    public string GetProgressText()
    {
        if (!showProgress) return description;

        switch (type)
        {
            case ObjectiveType.CollectItems:
            case ObjectiveType.DefeatEnemies:
                return $"{description} ({currentAmount}/{targetAmount})";
            default:
                return description;
        }
    }

    public float GetProgressPercentage()
    {
        switch (type)
        {
            case ObjectiveType.CollectItems:
            case ObjectiveType.DefeatEnemies:
                return targetAmount > 0 ? (float)currentAmount / targetAmount : 0f;
            case ObjectiveType.TalkToNPC:
            case ObjectiveType.VisitLocation:
            case ObjectiveType.FlagCondition:
            case ObjectiveType.Custom:
                return isCompleted ? 1f : 0f;
            case ObjectiveType.TimeDelay:
                // Time delay progress would need to be calculated by QuestManager
                return isCompleted ? 1f : 0f;
            default:
                return 0f;
        }
    }
}

[System.Serializable]
public class QuestReward
{
    public QuestRewardType type;
    public string itemID;
    public int amount;
    public string[] flagsToAdd;
    public string customRewardDescription;
}

public enum QuestRewardType
{
    None,
    Item,
    Experience,
    Gold,
    Flags,
    Custom
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public string questID;
    public string questTitle;
    [TextArea(3, 5)]
    public string questDescription;
    public QuestType questType = QuestType.Side;
    public int questLevel = 1;
    public Sprite questIcon;

    [Header("Quest Flow")]
    public string[] requiredFlags;          // Flags needed to start this quest
    public string[] flagsOnStart;           // Flags set when quest starts
    public string[] flagsToRemoveOnStart;   // Flags removed when quest starts
    public string[] flagsOnComplete;        // Flags set when quest completes
    public string[] flagsToRemoveOnComplete;// Flags removed when quest completes
    public string[] flagsOnFail;            // Flags set when quest fails
    public string[] flagsToRemoveOnFail;    // Flags removed when quest fails

    [Header("Quest Chain")]
    [Tooltip("Quest IDs that must be completed before this quest can start")]
    public string[] prerequisiteQuestIDs;
    [Tooltip("Quest IDs that become available when this quest completes")]
    public string[] unlocksQuestIDs;
    [Tooltip("Is this quest part of a chain?")]
    public bool isChainQuest = false;

    [Header("Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Rewards")]
    public List<QuestReward> rewards = new List<QuestReward>();

    [Header("Quest Settings")]
    public bool isRepeatable = false;
    public bool canAbandon = true;
    public bool autoComplete = true;        // Auto-complete when all objectives done
    public float timeLimit = 0f;            // 0 = no time limit (in game hours)

    [Header("UI Settings")]
    public bool showInJournal = true;
    public bool trackByDefault = true;
    public Color questColor = Color.white;

    // Runtime quest state (not serialized)
    [System.NonSerialized] public QuestStatus status = QuestStatus.NotStarted;
    [System.NonSerialized] public float startTime = 0f;

    public bool CanStart(List<string> gameFlags)
    {
        if (status != QuestStatus.NotStarted && !isRepeatable)
            return false;

        if (requiredFlags == null || requiredFlags.Length == 0)
            return true;

        return requiredFlags.All(flag => gameFlags.Contains(flag));
    }

    public bool IsCompleted()
    {
        if (objectives == null || objectives.Count == 0)
            return false;

        // Check if all non-optional objectives are completed
        return objectives.Where(obj => !obj.isOptional).All(obj => obj.isCompleted);
    }

    public int GetCompletedObjectiveCount()
    {
        return objectives?.Count(obj => obj.isCompleted) ?? 0;
    }

    public int GetTotalObjectiveCount()
    {
        return objectives?.Count ?? 0;
    }

    public float GetProgressPercentage()
    {
        if (objectives == null || objectives.Count == 0)
            return 0f;

        int totalObjectives = objectives.Count(obj => !obj.isOptional);
        int completedObjectives = objectives.Count(obj => !obj.isOptional && obj.isCompleted);

        return totalObjectives > 0 ? (float)completedObjectives / totalObjectives : 0f;
    }

    public List<QuestObjective> GetAvailableObjectives(List<string> gameFlags)
    {
        if (objectives == null) return new List<QuestObjective>();

        return objectives.Where(obj => obj.IsAvailable(gameFlags)).ToList();
    }

    public QuestObjective GetObjective(string objectiveID)
    {
        return objectives?.FirstOrDefault(obj => obj.objectiveID == objectiveID);
    }

    // Validation method for editor
    public void ValidateQuest()
    {
        if (string.IsNullOrEmpty(questID))
            Debug.LogWarning($"Quest '{questTitle}' has no questID set!");

        if (objectives != null)
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                var obj = objectives[i];
                if (string.IsNullOrEmpty(obj.objectiveID))
                    obj.objectiveID = $"obj_{i}";
            }
        }
    }

    private void OnValidate()
    {
        ValidateQuest();
    }
}
using System;

/// <summary>
/// Static event hub for the Quest system.
/// Provides global access to quest events without requiring a reference to QuestManager.
/// </summary>
public static class QuestEvents
{
    #region Quest Lifecycle Events

    /// <summary>
    /// Fired when a quest becomes available (prerequisites met)
    /// </summary>
    public static event Action<QuestData> OnQuestAvailable;

    /// <summary>
    /// Fired when a quest is started
    /// </summary>
    public static event Action<QuestData> OnQuestStarted;

    /// <summary>
    /// Fired when a quest is completed successfully
    /// </summary>
    public static event Action<QuestData> OnQuestCompleted;

    /// <summary>
    /// Fired when a quest fails
    /// </summary>
    public static event Action<QuestData> OnQuestFailed;

    /// <summary>
    /// Fired when a quest is abandoned by the player
    /// </summary>
    public static event Action<QuestData> OnQuestAbandoned;

    #endregion

    #region Objective Events

    /// <summary>
    /// Fired when an objective is completed
    /// </summary>
    public static event Action<QuestData, QuestObjective> OnObjectiveCompleted;

    /// <summary>
    /// Fired when objective progress is updated (e.g., 3/5 items collected)
    /// </summary>
    public static event Action<QuestData, QuestObjective, int> OnObjectiveProgressUpdated;

    #endregion

    #region Invoke Methods

    public static void InvokeQuestAvailable(QuestData quest)
    {
        OnQuestAvailable?.Invoke(quest);
    }

    public static void InvokeQuestStarted(QuestData quest)
    {
        OnQuestStarted?.Invoke(quest);
    }

    public static void InvokeQuestCompleted(QuestData quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }

    public static void InvokeQuestFailed(QuestData quest)
    {
        OnQuestFailed?.Invoke(quest);
    }

    public static void InvokeQuestAbandoned(QuestData quest)
    {
        OnQuestAbandoned?.Invoke(quest);
    }

    public static void InvokeObjectiveCompleted(QuestData quest, QuestObjective objective)
    {
        OnObjectiveCompleted?.Invoke(quest, objective);
    }

    public static void InvokeObjectiveProgressUpdated(QuestData quest, QuestObjective objective, int newProgress)
    {
        OnObjectiveProgressUpdated?.Invoke(quest, objective, newProgress);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Clears all event subscribers. Useful for scene transitions or testing.
    /// </summary>
    public static void ClearAllListeners()
    {
        OnQuestAvailable = null;
        OnQuestStarted = null;
        OnQuestCompleted = null;
        OnQuestFailed = null;
        OnQuestAbandoned = null;
        OnObjectiveCompleted = null;
        OnObjectiveProgressUpdated = null;
    }

    #endregion
}

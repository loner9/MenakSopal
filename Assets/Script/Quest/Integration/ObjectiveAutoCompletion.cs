using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Automatic completion system for different objective types.
/// Extends the quest system to automatically detect and complete objectives based on game events.
/// </summary>
public class ObjectiveAutoCompletion : MonoBehaviour
{
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private static ObjectiveAutoCompletion instance;
    public static ObjectiveAutoCompletion Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectiveAutoCompletion>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ObjectiveAutoCompletion");
                    instance = go.AddComponent<ObjectiveAutoCompletion>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private QuestManager questManager;
    private NPCInteractionSystem npcInteractionSystem;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeReferences();
        SetupObjectiveWatchers();
    }
    
    void InitializeReferences()
    {
        questManager = QuestManager.Instance;
        npcInteractionSystem = FindObjectOfType<NPCInteractionSystem>();
        
        LogDebug("ObjectiveAutoCompletion system initialized");
    }
    
    void SetupObjectiveWatchers()
    {
        // Set up flag-based objective completion
        SetupFlagConditionObjectives();
        
        // Set up time-based objective completion
        SetupTimeDelayObjectives();
        
        LogDebug("Objective watchers configured");
    }
    
    #region FlagCondition Objectives
    
    /// <summary>
    /// Set up automatic completion for FlagCondition objectives
    /// </summary>
    void SetupFlagConditionObjectives()
    {
        if (questManager == null) return;
        
        // Watch for any flag changes and check FlagCondition objectives
        FlagMonitorSystem.OnFlagAdded += CheckFlagConditionObjectives;
    }
    
    void CheckFlagConditionObjectives(string flagAdded)
    {
        if (questManager == null) return;
        
        var activeQuests = questManager.ActiveQuests;
        if (activeQuests == null) return;
        
        foreach (var quest in activeQuests)
        {
            if (quest.objectives == null) continue;
            
            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.FlagCondition && !objective.isCompleted)
                {
                    // Check if the required flag for this objective was just added
                    if (objective.targetItem == flagAdded || // targetItem used as flag name for FlagCondition
                        (objective.requiredFlags != null && System.Array.Exists(objective.requiredFlags, flag => flag == flagAdded)))
                    {
                        bool completed = questManager.CompleteObjective(quest.questID, objective.objectiveID);
                        if (completed)
                        {
                            LogDebug($"Auto-completed FlagCondition objective '{objective.description}' due to flag: {flagAdded}");
                        }
                    }
                }
            }
        }
    }
    
    #endregion
    
    #region TimeDelay Objectives
    
    /// <summary>
    /// Set up automatic completion for TimeDelay objectives
    /// </summary>
    void SetupTimeDelayObjectives()
    {
        // Start coroutine to check time-based objectives periodically
        StartCoroutine(CheckTimeDelayObjectives());
    }
    
    System.Collections.IEnumerator CheckTimeDelayObjectives()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // Check every minute
            
            if (questManager == null) continue;
            
            var activeQuests = questManager.ActiveQuests;
            if (activeQuests == null) continue;
            
            var dayNightCycle = DayNightCycle.Instance;
            if (dayNightCycle == null) continue;
            
            float currentTime = dayNightCycle.CurrentTime;
            
            foreach (var quest in activeQuests)
            {
                if (quest.objectives == null) continue;
                
                foreach (var objective in quest.objectives)
                {
                    if (objective.type == ObjectiveType.TimeDelay && !objective.isCompleted)
                    {
                        // Check if enough time has passed
                        // Note: This is a simple implementation - you might want to store start times
                        if (currentTime >= objective.timeDelay || objective.timeDelay <= 0)
                        {
                            bool completed = questManager.CompleteObjective(quest.questID, objective.objectiveID);
                            if (completed)
                            {
                                LogDebug($"Auto-completed TimeDelay objective '{objective.description}' at time: {currentTime}");
                            }
                        }
                    }
                }
            }
        }
    }
    
    #endregion
    
    #region DefeatEnemies Objectives
    
    /// <summary>
    /// Call this when an enemy is defeated to check DefeatEnemies objectives
    /// </summary>
    /// <param name="enemyType">Type/name of the defeated enemy</param>
    public void OnEnemyDefeated(string enemyType)
    {
        if (questManager == null || string.IsNullOrEmpty(enemyType)) return;
        
        var activeQuests = questManager.ActiveQuests;
        if (activeQuests == null) return;
        
        foreach (var quest in activeQuests)
        {
            if (quest.objectives == null) continue;
            
            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.DefeatEnemies && !objective.isCompleted)
                {
                    // Check if this enemy type matches the objective
                    if (string.Equals(objective.targetItem, enemyType, System.StringComparison.OrdinalIgnoreCase))
                    {
                        // Increment progress
                        objective.currentAmount++;
                        
                        LogDebug($"Enemy defeated progress: {objective.currentAmount}/{objective.targetAmount} {enemyType}");
                        
                        // Check if objective is complete
                        if (objective.currentAmount >= objective.targetAmount)
                        {
                            bool completed = questManager.CompleteObjective(quest.questID, objective.objectiveID);
                            if (completed)
                            {
                                LogDebug($"Auto-completed DefeatEnemies objective '{objective.description}'");
                            }
                        }
                    }
                }
            }
        }
    }
    
    #endregion
    
    #region VisitLocation Objectives
    
    /// <summary>
    /// Call this when player visits a location to check VisitLocation objectives
    /// </summary>
    /// <param name="locationName">Name/ID of the visited location</param>
    public void OnLocationVisited(string locationName)
    {
        if (questManager == null || string.IsNullOrEmpty(locationName)) return;
        
        var activeQuests = questManager.ActiveQuests;
        if (activeQuests == null) return;
        
        foreach (var quest in activeQuests)
        {
            if (quest.objectives == null) continue;
            
            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.VisitLocation && !objective.isCompleted)
                {
                    // Check if this location matches the objective
                    if (string.Equals(objective.targetLocation, locationName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        bool completed = questManager.CompleteObjective(quest.questID, objective.objectiveID);
                        if (completed)
                        {
                            LogDebug($"Auto-completed VisitLocation objective '{objective.description}' at: {locationName}");
                        }
                    }
                }
            }
        }
    }
    
    #endregion
    
    #region CollectItems Integration
    
    /// <summary>
    /// This should be called by your QuestTrigger system for CollectItems objectives
    /// No changes needed - already works with progressAmount in QuestTrigger
    /// </summary>
    public void OnItemCollected(string itemType, int amount = 1)
    {
        // This functionality is already handled by QuestTrigger with progressAmount
        // This method is here for consistency and future expansion
        LogDebug($"Item collected: {amount}x {itemType}");
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Get all active objectives of a specific type
    /// </summary>
    public List<QuestObjective> GetActiveObjectivesByType(ObjectiveType type)
    {
        var objectives = new List<QuestObjective>();
        
        if (questManager == null) return objectives;
        
        var activeQuests = questManager.ActiveQuests;
        if (activeQuests == null) return objectives;
        
        foreach (var quest in activeQuests)
        {
            if (quest.objectives == null) continue;
            
            foreach (var objective in quest.objectives)
            {
                if (objective.type == type && !objective.isCompleted)
                {
                    objectives.Add(objective);
                }
            }
        }
        
        return objectives;
    }
    
    /// <summary>
    /// Force check all objectives for completion (useful for debugging)
    /// </summary>
    [ContextMenu("Force Check All Objectives")]
    public void ForceCheckAllObjectives()
    {
        LogDebug("Force checking all objectives...");
        
        // Check flag conditions
        if (npcInteractionSystem != null)
        {
            var flags = npcInteractionSystem.GetGameFlags();
            foreach (string flag in flags)
            {
                CheckFlagConditionObjectives(flag);
            }
        }
        
        LogDebug("Force check completed");
    }
    
    #endregion
    
    #region Utility
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ObjectiveAutoCompletion] {message}");
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            FlagMonitorSystem.OnFlagAdded -= CheckFlagConditionObjectives;
        }
    }
    
    #endregion
}
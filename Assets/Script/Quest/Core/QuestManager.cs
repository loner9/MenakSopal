using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    [Header("Quest System Settings")]
    public bool enableQuestSystem = true;
    public bool showDebugLogs = false;
    
    [Header("Quest Collections")]
    public List<QuestData> availableQuests = new List<QuestData>();
    
    // Singleton instance
    public static QuestManager Instance { get; private set; }
    
    // Quest tracking
    private List<QuestData> activeQuests = new List<QuestData>();
    private List<QuestData> completedQuests = new List<QuestData>();
    private List<QuestData> failedQuests = new List<QuestData>();
    
    // System references
    private NPCInteractionSystem interactionSystem;
    private DayNightCycle dayNightCycle;
    
    // Events for UI and other systems
    public System.Action<QuestData> OnQuestStarted;
    public System.Action<QuestData> OnQuestCompleted;
    public System.Action<QuestData> OnQuestFailed;
    public System.Action<QuestData> OnQuestAbandoned;
    public System.Action<QuestData, QuestObjective> OnObjectiveCompleted;
    public System.Action<QuestData, QuestObjective> OnObjectiveUpdated;
    
    // Properties
    public List<QuestData> ActiveQuests => new List<QuestData>(activeQuests);
    public List<QuestData> CompletedQuests => new List<QuestData>(completedQuests);
    public List<QuestData> FailedQuests => new List<QuestData>(failedQuests);
    public int ActiveQuestCount => activeQuests.Count;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeQuestSystem();
    }
    
    private void Start()
    {
        // Find required systems
        interactionSystem = FindObjectOfType<NPCInteractionSystem>();
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        
        if (interactionSystem == null && enableQuestSystem)
        {
            Debug.LogWarning("QuestManager: NPCInteractionSystem not found! Quest system may not work properly.");
        }
        
        ValidateQuestData();
    }
    
    private void InitializeQuestSystem()
    {
        if (!enableQuestSystem) return;
        
        // Initialize quest collections
        activeQuests.Clear();
        completedQuests.Clear();
        failedQuests.Clear();
        
        // Load available quests from Resources if list is empty
        if (availableQuests.Count == 0)
        {
            LoadQuestsFromResources();
        }
        
        if (showDebugLogs)
            Debug.Log($"QuestManager initialized with {availableQuests.Count} available quests");
    }
    
    private void LoadQuestsFromResources()
    {
        QuestData[] questAssets = Resources.LoadAll<QuestData>("Quests");
        availableQuests.AddRange(questAssets);
        
        if (showDebugLogs)
            Debug.Log($"Loaded {questAssets.Length} quests from Resources/Quests");
    }
    
    private void ValidateQuestData()
    {
        foreach (var quest in availableQuests)
        {
            if (quest != null)
                quest.ValidateQuest();
        }
    }
    
    #region Quest Management
    
    public bool StartQuest(string questID)
    {
        if (!enableQuestSystem) return false;
        
        QuestData quest = GetQuestByID(questID);
        if (quest == null)
        {
            Debug.LogWarning($"QuestManager: Quest with ID '{questID}' not found!");
            return false;
        }
        
        return StartQuest(quest);
    }
    
    public bool StartQuest(QuestData quest)
    {
        if (!enableQuestSystem || quest == null) return false;
        
        // Check if quest can be started
        List<string> gameFlags = GetGameFlags();
        if (!quest.CanStart(gameFlags))
        {
            if (showDebugLogs)
                Debug.Log($"Cannot start quest '{quest.questTitle}' - requirements not met");
            return false;
        }
        
        // Check if already active
        if (IsQuestActive(quest.questID))
        {
            if (showDebugLogs)
                Debug.Log($"Quest '{quest.questTitle}' is already active");
            return false;
        }
        
        // Start the quest
        quest.status = QuestStatus.Active;
        quest.startTime = dayNightCycle?.CurrentTime ?? 0f;
        activeQuests.Add(quest);
        
        // Set start flags
        if (quest.flagsOnStart != null)
        {
            foreach (string flag in quest.flagsOnStart)
            {
                AddGameFlag(flag);
            }
        }
        
        // Initialize objectives
        foreach (var objective in quest.objectives)
        {
            objective.isCompleted = false;
            objective.currentAmount = 0;
        }
        
        OnQuestStarted?.Invoke(quest);
        
        if (showDebugLogs)
            Debug.Log($"Started quest: {quest.questTitle}");
            
        return true;
    }
    
    public bool CompleteQuest(string questID)
    {
        QuestData quest = GetActiveQuest(questID);
        if (quest == null) return false;
        
        return CompleteQuest(quest);
    }
    
    public bool CompleteQuest(QuestData quest)
    {
        if (!enableQuestSystem || quest == null) return false;
        
        if (!activeQuests.Contains(quest))
        {
            Debug.LogWarning($"Trying to complete quest '{quest.questTitle}' that is not active");
            return false;
        }
        
        // Complete the quest
        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        
        // Set completion flags
        if (quest.flagsOnComplete != null)
        {
            foreach (string flag in quest.flagsOnComplete)
            {
                AddGameFlag(flag);
            }
        }
        
        // Give rewards
        GiveQuestRewards(quest);
        
        OnQuestCompleted?.Invoke(quest);
        
        if (showDebugLogs)
            Debug.Log($"Completed quest: {quest.questTitle}");
            
        return true;
    }
    
    public bool FailQuest(string questID)
    {
        QuestData quest = GetActiveQuest(questID);
        if (quest == null) return false;
        
        return FailQuest(quest);
    }
    
    public bool FailQuest(QuestData quest)
    {
        if (!enableQuestSystem || quest == null) return false;
        
        if (!activeQuests.Contains(quest))
            return false;
            
        quest.status = QuestStatus.Failed;
        activeQuests.Remove(quest);
        failedQuests.Add(quest);
        
        // Set failure flags
        if (quest.flagsOnFail != null)
        {
            foreach (string flag in quest.flagsOnFail)
            {
                AddGameFlag(flag);
            }
        }
        
        OnQuestFailed?.Invoke(quest);
        
        if (showDebugLogs)
            Debug.Log($"Failed quest: {quest.questTitle}");
            
        return true;
    }
    
    public bool AbandonQuest(string questID)
    {
        QuestData quest = GetActiveQuest(questID);
        if (quest == null || !quest.canAbandon) return false;
        
        quest.status = QuestStatus.Abandoned;
        activeQuests.Remove(quest);
        
        OnQuestAbandoned?.Invoke(quest);
        
        if (showDebugLogs)
            Debug.Log($"Abandoned quest: {quest.questTitle}");
            
        return true;
    }
    
    #endregion
    
    #region Objective Management
    
    public bool CompleteObjective(string questID, string objectiveID)
    {
        QuestData quest = GetActiveQuest(questID);
        if (quest == null) return false;
        
        QuestObjective objective = quest.GetObjective(objectiveID);
        if (objective == null || objective.isCompleted) return false;
        
        objective.isCompleted = true;
        
        // Set objective completion flag
        if (!string.IsNullOrEmpty(objective.flagToSetOnComplete))
        {
            AddGameFlag(objective.flagToSetOnComplete);
        }
        
        OnObjectiveCompleted?.Invoke(quest, objective);
        
        // Check if quest should auto-complete
        if (quest.autoComplete && quest.IsCompleted())
        {
            CompleteQuest(quest);
        }
        
        if (showDebugLogs)
            Debug.Log($"Completed objective '{objective.description}' in quest '{quest.questTitle}'");
            
        return true;
    }
    
    public bool UpdateObjectiveProgress(string questID, string objectiveID, int amount = 1)
    {
        QuestData quest = GetActiveQuest(questID);
        if (quest == null) return false;
        
        QuestObjective objective = quest.GetObjective(objectiveID);
        if (objective == null || objective.isCompleted) return false;
        
        objective.currentAmount = Mathf.Min(objective.currentAmount + amount, objective.targetAmount);
        
        OnObjectiveUpdated?.Invoke(quest, objective);
        
        // Check if objective is now complete
        if (objective.currentAmount >= objective.targetAmount)
        {
            CompleteObjective(questID, objectiveID);
        }
        
        return true;
    }
    
    #endregion
    
    #region Quest Queries
    
    public QuestData GetQuestByID(string questID)
    {
        return availableQuests.FirstOrDefault(q => q.questID == questID);
    }
    
    public QuestData GetActiveQuest(string questID)
    {
        return activeQuests.FirstOrDefault(q => q.questID == questID);
    }
    
    public bool IsQuestActive(string questID)
    {
        return activeQuests.Any(q => q.questID == questID);
    }
    
    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.Any(q => q.questID == questID);
    }
    
    public bool IsQuestFailed(string questID)
    {
        return failedQuests.Any(q => q.questID == questID);
    }
    
    public List<QuestData> GetQuestsByType(QuestType questType)
    {
        return availableQuests.Where(q => q.questType == questType).ToList();
    }
    
    public List<QuestData> GetAvailableQuests()
    {
        List<string> gameFlags = GetGameFlags();
        return availableQuests.Where(q => q.CanStart(gameFlags)).ToList();
    }
    
    #endregion
    
    #region Rewards
    
    private void GiveQuestRewards(QuestData quest)
    {
        if (quest.rewards == null) return;
        
        foreach (var reward in quest.rewards)
        {
            switch (reward.type)
            {
                case QuestRewardType.Flags:
                    if (reward.flagsToAdd != null)
                    {
                        foreach (string flag in reward.flagsToAdd)
                        {
                            AddGameFlag(flag);
                        }
                    }
                    break;
                    
                case QuestRewardType.Item:
                case QuestRewardType.Experience:
                case QuestRewardType.Gold:
                case QuestRewardType.Custom:
                    // These will be implemented when inventory/progression systems are added
                    if (showDebugLogs)
                        Debug.Log($"Reward {reward.type} not yet implemented: {reward.itemID} x{reward.amount}");
                    break;
            }
        }
    }
    
    #endregion
    
    #region Flag Integration
    
    private List<string> GetGameFlags()
    {
        if (interactionSystem != null)
            return interactionSystem.GetGameFlags();
        
        return new List<string>();
    }
    
    private void AddGameFlag(string flag)
    {
        if (interactionSystem != null)
            interactionSystem.AddGameFlag(flag);
    }
    
    #endregion
    
    #region Save/Load System
    
    [System.Serializable]
    public class QuestManagerSaveData
    {
        public List<string> activeQuestIDs = new List<string>();
        public List<string> completedQuestIDs = new List<string>();
        public List<string> failedQuestIDs = new List<string>();
        public List<QuestSaveData> questSaveData = new List<QuestSaveData>();
    }
    
    [System.Serializable]
    public class QuestSaveData
    {
        public string questID;
        public QuestStatus status;
        public float startTime;
        public List<ObjectiveSaveData> objectives = new List<ObjectiveSaveData>();
    }
    
    [System.Serializable]
    public class ObjectiveSaveData
    {
        public string objectiveID;
        public bool isCompleted;
        public int currentAmount;
    }
    
    public QuestManagerSaveData GetSaveData()
    {
        var saveData = new QuestManagerSaveData();
        
        // Save quest IDs by status
        saveData.activeQuestIDs.AddRange(activeQuests.Select(q => q.questID));
        saveData.completedQuestIDs.AddRange(completedQuests.Select(q => q.questID));
        saveData.failedQuestIDs.AddRange(failedQuests.Select(q => q.questID));
        
        // Save detailed quest data
        foreach (var quest in activeQuests.Concat(completedQuests).Concat(failedQuests))
        {
            var questSave = new QuestSaveData
            {
                questID = quest.questID,
                status = quest.status,
                startTime = quest.startTime
            };
            
            if (quest.objectives != null)
            {
                questSave.objectives.AddRange(quest.objectives.Select(obj => new ObjectiveSaveData
                {
                    objectiveID = obj.objectiveID,
                    isCompleted = obj.isCompleted,
                    currentAmount = obj.currentAmount
                }));
            }
            
            saveData.questSaveData.Add(questSave);
        }
        
        return saveData;
    }
    
    public void LoadSaveData(QuestManagerSaveData saveData)
    {
        if (saveData == null) return;
        
        // Clear current quest data
        activeQuests.Clear();
        completedQuests.Clear();
        failedQuests.Clear();
        
        // Load quests by status
        foreach (string questID in saveData.activeQuestIDs)
        {
            QuestData quest = GetQuestByID(questID);
            if (quest != null)
            {
                quest.status = QuestStatus.Active;
                activeQuests.Add(quest);
            }
        }
        
        foreach (string questID in saveData.completedQuestIDs)
        {
            QuestData quest = GetQuestByID(questID);
            if (quest != null)
            {
                quest.status = QuestStatus.Completed;
                completedQuests.Add(quest);
            }
        }
        
        foreach (string questID in saveData.failedQuestIDs)
        {
            QuestData quest = GetQuestByID(questID);
            if (quest != null)
            {
                quest.status = QuestStatus.Failed;
                failedQuests.Add(quest);
            }
        }
        
        // Restore detailed quest data
        foreach (var questSave in saveData.questSaveData)
        {
            QuestData quest = GetQuestByID(questSave.questID);
            if (quest != null)
            {
                quest.status = questSave.status;
                quest.startTime = questSave.startTime;
                
                // Restore objective progress
                if (quest.objectives != null)
                {
                    foreach (var objSave in questSave.objectives)
                    {
                        var objective = quest.GetObjective(objSave.objectiveID);
                        if (objective != null)
                        {
                            objective.isCompleted = objSave.isCompleted;
                            objective.currentAmount = objSave.currentAmount;
                        }
                    }
                }
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"Loaded quest data - Active: {activeQuests.Count}, Completed: {completedQuests.Count}, Failed: {failedQuests.Count}");
    }
    
    #endregion
    
    #region Utility Methods
    
    public void ResetAllQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        failedQuests.Clear();
        
        foreach (var quest in availableQuests)
        {
            quest.status = QuestStatus.NotStarted;
            quest.startTime = 0f;
            
            if (quest.objectives != null)
            {
                foreach (var objective in quest.objectives)
                {
                    objective.isCompleted = false;
                    objective.currentAmount = 0;
                }
            }
        }
        
        Debug.Log("All quests have been reset");
    }
    
    #endregion
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
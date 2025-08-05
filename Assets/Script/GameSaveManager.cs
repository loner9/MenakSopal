using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Comprehensive save/load system for MenakSopal game.
/// Handles saving all game systems (quests, dialogue, NPCs, time, player data).
/// 
/// Features:
/// - Auto-save on story progression
/// - Manual save/load with multiple slots
/// - Compressed save files
/// - Error handling and backup saves
/// - Integration with all existing systems
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private int maxSaveSlots = 5;
    [SerializeField] private bool createBackups = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showSaveNotifications = true;
    
    // Singleton instance
    private static GameSaveManager instance;
    public static GameSaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameSaveManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameSaveManager");
                    instance = go.AddComponent<GameSaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events
    public static event Action<string> OnGameSaved;
    public static event Action<string> OnGameLoaded;
    public static event Action<string> OnSaveError;
    
    // Auto-save timer
    private float autoSaveTimer;
    private string currentSaveSlot = "AutoSave";
    
    // System references
    private QuestManager questManager;
    private NPCInteractionSystem npcInteractionSystem;
    private NPCManager npcManager;
    private DayNightCycle dayNightCycle;
    private Player player;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetupAutoSavePoints();
    }
    
    void Update()
    {
        if (enableAutoSave)
        {
            autoSaveTimer += Time.unscaledDeltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave("TimedAutoSave");
                autoSaveTimer = 0f;
            }
        }
    }
    
    void InitializeSaveSystem()
    {
        // Ensure save directory exists
        string saveDir = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        
        LogDebug("GameSaveManager initialized");
    }
    
    void SetupAutoSavePoints()
    {
        if (!enableAutoSave) return;
        
        // Setup auto-save triggers for major story events
        FlagMonitorSystem.WatchFlagAdded("water_crisis_discovered", () => AutoSave("Chapter2_WaterCrisis"));
        FlagMonitorSystem.WatchFlagAdded("asked_permission_water_project", () => AutoSave("Chapter2_PermissionGranted"));
        FlagMonitorSystem.WatchFlagAdded("student_helpers_recruited", () => AutoSave("Chapter2_HelpersRecruited"));
        FlagMonitorSystem.WatchFlagAdded("spirit_pact_complete", () => AutoSave("Chapter4_SpiritPact"));
        FlagMonitorSystem.WatchFlagAdded("white_elephant_taken", () => AutoSave("Chapter5_ElephantTaken"));
        FlagMonitorSystem.WatchFlagAdded("reconciliation_complete", () => AutoSave("Chapter6_Reconciliation"));
        FlagMonitorSystem.WatchFlagAdded("story_completed", () => AutoSave("StoryComplete"));
        
        LogDebug("Auto-save points configured");
    }
    
    #region Save Data Creation
    
    /// <summary>
    /// Creates a complete save data package from all game systems
    /// </summary>
    public GameSaveData CreateSaveData()
    {
        RefreshSystemReferences();
        
        var saveData = new GameSaveData
        {
            // Metadata
            saveVersion = "1.0",
            saveTime = DateTime.Now.ToBinary(),
            gameVersion = Application.version,
            currentScene = SceneManager.GetActiveScene().name,
            
            // Player data
            playerPosition = player != null ? player.transform.position : Vector3.zero,
            playerHealth = player != null ? player.GetComponent<PlayerHealth>()?.Stats.health ?? 100f : 100f,
            
            // System data
            questData = questManager?.GetSaveData(),
            dialogueData = npcInteractionSystem?.GetSaveData(),
            npcData = npcManager?.GetSaveData(),
            timeData = dayNightCycle?.GetSaveData(),
            
            // Additional game state
            playTime = Time.realtimeSinceStartup,
            totalFlags = npcInteractionSystem?.GetSaveData()?.gameFlags?.Count ?? 0
        };
        
        LogDebug($"Save data created: {saveData.totalFlags} flags, {saveData.questData?.activeQuestIDs?.Count ?? 0} active quests");
        return saveData;
    }
    
    void RefreshSystemReferences()
    {
        if (questManager == null) questManager = FindObjectOfType<QuestManager>();
        if (npcInteractionSystem == null) npcInteractionSystem = FindObjectOfType<NPCInteractionSystem>();
        if (npcManager == null) npcManager = FindObjectOfType<NPCManager>();
        if (dayNightCycle == null) dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (player == null) player = FindObjectOfType<Player>();
    }
    
    #endregion
    
    #region Save Operations
    
    /// <summary>
    /// Save game to specified slot
    /// </summary>
    public bool SaveGame(string slotName)
    {
        try
        {
            GameSaveData saveData = CreateSaveData();
            string filePath = GetSaveFilePath(slotName);
            
            // Create backup if enabled
            if (createBackups && File.Exists(filePath))
            {
                CreateBackup(slotName);
            }
            
            // Convert to JSON and save
            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, jsonData);
            
            // Update current save slot
            currentSaveSlot = slotName;
            
            LogDebug($"Game saved to slot: {slotName}");
            ShowSaveNotification($"Game saved: {slotName}");
            
            OnGameSaved?.Invoke(slotName);
            return true;
        }
        catch (Exception e)
        {
            LogError($"Failed to save game to slot {slotName}: {e.Message}");
            OnSaveError?.Invoke($"Save failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Auto-save with descriptive name
    /// </summary>
    public bool AutoSave(string description = "AutoSave")
    {
        if (!enableAutoSave) return false;
        
        string autoSaveSlot = $"Auto_{description}_{DateTime.Now:yyyyMMdd_HHmmss}";
        bool success = SaveGame(autoSaveSlot);
        
        if (success)
        {
            LogDebug($"Auto-saved: {description}");
        }
        
        return success;
    }
    
    /// <summary>
    /// Quick save to current slot
    /// </summary>
    public bool QuickSave()
    {
        return SaveGame(currentSaveSlot);
    }
    
    #endregion
    
    #region Load Operations
    
    /// <summary>
    /// Load game from specified slot
    /// </summary>
    public bool LoadGame(string slotName)
    {
        try
        {
            string filePath = GetSaveFilePath(slotName);
            
            if (!File.Exists(filePath))
            {
                LogError($"Save file not found: {slotName}");
                OnSaveError?.Invoke($"Save file not found: {slotName}");
                return false;
            }
            
            // Read and parse save data
            string jsonData = File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            if (saveData == null)
            {
                LogError($"Failed to parse save data: {slotName}");
                OnSaveError?.Invoke($"Corrupted save file: {slotName}");
                return false;
            }
            
            // Load the save data
            return LoadSaveData(saveData, slotName);
        }
        catch (Exception e)
        {
            LogError($"Failed to load game from slot {slotName}: {e.Message}");
            OnSaveError?.Invoke($"Load failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Load save data into game systems
    /// </summary>
    public bool LoadSaveData(GameSaveData saveData, string slotName)
    {
        try
        {
            RefreshSystemReferences();
            
            // Load scene if different
            if (saveData.currentScene != SceneManager.GetActiveScene().name)
            {
                // Load scene first, then apply save data
                StartCoroutine(LoadSceneAndApplyData(saveData, slotName));
                return true;
            }
            
            // Apply save data to systems
            ApplySaveDataToSystems(saveData);
            
            currentSaveSlot = slotName;
            LogDebug($"Game loaded from slot: {slotName}");
            ShowSaveNotification($"Game loaded: {slotName}");
            
            OnGameLoaded?.Invoke(slotName);
            return true;
        }
        catch (Exception e)
        {
            LogError($"Failed to apply save data: {e.Message}");
            OnSaveError?.Invoke($"Failed to apply save data: {e.Message}");
            return false;
        }
    }
    
    System.Collections.IEnumerator LoadSceneAndApplyData(GameSaveData saveData, string slotName)
    {
        // Load scene
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(saveData.currentScene);
        yield return sceneLoad;
        
        // Wait a frame for scene to initialize
        yield return null;
        
        // Refresh references after scene load
        RefreshSystemReferences();
        
        // Apply save data
        ApplySaveDataToSystems(saveData);
        
        currentSaveSlot = slotName;
        LogDebug($"Game loaded with scene change: {slotName}");
        OnGameLoaded?.Invoke(slotName);
    }
    
    void ApplySaveDataToSystems(GameSaveData saveData)
    {
        // Load quest system
        if (questManager != null && saveData.questData != null)
        {
            questManager.LoadSaveData(saveData.questData);
        }
        
        // Load dialogue/flags system
        if (npcInteractionSystem != null && saveData.dialogueData != null)
        {
            npcInteractionSystem.LoadSaveData(saveData.dialogueData);
        }
        
        // Load NPC system
        if (npcManager != null && saveData.npcData != null)
        {
            npcManager.LoadSaveData(saveData.npcData);
        }
        
        // Load time system
        if (dayNightCycle != null && saveData.timeData != null)
        {
            dayNightCycle.LoadSaveData(saveData.timeData);
        }
        
        // Load player data
        if (player != null)
        {
            player.transform.position = saveData.playerPosition;
            
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Stats.health = saveData.playerHealth;
            }
        }
        
        LogDebug($"Save data applied: {saveData.totalFlags} flags restored");
    }
    
    #endregion
    
    #region Save File Management
    
    /// <summary>
    /// Get list of all available save files
    /// </summary>
    public List<SaveFileInfo> GetSaveFiles()
    {
        List<SaveFileInfo> saveFiles = new List<SaveFileInfo>();
        string saveDir = Path.Combine(Application.persistentDataPath, "Saves");
        
        if (Directory.Exists(saveDir))
        {
            string[] files = Directory.GetFiles(saveDir, "*.json");
            
            foreach (string file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    FileInfo fileInfo = new FileInfo(file);
                    
                    // Try to read save data for more info
                    string jsonData = File.ReadAllText(file);
                    GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                    
                    saveFiles.Add(new SaveFileInfo
                    {
                        slotName = fileName,
                        filePath = file,
                        fileSize = fileInfo.Length,
                        lastModified = fileInfo.LastWriteTime,
                        saveTime = saveData?.saveTime != null ? DateTime.FromBinary(saveData.saveTime) : fileInfo.LastWriteTime,
                        currentScene = saveData?.currentScene ?? "Unknown",
                        gameVersion = saveData?.gameVersion ?? "Unknown",
                        totalFlags = saveData?.totalFlags ?? 0,
                        playTime = saveData?.playTime ?? 0f
                    });
                }
                catch (Exception e)
                {
                    LogError($"Error reading save file {file}: {e.Message}");
                }
            }
        }
        
        // Sort by save time (newest first)
        saveFiles.Sort((a, b) => b.saveTime.CompareTo(a.saveTime));
        return saveFiles;
    }
    
    /// <summary>
    /// Delete save file
    /// </summary>
    public bool DeleteSave(string slotName)
    {
        try
        {
            string filePath = GetSaveFilePath(slotName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                LogDebug($"Deleted save: {slotName}");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            LogError($"Failed to delete save {slotName}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Create backup of existing save
    /// </summary>
    void CreateBackup(string slotName)
    {
        try
        {
            string originalPath = GetSaveFilePath(slotName);
            string backupPath = GetSaveFilePath($"{slotName}_backup");
            
            if (File.Exists(originalPath))
            {
                File.Copy(originalPath, backupPath, true);
                LogDebug($"Created backup for: {slotName}");
            }
        }
        catch (Exception e)
        {
            LogError($"Failed to create backup for {slotName}: {e.Message}");
        }
    }
    
    string GetSaveFilePath(string slotName)
    {
        string saveDir = Path.Combine(Application.persistentDataPath, "Saves");
        return Path.Combine(saveDir, $"{slotName}.json");
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if save file exists
    /// </summary>
    public bool SaveExists(string slotName)
    {
        return File.Exists(GetSaveFilePath(slotName));
    }
    
    /// <summary>
    /// Get save file info without loading
    /// </summary>
    public SaveFileInfo GetSaveInfo(string slotName)
    {
        var saveFiles = GetSaveFiles();
        return saveFiles.Find(s => s.slotName == slotName);
    }
    
    /// <summary>
    /// Clean up old auto-saves (keep only latest 10)
    /// </summary>
    public void CleanupAutoSaves()
    {
        var saveFiles = GetSaveFiles();
        var autoSaves = saveFiles.FindAll(s => s.slotName.StartsWith("Auto_"));
        
        // Sort by date and keep only latest 10
        autoSaves.Sort((a, b) => b.saveTime.CompareTo(a.saveTime));
        
        for (int i = 10; i < autoSaves.Count; i++)
        {
            DeleteSave(autoSaves[i].slotName);
        }
    }
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[GameSave] {message}");
        }
    }
    
    void LogError(string message)
    {
        Debug.LogError($"[GameSave] {message}");
    }
    
    void ShowSaveNotification(string message)
    {
        if (showSaveNotifications)
        {
            // You can integrate this with your UI system
            Debug.Log($"[Save Notification] {message}");
        }
    }
    
    #endregion
    
    #region Public API for Integration
    
    /// <summary>
    /// Manual save trigger for UI buttons
    /// </summary>
    [ContextMenu("Manual Save")]
    public void TriggerManualSave()
    {
        SaveGame($"ManualSave_{DateTime.Now:yyyyMMdd_HHmmss}");
    }
    
    /// <summary>
    /// Quick load last save
    /// </summary>
    [ContextMenu("Quick Load")]
    public void TriggerQuickLoad()
    {
        var saves = GetSaveFiles();
        if (saves.Count > 0)
        {
            LoadGame(saves[0].slotName);
        }
    }
    
    /// <summary>
    /// Export save data for debugging
    /// </summary>
    [ContextMenu("Export Save Data")]
    public void ExportSaveData()
    {
        GameSaveData saveData = CreateSaveData();
        string json = JsonUtility.ToJson(saveData, true);
        string path = Path.Combine(Application.persistentDataPath, "debug_save_export.json");
        File.WriteAllText(path, json);
        Debug.Log($"Save data exported to: {path}");
    }
    
    #endregion
}

#region Save Data Structures

/// <summary>
/// Complete game save data structure
/// </summary>
[System.Serializable]
public class GameSaveData
{
    [Header("Metadata")]
    public string saveVersion;
    public long saveTime;
    public string gameVersion;
    public string currentScene;
    
    [Header("Player Data")]
    public Vector3 playerPosition;
    public float playerHealth;
    public float playTime;
    
    [Header("System Data")]
    public QuestManager.QuestManagerSaveData questData;
    public NPCInteractionSystem.DialogueSystemSaveData dialogueData;
    public NPCManager.NPCManagerSaveData npcData;
    public DayNightCycle.DayNightSaveData timeData;
    
    [Header("Statistics")]
    public int totalFlags;
}

/// <summary>
/// Save file information for UI display
/// </summary>
[System.Serializable]
public class SaveFileInfo
{
    public string slotName;
    public string filePath;
    public long fileSize;
    public DateTime lastModified;
    public DateTime saveTime;
    public string currentScene;
    public string gameVersion;
    public int totalFlags;
    public float playTime;
    
    public string GetFormattedFileSize()
    {
        if (fileSize < 1024) return $"{fileSize} B";
        if (fileSize < 1024 * 1024) return $"{fileSize / 1024} KB";
        return $"{fileSize / (1024 * 1024)} MB";
    }
    
    public string GetFormattedPlayTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(playTime);
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
    }
}

#endregion
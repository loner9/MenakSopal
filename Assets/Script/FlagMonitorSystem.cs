using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efficient event-driven flag monitoring system for triggering actions based on flag changes.
/// Uses Unity Events and C# Actions for performance-optimized flag watching.
/// 
/// Usage Examples:
/// - FlagMonitorSystem.WatchFlagAdded("water_crisis_discovered", () => StartQuest("DamConstruction"));
/// - FlagMonitorSystem.WatchFlag("is_daytime", (isAdded) => SetLighting(isAdded));
/// - FlagMonitorSystem.UnwatchFlag("completed_quest");
/// </summary>
public class FlagMonitorSystem : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showWatcherCount = false;
    
    // Event system for flag changes
    public static event Action<string> OnFlagAdded;
    public static event Action<string> OnFlagRemoved;
    public static event Action<string, bool> OnFlagChanged; // flag, isAdded
    
    // Dictionary of flag watchers for efficient lookup
    private static Dictionary<string, List<FlagWatcher>> flagWatchers = new Dictionary<string, List<FlagWatcher>>();
    
    // Instance reference for MonoBehaviour access
    private static FlagMonitorSystem instance;
    
    public static FlagMonitorSystem Instance 
    { 
        get 
        { 
            if (instance == null)
            {
                instance = FindObjectOfType<FlagMonitorSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FlagMonitorSystem");
                    instance = go.AddComponent<FlagMonitorSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        } 
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[FlagMonitor] System initialized");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    #region Flag Watching Registration
    
    /// <summary>
    /// Register a watcher for a specific flag. Triggers immediately if flag already exists.
    /// </summary>
    /// <param name="flagName">The flag to watch</param>
    /// <param name="callback">Callback with bool parameter (true = added, false = removed)</param>
    /// <param name="triggerIfExists">If true, triggers callback immediately if flag already exists</param>
    public static void WatchFlag(string flagName, Action<bool> callback, bool triggerIfExists = true)
    {
        if (string.IsNullOrEmpty(flagName) || callback == null) 
        {
            Debug.LogWarning("[FlagMonitor] Cannot watch flag: invalid parameters");
            return;
        }
        
        FlagWatcher watcher = new FlagWatcher(flagName, callback);
        
        if (!flagWatchers.ContainsKey(flagName))
        {
            flagWatchers[flagName] = new List<FlagWatcher>();
        }
        
        flagWatchers[flagName].Add(watcher);
        
        // Trigger immediately if flag already exists
        if (triggerIfExists && HasFlag(flagName))
        {
            try
            {
                callback.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FlagMonitor] Error in immediate callback for '{flagName}': {e.Message}");
            }
        }
        
        if (Instance.enableDebugLogs)
        {
            Debug.Log($"[FlagMonitor] Registered watcher for flag: {flagName} (Total watchers: {GetWatcherCount(flagName)})");
        }
    }
    
    /// <summary>
    /// Register a watcher that only triggers when flag is added (not removed)
    /// </summary>
    /// <param name="flagName">The flag to watch</param>
    /// <param name="callback">Callback to execute when flag is added</param>
    /// <param name="triggerIfExists">If true, triggers callback immediately if flag already exists</param>
    public static void WatchFlagAdded(string flagName, Action callback, bool triggerIfExists = true)
    {
        if (callback == null) return;
        
        WatchFlag(flagName, (isAdded) => {
            if (isAdded) callback.Invoke();
        }, triggerIfExists);
    }
    
    /// <summary>
    /// Register a watcher that only triggers when flag is removed
    /// </summary>
    /// <param name="flagName">The flag to watch</param>
    /// <param name="callback">Callback to execute when flag is removed</param>
    public static void WatchFlagRemoved(string flagName, Action callback)
    {
        if (callback == null) return;
        
        WatchFlag(flagName, (isAdded) => {
            if (!isAdded) callback.Invoke();
        }, false);
    }
    
    /// <summary>
    /// Remove all watchers for a specific flag
    /// </summary>
    /// <param name="flagName">The flag to stop watching</param>
    public static void UnwatchFlag(string flagName)
    {
        if (flagWatchers.ContainsKey(flagName))
        {
            int count = flagWatchers[flagName].Count;
            flagWatchers.Remove(flagName);
            if (Instance.enableDebugLogs)
            {
                Debug.Log($"[FlagMonitor] Removed {count} watchers for flag: {flagName}");
            }
        }
    }
    
    /// <summary>
    /// Remove a specific callback from flag watchers
    /// </summary>
    /// <param name="flagName">The flag to stop watching</param>
    /// <param name="callback">The specific callback to remove</param>
    public static void UnwatchFlag(string flagName, Action<bool> callback)
    {
        if (flagWatchers.ContainsKey(flagName))
        {
            int beforeCount = flagWatchers[flagName].Count;
            flagWatchers[flagName].RemoveAll(w => w.callback == callback);
            int afterCount = flagWatchers[flagName].Count;
            
            if (flagWatchers[flagName].Count == 0)
            {
                flagWatchers.Remove(flagName);
            }
            
            if (Instance.enableDebugLogs && beforeCount != afterCount)
            {
                Debug.Log($"[FlagMonitor] Removed specific watcher for flag: {flagName}");
            }
        }
    }
    
    #endregion
    
    #region Flag Change Notifications
    
    /// <summary>
    /// Call this when a flag is added to trigger all registered watchers
    /// </summary>
    /// <param name="flagName">The flag that was added</param>
    public static void NotifyFlagAdded(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        
        // Trigger specific flag watchers
        if (flagWatchers.ContainsKey(flagName))
        {
            foreach (var watcher in flagWatchers[flagName])
            {
                try
                {
                    watcher.callback.Invoke(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FlagMonitor] Error in flag watcher for '{flagName}': {e.Message}");
                }
            }
        }
        
        // Trigger global events
        OnFlagAdded?.Invoke(flagName);
        OnFlagChanged?.Invoke(flagName, true);
        
        if (Instance.enableDebugLogs)
        {
            Debug.Log($"[FlagMonitor] Flag added: {flagName} (Triggered {GetWatcherCount(flagName)} watchers)");
        }
    }
    
    /// <summary>
    /// Call this when a flag is removed to trigger all registered watchers
    /// </summary>
    /// <param name="flagName">The flag that was removed</param>
    public static void NotifyFlagRemoved(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        
        // Trigger specific flag watchers
        if (flagWatchers.ContainsKey(flagName))
        {
            foreach (var watcher in flagWatchers[flagName])
            {
                try
                {
                    watcher.callback.Invoke(false);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FlagMonitor] Error in flag watcher for '{flagName}': {e.Message}");
                }
            }
        }
        
        // Trigger global events
        OnFlagRemoved?.Invoke(flagName);
        OnFlagChanged?.Invoke(flagName, false);
        
        if (Instance.enableDebugLogs)
        {
            Debug.Log($"[FlagMonitor] Flag removed: {flagName} (Triggered {GetWatcherCount(flagName)} watchers)");
        }
    }
    
    #endregion
    
    #region Flag State Queries
    
    /// <summary>
    /// Check if a flag currently exists (queries NPCInteractionSystem)
    /// </summary>
    /// <param name="flagName">The flag to check</param>
    /// <returns>True if flag exists</returns>
    public static bool HasFlag(string flagName)
    {
        var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
        return interactionSystem != null && interactionSystem.HasGameFlag(flagName);
    }
    
    /// <summary>
    /// Get all current flags (for debugging purposes)
    /// </summary>
    /// <returns>List of all current flags</returns>
    public static List<string> GetAllFlags()
    {
        var interactionSystem = FindObjectOfType<NPCInteractionSystem>();
        if (interactionSystem != null)
        {
            // Note: This requires a public getter for gameFlags in NPCInteractionSystem
            // For now, returns empty list - implement based on your needs
            return new List<string>();
        }
        return new List<string>();
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get count of watchers for a specific flag (for debugging)
    /// </summary>
    /// <param name="flagName">The flag to check</param>
    /// <returns>Number of watchers for this flag</returns>
    public static int GetWatcherCount(string flagName)
    {
        return flagWatchers.ContainsKey(flagName) ? flagWatchers[flagName].Count : 0;
    }
    
    /// <summary>
    /// Get total number of active watchers (for debugging)
    /// </summary>
    /// <returns>Total number of active watchers</returns>
    public static int GetTotalWatcherCount()
    {
        int total = 0;
        foreach (var kvp in flagWatchers)
        {
            total += kvp.Value.Count;
        }
        return total;
    }
    
    /// <summary>
    /// Get all watched flags (for debugging)
    /// </summary>
    /// <returns>List of all flags being watched</returns>
    public static List<string> GetWatchedFlags()
    {
        return new List<string>(flagWatchers.Keys);
    }
    
    /// <summary>
    /// Clear all watchers (useful for scene transitions)
    /// </summary>
    public static void ClearAllWatchers()
    {
        int totalCleared = GetTotalWatcherCount();
        flagWatchers.Clear();
        if (Instance.enableDebugLogs)
        {
            Debug.Log($"[FlagMonitor] Cleared {totalCleared} flag watchers");
        }
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Update()
    {
        // Show watcher count in debug mode
        if (showWatcherCount && enableDebugLogs)
        {
            int totalWatchers = GetTotalWatcherCount();
            if (totalWatchers > 0)
            {
                Debug.Log($"[FlagMonitor] Active watchers: {totalWatchers} for {flagWatchers.Count} flags");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            ClearAllWatchers();
            instance = null;
        }
    }
    
    #endregion
}

/// <summary>
/// Internal class to store flag watcher information
/// </summary>
[System.Serializable]
public class FlagWatcher
{
    public string flagName;
    public Action<bool> callback;
    
    public FlagWatcher(string flagName, Action<bool> callback)
    {
        this.flagName = flagName;
        this.callback = callback;
    }
}
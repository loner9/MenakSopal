using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Advanced NPC Manager that combines ScriptableObject-based spawn data with tight DayNightCycle synchronization.
/// 
/// This represents the evolution of our NPC management system, combining:
/// - Asset-based spawn data organization (from ScriptableNPCManager)
/// - Tight time synchronization (from SyncedSimpleNPCManager)
/// - Advanced spawn conditions and debugging features
/// 
/// Design Philosophy:
/// Your DayNightCycle remains the single source of truth for time, ensuring perfect synchronization
/// between visual effects and NPC behavior, while spawn data assets provide flexible organization
/// and team-friendly development workflows.
/// </summary>
public class SyncedScriptableNPCManager : MonoBehaviour
{
    [Header("NPC Spawn Configuration")]
    [Tooltip("List of NPC spawn data assets - drag your NPCSpawnData assets here")]
    public List<NPCSpawnData> npcSpawnDataAssets = new List<NPCSpawnData>();
    
    [Header("Spawn Filtering")]
    [Tooltip("Only spawn NPCs from these categories (empty = spawn all categories)")]
    public string[] spawnOnlyCategories = new string[0];
    
    [Tooltip("Never spawn NPCs from these categories")]
    public string[] excludeCategories = new string[0];
    
    [Header("Day/Night Integration")]
    [Tooltip("Your existing DayNightCycle - the master timekeeper (found automatically if not assigned)")]
    public DayNightCycle dayNightCycle;
    
    [Tooltip("Enhanced interaction system for game flags (found automatically if not assigned)")]
    public EnhancedNPCInteractionSystem interactionSystem;
    
    [Header("Organization")]
    [Tooltip("Parent transform for spawned NPCs")]
    public Transform npcParent;
    
    [Header("Debug and Monitoring")]
    public bool showDetailedLogs = false;
    public bool showSpawnStatus = true;
    public bool showTimeSyncInfo = false;
    
    // Time synchronization tracking - this is the key improvement
    private int lastHour = -1;
    private float lastSyncTime = 0f;
    
    // Runtime NPC tracking
    private Dictionary<string, SimpleNPC> spawnedNPCs = new Dictionary<string, SimpleNPC>();
    
    // Cached game state for performance
    private List<string> currentGameFlags = new List<string>();
    private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    
    // Events that other systems can subscribe to
    public System.Action<NPCSpawnData, SimpleNPC> OnNPCSpawned;
    public System.Action<NPCSpawnData> OnNPCDespawned;
    public System.Action<int> OnHourChanged; // Fired when hour changes, perfectly synced with DayNightCycle
    
    /// <summary>
    /// Properties that bridge to your DayNightCycle system.
    /// These ensure we always get time information from the authoritative source.
    /// </summary>
    public int CurrentHour 
    { 
        get 
        { 
            return dayNightCycle != null ? Mathf.FloorToInt(dayNightCycle.CurrentTime) : 6; 
        } 
    }
    
    public float CurrentGameTime 
    { 
        get 
        { 
            return dayNightCycle != null ? dayNightCycle.CurrentTime : 6f; 
        } 
    }
    
    public TimeOfDay CurrentTimeOfDay 
    { 
        get 
        { 
            return dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day; 
        } 
    }
    
    void Start()
    {
        // Step 1: Establish connection to your existing time system
        SetupDayNightSynchronization();
        
        // Step 2: Set up our organizational structure
        SetupNPCParent();
        
        // Step 3: Validate our spawn data assets
        ValidateSpawnData();
        
        // Step 4: Perform initial spawn check based on current time
        ProcessInitialSpawns();
        
        if (showDetailedLogs)
        {
            Debug.Log($"SyncedScriptableNPCManager: Initialized with {npcSpawnDataAssets.Count} spawn configurations at time {CurrentGameTime:F1}");
        }
    }
    
    void Update()
    {
        // The heart of the synchronization system - check for hour changes every frame
        // This ensures we catch hour transitions immediately, maintaining perfect sync
        CheckForHourChange();
    }
    
    #region Day/Night Cycle Synchronization
    
    /// <summary>
    /// Establishes tight integration with your existing DayNightCycle system.
    /// This method demonstrates how to properly connect to existing game systems
    /// while handling edge cases like missing components gracefully.
    /// </summary>
    void SetupDayNightSynchronization()
    {
        // Find your DayNightCycle if not manually assigned
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        }
        
        if (dayNightCycle == null)
        {
            Debug.LogError("SyncedScriptableNPCManager: Could not find DayNightCycle component! " +
                          "NPC time synchronization will not work. Please assign it manually or ensure it exists in the scene.");
            return;
        }
        
        // Subscribe to your DayNightCycle's time change events
        // This creates the synchronization bridge between your time system and NPC system
        dayNightCycle.OnTimeChanged += OnDayNightTimeChanged;
        
        // Initialize our time tracking with current values
        lastHour = CurrentHour;
        currentTimeOfDay = CurrentTimeOfDay;
        lastSyncTime = Time.time;
        
        // Find the interaction system for game flag integration
        if (interactionSystem == null)
        {
            interactionSystem = FindObjectOfType<EnhancedNPCInteractionSystem>();
        }
        
        if (interactionSystem != null)
        {
            // Get initial game flags
            currentGameFlags = interactionSystem.GetGameFlags();
        }
        
        if (showDetailedLogs)
        {
            Debug.Log($"SyncedScriptableNPCManager: Successfully synchronized with DayNightCycle. " +
                     $"Current time: {CurrentGameTime:F1} hours, Hour: {CurrentHour}, TimeOfDay: {CurrentTimeOfDay}");
        }
    }
    
    /// <summary>
    /// Called whenever your DayNightCycle updates its time.
    /// This is the synchronization point that keeps everything in perfect alignment.
    /// </summary>
    void OnDayNightTimeChanged(float newTime)
    {
        // Update our cached time-dependent data
        currentTimeOfDay = CurrentTimeOfDay;
        
        // Update game flags if interaction system is available
        if (interactionSystem != null)
        {
            currentGameFlags = interactionSystem.GetGameFlags();
        }
        
        // Track synchronization for debugging
        lastSyncTime = Time.time;
        
        if (showTimeSyncInfo && Time.frameCount % 300 == 0) // Log every 5 seconds at 60fps
        {
            Debug.Log($"SyncedScriptableNPCManager: Time sync update - DayNight: {newTime:F1}, " +
                     $"NPC Hour: {CurrentHour}, TimeOfDay: {currentTimeOfDay}");
        }
    }
    
    /// <summary>
    /// Detects when the hour changes and triggers NPC schedule updates.
    /// This method is called every frame but only processes when the actual hour changes,
    /// ensuring immediate response to time transitions while maintaining performance.
    /// </summary>
    void CheckForHourChange()
    {
        int currentHour = CurrentHour;
        
        if (currentHour != lastHour)
        {
            if (showDetailedLogs)
            {
                Debug.Log($"SyncedScriptableNPCManager: Hour changed from {lastHour} to {currentHour} " +
                         $"(Game time: {CurrentGameTime:F2})");
            }
            
            // Update our hour tracking
            lastHour = currentHour;
            
            // Notify other systems about the hour change
            OnHourChanged?.Invoke(currentHour);
            
            // Process NPC spawn/despawn conditions based on new hour
            ProcessHourlySpawnConditions();
            
            // Update schedules for existing NPCs
            UpdateExistingNPCSchedules();
            
            if (showDetailedLogs)
            {
                LogCurrentNPCStatus();
            }
        }
    }
    
    #endregion
    
    #region Spawn Data Management and Validation
    
    /// <summary>
    /// Validates all spawn data assets to catch configuration problems early.
    /// This kind of validation is crucial for asset-based systems where data
    /// comes from external files that might have been edited incorrectly.
    /// </summary>
    void ValidateSpawnData()
    {
        List<string> usedIDs = new List<string>();
        List<NPCSpawnData> invalidData = new List<NPCSpawnData>();
        
        foreach (var spawnData in npcSpawnDataAssets)
        {
            if (spawnData == null)
            {
                Debug.LogError("SyncedScriptableNPCManager: Found null spawn data in asset list");
                continue;
            }
            
            // Check for duplicate IDs (would cause runtime conflicts)
            if (usedIDs.Contains(spawnData.npcID))
            {
                Debug.LogError($"SyncedScriptableNPCManager: Duplicate NPC ID '{spawnData.npcID}' " +
                              $"in spawn data '{spawnData.name}'. Each NPC must have a unique ID.");
                invalidData.Add(spawnData);
                continue;
            }
            usedIDs.Add(spawnData.npcID);
            
            // Check for missing required components
            if (spawnData.npcPrefab == null)
            {
                Debug.LogError($"SyncedScriptableNPCManager: Missing NPC prefab in spawn data '{spawnData.name}'. " +
                              "Cannot spawn NPC without a prefab.");
                invalidData.Add(spawnData);
                continue;
            }
            
            if (spawnData.scheduleData == null)
            {
                Debug.LogError($"SyncedScriptableNPCManager: Missing schedule data in spawn data '{spawnData.name}'. " +
                              "NPCs need schedule data to know when and where to appear.");
                invalidData.Add(spawnData);
                continue;
            }
            
            // Validate prefab components
            SimpleNPC npcComponent = spawnData.npcPrefab.GetComponent<SimpleNPC>();
            NPCInteractionBridge bridgeComponent = spawnData.npcPrefab.GetComponent<NPCInteractionBridge>();
            
            if (npcComponent == null)
            {
                Debug.LogError($"SyncedScriptableNPCManager: NPC prefab '{spawnData.npcPrefab.name}' " +
                              "missing SimpleNPC component. This component is required for movement and scheduling.");
                invalidData.Add(spawnData);
            }
            
            if (bridgeComponent == null)
            {
                Debug.LogWarning($"SyncedScriptableNPCManager: NPC prefab '{spawnData.npcPrefab.name}' " +
                                "missing NPCInteractionBridge component. NPC will not be interactable with players.");
            }
        }
        
        // Remove invalid data from our processing list
        foreach (var invalid in invalidData)
        {
            npcSpawnDataAssets.Remove(invalid);
        }
        
        if (showDetailedLogs)
        {
            Debug.Log($"SyncedScriptableNPCManager: Validated {npcSpawnDataAssets.Count} spawn data assets. " +
                     $"Removed {invalidData.Count} invalid entries.");
        }
    }
    
    #endregion
    
    #region Spawn Condition Processing
    
    /// <summary>
    /// Processes initial spawns when the game starts.
    /// This handles NPCs that should already be active based on the current time.
    /// </summary>
    void ProcessInitialSpawns()
    {
        int initialSpawnCount = 0;
        
        foreach (var spawnData in npcSpawnDataAssets)
        {
            if (spawnData == null) continue;
            
            // Skip if category filtering excludes this NPC
            if (!ShouldProcessCategory(spawnData.category)) continue;
            
            // Check if this NPC should be spawned at the current time
            if (ShouldNPCBeActive(spawnData))
            {
                SpawnNPC(spawnData);
                initialSpawnCount++;
            }
        }
        
        if (showDetailedLogs)
        {
            Debug.Log($"SyncedScriptableNPCManager: Initial spawn complete. " +
                     $"Spawned {initialSpawnCount} NPCs for current time ({CurrentHour}:00)");
        }
    }
    
    /// <summary>
    /// Processes spawn conditions when the hour changes.
    /// This is called every time we detect an hour transition from the DayNightCycle.
    /// </summary>
    void ProcessHourlySpawnConditions()
    {
        int spawnedCount = 0;
        int despawnedCount = 0;
        
        foreach (var spawnData in npcSpawnDataAssets)
        {
            if (spawnData == null) continue;
            
            // Skip if category filtering excludes this NPC
            if (!ShouldProcessCategory(spawnData.category)) continue;
            
            bool shouldBeActive = ShouldNPCBeActive(spawnData);
            bool isCurrentlySpawned = spawnData.isCurrentlySpawned;
            
            if (shouldBeActive && !isCurrentlySpawned)
            {
                // NPC should be active but isn't spawned - spawn it
                SpawnNPC(spawnData);
                spawnedCount++;
            }
            else if (!shouldBeActive && isCurrentlySpawned)
            {
                // NPC shouldn't be active but is spawned - despawn it
                DespawnNPC(spawnData);
                despawnedCount++;
            }
        }
        
        if (showDetailedLogs && (spawnedCount > 0 || despawnedCount > 0))
        {
            Debug.Log($"SyncedScriptableNPCManager: Hour {CurrentHour} spawn update - " +
                     $"Spawned: {spawnedCount}, Despawned: {despawnedCount}");
        }
    }
    
    /// <summary>
    /// Determines if an NPC should be active based on current game conditions.
    /// This encapsulates all the spawn condition logic in one place.
    /// </summary>
    bool ShouldNPCBeActive(NPCSpawnData spawnData)
    {
        // Use the spawn data's built-in condition checking
        // This considers time restrictions, game flags, and basic requirements
        return spawnData.CanSpawnNow(currentTimeOfDay, currentGameFlags);
    }
    
    /// <summary>
    /// Checks if an NPC category should be processed based on include/exclude filters.
    /// This allows for flexible filtering of which NPCs the manager handles.
    /// </summary>
    bool ShouldProcessCategory(string category)
    {
        // Check exclude list first (exclusions take priority)
        if (excludeCategories != null && excludeCategories.Length > 0)
        {
            if (excludeCategories.Contains(category))
            {
                return false;
            }
        }
        
        // Check include list (if specified, only these categories are processed)
        if (spawnOnlyCategories != null && spawnOnlyCategories.Length > 0)
        {
            return spawnOnlyCategories.Contains(category);
        }
        
        // No restrictions, process all categories
        return true;
    }
    
    /// <summary>
    /// Updates schedules for all currently spawned NPCs when the hour changes.
    /// This ensures NPCs immediately respond to time transitions.
    /// </summary>
    void UpdateExistingNPCSchedules()
    {
        foreach (var npc in spawnedNPCs.Values)
        {
            if (npc != null)
            {
                // Tell each NPC about the hour change so they can update their schedules
                npc.OnHourChanged(CurrentHour);
            }
        }
    }
    
    #endregion
    
    #region NPC Lifecycle Management
    
    /// <summary>
    /// Spawns an NPC from spawn data, handling all the setup and tracking.
    /// </summary>
    public SimpleNPC SpawnNPC(NPCSpawnData spawnData)
    {
        if (spawnData == null || spawnData.isCurrentlySpawned)
        {
            return null;
        }
        
        // Get the spawn position (considering custom overrides)
        Vector2 spawnPosition = spawnData.GetSpawnPosition();
        
        // Create the NPC GameObject
        GameObject npcGO = Instantiate(spawnData.npcPrefab, spawnPosition, Quaternion.identity, npcParent);
        
        // Get the NPC component and validate it
        SimpleNPC npc = npcGO.GetComponent<SimpleNPC>();
        if (npc == null)
        {
            Debug.LogError($"SyncedScriptableNPCManager: Spawned prefab missing SimpleNPC component");
            Destroy(npcGO);
            return null;
        }
        
        // Initialize the NPC with its schedule and this manager reference
        npc.Initialize(spawnData.scheduleData, this);
        npc.gameObject.name = $"NPC_{spawnData.npcID}";
        
        // Update spawn data tracking
        spawnData.isCurrentlySpawned = true;
        spawnData.spawnedInstance = npc;
        
        // Add to our runtime tracking dictionary
        spawnedNPCs[spawnData.npcID] = npc;
        
        // Set up the interaction bridge if present
        NPCInteractionBridge bridge = npc.GetComponent<NPCInteractionBridge>();
        if (bridge != null)
        {
            bridge.npcName = spawnData.displayName;
        }
        
        // Notify other systems
        OnNPCSpawned?.Invoke(spawnData, npc);
        
        if (showDetailedLogs)
        {
            Debug.Log($"SyncedScriptableNPCManager: Spawned '{spawnData.displayName}' " +
                     $"(ID: {spawnData.npcID}) at {spawnPosition} during hour {CurrentHour}");
        }
        
        return npc;
    }
    
    /// <summary>
    /// Despawns an NPC and cleans up all tracking references.
    /// </summary>
    public void DespawnNPC(NPCSpawnData spawnData)
    {
        if (spawnData == null || !spawnData.isCurrentlySpawned)
        {
            return;
        }
        
        // Remove from runtime tracking
        spawnedNPCs.Remove(spawnData.npcID);
        
        // Destroy the GameObject
        if (spawnData.spawnedInstance != null)
        {
            Destroy(spawnData.spawnedInstance.gameObject);
        }
        
        // Update spawn data state
        spawnData.isCurrentlySpawned = false;
        spawnData.spawnedInstance = null;
        
        // Notify other systems
        OnNPCDespawned?.Invoke(spawnData);
        
        if (showDetailedLogs)
        {
            Debug.Log($"SyncedScriptableNPCManager: Despawned '{spawnData.displayName}' " +
                     $"(ID: {spawnData.npcID}) during hour {CurrentHour}");
        }
    }
    
    /// <summary>
    /// Called by NPCs when they reach a destination that should trigger despawn.
    /// This provides the bridge from NPC components back to the manager.
    /// </summary>
    public void RequestNPCDespawn(SimpleNPC npc)
    {
        if (npc == null) return;
        
        // Find the spawn data for this NPC
        NPCSpawnData spawnData = npcSpawnDataAssets.FirstOrDefault(data => data.spawnedInstance == npc);
        if (spawnData != null)
        {
            DespawnNPC(spawnData);
        }
        else
        {
            Debug.LogWarning($"SyncedScriptableNPCManager: Could not find spawn data for despawn request from {npc.name}");
        }
    }
    
    #endregion
    
    #region Utility and Query Methods
    
    void SetupNPCParent()
    {
        if (npcParent == null)
        {
            GameObject parentGO = new GameObject("NPCs");
            parentGO.transform.SetParent(transform);
            npcParent = parentGO.transform;
        }
    }
    
    /// <summary>
    /// Gets spawn data by NPC ID.
    /// </summary>
    public NPCSpawnData GetSpawnData(string npcID)
    {
        return npcSpawnDataAssets.FirstOrDefault(data => data.npcID == npcID);
    }
    
    /// <summary>
    /// Gets currently spawned NPC by ID.
    /// </summary>
    public SimpleNPC GetSpawnedNPC(string npcID)
    {
        return spawnedNPCs.ContainsKey(npcID) ? spawnedNPCs[npcID] : null;
    }
    
    /// <summary>
    /// Gets all currently spawned NPCs.
    /// </summary>
    public List<SimpleNPC> GetAllSpawnedNPCs()
    {
        return spawnedNPCs.Values.Where(npc => npc != null).ToList();
    }
    
    /// <summary>
    /// Gets the current number of spawned NPCs.
    /// </summary>
    public int GetSpawnedNPCCount()
    {
        return spawnedNPCs.Count;
    }
    
    /// <summary>
    /// Bridge method for backward compatibility with other systems.
    /// </summary>
    public float GetCurrentGameTime()
    {
        return CurrentGameTime;
    }
    
    public int GetCurrentHour()
    {
        return CurrentHour;
    }
    
    #endregion
    
    #region Debug and Status Reporting
    
    /// <summary>
    /// Logs current NPC status for debugging.
    /// This helps track what NPCs are doing and why.
    /// </summary>
    void LogCurrentNPCStatus()
    {
        Debug.Log($"=== NPC Status at Hour {CurrentHour} ===");
        Debug.Log($"Active NPCs: {spawnedNPCs.Count}/{npcSpawnDataAssets.Count}");
        
        foreach (var spawnData in npcSpawnDataAssets)
        {
            if (spawnData != null)
            {
                string status = spawnData.isCurrentlySpawned ? "SPAWNED" : "WAITING";
                string reason = spawnData.GetSpawnStatus(currentTimeOfDay, currentGameFlags);
                Debug.Log($"  {spawnData.displayName}: {status} - {reason}");
            }
        }
    }
    
    /// <summary>
    /// Gets a comprehensive status report for debugging and monitoring.
    /// </summary>
    public string GetSpawnStatusReport()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine($"=== Synced NPC Manager Status (Hour {CurrentHour}, {currentTimeOfDay}) ===");
        report.AppendLine($"Time Sync: Last sync {Time.time - lastSyncTime:F1}s ago");
        report.AppendLine($"Game Flags: {currentGameFlags.Count} active");
        
        var groupedByCategory = npcSpawnDataAssets.Where(data => data != null).GroupBy(data => data.category);
        
        foreach (var group in groupedByCategory)
        {
            report.AppendLine($"\n{group.Key} NPCs:");
            foreach (var spawnData in group)
            {
                string status = spawnData.GetSpawnStatus(currentTimeOfDay, currentGameFlags);
                string spawnedMark = spawnData.isCurrentlySpawned ? "✓" : "○";
                report.AppendLine($"  {spawnedMark} {spawnData.displayName}: {status}");
            }
        }
        
        report.AppendLine($"\nTotal: {spawnedNPCs.Count}/{npcSpawnDataAssets.Count} spawned");
        
        return report.ToString();
    }
    
    #endregion
    
    #region Gizmos and Visual Debug
    
    void OnDrawGizmos()
    {
        if (!showSpawnStatus || npcSpawnDataAssets == null) return;
        
        foreach (var spawnData in npcSpawnDataAssets)
        {
            if (spawnData == null) continue;
            
            Vector2 spawnPos = spawnData.GetSpawnPosition();
            
            // Color coding for spawn status
            if (spawnData.isCurrentlySpawned)
            {
                Gizmos.color = Color.green; // Currently spawned
            }
            else if (Application.isPlaying && spawnData.CanSpawnNow(currentTimeOfDay, currentGameFlags))
            {
                Gizmos.color = Color.yellow; // Ready to spawn
            }
            else
            {
                Gizmos.color = Color.red; // Cannot spawn
            }
            
            Gizmos.DrawWireSphere(spawnPos, 0.5f);
            
            #if UNITY_EDITOR
            // Show NPC info in editor
            UnityEditor.Handles.Label(spawnPos + Vector2.up * 0.7f, 
                $"{spawnData.displayName}\n{spawnData.category}\nStart: {spawnData.scheduleData?.startHour}:00");
            #endif
        }
    }
    
    #endregion
    
    #region Cleanup
    
    void OnDestroy()
    {
        // Clean up event subscriptions to prevent memory leaks
        if (dayNightCycle != null)
        {
            dayNightCycle.OnTimeChanged -= OnDayNightTimeChanged;
        }
    }
    
    #endregion
}
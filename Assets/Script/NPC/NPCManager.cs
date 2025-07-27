using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ConditionalSchedule
{
    [Header("Schedule Configuration")]
    [Tooltip("The schedule data to use when conditions are met")]
    public NPCScheduleData scheduleData;
    
    [Header("Conditions")]
    [Tooltip("All of these flags must be present for this schedule to be active")]
    public string[] requiredFlags = new string[0];
    
    [Tooltip("If any of these flags are present, this schedule will NOT be used")]
    public string[] excludeFlags = new string[0];
    
    [Header("Priority")]
    [Tooltip("Higher priority schedules override lower ones when multiple schedules match")]
    public int priority = 0;
    
    [Header("Description")]
    [Tooltip("Description for clarity in the editor (e.g., 'Normal schedule', 'Dam construction', etc.)")]
    public string description = "Default Schedule";
    
    /// <summary>
    /// Check if this conditional schedule should be active based on current flags
    /// </summary>
    public bool IsActiveForFlags(System.Func<string, bool> hasFlag)
    {
        // Check required flags - ALL must be present
        if (requiredFlags != null && requiredFlags.Length > 0)
        {
            foreach (string flag in requiredFlags)
            {
                if (!hasFlag(flag))
                {
                    return false;
                }
            }
        }
        
        // Check exclude flags - NONE must be present
        if (excludeFlags != null && excludeFlags.Length > 0)
        {
            foreach (string flag in excludeFlags)
            {
                if (hasFlag(flag))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}

[System.Serializable]
public class NPCSpawnData
{
    [Header("NPC Prefab")]
    public GameObject npcPrefab;
    public string npcID; // Unique identifier for save/load

    [Header("Spawn Settings")]
    public bool spawnAtStart = true;

    [Header("Schedule Data")]
    [Tooltip("Multiple schedule variants based on story flags. First matching schedule with highest priority is used.")]
    public ConditionalSchedule[] conditionalSchedules = new ConditionalSchedule[0];
    
    [Header("Legacy Support (Deprecated)")]
    [Tooltip("Legacy single schedule - will be converted to conditional schedule automatically")]
    public NPCScheduleData scheduleData;

    [Header("Spawn Conditions")]
    [Tooltip("Global spawn conditions - NPC won't spawn at all if these aren't met")]
    public string[] requiredFlags; // For quest-based spawning
    public string[] excludeFlags;  // Won't spawn if any of these flags are present
    
    /// <summary>
    /// Get the active schedule based on current flags, returns null if no schedule matches
    /// </summary>
    public NPCScheduleData GetActiveSchedule(System.Func<string, bool> hasFlag)
    {
        // Convert legacy schedule to conditional schedule if needed
        if ((conditionalSchedules == null || conditionalSchedules.Length == 0) && scheduleData != null)
        {
            return scheduleData; // Fallback to legacy schedule
        }
        
        if (conditionalSchedules == null || conditionalSchedules.Length == 0)
        {
            return null;
        }
        
        // Find all matching schedules
        var matchingSchedules = new System.Collections.Generic.List<ConditionalSchedule>();
        foreach (var conditionalSchedule in conditionalSchedules)
        {
            if (conditionalSchedule != null && conditionalSchedule.scheduleData != null &&
                conditionalSchedule.IsActiveForFlags(hasFlag))
            {
                matchingSchedules.Add(conditionalSchedule);
            }
        }
        
        if (matchingSchedules.Count == 0)
        {
            // No conditional schedules match, fall back to legacy schedule
            return scheduleData;
        }
        
        // Sort by priority (highest first) and return the best match
        matchingSchedules.Sort((a, b) => b.priority.CompareTo(a.priority));
        return matchingSchedules[0].scheduleData;
    }
    
    /// <summary>
    /// Check if this NPC should be spawned based on global spawn conditions
    /// </summary>
    public bool ShouldSpawn(System.Func<string, bool> hasFlag)
    {
        // Check required flags - ALL must be present
        if (requiredFlags != null && requiredFlags.Length > 0)
        {
            foreach (string flag in requiredFlags)
            {
                if (!hasFlag(flag))
                {
                    return false;
                }
            }
        }
        
        // Check exclude flags - NONE must be present
        if (excludeFlags != null && excludeFlags.Length > 0)
        {
            foreach (string flag in excludeFlags)
            {
                if (hasFlag(flag))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}

public class NPCManager : MonoBehaviour
{
    [Header("NPC Management")]
    public List<NPCSpawnData> npcSpawnList = new List<NPCSpawnData>();
    public Transform npcParent; // Optional parent object for organization


    [Header("Performance Settings")]
    [Tooltip("Enable basic performance optimization for large numbers of NPCs")]
    public bool enablePerformanceOptimization = false;

    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool showSpawnAreas = true;

    // Runtime data
    private List<NPC> spawnedNPCs = new List<NPC>();

    // Systems integration
    private DayNightCycle dayNightCycle;
    private NPCInteractionSystem interactionSystem;
    private Transform player;

    // Schedule management
    private int currentHour = -1;
    private Dictionary<NPC, ScheduleCommand> pendingCommands = new Dictionary<NPC, ScheduleCommand>();
    
    // Conditional schedule management
    private Dictionary<NPC, PendingScheduleChange> pendingScheduleChanges = new Dictionary<NPC, PendingScheduleChange>();
    private Dictionary<string, NPCScheduleData> activeScheduleCache = new Dictionary<string, NPCScheduleData>();

    // Events
    public System.Action<NPC> OnNPCSpawned;
    public System.Action<NPC> OnNPCDespawned;
    public System.Action<List<NPC>> OnNPCListUpdated;

    #region Initialization

    private void Awake()
    {
        // Create NPC parent if not assigned
        if (npcParent == null)
        {
            GameObject parentGO = new GameObject("NPCs");
            parentGO.transform.SetParent(transform);
            npcParent = parentGO.transform;
        }
    }

    private void Start()
    {
        // Find required systems
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        interactionSystem = FindObjectOfType<NPCInteractionSystem>();

        // Find player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;

        // Subscribe to time events
        if (dayNightCycle != null)
        {
            dayNightCycle.OnTimeChanged += OnTimeChanged;
        }
        
        // Subscribe to dialogue events for schedule change processing
        if (interactionSystem != null)
        {
            interactionSystem.OnDialogueEnd += OnDialogueEnded;
        }

        // Spawn initial NPCs
        SpawnInitialNPCs();

        // Initialize tag cache for better performance
        NPCScheduleData.ClearAllCaches();
    }

    private void OnDestroy()
    {
        if (dayNightCycle != null)
        {
            dayNightCycle.OnTimeChanged -= OnTimeChanged;
        }
        
        if (interactionSystem != null)
        {
            interactionSystem.OnDialogueEnd -= OnDialogueEnded;
        }

        CancelInvoke();
    }

    #endregion

    #region NPC Spawning

    private void SpawnInitialNPCs()
    {
        foreach (var spawnData in npcSpawnList)
        {
            if (spawnData.spawnAtStart && ShouldSpawnNPC(spawnData))
            {
                SpawnNPC(spawnData);
            }
        }

        Debug.Log($"NPCManager: Spawned {spawnedNPCs.Count} NPCs at start");
    }

    public NPC SpawnNPC(NPCSpawnData spawnData)
    {
        if (spawnData.npcPrefab == null)
        {
            Debug.LogError($"NPCManager: No prefab assigned for NPC spawn data");
            return null;
        }

        // Check if NPC already exists
        if (GetNPCByID(spawnData.npcID) != null)
        {
            Debug.LogWarning($"NPCManager: NPC with ID '{spawnData.npcID}' already exists");
            return null;
        }

        // Get active schedule and spawn position
        NPCScheduleData activeSchedule = GetActiveScheduleForNPC(spawnData);
        if (activeSchedule == null)
        {
            Debug.LogError($"NPCManager: No active schedule found for NPC '{spawnData.npcID}'");
            return null;
        }
        
        Vector2 spawnPos = activeSchedule.GetHomePosition();

        GameObject npcGO = Instantiate(spawnData.npcPrefab, spawnPos, Quaternion.identity, npcParent);
        NPC npc = npcGO.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError($"NPCManager: Spawned object does not have NPC component!");
            Destroy(npcGO);
            return null;
        }

        // Configure NPC
        SetupNPC(npc, spawnData);

        // Track the NPC
        spawnedNPCs.Add(npc);

        // Notify systems
        OnNPCSpawned?.Invoke(npc);
        OnNPCListUpdated?.Invoke(spawnedNPCs);

        Debug.Log($"NPCManager: Spawned NPC '{npc.npcName}' at {spawnPos}");
        return npc;
    }

    private void SetupNPC(NPC npc, NPCSpawnData spawnData)
    {
        // Set unique ID
        npc.gameObject.name = $"NPC_{spawnData.npcID}";

        // Apply active schedule data
        NPCScheduleData activeSchedule = GetActiveScheduleForNPC(spawnData);
        if (activeSchedule != null)
        {
            npc.scheduleData = activeSchedule;
            // Cache the active schedule for this NPC
            activeScheduleCache[spawnData.npcID] = activeSchedule;
        }
        else
        {
            Debug.LogError($"NPCManager: No active schedule for NPC '{npc.npcName}'");
        }

        // Force schedule update based on current time
        if (dayNightCycle != null)
        {
            npc.currentTimeOfDay = dayNightCycle.CurrentTimeOfDay;
        }
    }

    public NPC SpawnNPCAt(GameObject npcPrefab, NPCScheduleData scheduleData, string npcID = null)
    {
        NPCSpawnData tempSpawnData = new NPCSpawnData
        {
            npcPrefab = npcPrefab,
            npcID = npcID ?? System.Guid.NewGuid().ToString(),
            scheduleData = scheduleData,
            spawnAtStart = false
        };

        return SpawnNPC(tempSpawnData);
    }

    public void DespawnNPC(string npcID)
    {
        NPC npc = GetNPCByID(npcID);
        if (npc != null)
        {
            DespawnNPC(npc);
        }
    }

    public void DespawnNPC(NPC npc)
    {
        if (npc == null) return;

        // Remove from tracking lists
        spawnedNPCs.Remove(npc);

        // Notify systems
        OnNPCDespawned?.Invoke(npc);
        OnNPCListUpdated?.Invoke(spawnedNPCs);

        // Destroy the GameObject
        Destroy(npc.gameObject);

        Debug.Log($"NPCManager: Despawned NPC '{npc.npcName}'");
    }

    #endregion

    #region NPC Management

    public NPC GetNPCByID(string npcID)
    {
        return spawnedNPCs.FirstOrDefault(npc => npc.gameObject.name.Contains(npcID));
    }

    public NPC GetNPCByName(string npcName)
    {
        return spawnedNPCs.FirstOrDefault(npc => npc.npcName == npcName);
    }

    public List<NPC> GetNPCsByType(NPC.NPCType npcType)
    {
        return spawnedNPCs.Where(npc => npc.npcType == npcType).ToList();
    }

    public List<NPC> GetNPCsInRadius(Vector2 center, float radius)
    {
        return spawnedNPCs.Where(npc =>
            Vector2.Distance(npc.transform.position, center) <= radius).ToList();
    }

    public List<NPC> GetActiveNPCs()
    {
        return new List<NPC>(spawnedNPCs);
    }

    public int GetTotalNPCCount()
    {
        return spawnedNPCs.Count;
    }

    #endregion
    
    #region Dialogue Integration
    
    /// <summary>
    /// Called when any dialogue ends - processes pending schedule changes
    /// </summary>
    private void OnDialogueEnded(NPC npc)
    {
        Debug.Log($"[NPC MANAGER DEBUG] Dialogue ended with {(npc != null ? npc.npcName : "unknown NPC")}, processing pending schedule changes");
        ProcessPendingScheduleChanges();
    }
    
    #endregion

    #region Day/Night Integration


    private void OnTimeChanged(float currentTime)
    {
        int hour = Mathf.FloorToInt(currentTime);

        // Only process schedule changes when the hour actually changes
        if (hour != currentHour)
        {
            currentHour = hour;
            ProcessHourlyScheduleUpdate(hour);
        }
    }

    private void ProcessHourlyScheduleUpdate(int hour)
    {
        Debug.Log($"NPCManager: Processing hourly schedule update for hour {hour}");

        // Process each NPC's schedule for this hour
        foreach (var spawnData in npcSpawnList)
        {
            ProcessNPCScheduleForHour(spawnData, hour);
        }

        // Execute any pending schedule commands
        ExecutePendingScheduleCommands();
    }

    private void ProcessNPCScheduleForHour(NPCSpawnData spawnData, int hour)
    {
        NPCScheduleData activeSchedule = GetActiveScheduleForNPC(spawnData);
        if (activeSchedule == null) 
        {
            Debug.Log($"[NPC MANAGER DEBUG] No active schedule for {spawnData.npcID}");
            return;
        }

        Debug.Log($"[NPC MANAGER DEBUG] Processing schedule for {spawnData.npcID} at hour {hour}");
        
        NPC existingNPC = GetNPCByID(spawnData.npcID);
        
        // Check if the NPC needs a schedule change (but not during dialogue)
        CheckForScheduleChange(spawnData, existingNPC, activeSchedule);
        
        bool shouldBeActive = ShouldNPCBeActiveAtHour(activeSchedule, hour);
        
        Debug.Log($"[NPC MANAGER DEBUG] {spawnData.npcID} - spawnHour: {activeSchedule.spawnHour}, currentHour: {hour}, shouldBeActive: {shouldBeActive}, existingNPC: {(existingNPC != null ? "EXISTS" : "NULL")}");

        if (shouldBeActive && existingNPC == null)
        {
            Debug.Log($"[NPC MANAGER DEBUG] Spawning {spawnData.npcID} for hour {hour}");
            // Spawn NPC for this hour
            SpawnNPC(spawnData);
        }
        else if (!shouldBeActive && existingNPC != null)
        {
            Debug.Log($"[NPC MANAGER DEBUG] Sending {spawnData.npcID} home and despawning");
            // Send NPC home and despawn
            Vector2 homePos = activeSchedule.GetHomePosition();
            Debug.Log($"[NPC MANAGER DEBUG] Home position for {spawnData.npcID}: {homePos}");
            
            var homeCommand = new ScheduleCommand
            {
                commandType = ScheduleCommandType.GoHome,
                targetPosition = homePos
            };

            pendingCommands[existingNPC] = homeCommand;
        }
        else if (shouldBeActive && existingNPC != null)
        {
            Debug.Log($"[NPC MANAGER DEBUG] Checking for schedule event for {spawnData.npcID} at hour {hour}");
            // Use the current active schedule (might have changed due to flags)
            NPCScheduleData currentSchedule = GetCurrentNPCSchedule(existingNPC, spawnData);
            
            // Check if there's a schedule event for this hour
            var scheduleEvent = currentSchedule.GetScheduleEventForHour(hour);

            if (scheduleEvent != null && scheduleEvent.hour == hour)
            {
                Debug.Log($"[NPC MANAGER DEBUG] ✅ Found schedule event for {spawnData.npcID} at hour {hour}");
                Debug.Log($"[NPC MANAGER DEBUG] Event details - Tag: '{scheduleEvent.targetObjectTag}', Name: '{scheduleEvent.targetObjectName}', Behavior: {scheduleEvent.behavior}");
                
                // This hour has a specific event - execute it
                ScheduleCommandType commandType = scheduleEvent.shouldDespawn ?
                    ScheduleCommandType.GoHome : ScheduleCommandType.Move;
                
                Vector2 targetPos = scheduleEvent.GetTargetPosition();
                Debug.Log($"[NPC MANAGER DEBUG] Target position for {spawnData.npcID}: {targetPos}");

                var scheduleCommand = new ScheduleCommand
                {
                    commandType = commandType,
                    targetPosition = targetPos,
                    behavior = scheduleEvent.behavior,
                    shouldIdleWhenReached = scheduleEvent.shouldIdleWhenReached,
                    canInteract = true // Always allow interaction unless specified otherwise
                };

                pendingCommands[existingNPC] = scheduleCommand;
                Debug.Log($"[NPC MANAGER DEBUG] ✅ Created {commandType} command for {spawnData.npcID} to position {targetPos}" +
                         (scheduleEvent.shouldDespawn ? " (will despawn on arrival)" : ""));
            }
            else
            {
                Debug.Log($"[NPC MANAGER DEBUG] ⚠️ No schedule event found for {spawnData.npcID} at hour {hour}");
            }
            // If no specific event for this hour, NPC continues current behavior
        }
    }

    private void ExecutePendingScheduleCommands()
    {
        Debug.Log($"[NPC MANAGER DEBUG] Executing {pendingCommands.Count} pending schedule commands");
        
        foreach (var kvp in pendingCommands)
        {
            NPC npc = kvp.Key;
            ScheduleCommand command = kvp.Value;

            if (npc != null)
            {
                Debug.Log($"[NPC MANAGER DEBUG] Sending {command.commandType} command to {npc.npcName} for position {command.targetPosition}");
                npc.ReceiveScheduleCommand(command);
            }
            else
            {
                Debug.LogWarning($"[NPC MANAGER DEBUG] Trying to send command to NULL NPC!");
            }
        }

        pendingCommands.Clear();
        Debug.Log($"[NPC MANAGER DEBUG] All pending commands executed and cleared");
    }

    // Public methods for NPCs to interact with the manager
    public void NotifyNPCDestinationReached(NPC npc)
    {
        // Could trigger additional logic here if needed
    }

    public void RequestNPCDespawn(NPC npc)
    {
        Debug.Log($"NPCManager: Despawn requested for NPC {npc.npcName}");
        DespawnNPC(npc);
    }

    private bool ShouldNPCBeActiveAtHour(NPCScheduleData scheduleData, int hour)
    {
        // First check if it's past spawn hour
        if (hour < scheduleData.spawnHour)
        {
            return false;
        }
        
        // Check if there's a despawn event that has already occurred
        if (scheduleData.scheduleEvents != null)
        {
            // Find the most recent event that has occurred (including current hour)
            ScheduleEvent mostRecentEvent = null;
            foreach (var scheduleEvent in scheduleData.scheduleEvents)
            {
                if (scheduleEvent != null && scheduleEvent.hour <= hour)
                {
                    if (mostRecentEvent == null || scheduleEvent.hour > mostRecentEvent.hour)
                    {
                        mostRecentEvent = scheduleEvent;
                    }
                }
            }
            
            // If the most recent event is a despawn event, NPC should not be active
            if (mostRecentEvent != null && mostRecentEvent.shouldDespawn)
            {
                return false;
            }
        }
        
        return true;
    }

    private bool ShouldSpawnNPC(NPCSpawnData spawnData)
    {
        // Use the improved flag checking from NPCSpawnData
        return spawnData.ShouldSpawn(HasGameFlag) && GetActiveScheduleForNPC(spawnData) != null;
    }
    
    /// <summary>
    /// Get the active schedule for an NPC based on current flags
    /// </summary>
    private NPCScheduleData GetActiveScheduleForNPC(NPCSpawnData spawnData)
    {
        return spawnData.GetActiveSchedule(HasGameFlag);
    }
    
    /// <summary>
    /// Get current schedule for an already spawned NPC (checks cache first)
    /// </summary>
    private NPCScheduleData GetCurrentNPCSchedule(NPC npc, NPCSpawnData spawnData)
    {
        // If NPC is in dialogue, use cached schedule to avoid mid-conversation changes
        if (IsNPCInDialogue(npc))
        {
            if (activeScheduleCache.TryGetValue(spawnData.npcID, out NPCScheduleData cachedSchedule))
            {
                return cachedSchedule;
            }
        }
        
        return GetActiveScheduleForNPC(spawnData);
    }
    
    /// <summary>
    /// Check if an NPC needs a schedule change and queue it if they're in dialogue
    /// </summary>
    private void CheckForScheduleChange(NPCSpawnData spawnData, NPC npc, NPCScheduleData newActiveSchedule)
    {
        if (npc == null) return;
        
        // Get current schedule from cache
        if (!activeScheduleCache.TryGetValue(spawnData.npcID, out NPCScheduleData currentSchedule))
        {
            currentSchedule = npc.scheduleData;
        }
        
        // Check if schedule should change
        if (currentSchedule != newActiveSchedule)
        {
            Debug.Log($"[NPC MANAGER DEBUG] Schedule change detected for {spawnData.npcID}");
            
            // If NPC is in dialogue, queue the change for later
            if (IsNPCInDialogue(npc))
            {
                Debug.Log($"[NPC MANAGER DEBUG] {spawnData.npcID} is in dialogue, queuing schedule change");
                QueueScheduleChange(npc, spawnData, newActiveSchedule, "Flag change during dialogue");
            }
            else
            {
                // Apply schedule change immediately
                Debug.Log($"[NPC MANAGER DEBUG] Applying immediate schedule change for {spawnData.npcID}");
                ApplyScheduleChange(npc, spawnData, newActiveSchedule);
            }
        }
    }
    
    /// <summary>
    /// Check if an NPC is currently in dialogue
    /// </summary>
    private bool IsNPCInDialogue(NPC npc)
    {
        if (npc == null || interactionSystem == null) return false;
        
        // Check if this NPC is the current dialogue target
        // This assumes NPCInteractionSystem has a way to check current dialogue state
        // You may need to adjust this based on your dialogue system implementation
        return interactionSystem.IsInDialogueWith(npc);
    }
    
    /// <summary>
    /// Queue a schedule change for when dialogue ends
    /// </summary>
    private void QueueScheduleChange(NPC npc, NPCSpawnData spawnData, NPCScheduleData newSchedule, string reason)
    {
        var pendingChange = new PendingScheduleChange
        {
            targetNPC = npc,
            spawnData = spawnData,
            newSchedule = newSchedule,
            changeRequestTime = Time.time,
            reason = reason
        };
        
        pendingScheduleChanges[npc] = pendingChange;
        Debug.Log($"[NPC MANAGER DEBUG] Queued schedule change for {spawnData.npcID}: {reason}");
    }
    
    /// <summary>
    /// Apply a schedule change immediately
    /// </summary>
    private void ApplyScheduleChange(NPC npc, NPCSpawnData spawnData, NPCScheduleData newSchedule)
    {
        npc.scheduleData = newSchedule;
        activeScheduleCache[spawnData.npcID] = newSchedule;
        
        // Force the NPC to update to the new schedule
        npc.ResetToScheduledBehavior();
        
        Debug.Log($"[NPC MANAGER DEBUG] Applied schedule change for {spawnData.npcID}");
    }
    
    /// <summary>
    /// Process pending schedule changes (call this when dialogues end)
    /// </summary>
    public void ProcessPendingScheduleChanges()
    {
        var changesToProcess = new System.Collections.Generic.List<NPC>(pendingScheduleChanges.Keys);
        
        foreach (NPC npc in changesToProcess)
        {
            if (npc != null && !IsNPCInDialogue(npc))
            {
                var pendingChange = pendingScheduleChanges[npc];
                Debug.Log($"[NPC MANAGER DEBUG] Processing pending schedule change for {pendingChange.spawnData.npcID}: {pendingChange.reason}");
                
                ApplyScheduleChange(pendingChange.targetNPC, pendingChange.spawnData, pendingChange.newSchedule);
                pendingScheduleChanges.Remove(npc);
            }
        }
    }
    
    /// <summary>
    /// Helper method to check game flags (integrates with your flag system)
    /// </summary>
    private bool HasGameFlag(string flagName)
    {
        if (interactionSystem != null)
        {
            return interactionSystem.HasGameFlag(flagName);
        }
        return false;
    }

    #endregion
    
    #region Conditional Schedule Debugging
    
    /// <summary>
    /// Debug method to check what schedule an NPC would use with current flags
    /// </summary>
    public void DebugNPCSchedule(string npcID)
    {
        NPCSpawnData spawnData = npcSpawnList.Find(data => data.npcID == npcID);
        if (spawnData == null)
        {
            Debug.LogWarning($"[SCHEDULE DEBUG] NPC '{npcID}' not found in spawn list");
            return;
        }
        
        NPCScheduleData activeSchedule = GetActiveScheduleForNPC(spawnData);
        
        Debug.Log($"[SCHEDULE DEBUG] === Schedule Debug for {npcID} ===");
        Debug.Log($"[SCHEDULE DEBUG] Should spawn: {spawnData.ShouldSpawn(HasGameFlag)}");
        Debug.Log($"[SCHEDULE DEBUG] Active schedule: {(activeSchedule != null ? activeSchedule.scheduleName : "NONE")}");
        
        if (spawnData.conditionalSchedules != null && spawnData.conditionalSchedules.Length > 0)
        {
            Debug.Log($"[SCHEDULE DEBUG] Available conditional schedules ({spawnData.conditionalSchedules.Length}):");
            for (int i = 0; i < spawnData.conditionalSchedules.Length; i++)
            {
                var schedule = spawnData.conditionalSchedules[i];
                if (schedule != null)
                {
                    bool isActive = schedule.IsActiveForFlags(HasGameFlag);
                    Debug.Log($"[SCHEDULE DEBUG]   {i}: '{schedule.description}' - Priority: {schedule.priority}, Active: {isActive}");
                }
            }
        }
        else if (spawnData.scheduleData != null)
        {
            Debug.Log($"[SCHEDULE DEBUG] Using legacy schedule: {spawnData.scheduleData.scheduleName}");
        }
        else
        {
            Debug.Log($"[SCHEDULE DEBUG] ❌ No schedule data found!");
        }
        
    }
    
    /// <summary>
    /// Debug method to show all pending schedule changes
    /// </summary>
    public void DebugPendingScheduleChanges()
    {
        Debug.Log($"[SCHEDULE DEBUG] === Pending Schedule Changes ({pendingScheduleChanges.Count}) ===");
        
        if (pendingScheduleChanges.Count == 0)
        {
            Debug.Log($"[SCHEDULE DEBUG] No pending schedule changes");
            return;
        }
        
        foreach (var kvp in pendingScheduleChanges)
        {
            var change = kvp.Value;
            Debug.Log($"[SCHEDULE DEBUG] {change.spawnData.npcID}: {change.reason}");
            Debug.Log($"[SCHEDULE DEBUG]   New schedule: {change.newSchedule?.scheduleName ?? "NULL"}");
            Debug.Log($"[SCHEDULE DEBUG]   Requested at: {change.changeRequestTime}");
            Debug.Log($"[SCHEDULE DEBUG]   In dialogue: {IsNPCInDialogue(change.targetNPC)}");
        }
    }
    
    /// <summary>
    /// Get all active flags from the interaction system
    /// </summary>
    private string[] GetAllActiveFlags()
    {
        if (interactionSystem != null)
        {
            return interactionSystem.GetGameFlags().ToArray();
        }
        return new string[0];
    }
    
    /// <summary>
    /// Force refresh all NPC schedules (useful for testing flag changes)
    /// </summary>
    public void ForceRefreshAllSchedules()
    {
        Debug.Log($"[SCHEDULE DEBUG] Force refreshing all NPC schedules...");
        
        foreach (var spawnData in npcSpawnList)
        {
            NPC existingNPC = GetNPCByID(spawnData.npcID);
            if (existingNPC != null)
            {
                NPCScheduleData newActiveSchedule = GetActiveScheduleForNPC(spawnData);
                CheckForScheduleChange(spawnData, existingNPC, newActiveSchedule);
            }
        }
        
        // Process any changes immediately if no dialogue is active
        if (interactionSystem == null || !interactionSystem.IsInDialogue)
        {
            ProcessPendingScheduleChanges();
        }
        
        Debug.Log($"[SCHEDULE DEBUG] Schedule refresh complete");
    }
    
    #endregion

    #region Utility Methods


    public void ForceUpdateAllNPCs()
    {
        foreach (NPC npc in spawnedNPCs)
        {
            if (npc != null)
            {
                npc.ResetToScheduledBehavior();
            }
        }
    }

    public void SetAllNPCsBehavior(NPCBehavior behavior)
    {
        foreach (NPC npc in spawnedNPCs)
        {
            if (npc != null)
            {
                // This would require implementing a method to force specific behaviors
                switch (behavior)
                {
                    case NPCBehavior.Flee:
                        // Flee behavior is no longer supported in the new system
                        // Force to idle instead
                        npc.ForceBehavior(npc.IdleState);
                        break;
                    case NPCBehavior.Idle:
                        npc.ForceBehavior(npc.IdleState);
                        break;
                    case NPCBehavior.Walk:
                        npc.ForceBehavior(npc.MoveState);
                        break;
                    case NPCBehavior.Interact:
                        npc.ForceBehavior(npc.InteractionState);
                        break;
                        // Work and Sleep behaviors are handled through scheduling
                }
            }
        }
    }

    #endregion

    #region Save/Load System

    [System.Serializable]
    public class NPCManagerSaveData
    {
        public List<string> spawnedNPCIDs;
        public List<Vector2> npcPositions;
        public List<string> npcNames;
    }

    public NPCManagerSaveData GetSaveData()
    {
        NPCManagerSaveData saveData = new NPCManagerSaveData
        {
            spawnedNPCIDs = new List<string>(),
            npcPositions = new List<Vector2>(),
            npcNames = new List<string>()
        };

        foreach (NPC npc in spawnedNPCs)
        {
            if (npc != null)
            {
                saveData.spawnedNPCIDs.Add(npc.gameObject.name);
                saveData.npcPositions.Add(npc.transform.position);
                saveData.npcNames.Add(npc.npcName);
            }
        }

        return saveData;
    }

    public void LoadSaveData(NPCManagerSaveData saveData)
    {
        if (saveData == null) return;

        // Clear existing NPCs
        foreach (NPC npc in spawnedNPCs.ToList())
        {
            if (npc != null)
                DespawnNPC(npc);
        }

        // Respawn NPCs from save data
        for (int i = 0; i < saveData.spawnedNPCIDs.Count; i++)
        {
            string npcID = saveData.spawnedNPCIDs[i];
            // Note: Position is ignored now - NPCs spawn at their schedule data home position

            // Find matching spawn data
            NPCSpawnData spawnData = npcSpawnList.FirstOrDefault(data => data.npcID == npcID);
            if (spawnData != null)
            {
                SpawnNPC(spawnData);
            }
        }
    }

    #endregion

    #region Debug and Gizmos

    private void OnDrawGizmosSelected()
    {

        // Draw NPC home positions from schedule data
        Gizmos.color = Color.blue;
        foreach (var spawnData in npcSpawnList)
        {
            if (spawnData.scheduleData != null)
            {
                Vector2 homePos = spawnData.scheduleData.GetHomePosition();
                Gizmos.DrawWireSphere(homePos, 0.5f);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(homePos + Vector2.up * 0.8f, spawnData.npcID);
#endif
            }
        }

    }

    private void Update()
    {
        if (showDebugInfo && Application.isPlaying)
        {
            // Debug GUI can be added here for runtime information
        }
    }

    #endregion
}

[System.Serializable]
public struct ScheduleCommand
{
    public ScheduleCommandType commandType;
    public Vector2 targetPosition;
    public NPCBehavior behavior;
    public bool shouldIdleWhenReached;
    public bool canInteract;
}

[System.Serializable]
public struct PendingScheduleChange
{
    public NPC targetNPC;
    public NPCSpawnData spawnData;
    public NPCScheduleData newSchedule;
    public float changeRequestTime;
    public string reason; // For debugging
}

public enum ScheduleCommandType
{
    Move,
    Idle,
    GoHome,
    Despawn
}
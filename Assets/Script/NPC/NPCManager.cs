using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ConditionalSchedule
{
    public NPCScheduleData scheduleData;
    public string[] requiredFlags = new string[0];
    public string[] excludeFlags = new string[0];
    public int priority = 0;
    public string description = "Default Schedule";
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
    [Tooltip("NPCScheduleData contains spawn position (home location) and all scheduling info")]
    public NPCScheduleData scheduleData;

    [Header("Conditional Schedules (Optional)")]
    [Tooltip("Alternative schedules based on story flags. If empty, uses scheduleData above.")]
    public ConditionalSchedule[] conditionalSchedules = new ConditionalSchedule[0];

    [Header("Spawn Conditions")]
    public string[] requiredFlags; // For quest-based spawning
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

        // Get spawn position from schedule data (home position)
        Vector2 spawnPos = Vector2.zero;
        if (spawnData.scheduleData != null)
        {
            spawnPos = spawnData.scheduleData.GetHomePosition();
        }
        else
        {
            Debug.LogError($"NPCManager: No schedule data assigned for NPC '{spawnData.npcID}'");
            return null;
        }

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

        // Apply schedule data
        if (spawnData.scheduleData != null)
        {
            npc.scheduleData = spawnData.scheduleData;
        }
        else
        {
            Debug.LogError($"NPCManager: No schedule data for NPC '{npc.npcName}'");
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
        if (spawnData.scheduleData == null) 
        {
            Debug.Log($"[NPC MANAGER DEBUG] No schedule data for {spawnData.npcID}");
            return;
        }

        Debug.Log($"[NPC MANAGER DEBUG] Processing schedule for {spawnData.npcID} at hour {hour}");
        
        NPC existingNPC = GetNPCByID(spawnData.npcID);
        bool shouldBeActive = ShouldNPCBeActiveAtHour(spawnData.scheduleData, hour);
        
        Debug.Log($"[NPC MANAGER DEBUG] {spawnData.npcID} - spawnHour: {spawnData.scheduleData.spawnHour}, currentHour: {hour}, shouldBeActive: {shouldBeActive}, existingNPC: {(existingNPC != null ? "EXISTS" : "NULL")}");

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
            Vector2 homePos = spawnData.scheduleData.GetHomePosition();
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
            // Check if there's a schedule event for this hour
            var scheduleEvent = spawnData.scheduleData.GetScheduleEventForHour(hour);

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
        // Check required flags (quest integration)
        if (spawnData.requiredFlags != null && spawnData.requiredFlags.Length > 0)
        {
            if (interactionSystem != null)
            {
                foreach (string flag in spawnData.requiredFlags)
                {
                    if (!interactionSystem.HasGameFlag(flag))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
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

public enum ScheduleCommandType
{
    Move,
    Idle,
    GoHome,
    Despawn
}
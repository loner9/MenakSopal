using UnityEngine;

/// <summary>
/// Debug helper to understand what's happening with your NPC spawn/despawn behavior.
/// Attach this to your NPC Manager to get detailed logging about NPC lifecycle events.
/// </summary>
public class NPCDebugHelper : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool logSpawnReasons = true;
    public bool logDespawnReasons = true;
    public bool logScheduleChanges = true;
    public bool logTimeChanges = true;
    
    private SyncedScriptableNPCManager npcManager;
    private DayNightCycle dayNightCycle;
    
    void Start()
    {
        // Find the components we need to monitor
        npcManager = GetComponent<SyncedScriptableNPCManager>();
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        
        if (npcManager != null)
        {
            // Subscribe to NPC events
            npcManager.OnNPCSpawned += OnNPCSpawned;
            npcManager.OnNPCDespawned += OnNPCDespawned;
            npcManager.OnHourChanged += OnHourChanged;
        }
        
        Debug.Log("=== NPC Debug Helper Active ===");
        Debug.Log($"Current time: {dayNightCycle?.CurrentTime:F1} hours");
        Debug.Log($"Current hour: {npcManager?.CurrentHour}");
        Debug.Log($"Managing {npcManager?.npcSpawnDataAssets?.Count} NPC configurations");
    }
    
    void OnNPCSpawned(NPCSpawnData spawnData, SimpleNPC npc)
    {
        if (!logSpawnReasons) return;
        
        Debug.Log($"🎭 NPC SPAWNED: {spawnData.displayName}");
        Debug.Log($"  Reason: Hour {npcManager.CurrentHour} spawn conditions met");
        Debug.Log($"  Start Hour: {spawnData.scheduleData?.startHour}");
        Debug.Log($"  Position: {spawnData.GetSpawnPosition()}");
        Debug.Log($"  Time restrictions: {spawnData.hasTimeRestrictions}");
        
        if (spawnData.hasTimeRestrictions)
        {
            string allowedTimes = string.Join(", ", spawnData.allowedSpawnTimes);
            Debug.Log($"  Allowed times: {allowedTimes}");
            Debug.Log($"  Current time of day: {npcManager.CurrentTimeOfDay}");
        }
    }
    
    void OnNPCDespawned(NPCSpawnData spawnData)
    {
        if (!logDespawnReasons) return;
        
        Debug.Log($"🎭 NPC DESPAWNED: {spawnData.displayName}");
        Debug.Log($"  Reason: Hour {npcManager.CurrentHour} spawn conditions no longer met");
        
        // Check why the NPC was despawned
        bool canSpawnNow = spawnData.CanSpawnNow(npcManager.CurrentTimeOfDay, 
            FindObjectOfType<EnhancedNPCInteractionSystem>()?.GetGameFlags() ?? new System.Collections.Generic.List<string>());
        
        if (!canSpawnNow)
        {
            string reason = spawnData.GetSpawnStatus(npcManager.CurrentTimeOfDay, 
                FindObjectOfType<EnhancedNPCInteractionSystem>()?.GetGameFlags() ?? new System.Collections.Generic.List<string>());
            Debug.Log($"  Specific reason: {reason}");
        }
    }
    
    void OnHourChanged(int newHour)
    {
        if (!logTimeChanges) return;
        
        Debug.Log($"⏰ HOUR CHANGED to {newHour}:00");
        Debug.Log($"  Game time: {dayNightCycle?.CurrentTime:F2}");
        Debug.Log($"  Time of day: {npcManager?.CurrentTimeOfDay}");
        
        if (logScheduleChanges)
        {
            Debug.Log("  NPCs checking schedules...");
        }
    }
    
    /// <summary>
    /// Call this method to get a detailed status report of all NPCs
    /// </summary>
    [ContextMenu("Print NPC Status Report")]
    public void PrintStatusReport()
    {
        if (npcManager == null) return;
        
        Debug.Log("=== DETAILED NPC STATUS REPORT ===");
        Debug.Log(npcManager.GetSpawnStatusReport());
        
        // Additional debugging for common issues
        Debug.Log("\n=== COMMON ISSUE CHECKS ===");
        
        foreach (var spawnData in npcManager.npcSpawnDataAssets)
        {
            if (spawnData == null) continue;
            
            // Check for NPCs that spawn and immediately despawn
            if (spawnData.scheduleData != null)
            {
                int startHour = spawnData.scheduleData.startHour;
                var schedule = spawnData.scheduleData.GetScheduleForHour(startHour);
                
                if (schedule != null && schedule.shouldDespawn)
                {
                    Debug.LogWarning($"⚠️  {spawnData.displayName}: Spawns at hour {startHour} but immediately despawns! " +
                                   "Check if first schedule entry has shouldDespawn = true");
                }
            }
            
            // Check for NPCs with no schedule entries
            if (spawnData.scheduleData != null && 
                (spawnData.scheduleData.hourlySchedule == null || spawnData.scheduleData.hourlySchedule.Length == 0))
            {
                Debug.LogWarning($"⚠️  {spawnData.displayName}: No hourly schedule entries defined! " +
                               "NPC will spawn but won't have any destinations.");
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (npcManager != null)
        {
            npcManager.OnNPCSpawned -= OnNPCSpawned;
            npcManager.OnNPCDespawned -= OnNPCDespawned;
            npcManager.OnHourChanged -= OnHourChanged;
        }
    }
}
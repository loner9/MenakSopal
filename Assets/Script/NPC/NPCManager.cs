using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class NPCSpawnData
{
    [Header("NPC Prefab")]
    public GameObject npcPrefab;
    public string npcID; // Unique identifier for save/load
    
    [Header("Spawn Settings")]
    public Vector2 spawnPosition;
    public bool spawnAtStart = true;
    
    [Header("Schedule Override")]
    public bool useCustomSchedule = false;
    public NPCScheduleData customSchedule;
    
    [Header("Spawn Conditions")]
    public bool spawnBasedOnTime = false;
    public TimeOfDay[] availableSpawnTimes;
    public string[] requiredFlags; // For quest-based spawning
}

public class NPCManager : MonoBehaviour
{
    [Header("NPC Management")]
    public List<NPCSpawnData> npcSpawnList = new List<NPCSpawnData>();
    public Transform npcParent; // Optional parent object for organization
    
    [Header("Spawn Areas")]
    public Vector2 villageAreaMin = new Vector2(-20, -20);
    public Vector2 villageAreaMax = new Vector2(20, 20);
    public LayerMask obstacleLayerMask;
    
    [Header("Performance Settings")]
    public float npcUpdateInterval = 0.5f; // How often to update distant NPCs
    public float maxActiveDistance = 30f; // Distance to keep NPCs fully active
    public float cullingDistance = 50f; // Distance to completely disable NPCs
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool showSpawnAreas = true;
    
    // Runtime data
    private List<NPC> spawnedNPCs = new List<NPC>();
    private List<NPC> activeNPCs = new List<NPC>();
    private List<NPC> dormantNPCs = new List<NPC>();
    
    // Systems integration
    private DayNightCycle dayNightCycle;
    private NPCInteractionSystem interactionSystem;
    private Transform player;
    
    // Performance optimization
    private float lastUpdateTime;
    
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
        
        // Subscribe to day/night events
        if (dayNightCycle != null)
        {
            dayNightCycle.OnTimeOfDayChanged += OnTimeOfDayChanged;
        }
        
        // Spawn initial NPCs
        SpawnInitialNPCs();
        
        // Start performance optimization routine
        InvokeRepeating(nameof(OptimizeNPCPerformance), 1f, npcUpdateInterval);
    }
    
    private void OnDestroy()
    {
        if (dayNightCycle != null)
        {
            dayNightCycle.OnTimeOfDayChanged -= OnTimeOfDayChanged;
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
        
        // Spawn the NPC
        Vector2 spawnPos = spawnData.spawnPosition != Vector2.zero ? 
                          spawnData.spawnPosition : 
                          GetValidSpawnPosition();
        
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
        activeNPCs.Add(npc);
        
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
        
        // Apply custom schedule if specified
        if (spawnData.useCustomSchedule && spawnData.customSchedule != null)
        {
            npc.scheduleData = spawnData.customSchedule;
        }
        
        // Force schedule update based on current time
        if (dayNightCycle != null)
        {
            npc.currentTimeOfDay = dayNightCycle.CurrentTimeOfDay;
        }
    }
    
    public NPC SpawnNPCAt(GameObject npcPrefab, Vector2 position, string npcID = null)
    {
        NPCSpawnData tempSpawnData = new NPCSpawnData
        {
            npcPrefab = npcPrefab,
            npcID = npcID ?? System.Guid.NewGuid().ToString(),
            spawnPosition = position,
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
        activeNPCs.Remove(npc);
        dormantNPCs.Remove(npc);
        
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
        return new List<NPC>(activeNPCs);
    }
    
    public int GetTotalNPCCount()
    {
        return spawnedNPCs.Count;
    }
    
    #endregion
    
    #region Day/Night Integration
    
    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        // Handle time-based NPC spawning/despawning
        foreach (var spawnData in npcSpawnList)
        {
            if (spawnData.spawnBasedOnTime)
            {
                bool shouldBeSpawned = spawnData.availableSpawnTimes.Contains(newTimeOfDay);
                NPC existingNPC = GetNPCByID(spawnData.npcID);
                
                if (shouldBeSpawned && existingNPC == null)
                {
                    SpawnNPC(spawnData);
                }
                else if (!shouldBeSpawned && existingNPC != null)
                {
                    DespawnNPC(existingNPC);
                }
            }
        }
        
        Debug.Log($"NPCManager: Time changed to {newTimeOfDay}, managing {spawnedNPCs.Count} NPCs");
    }
    
    private bool ShouldSpawnNPC(NPCSpawnData spawnData)
    {
        // Check time-based spawning
        if (spawnData.spawnBasedOnTime && dayNightCycle != null)
        {
            if (!spawnData.availableSpawnTimes.Contains(dayNightCycle.CurrentTimeOfDay))
            {
                return false;
            }
        }
        
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
    
    #region Performance Optimization
    
    private void OptimizeNPCPerformance()
    {
        if (player == null) return;
        
        activeNPCs.Clear();
        dormantNPCs.Clear();
        
        foreach (NPC npc in spawnedNPCs)
        {
            if (npc == null) continue;
            
            float distance = Vector2.Distance(player.position, npc.transform.position);
            
            if (distance > cullingDistance)
            {
                // Completely disable NPCs that are very far away
                npc.gameObject.SetActive(false);
                dormantNPCs.Add(npc);
            }
            else if (distance > maxActiveDistance)
            {
                // Reduce update frequency for distant NPCs
                npc.gameObject.SetActive(true);
                
                // Optionally disable complex behaviors for distant NPCs
                if (npc.StateMachine != null)
                {
                    // Keep basic state but reduce updates
                    npc.enabled = false;
                }
                
                dormantNPCs.Add(npc);
            }
            else
            {
                // Keep NPCs near player fully active
                npc.gameObject.SetActive(true);
                npc.enabled = true;
                activeNPCs.Add(npc);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"NPCManager: {activeNPCs.Count} active, {dormantNPCs.Count} dormant NPCs");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    private Vector2 GetValidSpawnPosition()
    {
        Vector2 pos;
        int attempts = 0;
        int maxAttempts = 50;
        
        do
        {
            pos = new Vector2(
                Random.Range(villageAreaMin.x, villageAreaMax.x),
                Random.Range(villageAreaMin.y, villageAreaMax.y)
            );
            attempts++;
            
            // Check if position is valid (not overlapping with obstacles)
            Collider2D hitCollider = Physics2D.OverlapCircle(pos, 0.5f, obstacleLayerMask);
            if (hitCollider == null)
            {
                return pos;
            }
            
        } while (attempts < maxAttempts);
        
        Debug.LogWarning($"NPCManager: Could not find valid spawn position after {maxAttempts} attempts");
        return (villageAreaMin + villageAreaMax) * 0.5f;
    }
    
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
                        if (player != null)
                            npc.FleeFromThreat(player.position);
                        break;
                    case NPCBehavior.Idle:
                        npc.ForceBehavior(npc.IdleState);
                        break;
                    // Add other behaviors as needed
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
            Vector2 position = saveData.npcPositions[i];
            
            // Find matching spawn data
            NPCSpawnData spawnData = npcSpawnList.FirstOrDefault(data => data.npcID == npcID);
            if (spawnData != null)
            {
                spawnData.spawnPosition = position;
                SpawnNPC(spawnData);
            }
        }
    }
    
    #endregion
    
    #region Debug and Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (showSpawnAreas)
        {
            // Draw village area
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Vector3 center = new Vector3(
                (villageAreaMin.x + villageAreaMax.x) * 0.5f,
                (villageAreaMin.y + villageAreaMax.y) * 0.5f,
                0
            );
            Vector3 size = new Vector3(
                villageAreaMax.x - villageAreaMin.x,
                villageAreaMax.y - villageAreaMin.y,
                0.1f
            );
            Gizmos.DrawCube(center, size);
        }
        
        // Draw spawn positions
        Gizmos.color = Color.blue;
        foreach (var spawnData in npcSpawnList)
        {
            if (spawnData.spawnPosition != Vector2.zero)
            {
                Gizmos.DrawWireSphere(spawnData.spawnPosition, 0.5f);
            }
        }
        
        // Draw performance zones around player if in play mode
        if (Application.isPlaying && player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, maxActiveDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, cullingDistance);
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
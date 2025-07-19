using UnityEngine;

/// <summary>
/// ScriptableObject version of NPC spawn data.
/// This creates reusable, modular NPC configurations that can be shared between scenes,
/// organized as separate asset files, and easily managed in large projects.
/// 
/// Design Philosophy:
/// - Each NPC configuration is a separate asset file
/// - Can be referenced by multiple managers or systems
/// - Easy to organize in folders by NPC type, location, or purpose
/// - Supports runtime modification and save/load systems
/// - Better for team development (no merge conflicts)
/// </summary>
[CreateAssetMenu(fileName = "New NPC Spawn Data", menuName = "NPC/Spawn Configuration")]
public class NPCSpawnData : ScriptableObject
{
    [Header("NPC Identity")]
    [Tooltip("Unique identifier for this NPC - used for save/load and tracking")]
    public string npcID = "";
    
    [Tooltip("Human-readable name for organization - auto-filled from asset name")]
    public string displayName = "";
    
    [Header("Prefab and Schedule")]
    [Tooltip("The NPC prefab to spawn - should have SimpleNPC and NPCInteractionBridge components")]
    public GameObject npcPrefab;
    
    [Tooltip("Schedule data that defines this NPC's daily routine")]
    public SimpleNPCScheduleData scheduleData;
    
    [Header("Spawn Settings")]
    [Tooltip("Where this NPC spawns when first created")]
    public Vector2 spawnPosition = Vector2.zero;
    
    [Tooltip("Should this NPC spawn automatically when the game starts?")]
    public bool spawnAtGameStart = true;
    
    [Header("Spawn Conditions")]
    [Tooltip("Only spawn this NPC during specific times of day")]
    public bool hasTimeRestrictions = false;
    
    [Tooltip("Times when this NPC can be spawned (if time restrictions enabled)")]
    public TimeOfDay[] allowedSpawnTimes = { TimeOfDay.Day };
    
    [Header("Quest Integration")]
    [Tooltip("Game flags required before this NPC can spawn")]
    public string[] requiredFlags = new string[0];
    
    [Tooltip("Game flags that prevent this NPC from spawning")]
    public string[] blockedByFlags = new string[0];
    
    [Header("Advanced Settings")]
    [Tooltip("Custom spawn position override - use this for NPCs that spawn in different locations")]
    public bool useCustomSpawnPosition = false;
    
    [Tooltip("Custom spawn position (overrides schedule spawn position if enabled)")]
    public Vector2 customSpawnPosition = Vector2.zero;
    
    [Header("Metadata")]
    [Tooltip("Category for organization - Village, Shop, Guard, Quest, etc.")]
    public string category = "Village";
    
    [Tooltip("Description of this NPC's role and behavior")]
    [TextArea(3, 5)]
    public string description = "Describe what this NPC does and when they appear...";
    
    [Header("Runtime Data - Do Not Edit")]
    [Tooltip("Is this NPC currently spawned? (Runtime only - don't edit manually)")]
    [System.NonSerialized]
    public bool isCurrentlySpawned = false;
    
    [Tooltip("Reference to spawned instance (Runtime only - don't edit manually)")]
    [System.NonSerialized] 
    public SimpleNPC spawnedInstance = null;
    
    /// <summary>
    /// Gets the actual spawn position, considering custom position overrides.
    /// This method encapsulates the logic for determining where an NPC should spawn.
    /// </summary>
    public Vector2 GetSpawnPosition()
    {
        // Priority 1: Custom spawn position (for special cases)
        if (useCustomSpawnPosition)
        {
            return customSpawnPosition;
        }
        
        // Priority 2: Position specified in this spawn data
        if (spawnPosition != Vector2.zero)
        {
            return spawnPosition;
        }
        
        // Priority 3: Default spawn position from schedule data
        if (scheduleData != null)
        {
            return scheduleData.spawnPosition;
        }
        
        // Fallback: origin (should rarely happen)
        Debug.LogWarning($"NPCSpawnData {name}: No spawn position configured, using Vector2.zero");
        return Vector2.zero;
    }
    
    /// <summary>
    /// Checks if this NPC can be spawned based on current game conditions.
    /// This method encapsulates all the spawn condition logic in one place.
    /// </summary>
    public bool CanSpawnNow(TimeOfDay currentTimeOfDay, System.Collections.Generic.List<string> gameFlags)
    {
        // Check if already spawned
        if (isCurrentlySpawned)
        {
            return false;
        }
        
        // Check time restrictions
        if (hasTimeRestrictions)
        {
            bool timeAllowed = false;
            foreach (TimeOfDay allowedTime in allowedSpawnTimes)
            {
                if (currentTimeOfDay == allowedTime)
                {
                    timeAllowed = true;
                    break;
                }
            }
            
            if (!timeAllowed)
            {
                return false;
            }
        }
        
        // Check required flags
        if (requiredFlags != null && requiredFlags.Length > 0)
        {
            foreach (string requiredFlag in requiredFlags)
            {
                if (!gameFlags.Contains(requiredFlag))
                {
                    return false; // Missing required flag
                }
            }
        }
        
        // Check blocked flags
        if (blockedByFlags != null && blockedByFlags.Length > 0)
        {
            foreach (string blockedFlag in blockedByFlags)
            {
                if (gameFlags.Contains(blockedFlag))
                {
                    return false; // Blocked by flag
                }
            }
        }
        
        // Check basic requirements
        if (npcPrefab == null || scheduleData == null)
        {
            Debug.LogError($"NPCSpawnData {name}: Missing required components (prefab or schedule data)");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets a human-readable status description for debugging and editor display.
    /// This helps developers understand why an NPC might not be spawning.
    /// </summary>
    public string GetSpawnStatus(TimeOfDay currentTimeOfDay, System.Collections.Generic.List<string> gameFlags)
    {
        if (isCurrentlySpawned)
        {
            return "Currently spawned";
        }
        
        if (npcPrefab == null)
        {
            return "Missing NPC prefab";
        }
        
        if (scheduleData == null)
        {
            return "Missing schedule data";
        }
        
        if (hasTimeRestrictions)
        {
            bool timeAllowed = false;
            foreach (TimeOfDay allowedTime in allowedSpawnTimes)
            {
                if (currentTimeOfDay == allowedTime)
                {
                    timeAllowed = true;
                    break;
                }
            }
            
            if (!timeAllowed)
            {
                return $"Time restricted (current: {currentTimeOfDay})";
            }
        }
        
        if (requiredFlags != null && requiredFlags.Length > 0)
        {
            foreach (string requiredFlag in requiredFlags)
            {
                if (!gameFlags.Contains(requiredFlag))
                {
                    return $"Missing flag: {requiredFlag}";
                }
            }
        }
        
        if (blockedByFlags != null && blockedByFlags.Length > 0)
        {
            foreach (string blockedFlag in blockedByFlags)
            {
                if (gameFlags.Contains(blockedFlag))
                {
                    return $"Blocked by flag: {blockedFlag}";
                }
            }
        }
        
        return "Ready to spawn";
    }
    
    /// <summary>
    /// Validation method called by Unity when values change in the inspector.
    /// This helps prevent common configuration mistakes.
    /// </summary>
    void OnValidate()
    {
        // Auto-fill display name from asset name if empty
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = name;
        }
        
        // Auto-generate NPC ID if empty
        if (string.IsNullOrEmpty(npcID))
        {
            npcID = name.Replace(" ", "_").ToLower();
        }
        
        // Ensure category is set
        if (string.IsNullOrEmpty(category))
        {
            category = "Village";
        }
        
        // Validate spawn position if using schedule data
        if (!useCustomSpawnPosition && spawnPosition == Vector2.zero && scheduleData != null)
        {
            spawnPosition = scheduleData.spawnPosition;
        }
    }
    
    /// <summary>
    /// Creates a copy of this spawn data with a new ID.
    /// Useful for creating variations of similar NPCs.
    /// </summary>
    public NPCSpawnData CreateVariant(string newID, string newDisplayName = "")
    {
        NPCSpawnData variant = CreateInstance<NPCSpawnData>();
        
        // Copy all properties
        variant.npcID = newID;
        variant.displayName = string.IsNullOrEmpty(newDisplayName) ? newID : newDisplayName;
        variant.npcPrefab = this.npcPrefab;
        variant.scheduleData = this.scheduleData;
        variant.spawnPosition = this.spawnPosition;
        variant.spawnAtGameStart = this.spawnAtGameStart;
        variant.hasTimeRestrictions = this.hasTimeRestrictions;
        variant.allowedSpawnTimes = (TimeOfDay[])this.allowedSpawnTimes.Clone();
        variant.requiredFlags = (string[])this.requiredFlags.Clone();
        variant.blockedByFlags = (string[])this.blockedByFlags.Clone();
        variant.useCustomSpawnPosition = this.useCustomSpawnPosition;
        variant.customSpawnPosition = this.customSpawnPosition;
        variant.category = this.category;
        variant.description = this.description;
        
        return variant;
    }
    
    #region Editor Helpers
    
    #if UNITY_EDITOR
    /// <summary>
    /// Custom menu item to create spawn data for existing NPCs.
    /// This provides a workflow for converting existing setups to the new system.
    /// </summary>
    [UnityEditor.MenuItem("Assets/Create/NPC/Spawn Data from Selected NPC")]
    static void CreateSpawnDataFromNPC()
    {
        GameObject selectedNPC = UnityEditor.Selection.activeGameObject;
        if (selectedNPC == null)
        {
            UnityEditor.EditorUtility.DisplayDialog("No NPC Selected", 
                "Please select an NPC GameObject in the scene or project.", "OK");
            return;
        }
        
        SimpleNPC npcComponent = selectedNPC.GetComponent<SimpleNPC>();
        if (npcComponent == null)
        {
            UnityEditor.EditorUtility.DisplayDialog("Invalid NPC", 
                "Selected GameObject doesn't have a SimpleNPC component.", "OK");
            return;
        }
        
        // Create new spawn data asset
        NPCSpawnData spawnData = CreateInstance<NPCSpawnData>();
        spawnData.npcID = selectedNPC.name.Replace(" ", "_").ToLower();
        spawnData.displayName = selectedNPC.name;
        spawnData.npcPrefab = selectedNPC;
        spawnData.spawnPosition = selectedNPC.transform.position;
        spawnData.category = "Generated";
        spawnData.description = $"Generated from existing NPC: {selectedNPC.name}";
        
        // Try to get schedule data
        if (npcComponent.GetScheduleData() != null)
        {
            spawnData.scheduleData = npcComponent.GetScheduleData();
        }
        
        // Save as asset
        string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
            "Save NPC Spawn Data", 
            selectedNPC.name + "_SpawnData", 
            "asset", 
            "Choose where to save the spawn data");
            
        if (!string.IsNullOrEmpty(path))
        {
            UnityEditor.AssetDatabase.CreateAsset(spawnData, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.Selection.activeObject = spawnData;
        }
    }
    #endif
    
    #endregion
}
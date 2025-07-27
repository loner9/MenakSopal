using UnityEngine;
using System.Collections.Generic;

public enum NPCBehavior
{
    Idle,
    Walk,
    Work,
    Sleep,
    Interact,
    Flee
}

[CreateAssetMenu(fileName = "New NPC Schedule", menuName = "NPC/Schedule Data")]
public class NPCScheduleData : ScriptableObject
{
    [Header("Basic Info")]
    public string scheduleName = "Default Schedule";
    [TextArea(2, 4)]
    public string scheduleDescription = "Description of this schedule...";
    
    [Header("Spawn Settings")]
    [Tooltip("Hour when NPC should spawn (0-23)")]
    [Range(0, 23)]
    public int spawnHour = 6;
    
    [Header("Home Location")]
    [Tooltip("Tag to search within for home objects (e.g., 'House', 'Bed', 'NPCHome')")]
    public string homeObjectTag = "House";
    
    [Tooltip("Home object name (e.g., 'House1', 'Bed', etc.). Leave empty to use manual position.")]
    public string homeObjectName;
    
    [Tooltip("Manual home position (used when Home Object Name is empty)")]
    public Vector2 homePosition;
    
    [Header("Schedule Events")]
    [Tooltip("Define specific hours when the NPC should change behavior. NPC maintains current behavior between events.")]
    public ScheduleEvent[] scheduleEvents;
    
    [Header("Behavior Settings")]
    [Tooltip("Should NPC move around randomly when idle at a destination?")]
    public bool moveAroundWhenIdle = false;
    
    [Tooltip("How far NPC can wander when moving around idle area")]
    public float idleMovementRange = 2f;
    
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float pauseAtDestination = 2f;
    
    [Header("Interaction Settings")]
    public string[] dialogues;
    public Transform workStation;
    public float workDuration = 5f;
    
    // New event-based schedule system
    public ScheduleEvent GetScheduleEventForHour(int hour)
    {
        Debug.Log($"[SCHEDULE DATA DEBUG] GetScheduleEventForHour({hour}) called for '{scheduleName}'");
        
        if (scheduleEvents == null || scheduleEvents.Length == 0)
        {
            Debug.Log($"[SCHEDULE DATA DEBUG] No schedule events found for '{scheduleName}'");
            return null;
        }
        
        Debug.Log($"[SCHEDULE DATA DEBUG] Searching through {scheduleEvents.Length} events:");
        // for (int i = 0; i < scheduleEvents.Length; i++)
        // {
        //     var evt = scheduleEvents[i];
        //     if (evt != null)
        //     {
        //         Debug.Log($"[SCHEDULE DATA DEBUG] Event {i}: Hour {evt.hour}, Tag: '{evt.targetObjectTag}', Name: '{evt.targetObjectName}', Behavior: {evt.behavior}");
        //     }
        //     else
        //     {
        //         Debug.Log($"[SCHEDULE DATA DEBUG] Event {i}: NULL");
        //     }
        // }
            
        // Find the most recent event that occurred at or before this hour
        ScheduleEvent currentEvent = null;
        foreach (var scheduleEvent in scheduleEvents)
        {
            if (scheduleEvent != null && scheduleEvent.hour <= hour)
            {
                if (currentEvent == null || scheduleEvent.hour > currentEvent.hour)
                {
                    currentEvent = scheduleEvent;
                }
            }
        }
        
        if (currentEvent != null)
        {
            Debug.Log($"[SCHEDULE DATA DEBUG] ✅ Found event for hour {hour}: Hour {currentEvent.hour}, Tag: '{currentEvent.targetObjectTag}', Name: '{currentEvent.targetObjectName}'");
        }
        else
        {
            Debug.Log($"[SCHEDULE DATA DEBUG] ❌ No event found for hour {hour}");
        }
        
        return currentEvent;
    }
    
    public ScheduleEvent GetNextScheduleEvent(int currentHour)
    {
        if (scheduleEvents == null || scheduleEvents.Length == 0)
            return null;
            
        // Find the next event after current hour
        ScheduleEvent nextEvent = null;
        foreach (var scheduleEvent in scheduleEvents)
        {
            if (scheduleEvent.hour > currentHour)
            {
                if (nextEvent == null || scheduleEvent.hour < nextEvent.hour)
                {
                    nextEvent = scheduleEvent;
                }
            }
        }
        
        return nextEvent;
    }
    
    public bool ShouldBeSpawnedAtHour(int hour)
    {
        // NPC should be spawned if hour is after spawn hour
        // Despawning is now handled by schedule events with shouldDespawn flag
        return hour >= spawnHour;
    }
    
    // Compatibility methods for external systems
    public NPCBehavior GetBehaviorForTime(float currentHour)
    {
        int hour = Mathf.FloorToInt(currentHour);
        var currentEvent = GetScheduleEventForHour(hour);
        
        return currentEvent?.behavior ?? NPCBehavior.Idle;
    }
    
    public Vector2 GetPositionForTime(float currentHour)
    {
        int hour = Mathf.FloorToInt(currentHour);
        var currentEvent = GetScheduleEventForHour(hour);
        
        return currentEvent?.GetTargetPosition() ?? GetHomePosition();
    }
    
    // Not used in new system - returns null
    public PatrolPoint[] GetPatrolPointsForTime(float currentHour)
    {
        return null;
    }
    
    // Static cache for tag-based object lookups
    private static Dictionary<string, Dictionary<string, GameObject>> taggedObjectCache 
        = new Dictionary<string, Dictionary<string, GameObject>>();
    
    // Static utility method for tag-based object lookup with caching
    public static GameObject FindObjectByTagAndName(string tag, string objectName)
    {
        if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(objectName))
            return null;
        
        // Check cache first
        if (taggedObjectCache.TryGetValue(tag, out var tagCache))
        {
            if (tagCache.TryGetValue(objectName, out GameObject cachedObj) && cachedObj != null)
                return cachedObj;
        }
        
        // Cache miss or invalid object - refresh cache for this tag
        RefreshTagCache(tag);
        
        // Try again from refreshed cache
        if (taggedObjectCache.TryGetValue(tag, out var refreshedCache))
        {
            refreshedCache.TryGetValue(objectName, out GameObject obj);
            return obj;
        }
        
        return null;
    }
    
    // Refresh cache for a specific tag
    public static void RefreshTagCache(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;
        
        // Initialize tag cache if needed
        if (!taggedObjectCache.ContainsKey(tag))
            taggedObjectCache[tag] = new Dictionary<string, GameObject>();
        
        var tagCache = taggedObjectCache[tag];
        tagCache.Clear();
        
        // Find all objects with the specified tag and cache them
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in taggedObjects)
        {
            if (obj != null)
                tagCache[obj.name] = obj;
        }
    }
    
    // Clear all caches (useful when scene changes)
    public static void ClearAllCaches()
    {
        taggedObjectCache.Clear();
    }
    
    // Get cached objects for a tag (useful for debugging)
    public static Dictionary<string, GameObject> GetCachedObjectsForTag(string tag)
    {
        if (taggedObjectCache.TryGetValue(tag, out var cache))
            return new Dictionary<string, GameObject>(cache);
        return new Dictionary<string, GameObject>();
    }
    
    // Smart home position resolution with tag-based search
    public Vector2 GetHomePosition()
    {
        // Priority: Tag-based object lookup > Manual position
        if (!string.IsNullOrEmpty(homeObjectName) && !string.IsNullOrEmpty(homeObjectTag))
        {
            GameObject homeObj = FindObjectByTagAndName(homeObjectTag, homeObjectName);
            if (homeObj != null)
                return homeObj.transform.position;
            else
            {
                Debug.LogWarning($"NPCScheduleData: Home object '{homeObjectName}' not found in tag '{homeObjectTag}'. Using manual position.");
                return homePosition;
            }
        }
        else
            return homePosition;
    }
    
    // Validation helpers for tag-based object references
    public bool ValidateHomeObject()
    {
        if (string.IsNullOrEmpty(homeObjectName)) return true; // Manual position is valid
        
        GameObject homeObj = FindObjectByTagAndName(homeObjectTag, homeObjectName);
        return homeObj != null;
    }
    
    public string[] GetMissingObjectNames()
    {
        List<string> missing = new List<string>();
        
        // Check home object
        if (!string.IsNullOrEmpty(homeObjectName) && FindObjectByTagAndName(homeObjectTag, homeObjectName) == null)
        {
            missing.Add($"Home: {homeObjectName} (tag: {homeObjectTag})");
        }
        
        // Check schedule event objects
        if (scheduleEvents != null)
        {
            for (int i = 0; i < scheduleEvents.Length; i++)
            {
                var evt = scheduleEvents[i];
                if (evt != null && !string.IsNullOrEmpty(evt.targetObjectName) && 
                    FindObjectByTagAndName(evt.targetObjectTag, evt.targetObjectName) == null)
                {
                    missing.Add($"Event {i} ({evt.hour}:00): {evt.targetObjectName} (tag: {evt.targetObjectTag})");
                }
            }
        }
        
        return missing.ToArray();
    }
    
    // Get all available objects for a specific tag (useful for editor)
    public static string[] GetAvailableObjectsForTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return new string[0];
        
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
        string[] objectNames = new string[taggedObjects.Length];
        
        for (int i = 0; i < taggedObjects.Length; i++)
        {
            objectNames[i] = taggedObjects[i].name;
        }
        
        return objectNames;
    }
    
    // Common tag constants for consistency
    public static class CommonTags
    {
        public const string House = "House";
        public const string WorkStation = "WorkStation";
        public const string Shop = "Shop";
        public const string Bed = "Bed";
        public const string NPCTarget = "NPCTarget";
        public const string Market = "Market";
        public const string Farm = "Farm";
        public const string Well = "Well";
        public const string Church = "Church";
        public const string Tavern = "Tavern";
    }
    
    // Utility methods for common NPC object types
    public static GameObject FindHouse(string houseName)
    {
        return FindObjectByTagAndName(CommonTags.House, houseName);
    }
    
    public static GameObject FindWorkStation(string stationName)
    {
        return FindObjectByTagAndName(CommonTags.WorkStation, stationName);
    }
    
    public static GameObject FindShop(string shopName)
    {
        return FindObjectByTagAndName(CommonTags.Shop, shopName);
    }
    
    public static string[] GetAllHouses()
    {
        return GetAvailableObjectsForTag(CommonTags.House);
    }
    
    public static string[] GetAllWorkStations()
    {
        return GetAvailableObjectsForTag(CommonTags.WorkStation);
    }
    
    public static string[] GetAllShops()
    {
        return GetAvailableObjectsForTag(CommonTags.Shop);
    }
    
    // Validation
    private void OnValidate()
    {
        // Ensure work duration is positive
        workDuration = Mathf.Max(0.1f, workDuration);
        
        // Validate speeds
        walkSpeed = Mathf.Max(0.1f, walkSpeed);
        pauseAtDestination = Mathf.Max(0.1f, pauseAtDestination);
        idleMovementRange = Mathf.Max(0.1f, idleMovementRange);
        
        // Clamp spawn hour
        spawnHour = Mathf.Clamp(spawnHour, 0, 23);
        
        // Validate schedule events
        if (scheduleEvents != null)
        {
            foreach (var scheduleEvent in scheduleEvents)
            {
                if (scheduleEvent != null)
                {
                    scheduleEvent.hour = Mathf.Clamp(scheduleEvent.hour, 0, 23);
                }
            }
        }
    }
}

[System.Serializable]
public class ScheduleEvent
{
    [Header("When")]
    [Range(0, 23)]
    [Tooltip("Hour when this event should occur (0-23)")]
    public int hour = 0;
    
    [Header("Where & What")]
    [Tooltip("Tag to search within for target objects (e.g., 'WorkStation', 'Shop', 'NPCTarget')")]
    public string targetObjectTag = "NPCTarget";
    
    [Tooltip("Target object name (e.g., 'WorkBench', 'Market', etc.). Leave empty to use manual position.")]
    public string targetObjectName;
    
    [Tooltip("Manual target position (used when Target Object Name is empty)")]
    public Vector2 targetPosition;
    
    [Tooltip("What the NPC should do")]
    public NPCBehavior behavior = NPCBehavior.Idle;
    
    [Header("Behavior")]
    [Tooltip("Should NPC idle when reaching the target position?")]
    public bool shouldIdleWhenReached = true;
    
    [Tooltip("Should NPC despawn when reaching this destination? (e.g., going home for the night)")]
    public bool shouldDespawn = false;
    
    [Header("Optional")]
    [Tooltip("Custom dialogue for this time period")]
    [TextArea(2, 3)]
    public string[] customDialogue;
    
    // Smart position resolution method with tag-based search
    public Vector2 GetTargetPosition()
    {
        Debug.Log($"[SCHEDULE DEBUG] GetTargetPosition() called - Tag: '{targetObjectTag}', Name: '{targetObjectName}', Manual Position: {targetPosition}");
        
        // Priority: Tag-based object lookup > Manual position
        if (!string.IsNullOrEmpty(targetObjectName) && !string.IsNullOrEmpty(targetObjectTag))
        {
            Debug.Log($"[SCHEDULE DEBUG] Attempting tag-based lookup for '{targetObjectName}' in tag '{targetObjectTag}'");
            GameObject targetObj = NPCScheduleData.FindObjectByTagAndName(targetObjectTag, targetObjectName);
            if (targetObj != null)
            {
                Vector2 objPosition = targetObj.transform.position;
                Debug.Log($"[SCHEDULE DEBUG] ✅ Object found! Using object position: {objPosition}");
                return objPosition;
            }
            else
            {
                Debug.LogWarning($"[SCHEDULE DEBUG] ❌ Target object '{targetObjectName}' not found in tag '{targetObjectTag}'. Using manual position: {targetPosition}");
                return targetPosition;
            }
        }
        else
        {
            Debug.Log($"[SCHEDULE DEBUG] ⚠️ Empty tag or name fields. Using manual position: {targetPosition}");
            Debug.Log($"[SCHEDULE DEBUG] - targetObjectTag empty: {string.IsNullOrEmpty(targetObjectTag)}");
            Debug.Log($"[SCHEDULE DEBUG] - targetObjectName empty: {string.IsNullOrEmpty(targetObjectName)}");
            return targetPosition;
        }
    }
    
    // Validation helper for tag-based search
    public bool ValidateTargetObject()
    {
        if (string.IsNullOrEmpty(targetObjectName)) return true; // Manual position is valid
        
        GameObject targetObj = NPCScheduleData.FindObjectByTagAndName(targetObjectTag, targetObjectName);
        return targetObj != null;
    }
}

[System.Serializable]
public class PatrolPoint
{
    public Vector2 position;
    [Tooltip("How long to pause at this point before moving to next")]
    public float pauseDuration = 2f;
    [Tooltip("What the NPC does while at this point")]
    public NPCBehavior activityAtPoint = NPCBehavior.Idle;
}

[System.Serializable]
public class ScheduleEntry
{
    [Header("Time Settings")]
    public TimeOfDay timeOfDay;
    [Range(0f, 24f)]
    public float startHour = 0f;
    [Range(0f, 24f)]
    public float endHour = 24f;
    
    [Header("Behavior")]
    public Vector2 position;
    public NPCBehavior behavior = NPCBehavior.Idle;
    public string animation = "Idle";
    
    [Header("Interaction")]
    public bool canInteract = true;
    public string[] specificDialogues;
}

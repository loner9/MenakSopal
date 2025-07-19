using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Simple NPC Schedule", menuName = "NPC/Simple Schedule Data")]
public class SimpleNPCScheduleData : ScriptableObject
{
    [Header("NPC Basic Info")]
    public string npcName = "Unnamed NPC";
    [Range(0, 23)]
    public int startHour = 6; // When this NPC spawns (used by NPCManager)
    public Vector2 spawnPosition; // Where they spawn (usually home)
    
    [Header("Hourly Schedule")]
    [Tooltip("Schedule entries for each hour of the day. Empty hours = idle at last location")]
    public HourlyScheduleEntry[] hourlySchedule;
    
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float idleWalkRadius = 2f; // How far they can randomly walk when idle
    
    [Header("Interaction")]
    public bool canInteract = true;
    public string[] dialogues;
    
    // Helper method to get schedule for specific hour
    public HourlyScheduleEntry GetScheduleForHour(int hour)
    {
        if (hourlySchedule == null || hourlySchedule.Length == 0)
            return null;
            
        foreach (var entry in hourlySchedule)
        {
            if (entry.hour == hour)
                return entry;
        }
        
        return null; // No schedule for this hour = idle
    }
    
    // Check if NPC should be active at given hour
    public bool ShouldBeActiveAtHour(int hour)
    {
        // If no schedule for this hour, stay active but idle
        // Only despawn when explicitly told to via shouldDespawn flag
        var schedule = GetScheduleForHour(hour);
        return schedule == null || !schedule.shouldDespawn;
    }
    
    // Get current active schedule (for NPCs that are spawned)
    public HourlyScheduleEntry GetCurrentSchedule(int currentHour)
    {
        return GetScheduleForHour(currentHour);
    }
}

[System.Serializable]
public class HourlyScheduleEntry
{
    [Header("Time")]
    [Range(0, 23)]
    public int hour = 0; // Which hour this schedule applies to
    
    [Header("Destination")]
    public DestinationType destinationType = DestinationType.SinglePoint;
    public Vector2 singleDestination; // For solo destinations
    public WaypointRoute waypointRoute; // For multiple point destinations
    
    [Header("Behavior")]
    public bool shouldDespawn = false; // If true, NPC despawns when reaching destination
    
    [Header("Arrival Behavior")]
    [Tooltip("What the NPC does after reaching destination")]
    public ArrivalBehavior arrivalBehavior = ArrivalBehavior.IdleAtLocation;
    
    [Tooltip("If true and destination type is SinglePoint, NPC stays completely still at destination")]
    public bool shouldCompletelyIdle = false;
    
    [Header("Debug")]
    [Tooltip("Description of what this schedule does")]
    public string description = "Go somewhere and do something";
}

[System.Serializable]
public class WaypointRoute
{
    [Header("Route Settings")]
    public Vector2[] waypoints;
    public bool isLooped = false; // If true, goes back to first waypoint after last
    public float breakTimeAtEachWaypoint = 0.5f; // In game hours (30 min default)
    
    [Header("Current State - Runtime Only")]
    [System.NonSerialized]
    public int currentWaypointIndex = 0;
    [System.NonSerialized]
    public float timeAtCurrentWaypoint = 0f;
}

public enum DestinationType
{
    SinglePoint,    // Go to one destination and stay there
    MultiplePoints  // Follow waypoints
}

public enum ArrivalBehavior
{
    IdleAtLocation,     // Just idle at the destination
    IdleAndWalkNearby   // Idle but can walk around nearby
}

// Extension methods for easier use
public static class ScheduleExtensions
{
    public static Vector2 GetCurrentDestination(this HourlyScheduleEntry entry)
    {
        if (entry.destinationType == DestinationType.SinglePoint)
        {
            return entry.singleDestination;
        }
        else if (entry.waypointRoute != null && entry.waypointRoute.waypoints.Length > 0)
        {
            int index = Mathf.Clamp(entry.waypointRoute.currentWaypointIndex, 0, entry.waypointRoute.waypoints.Length - 1);
            return entry.waypointRoute.waypoints[index];
        }
        
        return Vector2.zero;
    }
    
    public static bool HasMoreWaypoints(this HourlyScheduleEntry entry)
    {
        if (entry.destinationType != DestinationType.MultiplePoints || entry.waypointRoute == null)
            return false;
            
        return entry.waypointRoute.currentWaypointIndex < entry.waypointRoute.waypoints.Length - 1;
    }
    
    public static void MoveToNextWaypoint(this HourlyScheduleEntry entry)
    {
        if (entry.waypointRoute == null) return;
        
        entry.waypointRoute.currentWaypointIndex++;
        entry.waypointRoute.timeAtCurrentWaypoint = 0f;
        
        // Handle looping
        if (entry.waypointRoute.currentWaypointIndex >= entry.waypointRoute.waypoints.Length)
        {
            if (entry.waypointRoute.isLooped)
            {
                entry.waypointRoute.currentWaypointIndex = 0;
            }
            else
            {
                entry.waypointRoute.currentWaypointIndex = entry.waypointRoute.waypoints.Length - 1;
            }
        }
    }
}
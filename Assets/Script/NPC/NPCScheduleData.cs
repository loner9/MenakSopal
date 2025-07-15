using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Schedule", menuName = "NPC/Schedule Data")]
public class NPCScheduleData : ScriptableObject
{
    [Header("Basic Info")]
    public string scheduleName = "Default Schedule";
    [TextArea(2, 4)]
    public string scheduleDescription = "Description of this schedule...";
    
    [Header("Day Schedule")]
    public Vector2 dayPosition;
    public NPCBehavior dayBehavior = NPCBehavior.Work;
    public string dayAnimation = "Work";
    
    [Header("Day Patrol Points (for Walk behavior)")]
    [Tooltip("Array of points the NPC will walk between during the day")]
    public PatrolPoint[] dayPatrolPoints;
    
    [Header("Night Schedule")]
    public bool activeAtNight = false;
    [Tooltip("If false, NPC will go home and despawn at night")]
    public Vector2 homePosition; // Where NPC goes before despawning
    
    [Header("Night Activity (if active at night)")]
    public Vector2 nightPosition;
    public NPCBehavior nightBehavior = NPCBehavior.Sleep;
    public string nightAnimation = "Sleep";
    public PatrolPoint[] nightPatrolPoints;
    
    [Header("Work Settings")]
    public Transform workStation;
    public float workDuration = 5f;
    
    [Header("Interaction Settings")]
    public string[] dialogues;
    public bool availableAtNight = false;
    
    [Header("Schedule Timing")]
    [Range(0f, 24f)]
    public float dayStartHour = 6f;
    [Range(0f, 24f)]
    public float nightStartHour = 20f;
    
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float pauseAtDestination = 2f; // Default pause time
    
    [Header("Advanced Schedule")]
    [Tooltip("Optional: More detailed schedule for different times of day")]
    public ScheduleEntry[] detailedSchedule;
    
    // Helper methods to get patrol points
    public PatrolPoint[] GetPatrolPointsForTime(float currentHour)
    {
        if (currentHour >= dayStartHour && currentHour < nightStartHour)
        {
            return dayPatrolPoints;
        }
        else
        {
            return nightPatrolPoints;
        }
    }
    
    public NPCBehavior GetBehaviorForTime(float currentHour)
    {
        if (currentHour >= dayStartHour && currentHour < nightStartHour)
        {
            return dayBehavior;
        }
        else
        {
            return nightBehavior;
        }
    }
    
    public Vector2 GetPositionForTime(float currentHour)
    {
        if (currentHour >= dayStartHour && currentHour < nightStartHour)
        {
            return dayPosition;
        }
        else
        {
            return nightPosition;
        }
    }
    
    // Validation
    private void OnValidate()
    {
        // Clamp time values
        dayStartHour = Mathf.Clamp(dayStartHour, 0f, 24f);
        nightStartHour = Mathf.Clamp(nightStartHour, 0f, 24f);
        
        // Ensure work duration is positive
        workDuration = Mathf.Max(0.1f, workDuration);
        
        // Validate speeds
        walkSpeed = Mathf.Max(0.1f, walkSpeed);
        pauseAtDestination = Mathf.Max(0.1f, pauseAtDestination);
        
        // Validate patrol points
        if (dayPatrolPoints != null)
        {
            foreach (var point in dayPatrolPoints)
            {
                if (point != null)
                    point.pauseDuration = Mathf.Max(0.1f, point.pauseDuration);
            }
        }
        
        if (nightPatrolPoints != null)
        {
            foreach (var point in nightPatrolPoints)
            {
                if (point != null)
                    point.pauseDuration = Mathf.Max(0.1f, point.pauseDuration);
            }
        }
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

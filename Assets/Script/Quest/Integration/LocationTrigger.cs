using UnityEngine;

/// <summary>
/// Trigger component for VisitLocation objectives.
/// Place this on GameObjects to automatically complete VisitLocation objectives when player enters.
/// </summary>
public class LocationTrigger : MonoBehaviour
{
    [Header("Location Settings")]
    [Tooltip("Unique identifier for this location (must match targetLocation in quest objectives)")]
    public string locationID = "";
    
    [Tooltip("Display name for this location")]
    public string locationName = "";
    
    [Header("Trigger Settings")]
    [Tooltip("Tag required to trigger this location (usually 'Player')")]
    public string requiredTag = "Player";
    
    [Tooltip("Can this location be triggered multiple times?")]
    public bool isRepeatable = true;
    
    [Header("Feedback")]
    [Tooltip("Show debug messages when triggered")]
    public bool showDebugMessages = true;
    
    [Tooltip("Show location name to player when visited")]
    public bool showLocationMessage = true;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        // Auto-generate locationID if not set
        if (string.IsNullOrEmpty(locationID))
        {
            locationID = gameObject.name.Replace(" ", "").Replace("(", "").Replace(")", "");
        }
        
        // Auto-generate locationName if not set
        if (string.IsNullOrEmpty(locationName))
        {
            locationName = gameObject.name;
        }
        
        // Ensure we have a trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"[LocationTrigger] '{locationName}' ({locationID}) initialized");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the right object triggered this
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;
        
        // Check if already triggered and not repeatable
        if (hasTriggered && !isRepeatable)
            return;
        
        TriggerLocation();
    }
    
    void TriggerLocation()
    {
        hasTriggered = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"[LocationTrigger] Player visited: {locationName} ({locationID})");
        }
        
        // Show location message to player
        if (showLocationMessage)
        {
            ShowLocationMessage();
        }
        
        // Notify the objective system
        if (ObjectiveAutoCompletion.Instance != null)
        {
            ObjectiveAutoCompletion.Instance.OnLocationVisited(locationID);
        }
    }
    
    void ShowLocationMessage()
    {
        // Simple message - you can integrate with your UI system
        Debug.Log($"Location Discovered: {locationName}");
        
        // If you have a UI notification system, integrate it here
        // Example: NotificationManager.ShowLocationDiscovered(locationName);
        
        // Or integrate with your existing message system
        var gameSystemsManager = FindObjectOfType<GameSystemsManager>();
        if (gameSystemsManager != null)
        {
            // Use your existing message display if available
        }
    }
    
    /// <summary>
    /// Manually trigger this location (for scripted events)
    /// </summary>
    public void ManualTrigger()
    {
        TriggerLocation();
    }
    
    /// <summary>
    /// Reset the trigger state (useful for repeatable locations)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw the trigger area in editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Draw location name
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, locationName);
        #endif
    }
}
using UnityEngine;
using System.Collections.Generic;

public class WaypointAssigner : MonoBehaviour
{
    [Header("References")]
    public WaypointManager waypointManager;
    
    [Header("Assignment Settings")]
    public int waypointsPerEnemy = 3;
    
    // Track which waypoints have been assigned
    private HashSet<GameObject> assignedWaypoints = new HashSet<GameObject>();
    
    // Call this method when spawning a new enemy
    public List<GameObject> AssignWaypointsToEnemy(GameObject enemy)
    {
        // Get all available waypoints
        List<GameObject> allWaypoints = waypointManager.GetAllWaypoints();
        
        // Filter out already assigned waypoints
        List<GameObject> availableWaypoints = new List<GameObject>();
        foreach (GameObject waypoint in allWaypoints)
        {
            if (!assignedWaypoints.Contains(waypoint))
            {
                availableWaypoints.Add(waypoint);
            }
        }
        
        // Create list to hold assigned waypoints for this enemy
        List<GameObject> enemyWaypoints = new List<GameObject>();
        
        // Check if we have enough waypoints
        if (availableWaypoints.Count < waypointsPerEnemy)
        {
            Debug.LogWarning("Not enough unique waypoints available! Only assigning " + 
                            availableWaypoints.Count + " waypoints to enemy.");
            
            // Assign all remaining waypoints
            foreach (GameObject waypoint in availableWaypoints)
            {
                enemyWaypoints.Add(waypoint);
                assignedWaypoints.Add(waypoint);
            }
        }
        else
        {
            // Assign waypoints based on proximity to enemy
            Vector2 enemyPos = enemy.transform.position;
            
            // Sort waypoints by distance from enemy
            availableWaypoints.Sort((a, b) => 
                Vector2.Distance(enemyPos, a.transform.position).CompareTo(
                Vector2.Distance(enemyPos, b.transform.position))
            );
            
            // Take the closest waypoints
            for (int i = 0; i < waypointsPerEnemy; i++)
            {
                enemyWaypoints.Add(availableWaypoints[i]);
                assignedWaypoints.Add(availableWaypoints[i]);
            }
        }
        
        // Return the assigned waypoints
        return enemyWaypoints;
    }
    
    // Unassign waypoints when an enemy is destroyed
    public void ReleaseWaypoints(List<GameObject> waypointsToRelease)
    {
        foreach (GameObject waypoint in waypointsToRelease)
        {
            assignedWaypoints.Remove(waypoint);
        }
    }
}
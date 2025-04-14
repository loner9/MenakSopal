using UnityEngine;
using System.Collections.Generic;
using Unity.Behavior;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public WaypointManager waypointManager;
    public WaypointAssigner waypointAssigner;
    
    [Header("Spawn Settings")]
    public int initialEnemyCount = 5;
    public Vector2 spawnAreaMin = new Vector2(-10, -10);
    public Vector2 spawnAreaMax = new Vector2(10, 10);
    public LayerMask obstacleLayerMask;
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    void Start()
    {
        // Ensure waypoint manager has generated waypoints
        if (waypointManager == null)
        {
            waypointManager = FindFirstObjectByType<WaypointManager>();
            if (waypointManager == null)
            {
                Debug.LogError("No WaypointManager found in scene!");
                return;
            }
        }
        
        // Ensure we have an assigner
        if (waypointAssigner == null)
        {
            waypointAssigner = GetComponent<WaypointAssigner>();
            if (waypointAssigner == null)
            {
                waypointAssigner = gameObject.AddComponent<WaypointAssigner>();
                waypointAssigner.waypointManager = waypointManager;
            }
        }
        
        // Spawn initial enemies
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }
    
    public GameObject SpawnEnemy()
    {
        // Find a valid spawn position
        Vector2 spawnPos = GetValidSpawnPosition();
        
        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
       
        // Assign unique waypoints to this enemy
        List<GameObject> assignedWaypoints = waypointAssigner.AssignWaypointsToEnemy(enemy);
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        enemy.GetComponent<EnemyAI>().waypoints = assignedWaypoints.ToArray();
        
        // Track this enemy
        spawnedEnemies.Add(enemy);
        
        return enemy;
    }
    
    Vector2 GetValidSpawnPosition()
    {
        Vector2 pos;
        int attempts = 0;
        int maxAttempts = 50;
        
        do
        {
            pos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            attempts++;
            
            // Check if position is valid (not overlapping with obstacles)
            Collider2D hitCollider = Physics2D.OverlapCircle(pos, 0.5f, obstacleLayerMask);
            if (hitCollider == null)
            {
                return pos; // Found a valid position
            }
            
        } while (attempts < maxAttempts);
        
        // If we couldn't find a valid position, use the center
        Debug.LogWarning("Could not find valid spawn position after " + maxAttempts + " attempts");
        return (spawnAreaMin + spawnAreaMax) * 0.5f;
    }
    
    // Optional: Method to spawn an enemy at a specific position
    public GameObject SpawnEnemyAt(Vector2 position)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        
        List<GameObject> assignedWaypoints = waypointAssigner.AssignWaypointsToEnemy(enemy);
        // patrol.InitializeWithWaypoints(assignedWaypoints, waypointAssigner);
        
        spawnedEnemies.Add(enemy);
        return enemy;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the spawn area
        Gizmos.color = new Color(1.5f, 0.5f, 0f, 0.3f);
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) * 0.5f,
            (spawnAreaMin.y + spawnAreaMax.y) * 0.5f,
            0
        );
        Vector3 size = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x,
            spawnAreaMax.y - spawnAreaMin.y,
            0.1f
        );
        Gizmos.DrawCube(center, size);
        
        // Draw collision check radius
        Gizmos.color = Color.black;
        }

}
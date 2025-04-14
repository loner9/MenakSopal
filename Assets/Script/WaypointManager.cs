using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public GameObject waypointPrefab;
    public int numberOfWaypoints = 10;
    public Color gizmoColor = Color.cyan;
    
    [Header("Area Settings")]
    public Vector2 areaMin = new Vector2(-10f, -10f);
    public Vector2 areaMax = new Vector2(10f, 10f);
    
    [Header("Generation Options")]
    public bool randomDistribution = true;
    public float minDistanceBetweenPoints = 1.5f;
    
    [Header("Collision Settings")]
    public LayerMask obstacleLayerMask; // Set this to include walls, boundaries, etc.
    public float collisionCheckRadius = 0.5f; // Size of collision check
    
    [Header("Open Area Settings")]
    public bool prioritizeOpenAreas = true;
    public int openAreaCheckPoints = 10; // Number of positions to evaluate
    public float openAreaCheckRadius = 5f; // How far to check for obstacles
    public int openAreaSampleSize = 8; // Number of sample points to use per position
    
    // Static reference for enemies to access
    public static WaypointManager Instance { get; private set; }
    
    private List<GameObject> waypoints = new List<GameObject>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        GenerateWaypoints();
    }
    
    public void GenerateWaypoints()
    {
        ClearExistingWaypoints();
        
        if (randomDistribution)
        {
            GenerateRandomWaypoints();
        }
        else
        {
            GenerateGridWaypoints();
        }
    }
    
    void GenerateRandomWaypoints()
    {
        int attempts = 0;
        int maxAttempts = numberOfWaypoints * 20; // Prevent infinite loops
        
        while (waypoints.Count < numberOfWaypoints && attempts < maxAttempts)
        {
            // Generate multiple candidate positions
            List<Vector2> candidates = new List<Vector2>();
            List<float> openScores = new List<float>();
            
            if (prioritizeOpenAreas)
            {
                // Generate multiple candidate positions and score them
                for (int i = 0; i < openAreaCheckPoints; i++)
                {
                    Vector2 candidatePos = new Vector2(
                        Random.Range(areaMin.x, areaMax.x),
                        Random.Range(areaMin.y, areaMax.y)
                    );
                    
                    // Only consider valid positions (no direct collision)
                    if (IsPositionValid(candidatePos))
                    {
                        candidates.Add(candidatePos);
                        openScores.Add(CalculateOpenAreaScore(candidatePos));
                    }
                }
                
                // If we have candidates, select the one with the highest open area score
                if (candidates.Count > 0)
                {
                    int bestCandidateIndex = 0;
                    float bestScore = openScores[0];
                    
                    for (int i = 1; i < openScores.Count; i++)
                    {
                        if (openScores[i] > bestScore)
                        {
                            bestScore = openScores[i];
                            bestCandidateIndex = i;
                        }
                    }
                    
                    Vector2 selectedPos = candidates[bestCandidateIndex];
                    
                    // Check minimum distance from other waypoints
                    bool tooClose = false;
                    foreach (GameObject existing in waypoints)
                    {
                        if (Vector2.Distance(existing.transform.position, selectedPos) < minDistanceBetweenPoints)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    
                    if (!tooClose)
                    {
                        InstantiateWaypoint(selectedPos);
                    }
                }
            }
            else
            {
                // Original logic without prioritizing open areas
                Vector2 newPos = new Vector2(
                    Random.Range(areaMin.x, areaMax.x),
                    Random.Range(areaMin.y, areaMax.y)
                );
                
                if (IsPositionValid(newPos))
                {
                    bool tooClose = false;
                    foreach (GameObject existing in waypoints)
                    {
                        if (Vector2.Distance(existing.transform.position, newPos) < minDistanceBetweenPoints)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    
                    if (!tooClose)
                    {
                        InstantiateWaypoint(newPos);
                    }
                }
            }
            
            attempts++;
        }
        
        Debug.Log($"Generated {waypoints.Count} waypoints after {attempts} attempts");
    }
    
    // Calculate how "open" an area is by sampling points around the position
    float CalculateOpenAreaScore(Vector2 position)
    {
        int obstacleCount = 0;
        
        // Sample in a circle around the position
        for (int i = 0; i < openAreaSampleSize; i++)
        {
            float angle = ((float)i / openAreaSampleSize) * 2 * Mathf.PI;
            Vector2 samplePoint = position + new Vector2(
                Mathf.Cos(angle) * openAreaCheckRadius,
                Mathf.Sin(angle) * openAreaCheckRadius
            );
            
            // Check for obstacles at the sample point
            Collider2D hitCollider = Physics2D.OverlapCircle(samplePoint, collisionCheckRadius, obstacleLayerMask);
            if (hitCollider != null)
            {
                obstacleCount++;
            }
        }
        
        // Calculate score - fewer obstacles means more open area
        float openScore = 1.0f - ((float)obstacleCount / openAreaSampleSize);
        return openScore;
    }
    
    void GenerateGridWaypoints()
    {
        float areaWidth = areaMax.x - areaMin.x;
        float areaHeight = areaMax.y - areaMin.y;
        
        int cols = Mathf.CeilToInt(Mathf.Sqrt(numberOfWaypoints * areaWidth / areaHeight));
        int rows = Mathf.CeilToInt((float)numberOfWaypoints / cols);
        
        float xSpacing = areaWidth / cols;
        float ySpacing = areaHeight / rows;
        
        // First, generate candidate positions for each grid cell
        List<Vector2> candidates = new List<Vector2>();
        List<float> openScores = new List<float>();
        
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector2 basePos = new Vector2(
                    areaMin.x + xSpacing * (x + 0.5f),
                    areaMin.y + ySpacing * (y + 0.5f)
                );
                
                if (IsPositionValid(basePos))
                {
                    candidates.Add(basePos);
                    if (prioritizeOpenAreas)
                    {
                        openScores.Add(CalculateOpenAreaScore(basePos));
                    }
                    else
                    {
                        openScores.Add(1.0f); // All same score if not prioritizing
                    }
                }
            }
        }
        
        // Sort candidates by open score (highest first)
        for (int i = 0; i < candidates.Count - 1; i++)
        {
            for (int j = i + 1; j < candidates.Count; j++)
            {
                if (openScores[j] > openScores[i])
                {
                    // Swap scores
                    float tempScore = openScores[i];
                    openScores[i] = openScores[j];
                    openScores[j] = tempScore;
                    
                    // Swap positions
                    Vector2 tempPos = candidates[i];
                    candidates[i] = candidates[j];
                    candidates[j] = tempPos;
                }
            }
        }
        
        // Use only the best candidates up to numberOfWaypoints
        int count = 0;
        for (int i = 0; i < candidates.Count && count < numberOfWaypoints; i++)
        {
            InstantiateWaypoint(candidates[i]);
            count++;
        }
    }
    
    bool IsPositionValid(Vector2 position)
    {
        // Check if position overlaps with objects in the obstacle layer
        Collider2D hitCollider = Physics2D.OverlapCircle(position, collisionCheckRadius, obstacleLayerMask);
        return hitCollider == null;
    }
    
    void InstantiateWaypoint(Vector2 position)
    {
        if (waypointPrefab != null)
        {
            GameObject waypoint = Instantiate(waypointPrefab, position, Quaternion.identity);
            waypoint.transform.parent = this.transform;
            waypoint.name = "Waypoint_" + waypoints.Count;
            waypoints.Add(waypoint);
        }
    }
    
    void ClearExistingWaypoints()
    {
        waypoints.Clear();
        
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    // Methods for enemies to access waypoints
    public GameObject GetRandomWaypoint()
    {
        if (waypoints.Count == 0) return null;
        return waypoints[Random.Range(0, waypoints.Count)];
    }
    
    public Transform GetClosestWaypoint(Vector2 position)
    {
        if (waypoints.Count == 0) return null;
        
        Transform closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (GameObject waypoint in waypoints)
        {
            float distance = Vector2.Distance(position, waypoint.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = waypoint.transform;
            }
        }
        
        return closest;
    }
    
    public List<GameObject> GetAllWaypoints()
    {
        return new List<GameObject>(waypoints);
    }
    
    // Optional: visualize the area and openness in the Scene view
    private void OnDrawGizmosSelected()
    {
        // Draw the spawn area
        Gizmos.color = gizmoColor;
        Vector3 center = new Vector3(
            (areaMin.x + areaMax.x) * 0.5f,
            (areaMin.y + areaMax.y) * 0.5f,
            0
        );
        Vector3 size = new Vector3(
            areaMax.x - areaMin.x,
            areaMax.y - areaMin.y,
            0.1f
        );
        Gizmos.DrawCube(center, size);
        
        // Draw collision check radius
        Gizmos.color = Color.black;
        foreach (GameObject wp in waypoints)
        {
            if (wp != null)
            {
                Gizmos.DrawWireSphere(wp.transform.position, collisionCheckRadius);
                
                // Visualize the open area check radius
                if (prioritizeOpenAreas)
                {
                    Gizmos.color = new Color(0f, 0.5f, 0.5f, 0.2f);
                    Gizmos.DrawWireSphere(wp.transform.position, openAreaCheckRadius);
                    
                    // Draw sample points
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                    for (int i = 0; i < openAreaSampleSize; i++)
                    {
                        float angle = ((float)i / openAreaSampleSize) * 2 * Mathf.PI;
                        Vector2 samplePoint = (Vector2)wp.transform.position + new Vector2(
                            Mathf.Cos(angle) * openAreaCheckRadius,
                            Mathf.Sin(angle) * openAreaCheckRadius
                        );
                        Gizmos.DrawSphere(samplePoint, 0.2f);
                    }
                }
            }
        }
    }
}
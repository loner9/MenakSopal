using System.Collections.Generic;
using Aoiti.Pathfinding;
using UnityEngine;

/// <summary>
/// Unified A* pathfinding component that replaces duplicated pathfinding code in Enemy.cs and NPC.cs.
/// Uses Aoiti.Pathfinding library for grid-based A* pathfinding.
/// </summary>
public class PathfindingAgent : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    [SerializeField] private float gridSize = 0.5f;
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private bool searchShortcut = false;
    [SerializeField] private bool snapToGrid = false;
    [SerializeField] private int maxIterations = 1000;

    [Header("Debug")]
    [SerializeField] private bool drawPathGizmos = true;
    [SerializeField] private Color pathColor = Color.green;

    // Pathfinding
    private Pathfinder<Vector2> pathfinder;
    private List<Vector2> currentPath = new List<Vector2>();

    /// <summary>
    /// The remaining path waypoints to follow.
    /// </summary>
    public List<Vector2> PathLeftToGo { get; private set; } = new List<Vector2>();

    /// <summary>
    /// Whether a valid path currently exists.
    /// </summary>
    public bool HasPath => PathLeftToGo.Count > 0;

    /// <summary>
    /// The current destination, if any.
    /// </summary>
    public Vector2 CurrentDestination { get; private set; }

    /// <summary>
    /// Whether the agent currently has a destination set.
    /// </summary>
    public bool HasDestination { get; private set; }

    // Public accessors for settings
    public float GridSize => gridSize;
    public LayerMask ObstacleLayer => obstacleLayer;

    private void Awake()
    {
        InitializePathfinder();
    }

    private void Start()
    {
        if (pathfinder == null)
        {
            InitializePathfinder();
        }
    }

    /// <summary>
    /// Initialize or reinitialize the pathfinder. Call this if settings change at runtime.
    /// </summary>
    public void InitializePathfinder()
    {
        pathfinder = new Pathfinder<Vector2>(GetDistance, GetNeighbourNodes, maxIterations);
    }

    /// <summary>
    /// Generate a path to the target position.
    /// </summary>
    /// <param name="target">The target position to path to.</param>
    /// <returns>True if a valid path was found, false otherwise.</returns>
    public bool GeneratePath(Vector2 target)
    {
        Vector2 startPos = transform.position;
        Vector2 closestStartNode = GetClosestNode(startPos);
        Vector2 closestTargetNode = GetClosestNode(target);

        if (pathfinder.GenerateAstarPath(closestStartNode, closestTargetNode, out currentPath))
        {
            if (searchShortcut && currentPath.Count > 0)
            {
                PathLeftToGo = ShortenPath(currentPath);
            }
            else
            {
                PathLeftToGo = new List<Vector2>(currentPath);
                if (!snapToGrid)
                {
                    PathLeftToGo.Add(target);
                }
            }

            CurrentDestination = target;
            HasDestination = true;
            return true;
        }

        HasDestination = false;
        return false;
    }

    /// <summary>
    /// Generate a path with an offset from the target (useful for stopping distance).
    /// </summary>
    /// <param name="target">The target position.</param>
    /// <param name="offsetDirection">Direction to offset from target.</param>
    /// <param name="offsetDistance">Distance to offset.</param>
    /// <returns>True if a valid path was found, false otherwise.</returns>
    public bool GeneratePathWithOffset(Vector2 target, Vector2 offsetDirection, float offsetDistance = 1f)
    {
        Vector2 adjustedTarget = target - offsetDirection.normalized * offsetDistance;
        return GeneratePath(adjustedTarget);
    }

    /// <summary>
    /// Get the next movement direction based on current path.
    /// </summary>
    /// <param name="moveSpeed">The movement speed to apply.</param>
    /// <param name="arrivalThreshold">Distance threshold to consider a waypoint reached.</param>
    /// <returns>The velocity vector for movement.</returns>
    public Vector2 GetMovementDirection(float moveSpeed, float arrivalThreshold = 0.5f)
    {
        if (PathLeftToGo.Count > 0)
        {
            Vector2 direction = (PathLeftToGo[0] - (Vector2)transform.position).normalized;

            // Remove waypoint if we're close enough
            if (Vector2.Distance(transform.position, PathLeftToGo[0]) < arrivalThreshold)
            {
                PathLeftToGo.RemoveAt(0);
            }

            return direction * moveSpeed;
        }

        HasDestination = false;
        return Vector2.zero;
    }

    /// <summary>
    /// Clear the current path.
    /// </summary>
    public void ClearPath()
    {
        PathLeftToGo.Clear();
        currentPath.Clear();
        HasDestination = false;
    }

    /// <summary>
    /// Check if a direct line to target is clear (no obstacles).
    /// </summary>
    /// <param name="target">Target position to check.</param>
    /// <returns>True if path is clear, false if blocked.</returns>
    public bool IsDirectPathClear(Vector2 target)
    {
        return !Physics2D.Linecast(transform.position, target, obstacleLayer);
    }

    #region Pathfinding Core Methods

    private float GetDistance(Vector2 a, Vector2 b)
    {
        // Uses square magnitude for performance
        return (a - b).sqrMagnitude;
    }

    private Dictionary<Vector2, float> GetNeighbourNodes(Vector2 pos)
    {
        Dictionary<Vector2, float> neighbours = new Dictionary<Vector2, float>();

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) continue;

                Vector2 dir = new Vector2(i, j) * gridSize;
                if (!Physics2D.Linecast(pos, pos + dir, obstacleLayer))
                {
                    neighbours.Add(GetClosestNode(pos + dir), dir.magnitude);
                }
            }
        }

        return neighbours;
    }

    private Vector2 GetClosestNode(Vector2 target)
    {
        return new Vector2(
            Mathf.Round(target.x / gridSize) * gridSize,
            Mathf.Round(target.y / gridSize) * gridSize
        );
    }

    private List<Vector2> ShortenPath(List<Vector2> path)
    {
        if (path == null || path.Count == 0)
            return new List<Vector2>();

        List<Vector2> newPath = new List<Vector2>();

        for (int i = 0; i < path.Count; i++)
        {
            newPath.Add(path[i]);
            for (int j = path.Count - 1; j > i; j--)
            {
                if (!Physics2D.Linecast(path[i], path[j], obstacleLayer))
                {
                    i = j;
                    break;
                }
            }
            newPath.Add(path[i]);
        }

        newPath.Add(path[path.Count - 1]);
        return newPath;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!drawPathGizmos || PathLeftToGo == null) return;

        Gizmos.color = pathColor;
        for (int i = 0; i < PathLeftToGo.Count - 1; i++)
        {
            Gizmos.DrawLine(PathLeftToGo[i], PathLeftToGo[i + 1]);
        }

        // Draw current position to first waypoint
        if (PathLeftToGo.Count > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, PathLeftToGo[0]);
        }

        // Draw destination
        if (HasDestination)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(CurrentDestination, 0.3f);
        }
    }

    #endregion
}

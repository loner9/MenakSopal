using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetDetector : MonoBehaviour
{
    [Header("Field of View Settings")]
    [SerializeField] private float viewRadius = 5f;
    [SerializeField, Range(0, 360)] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color fovColor = new Color(1, 1, 1, 0.2f);
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionCheckInterval = 0.2f;
    
    // Reference to the NavMeshAgent
    private NavMeshAgent agent;
    
    // Store facing direction
    private Vector2 facingDirection;
    
    // List to store currently visible targets
    public List<Transform> visibleTargets = new List<Transform>();
    
    private void Awake()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        
        // If this is a 2D project using NavMeshAgent, make sure it works in 2D space
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
    }
    
    private void Start()
    {
        // Start detection routine 
        StartCoroutine(FindTargetsWithDelay(detectionCheckInterval));
    }
    
    private void Update()
    {
        // Update facing direction based on NavMeshAgent's velocity
        UpdateFacingDirection();
    }
    
    void UpdateFacingDirection()
    {
        if (agent != null && agent.velocity.sqrMagnitude > 0.01f)
        {
            // Use agent's velocity to determine facing direction
            facingDirection = new Vector2(agent.velocity.x, agent.velocity.y).normalized;
        }
    }
    
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }
    
    void FindVisibleTargets()
    {
        // Clear the list each time
        visibleTargets.Clear();
        
        // Get all targets in radius
        Collider2D[] targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);
        
        // If we have no facing direction yet, use default
        if (facingDirection == Vector2.zero)
        {
            facingDirection = transform.right;
        }
        
        foreach (Collider2D targetCollider in targetsInRadius)
        {
            Transform target = targetCollider.transform;
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            
            // Check if target is within view angle relative to facing direction
            float angle = Vector2.Angle(facingDirection, directionToTarget);
            if (angle < viewAngle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                
                // Check for obstacles between detector and target
                if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    // Target is visible, add to list
                    visibleTargets.Add(target);
                    
                    // You can add events or direct actions here when target is detected
                    OnTargetDetected(target);
                }
            }
        }
    }
    
    // Called when a target is detected
    void OnTargetDetected(Transform target)
    {
        // Override this in subclasses or add your detection response logic here
        Debug.Log($"Target detected: {target.name}");
    }
    
    // Visualize the field of view
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;
        
        Gizmos.color = fovColor;
        
        // Draw view radius
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        
        // Use current facing direction or default to right if not set yet
        Vector2 currentFacingDir = facingDirection;
        if (currentFacingDir == Vector2.zero)
        {
            currentFacingDir = transform.right;
        }
        
        // Draw view angle lines
        Vector3 viewAngleA = Quaternion.Euler(0, 0, -viewAngle / 2) * (Vector3)currentFacingDir;
        Vector3 viewAngleB = Quaternion.Euler(0, 0, viewAngle / 2) * (Vector3)currentFacingDir;
        
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
        
        // Draw current facing direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, currentFacingDir * 2);
        
        // Draw lines to detected targets
        Gizmos.color = Color.red;
        foreach (Transform visibleTarget in visibleTargets)
        {
            Gizmos.DrawLine(transform.position, visibleTarget.position);
        }
    }
}
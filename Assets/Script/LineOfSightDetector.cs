using System.Collections;
using System.Collections.Generic;
using Accord.Statistics.Kernels;
using UnityEngine;
using UnityEngine.AI;

public class LineOfSightDetector : MonoBehaviour
{
    [Header("Line of Sight Settings")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float maxDetectionDistance = 10f;
    [SerializeField] private float viewAngle = 90f; // Field of view angle
    [SerializeField] private bool showDebugRay = true;
    
    // Reference to NavMeshAgent
    private NavMeshAgent agent;
    private Vector2 facingDirection;
    
    private bool lastDetectionResult;
    
    private void Awake()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
    }
    
    private void Update()
    {
        // Update facing direction based on NavMeshAgent movement
        UpdateFacingDirection();
    }
    
    private void UpdateFacingDirection()
    {
        // Use agent's velocity to determine facing direction
        if (agent != null && agent.velocity.sqrMagnitude > 0.01f)
        {
            facingDirection = new Vector2(agent.velocity.x, agent.velocity.y).normalized;
        }
    }
    
    // Method to check if target is within agent's field of view and line of sight
    public bool CanSeeTarget(GameObject target)
    {
        if (target == null){
            return false;
        }
        // else {
        //     Debug.Log("Target is not null"+ target.name);
        // } 
        return CheckLineOfSightToTarget(target.transform);
    }
    
    // Overload that accepts Transform directly
    public bool CanSeeTarget(Transform targetTransform)
    {
        if (targetTransform == null) return false;
        return CheckLineOfSightToTarget(targetTransform);
    }
    
    // Core line of sight checking function
    private bool CheckLineOfSightToTarget(Transform target)
    {
        if (target == null) return false;
        
        // If we have no valid facing direction, can't detect
        if (facingDirection == Vector2.zero)
        {
            lastDetectionResult = false;
            return false;
        }
        
        // Cast a ray toward the target
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            facingDirection, 
            maxDetectionDistance,
            obstacleMask | targetMask
        );
        
        if (hit.collider != null)
        {
            // Check if the hit object is on the excluded layer
            if ((obstacleMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.Log("!Hit "+ hit.collider.gameObject.name);
                return false;
            }

            // Check if the hit object is on the target layer
            if ((targetMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.Log("!!Hit "+ hit.collider.gameObject.name);
                return true;
            }
        }

        return false;
    }
    
    // For testing when no NavMeshAgent is active or velocity is zero
    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.1f)
        {
            facingDirection = direction.normalized;
        }
    }
    
    // For visual debugging
    private void OnDrawGizmos()
    {
        // Draw field of view cone
        if (showDebugRay)
        {
            // Draw view angle
            if (facingDirection != Vector2.zero)
            {
                Gizmos.color = Color.yellow;
                Vector3 viewAngleLeft = Quaternion.Euler(0, 0, viewAngle / 2) * (Vector3)facingDirection;
                Vector3 viewAngleRight = Quaternion.Euler(0, 0, -viewAngle / 2) * (Vector3)facingDirection;
                
                Gizmos.DrawRay(transform.position, viewAngleLeft * maxDetectionDistance);
                Gizmos.DrawRay(transform.position, viewAngleRight * maxDetectionDistance);
                
                // Draw current facing direction
                Gizmos.color = Color.black;
                Gizmos.DrawRay(transform.position, facingDirection * 2);
            }
            
            // Draw ray to target if we have one
        }
    }
}
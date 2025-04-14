using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FieldOfViewDetector : MonoBehaviour
{
    [Header("Field of View Settings")]
    [SerializeField] private float viewRadius = 5f;
    [SerializeField, Range(0, 360)] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float detectionCheckInterval = 0.2f;
    
    [Header("Visualization")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color fovColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private Color targetDetectedColor = new Color(1, 0, 0, 0.4f);
    
    // Direction the detector is facing
    private Vector2 facingDirection;
    
    // List to store currently visible targets
    private List<Transform> targetsInView = new List<Transform>();
    
    // Public property to check if any target is detected
    public bool TargetDetected;
    private NavMeshAgent agent;
    
    // Public property to get the nearest detected target
   
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // If this is a 2D project using NavMeshAgent, make sure it works in 2D space
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
    }

    // Public property to get all detected targets
    public List<Transform> TargetsInView => targetsInView;
    
    private void Update()
    {
        // Update facing direction based on the object's forward direction
        UpdateFacingDirection();
    }
    
    // public void UpdateFacingDirection()
    // {
    //     // Default: use the object's right vector as forward
    //     facingDirection = transform.right;
        
    //     // If the object has a Rigidbody2D and is moving, use its velocity
    //     Rigidbody2D rb = GetComponent<Rigidbody2D>();
    //     if (rb != null && rb.velocity.sqrMagnitude > 0.1f)
    //     {
    //         facingDirection = rb.velocity.normalized;
    //     }
        
    //     // If it has a NavMeshAgent component, this would be handled in a separate script
    //     // that would call SetFacingDirection
    // }

    void UpdateFacingDirection()
    {
        if (agent != null && agent.velocity.sqrMagnitude > 0.01f)
        {
            // Use agent's velocity to determine facing direction
            facingDirection = new Vector2(agent.velocity.x, agent.velocity.y).normalized;
        }
    }
    
    // Allow external scripts to set the facing direction
    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.1f)
        {
            facingDirection = direction.normalized;
        }
    }
    
    public bool FindTargetInView()
    {    
        // Get all targets in radius
        Collider2D[] targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);
        
        // If we have no facing direction yet, use default
        if (targetsInRadius.Length > 0)
        {
            targetsInView.Clear();
            Transform target = targetsInRadius[0].transform;
            targetsInView.Add(target);
            return true;
        }

        return false;
    }
    
    // Visualize the field of view
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;
        
        // Set color based on whether we have a target
        Gizmos.color = TargetDetected ? targetDetectedColor : fovColor;
        
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
        Gizmos.DrawRay(transform.position, currentFacingDir * 1.3f);
        
        // Draw lines to detected targets
        Gizmos.color = Color.red;
        foreach (Transform target in targetsInView)
        {
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
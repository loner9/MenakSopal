using UnityEngine;
using System.Collections;

public class RangedEnemy : Enemy
{
    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage = 20f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1f;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask playerLayer = 1;
    [SerializeField] private LayerMask obstacleLayer = 256;
    
    private float lastAttackTime = 0f;
    private GameObject player;
    private EnemyRangedAttackState rangedAttackState;
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize ranged attack state
        rangedAttackState = new EnemyRangedAttackState(this, StateMachine);
        
        // Find player reference
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Validate required components
        if (projectilePrefab == null)
        {
            Debug.LogError($"RangedEnemy {gameObject.name} requires a projectile prefab!");
        }
        
        if (firePoint == null)
        {
            // Create a default fire point if none is assigned
            GameObject firePointGO = new GameObject("FirePoint");
            firePointGO.transform.SetParent(transform);
            firePointGO.transform.localPosition = Vector3.zero;
            firePoint = firePointGO.transform;
            Debug.LogWarning($"RangedEnemy {gameObject.name} created default fire point. Consider assigning one manually.");
        }
    }
    
    public bool CanAttackPlayer()
    {
        if (player == null) return false;
        
        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown) return false;
        
        // Check if player is within attack range
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > attackRange) return false;
        
        // Check line of sight to player
        return HasLineOfSightToPlayer();
    }
    
    public bool CanDetectPlayer()
    {
        if (player == null) return false;
        
        // Check if player is within detection range
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > detectionRange) return false;
        
        // Check line of sight for detection
        return HasLineOfSightToPlayer();
    }
    
    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;
        
        Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        
        // Raycast to check for obstacles
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        
        // If raycast hits an obstacle, no line of sight
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer, Color.red, 0.1f);
            return false;
        }
        
        Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer, Color.green, 0.1f);
        return true;
    }
    
    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) 
        {
            Debug.LogError($"RangedEnemy {gameObject.name} cannot fire projectile - missing components!");
            return;
        }
        
        // Calculate direction to player
        Vector2 direction = (player.transform.position - firePoint.position).normalized;
        
        // Instantiate projectile
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Initialize projectile
        EnemyProjectile projectile = projectileGO.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, projectileSpeed, projectileDamage);
        }
        else
        {
            Debug.LogError($"Projectile prefab {projectilePrefab.name} does not have EnemyProjectile component!");
        }
        
        // Update last attack time
        lastAttackTime = Time.time;
        
        Debug.Log($"RangedEnemy {gameObject.name} fired projectile at {direction}");
    }
    
    public void TriggerRangedAttack()
    {
        if (CanAttackPlayer())
        {
            // Switch to ranged attack state
            StateMachine.ChangeState(rangedAttackState);
        }
    }
    
    // Override Update to handle ranged enemy logic
    protected override void Update()
    {
        base.Update();
        
        // Only process ranged logic if alive
        if (CurrentHealth <= 0) return;
        
        // Check for player detection if not already aggroed
        if (!isAggroed && CanDetectPlayer())
        {
            setAggroStatus(true);
            Debug.Log($"RangedEnemy {gameObject.name} detected player");
        }
        
        // Check if should switch to ranged attack
        if (isAggroed && !isInAttackRange && CanAttackPlayer())
        {
            // If not currently in attack state, switch to ranged attack
            if (StateMachine.CurrentEnemyState != rangedAttackState && StateMachine.CurrentEnemyState != AttackState)
            {
                TriggerRangedAttack();
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
    
    // Public getters for other components
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;
    public Transform FirePoint => firePoint;
    public GameObject ProjectilePrefab => projectilePrefab;
    public EnemyRangedAttackState RangedAttackState => rangedAttackState;
}
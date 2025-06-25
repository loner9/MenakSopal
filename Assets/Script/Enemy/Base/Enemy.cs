using Unity.VisualScripting;
using UnityEngine;
using Aoiti.Pathfinding;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable, ITriggerCheckable, IAnimationHandler, IKnockbackable
{
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;
    [field: SerializeField] public float CurrentHealth { get; set; }
    public Rigidbody2D rb { get; set; }

    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyAttackState AttackState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyKnockbackState KnockbackState { get; set; }
    public bool isAggroed { get; set; }
    public bool isInAttackRange { get; set; }
    public Animator animator { get; set; }

    [Header("Knockback Settings")]
    [field: SerializeField] private float knockbackResistance = 1f;

    private KnockbackHandler knockbackHandler;

    [field: SerializeField] protected float speed = 2.5f;

    [field: InspectorName("Pathfinder")]
    Pathfinder<Vector2> pathfinder;
    [field: SerializeField] LayerMask obstacles;
    public bool shouldMove { get; private set; } = false;
    [field: SerializeField] float gridSize = 0.5f;
    [field: SerializeField] bool searchShortcut = false;
    [field: SerializeField] bool snapToGrid = false;
    List<Vector2> path;
    public List<Vector2> pathLeftToGo { get; set; } = new List<Vector2>();
    // [SerializeField] Transform initialPosition;
    // [SerializeField] float stoppingDistance = 0.5f;

    // [SerializeField] Animator animator { get; set; }
    public float RandomMovementRange = 5f;
    public float RandomMovementSpeed = 2f;

    void Awake()
    {
        StateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        KnockbackState = new EnemyKnockbackState(this, StateMachine);
    }

    // public void Damage(float damageAmount)
    // {
    //     CurrentHealth -= damageAmount;
    //     if (CurrentHealth <= 0f){
    //         Die();
    //     }
    // }


    public void MoveEnemy(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        CurrentHealth = MaxHealth;
        StateMachine.Initialize(IdleState);

        // Initialize or add KnockbackHandler component
        knockbackHandler = GetComponent<KnockbackHandler>();
        if (knockbackHandler == null)
        {
            knockbackHandler = gameObject.AddComponent<KnockbackHandler>();
        }

        pathfinder = new Pathfinder<Vector2>(GetDistance, GetNeighbourNodes, 1000);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Only update state machine if enemy is alive and has a valid state
        if (StateMachine != null && StateMachine.CurrentEnemyState != null && CurrentHealth > 0)
        {
            StateMachine.CurrentEnemyState.FrameUpdate();
        }

        // Only draw pathfinding lines if enemy is alive
        if (CurrentHealth > 0)
        {
            for (int i = 0; i < pathLeftToGo.Count - 1; i++) //visualize your path in the sceneview
            {
                Debug.DrawLine(pathLeftToGo[i], pathLeftToGo[i + 1]);
            }
        }
    }

    void FixedUpdate()
    {
        // Only update physics if enemy is alive and has a valid state
        if (StateMachine != null && StateMachine.CurrentEnemyState != null && CurrentHealth > 0)
        {
            StateMachine.CurrentEnemyState.PhysicsUpdate();
        }

        // Only process pathfinding if enemy is alive
        if (CurrentHealth > 0 && pathLeftToGo.Count > 0)
        {
            Vector3 dir = (Vector3)pathLeftToGo[0] - transform.position;
            Vector2 newPosition = rb.position + (Vector2)(dir.normalized * speed);

            // rb.MovePosition(Vector3.MoveTowards(transform.position, newPosition, Time.deltaTime * speed)); // Move the Rigidbody2D to the new position
            rb.MovePosition(newPosition); // Move the Rigidbody2D to the new position

            if (((Vector2)transform.position - pathLeftToGo[0]).sqrMagnitude < speed * speed)
            {
                rb.MovePosition(pathLeftToGo[0]);
                pathLeftToGo.RemoveAt(0);
            }
        }
    }

    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        // Only process animation events if enemy is alive and has a valid state
        if (StateMachine != null && StateMachine.CurrentEnemyState != null && CurrentHealth > 0)
        {
            StateMachine.CurrentEnemyState.AnimationTriggerEvent(triggerType);
        }
        else
        {
            Debug.Log($"Animation event {triggerType} ignored - enemy is dead or state machine is null");
        }
    }

    public void setAggroStatus(bool isAggroed)
    {
        if (this.isAggroed != isAggroed)
        {
            Debug.Log($"{gameObject.name}: Aggro Status Changed: {this.isAggroed} -> {isAggroed}");
        }
        this.isAggroed = isAggroed;
    }

    public void setAttackRangeStatus(bool isInAttackRange)
    {
        if (this.isInAttackRange != isInAttackRange)
        {
            Debug.Log($"{gameObject.name}: Attack Range Status Changed: {this.isInAttackRange} -> {isInAttackRange}");
        }
        this.isInAttackRange = isInAttackRange;
    }

    public void SetFloat(string name, float value)
    {
        animator.SetFloat(name, value);
    }

    public void SetBool(string name, bool value)
    {
        animator.SetBool(name, value);
    }

    public void SetTrigger(string name)
    {
        animator.SetTrigger(name);
    }

    public enum AnimationTriggerType
    {
        EnemyDamage,
        PlayFootstepSound,
        AttackStart,
        AttackEnd,
        KnockbackEnd
    }

    #endregion

    #region pathfinding

    public void GetMoveCommand(Vector2 target)
    {
        Vector2 closestNode = GetClosestNode(transform.position);

        // Adjust the target position to account for the stopping distance
        Vector2 directionToTarget = (target - (Vector2)transform.position).normalized;
        target -= directionToTarget;

        if (pathfinder.GenerateAstarPath(closestNode, GetClosestNode(target), out path))
        {
            if (searchShortcut && path.Count > 0)
                pathLeftToGo = ShortenPath(path);
            else
            {
                pathLeftToGo = new List<Vector2>(path);
                if (!snapToGrid) pathLeftToGo.Add(target);
            }
        }
    }

    float GetDistance(Vector2 A, Vector2 B)
    {
        return (A - B).sqrMagnitude; //Uses square magnitude to lessen the CPU time.
    }

    Vector2 GetClosestNode(Vector2 target)
    {
        return new Vector2(Mathf.Round(target.x / gridSize) * gridSize, Mathf.Round(target.y / gridSize) * gridSize);
    }

    Dictionary<Vector2, float> GetNeighbourNodes(Vector2 pos)
    {
        Dictionary<Vector2, float> neighbours = new Dictionary<Vector2, float>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) continue;

                Vector2 dir = new Vector2(i, j) * gridSize;
                if (!Physics2D.Linecast(pos, pos + dir, obstacles))
                {
                    neighbours.Add(GetClosestNode(pos + dir), dir.magnitude);
                }
            }

        }
        return neighbours;
    }

    List<Vector2> ShortenPath(List<Vector2> path)
    {
        List<Vector2> newPath = new List<Vector2>();

        for (int i = 0; i < path.Count; i++)
        {
            newPath.Add(path[i]);
            for (int j = path.Count - 1; j > i; j--)
            {
                if (!Physics2D.Linecast(path[i], path[j], obstacles))
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

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, RandomMovementRange);
    }

    #region IKnockbackable Implementation

    public bool IsKnockedBack => knockbackHandler != null && knockbackHandler.IsKnockedBack;

    public float KnockbackResistance => knockbackResistance;

    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce, float duration)
    {
        if (knockbackHandler == null) return;

        // Store current state before switching to knockback
        if (StateMachine.CurrentEnemyState != KnockbackState)
        {
            KnockbackState.SetPreviousState(StateMachine.CurrentEnemyState);
            StateMachine.ChangeState(KnockbackState);
        }

        // Apply knockback through handler
        knockbackHandler.ApplyKnockback(knockbackDirection, knockbackForce, duration);
    }

    #endregion

    #region IDamageable Implementation

    public void Damage(float damageAmount)
    {
        if (CurrentHealth <= 0) return; // Already dead

        CurrentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {CurrentHealth}/{MaxHealth}");

        // Trigger damage animation if available
        if (animator != null)
        {
            animator.SetTrigger("Damaged");
        }

        // If currently being knocked back, don't interrupt it with other state changes
        if (StateMachine.CurrentEnemyState == KnockbackState && CurrentHealth >= 0)
        {
            Debug.Log($"{gameObject.name} damaged while in knockback - maintaining knockback state");
        }

        // Check if enemy should die
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"{gameObject.name} has died!");

        // Trigger death animation if available
        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }

        // Stop all enemy behavior
        if (StateMachine != null)
        {
            StateMachine.ChangeState(null); // Stop state machine
        }

        // Disable colliders and movement
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Destroy after a delay to allow death animation
        // Destroy(gameObject, 2f);
    }

    public void DestroyEnemy()
    {
        Debug.Log($"{gameObject.name} is being destroyed");
        Destroy(gameObject,0.2f);
    }

    #endregion
}

using UnityEngine;

public class EnemyRangedAttackState : EnemyState
{
    private RangedEnemy rangedEnemy;
    private GameObject player;
    private float attackTimer = 0f;
    private bool hasAttacked = false;
    private readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private readonly int XValue = Animator.StringToHash("X");
    private readonly int YValue = Animator.StringToHash("Y");
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    
    public EnemyRangedAttackState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        rangedEnemy = enemy as RangedEnemy;
        if (rangedEnemy == null)
        {
            Debug.LogError("EnemyRangedAttackState can only be used with RangedEnemy!");
        }
        
        player = GameObject.FindGameObjectWithTag("Player");
    }
    
    public override void EnterState()
    {
        base.EnterState();
        
        Debug.Log($"RangedEnemy {enemy.gameObject.name} entered ranged attack state");
        
        // Reset attack variables
        attackTimer = 0f;
        hasAttacked = false;
        
        // Stop movement
        enemy.pathLeftToGo.Clear();
        enemy.MoveEnemy(Vector2.zero);
        
        // Set animation parameters
        enemy.animator.SetBool(IsMoving, false);
        enemy.animator.SetBool(IsAttacking, true);
        
        // Set facing direction towards player
        if (player != null)
        {
            Vector2 directionToPlayer = (player.transform.position - enemy.transform.position).normalized;
            enemy.animator.SetFloat(XValue, directionToPlayer.x);
            enemy.animator.SetFloat(YValue, directionToPlayer.y);
        }
    }
    
    public override void ExitState()
    {
        base.ExitState();
        
        Debug.Log($"RangedEnemy {enemy.gameObject.name} exited ranged attack state");
        
        // Reset animation parameters
        enemy.animator.SetBool(IsAttacking, false);
        enemy.animator.SetBool(IsMoving, false);
        enemy.animator.SetFloat(XValue, 0f);
        enemy.animator.SetFloat(YValue, 0f);
    }
    
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        // Update attack timer
        attackTimer += Time.deltaTime;
        
        // Check if we should fire projectile (halfway through attack duration)
        if (!hasAttacked && attackTimer >= rangedEnemy.AttackDuration * 0.5f)
        {
            FireProjectile();
            hasAttacked = true;
        }
        
        // Check if attack duration is complete
        if (attackTimer >= rangedEnemy.AttackDuration)
        {
            CompleteAttack();
        }
        
        // Keep facing the player during attack
        if (player != null)
        {
            Vector2 directionToPlayer = (player.transform.position - enemy.transform.position).normalized;
            enemy.animator.SetFloat(XValue, directionToPlayer.x);
            enemy.animator.SetFloat(YValue, directionToPlayer.y);
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // Stay stationary during ranged attack
        enemy.MoveEnemy(Vector2.zero);
    }
    
    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
        
        switch (triggerType)
        {
            case Enemy.AnimationTriggerType.AttackStart:
                OnAttackStart();
                break;
                
            case Enemy.AnimationTriggerType.AttackEnd:
                OnAttackEnd();
                break;
        }
    }
    
    private void FireProjectile()
    {
        if (rangedEnemy != null)
        {
            rangedEnemy.FireProjectile();
            Debug.Log($"RangedEnemy {enemy.gameObject.name} fired projectile via state machine");
        }
    }
    
    private void OnAttackStart()
    {
        Debug.Log($"RangedEnemy {enemy.gameObject.name} attack animation started");
        // Additional logic for when attack animation starts
    }
    
    private void OnAttackEnd()
    {
        Debug.Log($"RangedEnemy {enemy.gameObject.name} attack animation ended");
        CompleteAttack();
    }
    
    private void CompleteAttack()
    {
        Debug.Log($"RangedEnemy {enemy.gameObject.name} completed ranged attack");
        
        // Determine next state based on player proximity and line of sight
        if (player == null)
        {
            enemyStateMachine.ChangeState(enemy.IdleState);
            return;
        }
        
        // Check if player is still in range and we have line of sight
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, player.transform.position);
        
        if (enemy.isAggroed)
        {
            // If player is still in attack range and we can attack again, stay ready
            if (rangedEnemy.CanAttackPlayer())
            {
                // Wait for cooldown before attacking again - switch to chase or idle
                enemyStateMachine.ChangeState(enemy.ChaseState);
            }
            else if (distanceToPlayer <= rangedEnemy.AttackRange * 1.5f) // Stay alert in extended range
            {
                // Player is close but not in direct attack range - chase
                enemyStateMachine.ChangeState(enemy.ChaseState);
            }
            else
            {
                // Player is too far - lose aggro and return to idle
                enemy.setAggroStatus(false);
                enemyStateMachine.ChangeState(enemy.IdleState);
            }
        }
        else
        {
            // Not aggroed - return to idle
            enemyStateMachine.ChangeState(enemy.IdleState);
        }
    }
    
    // Method to handle early exit if player gets too close (melee range)
    public void CheckForMeleeTransition()
    {
        if (player != null && enemy.isInAttackRange)
        {
            // Player got too close - switch to melee attack if available
            if (enemy.AttackState != null)
            {
                Debug.Log($"RangedEnemy {enemy.gameObject.name} switching to melee attack - player too close");
                enemyStateMachine.ChangeState(enemy.AttackState);
            }
        }
    }
}
using UnityEngine;

public class EnemyKnockbackState : EnemyState
{
    private KnockbackHandler knockbackHandler;
    private EnemyState previousState;
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int XValue = Animator.StringToHash("X");
    private readonly int YValue = Animator.StringToHash("Y");
    private float stateEnterTime;
    
    public EnemyKnockbackState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        knockbackHandler = enemy.GetComponent<KnockbackHandler>();
        if (knockbackHandler == null)
        {
            Debug.LogError("EnemyKnockbackState requires KnockbackHandler component on " + enemy.gameObject.name);
        }
    }
    
    public void SetPreviousState(EnemyState state)
    {
        previousState = state;
    }
    
    public override void EnterState()
    {
        base.EnterState();
        
        // Record the time when entering knockback state
        stateEnterTime = Time.time;
        
        // Stop all movement and pathfinding
        enemy.pathLeftToGo.Clear();
        enemy.MoveEnemy(Vector2.zero);
        
        // Set animation to not moving during knockback
        enemy.animator.SetBool(IsMoving, false);
        
        Debug.Log("Enemy entered knockback state");
    }
    
    public override void ExitState()
    {
        base.ExitState();
        
        // Clear any remaining knockback velocity
        if (knockbackHandler != null)
        {
            knockbackHandler.StopKnockback();
        }
        
        // Reset animation parameters for smooth transition
        enemy.animator.SetBool(IsMoving, false);
        enemy.animator.SetFloat(XValue, 0f);
        enemy.animator.SetFloat(YValue, 0f);
        
        Debug.Log("Enemy exited knockback state - animation parameters cleared");
    }
    
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        // Update animation based on knockback direction
        if (knockbackHandler != null && knockbackHandler.IsKnockedBack)
        {
            Vector2 knockbackVelocity = knockbackHandler.KnockbackVelocity;
            if (knockbackVelocity.sqrMagnitude > 0.1f)
            {
                enemy.animator.SetFloat(XValue, knockbackVelocity.normalized.x);
                enemy.animator.SetFloat(YValue, knockbackVelocity.normalized.y);
            }
        }
        
        // Note: State transition is now handled by animation events, not timers
        // The knockback animation should have an animation event that calls OnKnockbackAnimationEnd()
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // During knockback, disable normal movement
        // KnockbackHandler will handle the physics
        enemy.MoveEnemy(Vector2.zero);
    }
    
    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
        
        // Handle specific animation events during knockback
        switch (triggerType)
        {
            case Enemy.AnimationTriggerType.EnemyDamage:
                // Enemy is taking damage during knockback - this is handled by damage system
                break;
                
            case Enemy.AnimationTriggerType.KnockbackEnd:
                OnKnockbackAnimationEnd();
                break;
        }
    }
    
    // Method to be called when knockback animation ends (via animation event)
    public void OnKnockbackAnimationEnd()
    {
        Debug.Log($"{enemy.gameObject.name} knockback animation ended via animation event");
        
        // Check if enemy should chase player or return to previous state
        if (enemy.isAggroed)
        {
            enemyStateMachine.ChangeState(enemy.ChaseState);
        }
        else
        {
            // Return to previous state or default to idle
            EnemyState nextState = previousState ?? enemy.IdleState;
            enemyStateMachine.ChangeState(nextState);
        }
    }
}
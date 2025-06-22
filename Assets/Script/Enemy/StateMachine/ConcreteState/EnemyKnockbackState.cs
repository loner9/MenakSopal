using UnityEngine;

public class EnemyKnockbackState : EnemyState
{
    private KnockbackHandler knockbackHandler;
    private EnemyState previousState;
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int XValue = Animator.StringToHash("X");
    private readonly int YValue = Animator.StringToHash("Y");
    
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
        
        Debug.Log("Enemy exited knockback state");
    }
    
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        // Check if knockback is finished
        if (knockbackHandler != null && !knockbackHandler.IsKnockedBack)
        {
            // Return to previous state or default to idle
            EnemyState nextState = previousState ?? enemy.IdleState;
            enemyStateMachine.ChangeState(nextState);
            return;
        }
        
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
        
        // Check for aggro during knockback (enemy can still detect player)
        if (enemy.isAggroed && !knockbackHandler.IsKnockedBack)
        {
            // If aggro is triggered and knockback is finished, go to chase
            enemyStateMachine.ChangeState(enemy.ChaseState);
        }
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
        
        // Handle specific animation events during knockback if needed
        switch (triggerType)
        {
            case Enemy.AnimationTriggerType.EnemyDamage:
                // Enemy is taking damage during knockback - this is handled by damage system
                break;
        }
    }
}
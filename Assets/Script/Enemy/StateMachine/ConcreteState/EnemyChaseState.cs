using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private Transform _playerTransform;
    private float _movementSpeed = 1.75f;
    public EnemyChaseState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void EnterState()
    {
        base.EnterState();
        enemy.GetMoveCommand(_playerTransform.position);
        Debug.Log("Chase");
    }

    public override void ExitState()
    {
        base.ExitState();

        enemy.pathLeftToGo.Clear();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        //TODO : move enemy here using aoiti pathfinding
        // enemy.MoveEnemy(Vector2.zero);

        enemy.GetMoveCommand(_playerTransform.position);

        // Debug information to help diagnose the issue
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, _playerTransform.position);
        
        if (enemy.isInAttackRange)
        {
            Debug.Log($"{enemy.name}: Switching to Attack State (Distance: {distanceToPlayer:F2})");
            enemy.StateMachine.ChangeState(enemy.AttackState);
        }
        else
        {
            // Add debug logging to understand why not attacking
            if (distanceToPlayer < 3f) // Only log when close to avoid spam
            {
                Debug.Log($"{enemy.name}: Chase State - Not in attack range (Distance: {distanceToPlayer:F2}, isInAttackRange: {enemy.isInAttackRange})");
            }
        }

        if (!enemy.isAggroed)
        {
            Debug.Log($"{enemy.name}: Lost aggro, returning to Idle");
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }
}


using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private Transform _playerTransform;
    private float _movementSpeed = 1.75f;
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int XValue = Animator.StringToHash("X");
    private readonly int YValue = Animator.StringToHash("Y");
    
    public EnemyChaseState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void EnterState()
    {
        base.EnterState();
        
        // Reset animation parameters when entering chase state
        enemy.animator.SetBool(IsMoving, true);
        
        // Calculate initial direction to player
        Vector2 directionToPlayer = (_playerTransform.position - enemy.transform.position).normalized;
        enemy.animator.SetFloat(XValue, directionToPlayer.x);
        enemy.animator.SetFloat(YValue, directionToPlayer.y);
        
        enemy.GetMoveCommand(_playerTransform.position);
        Debug.Log("Chase state entered - animation parameters reset");
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
        
        // Update animation parameters based on movement direction
        Vector2 directionToPlayer = (_playerTransform.position - enemy.transform.position).normalized;
        enemy.animator.SetFloat(XValue, directionToPlayer.x);
        enemy.animator.SetFloat(YValue, directionToPlayer.y);
        enemy.animator.SetBool(IsMoving, true);

        // Debug information to help diagnose the issue
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, _playerTransform.position);
        
        if (enemy.isInAttackRange)
        {
            Debug.Log($"{enemy.name}: Switching to Attack State (Distance: {distanceToPlayer:F2})");
            enemy.StateMachine.ChangeState(enemy.AttackState);
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


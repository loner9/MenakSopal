using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private Vector3 _targetPos;
    private Vector3 _direction;
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int XValue = Animator.StringToHash("X");
    private readonly int YValue = Animator.StringToHash("Y");
    public EnemyIdleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        _targetPos = GetRandomPointInCircle();

        enemy.animator.SetBool(IsMoving, true);

        Debug.Log("Idle");
    }

    public override void ExitState()
    {
        base.ExitState();

        enemy.pathLeftToGo.Clear();
        enemy.MoveEnemy(Vector2.zero);

        // enemy.animator.SetBool(IsMoving, false);

        Debug.Log("Exit Idle");
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (enemy.isAggroed)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        _direction = (_targetPos - enemy.transform.position).normalized;
        // enemy.MoveEnemy(_direction * enemy.RandomMovementSpeed);
        enemy.GetMoveCommand(_targetPos);

        enemy.animator.SetFloat(XValue, _direction.x);
        enemy.animator.SetFloat(YValue, _direction.y);

        // if ((enemy.transform.position - _targetPos).sqrMagnitude < 0.1f)
        if (enemy.pathLeftToGo.Count == 1 || enemy.pathLeftToGo.Count == 0)
        {
            _targetPos = GetRandomPointInCircle();
            // Debug.Log(_targetPos);
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

    private Vector3 GetRandomPointInCircle(){
        return enemy.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * enemy.RandomMovementRange;
    }
}


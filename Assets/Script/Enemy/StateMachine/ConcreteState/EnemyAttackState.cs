using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float _attackCooldown = 1.2f;
    private float _attackTimer;
    private bool _isAttacking = false;
    private Transform _playerTransform;
    private readonly int AttackHash = Animator.StringToHash("Attack");
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int XValue = Animator.StringToHash("X");
    private readonly int YValue = Animator.StringToHash("Y");
    private GameObject attackBox { get; set; }
    public EnemyAttackState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void EnterState()
    {
        base.EnterState();

        attackBox = GameObject.Find("AttackBox");

        Debug.Log("Attack");

        enemy.rb.bodyType = RigidbodyType2D.Kinematic;

        enemy.animator.SetBool(IsMoving, false);
        
        enemy.MoveEnemy(Vector2.zero);
        
        // Start attack
        _attackTimer = 0;
        _isAttacking = true;

        // Trigger attack animation
        enemy.animator.SetTrigger(AttackHash);
    }

    public override void ExitState()
    {
        base.ExitState();

        enemy.rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        _attackTimer += Time.deltaTime;

        Vector2 direction = (_playerTransform.position - enemy.transform.position).normalized;

        enemy.animator.SetFloat(XValue, direction.x);
        enemy.animator.SetFloat(YValue, direction.y);

        if (_isAttacking && _attackTimer >= _attackCooldown)
        {
            _isAttacking = false;

            if (!enemy.isInAttackRange)
            {
                enemyStateMachine.ChangeState(enemy.IdleState);
            }
            else
            {
                // Still in range - attack again
                _attackTimer = 0;
                _isAttacking = true;
                // Trigger attack animation again
                enemy.animator.SetTrigger(AttackHash);
            }
        }
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
        CapsuleCollider2D collider = attackBox.GetComponent<CapsuleCollider2D>();
        if (triggerType.Equals(Enemy.AnimationTriggerType.AttackStart))
        {
            collider.enabled = true;
        }
        else if (triggerType.Equals(Enemy.AnimationTriggerType.AttackEnd))
        {
            collider.enabled = false;
        }
    }
}
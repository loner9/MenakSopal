using UnityEngine;

public class EnemyStateMachine
{
    public EnemyState CurrentEnemyState { get; set; }

    public void Initialize(EnemyState enemyState)
    {
        CurrentEnemyState = enemyState;
        CurrentEnemyState.EnterState();
    }

    public void ChangeState(EnemyState newState)
    {
        if (CurrentEnemyState != null)
        {
            CurrentEnemyState.ExitState();
        }
        
        CurrentEnemyState = newState;
        
        if (CurrentEnemyState != null)
        {
            CurrentEnemyState.EnterState();
        }
    }
}

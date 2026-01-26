using UnityEngine;

public class EnemyStateMachine
{
    public EnemyState CurrentEnemyState { get; set; }

    // Reference to owner enemy for event firing
    private Enemy owner;

    public void SetOwner(Enemy enemy)
    {
        owner = enemy;
    }

    public void Initialize(EnemyState enemyState)
    {
        CurrentEnemyState = enemyState;
        CurrentEnemyState.EnterState();
    }

    public void ChangeState(EnemyState newState)
    {
        string oldStateName = CurrentEnemyState?.GetType().Name ?? "None";
        string newStateName = newState?.GetType().Name ?? "None";

        if (CurrentEnemyState != null)
        {
            CurrentEnemyState.ExitState();
        }

        CurrentEnemyState = newState;

        if (CurrentEnemyState != null)
        {
            CurrentEnemyState.EnterState();
        }

        // Fire state change event
        if (owner != null && oldStateName != newStateName)
        {
            EnemyEvents.RaiseEnemyStateChanged(owner, oldStateName, newStateName);
        }
    }
}

using System;
using UnityEngine;

/// <summary>
/// Static event hub for enemy system events.
/// Subscribe to these events for loose coupling between systems.
/// </summary>
public static class EnemyEvents
{
    #region Damage & Death Events

    /// <summary>
    /// Fired when any enemy takes damage.
    /// Parameters: enemy instance, damage amount, remaining health
    /// </summary>
    public static event Action<Enemy, float, float> OnEnemyDamaged;

    /// <summary>
    /// Fired when any enemy dies.
    /// Parameters: enemy instance, enemy type
    /// </summary>
    public static event Action<Enemy, string> OnEnemyDied;

    /// <summary>
    /// Fired when an enemy is destroyed (removed from scene).
    /// Parameters: enemy instance
    /// </summary>
    public static event Action<Enemy> OnEnemyDestroyed;

    #endregion

    #region State Events

    /// <summary>
    /// Fired when an enemy changes state (idle, chase, attack, etc.)
    /// Parameters: enemy instance, old state name, new state name
    /// </summary>
    public static event Action<Enemy, string, string> OnEnemyStateChanged;

    /// <summary>
    /// Fired when an enemy becomes aggroed to player.
    /// Parameters: enemy instance, is aggroed
    /// </summary>
    public static event Action<Enemy, bool> OnEnemyAggroChanged;

    /// <summary>
    /// Fired when enemy enters/exits attack range.
    /// Parameters: enemy instance, is in attack range
    /// </summary>
    public static event Action<Enemy, bool> OnEnemyAttackRangeChanged;

    #endregion

    #region Combat Events

    /// <summary>
    /// Fired when an enemy starts an attack.
    /// Parameters: enemy instance
    /// </summary>
    public static event Action<Enemy> OnEnemyAttackStarted;

    /// <summary>
    /// Fired when an enemy attack ends.
    /// Parameters: enemy instance
    /// </summary>
    public static event Action<Enemy> OnEnemyAttackEnded;

    /// <summary>
    /// Fired when an enemy is knocked back.
    /// Parameters: enemy instance, knockback direction, force
    /// </summary>
    public static event Action<Enemy, Vector2, float> OnEnemyKnockedBack;

    #endregion

    #region Spawn Events

    /// <summary>
    /// Fired when a new enemy spawns.
    /// Parameters: enemy instance
    /// </summary>
    public static event Action<Enemy> OnEnemySpawned;

    #endregion

    #region Internal Raise Methods

    // These are called by Enemy.cs to fire events

    public static void RaiseEnemyDamaged(Enemy enemy, float damage, float remainingHealth)
    {
        OnEnemyDamaged?.Invoke(enemy, damage, remainingHealth);
    }

    public static void RaiseEnemyDied(Enemy enemy, string enemyType)
    {
        OnEnemyDied?.Invoke(enemy, enemyType);
    }

    public static void RaiseEnemyDestroyed(Enemy enemy)
    {
        OnEnemyDestroyed?.Invoke(enemy);
    }

    public static void RaiseEnemyStateChanged(Enemy enemy, string oldState, string newState)
    {
        OnEnemyStateChanged?.Invoke(enemy, oldState, newState);
    }

    public static void RaiseEnemyAggroChanged(Enemy enemy, bool isAggroed)
    {
        OnEnemyAggroChanged?.Invoke(enemy, isAggroed);
    }

    public static void RaiseEnemyAttackRangeChanged(Enemy enemy, bool isInRange)
    {
        OnEnemyAttackRangeChanged?.Invoke(enemy, isInRange);
    }

    public static void RaiseEnemyAttackStarted(Enemy enemy)
    {
        OnEnemyAttackStarted?.Invoke(enemy);
    }

    public static void RaiseEnemyAttackEnded(Enemy enemy)
    {
        OnEnemyAttackEnded?.Invoke(enemy);
    }

    public static void RaiseEnemyKnockedBack(Enemy enemy, Vector2 direction, float force)
    {
        OnEnemyKnockedBack?.Invoke(enemy, direction, force);
    }

    public static void RaiseEnemySpawned(Enemy enemy)
    {
        OnEnemySpawned?.Invoke(enemy);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Clear all event subscribers. Call when reloading scenes.
    /// </summary>
    public static void ClearAllSubscribers()
    {
        OnEnemyDamaged = null;
        OnEnemyDied = null;
        OnEnemyDestroyed = null;
        OnEnemyStateChanged = null;
        OnEnemyAggroChanged = null;
        OnEnemyAttackRangeChanged = null;
        OnEnemyAttackStarted = null;
        OnEnemyAttackEnded = null;
        OnEnemyKnockedBack = null;
        OnEnemySpawned = null;
    }

    #endregion
}

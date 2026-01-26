using UnityEngine;

public class EnemyEventTester : MonoBehaviour
{
    void OnEnable()
    {
        EnemyEvents.OnEnemySpawned += OnSpawned;
        EnemyEvents.OnEnemyDamaged += OnDamaged;
        EnemyEvents.OnEnemyDied += OnDied;
        EnemyEvents.OnEnemyStateChanged += OnStateChanged;
        EnemyEvents.OnEnemyAggroChanged += OnAggro;
    }
    
    void OnDisable()
    {
        EnemyEvents.OnEnemySpawned -= OnSpawned;
        EnemyEvents.OnEnemyDamaged -= OnDamaged;
        EnemyEvents.OnEnemyDied -= OnDied;
        EnemyEvents.OnEnemyStateChanged -= OnStateChanged;
        EnemyEvents.OnEnemyAggroChanged -= OnAggro;
    }
    
    void OnSpawned(Enemy e) => Debug.Log($"[EVENT] Spawned: {e.name}");
    void OnDamaged(Enemy e, float dmg, float hp) => Debug.Log($"[EVENT] Damaged: {e.name} -{dmg} HP:{hp}");
    void OnDied(Enemy e, string type) => Debug.Log($"[EVENT] Died: {e.name} Type:{type}");
    void OnStateChanged(Enemy e, string old, string nw) => Debug.Log($"[EVENT] State: {e.name} {old}->{nw}");
    void OnAggro(Enemy e, bool aggro) => Debug.Log($"[EVENT] Aggro: {e.name} = {aggro}");
}
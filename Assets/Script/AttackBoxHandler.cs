using UnityEngine;

public class AttackBoxHandler : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float knockbackDuration = 0.4f;
    
    private GameObject player;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Get enemy components
            IDamageable enemyDamageable = collision.GetComponent<IDamageable>();
            IKnockbackable enemyKnockback = collision.GetComponent<IKnockbackable>();
            
            if (enemyDamageable != null)
            {
                // Apply damage
                enemyDamageable.Damage(attackDamage);
                Debug.Log($"Player attacked {collision.name} for {attackDamage} damage");
            }
            
            if (enemyKnockback != null && player != null)
            {
                // Calculate knockback direction (away from player)
                Vector2 knockbackDirection = (collision.transform.position - player.transform.position).normalized;
                
                // Apply knockback
                enemyKnockback.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
                Debug.Log($"Applied knockback to {collision.name}");
            }
        }
    }
}

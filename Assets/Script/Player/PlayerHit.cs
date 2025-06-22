using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float invulnerabilityTime = 1f;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.3f;
    
    private PlayerHealth playerHealth;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;

    void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found in parent of " + gameObject.name);
        }
    }

    void Update()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvulnerable) return;
        
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Player Hit by: " + collision.name);

            if (playerHealth != null)
            {
                playerHealth.takeDamage(damageAmount);

                // Apply knockback to player
                IKnockbackable playerKnockback = GetComponentInParent<IKnockbackable>();
                if (playerKnockback != null)
                {
                    Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                    playerKnockback.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
                }

                StartInvulnerability();

            }
        }
    }

    private void StartInvulnerability()
    {
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
    }

    public void SetDamageAmount(float damage)
    {
        damageAmount = damage;
    }
}

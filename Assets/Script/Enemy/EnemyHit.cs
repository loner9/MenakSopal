using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private float invulnerabilityTime = 0.2f;

    private IDamageable enemyDamageable;
    private IKnockbackable enemyKnockback;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private GameObject player;

    void Start()
    {
        // Get required components from parent or self
        enemyDamageable = GetComponentInParent<IDamageable>();
        if (enemyDamageable == null)
        {
            enemyDamageable = GetComponent<IDamageable>();
        }

        enemyKnockback = GetComponentInParent<IKnockbackable>();
        if (enemyKnockback == null)
        {
            enemyKnockback = GetComponent<IKnockbackable>();
        }

        player = GameObject.FindGameObjectWithTag("Player");

        if (enemyDamageable == null)
        {
            Debug.LogError($"EnemyHit on {gameObject.name} could not find IDamageable component!");
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

        // Check if this is a player attack box and we're not invulnerable
        if (collision.CompareTag("Player") && collision.name.Contains("DamageBox") && !isInvulnerable)
        {

            Debug.Log($"Enemy {gameObject.name} hit by player attack box: {collision.name}");

            // Get attack damage from the AttackBoxHandler
            AttackBoxHandler attackHandler = collision.GetComponent<AttackBoxHandler>();
            if (attackHandler != null && enemyDamageable != null)
            {
                // Start invulnerability period first to prevent multiple hits
                StartInvulnerability();

                // Apply knockback first (this will change enemy state)
                if (enemyKnockback != null && player != null)
                {
                    Vector2 knockbackDirection = (transform.position - player.transform.position).normalized;
                    float knockbackForce = attackHandler.KnockbackForce;
                    float knockbackDuration = attackHandler.KnockbackDuration;

                    Debug.Log($"Before knockback - Enemy: {gameObject.name}, Direction: {knockbackDirection}, Force: {knockbackForce}, Duration: {knockbackDuration}");
                    Debug.Log($"Enemy knockback resistance: {enemyKnockback.KnockbackResistance}");
                    
                    enemyKnockback.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
                    
                    Debug.Log($"After knockback applied - IsKnockedBack: {enemyKnockback.IsKnockedBack}");
                }
                else
                {
                    if (enemyKnockback == null) Debug.LogWarning($"Enemy {gameObject.name} has no IKnockbackable component!");
                    if (player == null) Debug.LogWarning("Player reference is null!");
                }

                // Apply damage after knockback state is set
                float damage = attackHandler.AttackDamage;
                enemyDamageable.Damage(damage);
                Debug.Log($"Enemy {gameObject.name} took {damage} damage");
            }
        }


    }

    private void StartInvulnerability()
    {
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
    }

    public void SetInvulnerabilityTime(float time)
    {
        invulnerabilityTime = time;
    }
}

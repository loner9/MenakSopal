using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private bool destroyOnImpact = true;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 0.3f;
    
    private Vector2 direction;
    private float lifetimeTimer;
    private Rigidbody2D rb;
    private GameObject player;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        lifetimeTimer = lifetime;
    }
    
    private void Start()
    {
        // Set projectile velocity
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    private void Update()
    {
        // Countdown lifetime
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            DestroyProjectile();
        }
    }
    
    public void Initialize(Vector2 fireDirection, float projectileSpeed = 0f, float projectileDamage = 0f)
    {
        direction = fireDirection.normalized;
        
        // Use provided values or defaults
        if (projectileSpeed > 0f) speed = projectileSpeed;
        if (projectileDamage > 0f) damage = projectileDamage;
        
        Debug.Log($"Projectile initialized: Direction={direction}, Speed={speed}, Damage={damage}");
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HitPlayer(collision);
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            HitObstacle(collision);
        }
    }
    
    private void HitPlayer(Collider2D playerCollider)
    {
        Debug.Log($"Projectile hit player for {damage} damage");
        
        // Get player health component
        PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.takeDamage(damage);
        }
        
        // Apply knockback to player
        IKnockbackable playerKnockback = playerCollider.GetComponent<IKnockbackable>();
        if (playerKnockback != null)
        {
            Vector2 knockbackDirection = direction; // Same direction as projectile
            playerKnockback.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
            Debug.Log($"Applied knockback to player: Direction={knockbackDirection}, Force={knockbackForce}");
        }
        
        if (destroyOnImpact)
        {
            DestroyProjectile();
        }
    }
    
    private void HitObstacle(Collider2D obstacleCollider)
    {
        Debug.Log($"Projectile hit obstacle: {obstacleCollider.name}");
        
        if (destroyOnImpact)
        {
            DestroyProjectile();
        }
    }
    
    private void DestroyProjectile()
    {
        // Add impact effects here if needed (particles, sounds, etc.)
        Debug.Log("Projectile destroyed");
        Destroy(gameObject);
    }
    
    // Public getters for other components
    public float Damage => damage;
    public float Speed => speed;
    public Vector2 Direction => direction;
}
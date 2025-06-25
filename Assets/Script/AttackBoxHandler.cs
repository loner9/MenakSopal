using UnityEngine;

public class AttackBoxHandler : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float knockbackDuration = 0.4f;
    
    // Public properties for other components to access
    public float AttackDamage => attackDamage;
    public float KnockbackForce => knockbackForce;
    public float KnockbackDuration => knockbackDuration;
    
    private GameObject player;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"AttackBox collision detected with: {collision.name}, Tag: {collision.tag}");
        
        // Note: Enemy hit detection is now handled by EnemyHit component on enemies
        // This component only handles the attack box mechanics
    }
}

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;
    PlayerAnimation playerAnimation;
    
    private bool isDead = false;

    private void Awake(){
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.P))
        {
            takeDamage(1f);
        }
    }
    
    public void takeDamage(float damage)
    {
        // Don't take damage if already dead
        if (isDead) return;
        
        // Don't take damage if damage amount is invalid
        if (damage <= 0) return;
        
        playerAnimation.showHitAnimation();
        stats.health -= damage;
        
        // Clamp health to minimum of 0
        stats.health = Mathf.Max(0, stats.health);
        
        // Check if player just died (health reached 0 or below)
        if (stats.health <= 0 && !isDead)
        {
            playerDead();
        }
    }

    private void playerDead()
    {
        isDead = true;
        playerAnimation.showDeadAnimation();
        
        // Optional: Disable further interactions
        // GetComponent<Collider2D>().enabled = false;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public void Revive()
    {
        isDead = false;
        stats.health = stats.maxHealth; // Assuming you have maxHealth in PlayerStats
    }
}

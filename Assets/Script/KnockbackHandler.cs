using UnityEngine;

public class KnockbackHandler : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackResistance = 1f;
    [SerializeField] private AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private bool debugKnockback = false;
    
    [Header("Knockback State")]
    [SerializeField] private bool isKnockedBack = false;
    [SerializeField] private float currentKnockbackTime = 0f;
    [SerializeField] private float knockbackDuration = 0f;
    
    private Vector2 knockbackVelocity;
    private Vector2 baseKnockbackVelocity;
    private Rigidbody2D rb;
    
    public bool IsKnockedBack => isKnockedBack;
    public float KnockbackResistance => knockbackResistance;
    public Vector2 KnockbackVelocity => knockbackVelocity;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("KnockbackHandler requires a Rigidbody2D component on " + gameObject.name);
        }
    }
    
    private void Update()
    {
        if (isKnockedBack)
        {
            UpdateKnockback();
        }
    }
    
    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce, float duration)
    {
        if (knockbackForce <= 0f || duration <= 0f) return;
        
        // Calculate final knockback velocity with resistance
        Vector2 finalKnockbackVelocity = knockbackDirection.normalized * (knockbackForce / knockbackResistance);
        
        // Store base velocity for curve calculation
        baseKnockbackVelocity = finalKnockbackVelocity;
        knockbackVelocity = finalKnockbackVelocity;
        
        // Set knockback state
        isKnockedBack = true;
        knockbackDuration = duration;
        currentKnockbackTime = 0f;
        
        if (debugKnockback)
        {
            Debug.Log($"Applying knockback to {gameObject.name}: Direction={knockbackDirection}, Force={knockbackForce}, Duration={duration}");
        }
    }
    
    private void UpdateKnockback()
    {
        currentKnockbackTime += Time.deltaTime;
        
        // Calculate knockback progress (0 to 1)
        float progress = currentKnockbackTime / knockbackDuration;
        
        if (progress >= 1f)
        {
            // Knockback finished
            EndKnockback();
            return;
        }
        
        // Apply knockback curve to create smooth falloff
        float curveValue = knockbackCurve.Evaluate(progress);
        knockbackVelocity = baseKnockbackVelocity * curveValue;
        
        // Apply knockback velocity to rigidbody
        if (rb != null)
        {
            rb.linearVelocity = knockbackVelocity;
        }
    }
    
    private void EndKnockback()
    {
        isKnockedBack = false;
        currentKnockbackTime = 0f;
        knockbackDuration = 0f;
        knockbackVelocity = Vector2.zero;
        baseKnockbackVelocity = Vector2.zero;
        
        // Stop rigidbody movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        if (debugKnockback)
        {
            Debug.Log($"Knockback ended for {gameObject.name}");
        }
    }
    
    public void StopKnockback()
    {
        if (isKnockedBack)
        {
            EndKnockback();
        }
    }
    
    public float GetKnockbackProgress()
    {
        if (!isKnockedBack) return 0f;
        return Mathf.Clamp01(currentKnockbackTime / knockbackDuration);
    }
    
    private void OnDrawGizmos()
    {
        if (isKnockedBack && debugKnockback)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, knockbackVelocity.normalized * 2f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
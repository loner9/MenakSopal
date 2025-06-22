using UnityEngine;

[System.Serializable]
public class KnockbackData
{
    [Header("Knockback Configuration")]
    public float force = 10f;
    public float duration = 0.3f;
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Direction Settings")]
    public bool useFixedDirection = false;
    public Vector2 fixedDirection = Vector2.right;
    
    [Header("Special Effects")]
    public bool multiplyByDamage = false;
    public float damageMultiplier = 0.1f;
    
    public void ApplyKnockback(IKnockbackable target, Vector2 sourcePosition, Vector2 targetPosition, float damage = 0f)
    {
        if (target == null) return;
        
        Vector2 direction;
        if (useFixedDirection)
        {
            direction = fixedDirection.normalized;
        }
        else
        {
            direction = (targetPosition - sourcePosition).normalized;
        }
        
        float finalForce = force;
        if (multiplyByDamage && damage > 0f)
        {
            finalForce += damage * damageMultiplier;
        }
        
        target.ApplyKnockback(direction, finalForce, duration);
    }
}

[CreateAssetMenu(fileName = "KnockbackData", menuName = "Game/Knockback Data")]
public class KnockbackDataSO : ScriptableObject
{
    public KnockbackData knockbackData;
    
    public void ApplyKnockback(IKnockbackable target, Vector2 sourcePosition, Vector2 targetPosition, float damage = 0f)
    {
        knockbackData.ApplyKnockback(target, sourcePosition, targetPosition, damage);
    }
}
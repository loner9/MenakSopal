using UnityEngine;

public interface IKnockbackable
{
    void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce, float duration);
    bool IsKnockedBack { get; }
    float KnockbackResistance { get; }
}
using System;
using UnityEngine;

public enum DamageType
{
    Physical,
    Magical,
    True,
    Fire,
    Ice,
    Poison
}

public struct DamageContext
{
    public float Amount;            // lượng sát thương gốc
    public int SourceId;            // ID của thực thể gây damage (nếu có)
    public DamageType Type;         // kiểu sát thương
    public bool IsCritical;         // có crit không
    public Vector3 HitPoint;        // điểm va chạm
    public Vector3 HitNormal;       // pháp tuyến tại điểm va chạm
    public Vector3 Knockback;       // lực hất lùi (nếu áp dụng)
    public GameObject Instigator;   // object gây sát thương (nếu cần)

    public DamageContext(float amount, int sourceId = -1, DamageType type = DamageType.Physical,
                         bool isCritical = false, Vector3 hitPoint = default, Vector3 hitNormal = default,
                         Vector3 knockback = default, GameObject instigator = null)
    {
        Amount = amount;
        SourceId = sourceId;
        Type = type;
        IsCritical = isCritical;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
        Knockback = knockback;
        Instigator = instigator;
    }
}

public interface IDamageable
{
    int Id { get; }                 // (map với CharacterData.characterID)
    float MaxHealth { get; }
    float Health { get; }
    bool IsDead { get; }

    /// <summary>Gây sát thương. Trả về lượng sát thương thực tế đã trừ (sau khi tính giáp/kháng).</summary>
    float ApplyDamage(float Damage, DamageType type,float duration);

    /// <summary>Hồi máu. Trả về lượng máu thực tế đã hồi.</summary>
    float Heal(float amount);

    // Sự kiện runtime (không serialize): cho gameplay/UI bám vào
    event Action<DamageContext, float> OnDamaged; // (context, actualDamage)
    event Action<float> OnHealed;                 // actualHeal
    event Action OnDied;
}

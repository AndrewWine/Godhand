using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Character
{
    public override float ApplyDamage(in DamageContext ctx)
    {
        return base.ApplyDamage(ctx);
    }

    public override float Heal(float amount)
    {
        return base.Heal(amount);
    }

    protected override void ApplyTraitToRuntime(Trait t, ref RuntimeBase rb)
    {
        base.ApplyTraitToRuntime(t, ref rb);
    }

    protected override float CalculateDamageAfterMitigation(float amount, DamageType type)
    {
        return base.CalculateDamageAfterMitigation(amount, type);
    }

    protected override void Die()
    {
        base.Die();
    }

    protected override void Start()
    {
        base.Start();
    }
}

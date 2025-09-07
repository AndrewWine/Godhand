using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    MaxHealth,
    MaxMana,
    Attack,
    Health,
    Mana,
    Magic,
    AttackSpeed,
    MoveSpeed,
    Armor,
    MagicResist,
    LifeSteal,   
    STR,
    INT,
    DEX
}

[CreateAssetMenu(fileName = "NewTrait", menuName = "Game/Trait")]
public class Trait : ScriptableObject
{
    [Header("Info")]
    public string traitName;
    [TextArea] public string description;

    [System.Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public float addFlat;     // + tuy?t ??i (vd: +0.05 = +5% LS)
        [Tooltip("0.1 = +10% c?a GIÁ TR? HI?N T?I c?a ch? s?")]
        public float addPercent;  // + theo % (vd: 0.1 = t?ng 10% c?a stat hi?n t?i)
    }

    [Header("Modifiers")]
    public List<StatModifier> modifiers = new List<StatModifier>();

    [System.Serializable]
    public class AppliedTrait
    {
        public Trait source;
        public List<AppliedDelta> deltas = new List<AppliedDelta>();
    }

    [System.Serializable]
    public struct AppliedDelta
    {
        public StatType stat;
        public float delta;
    }

    public AppliedTrait Apply(CharacterData target)
    {
        if (target == null) return null;

        var handle = new AppliedTrait { source = this, deltas = new List<AppliedDelta>() };
        var perStatDelta = new Dictionary<StatType, float>();

        foreach (var m in modifiers)
        {
            float baseVal = GetStatValue(target, m.stat);
            float delta = baseVal * m.addPercent + m.addFlat;

            if (delta != 0f)
            {
                if (!perStatDelta.ContainsKey(m.stat))
                    perStatDelta[m.stat] = 0f;
                perStatDelta[m.stat] += delta;
            }
        }

        foreach (var kv in perStatDelta)
        {
            ref float statRef = ref GetStatRef(target, kv.Key);
            statRef += kv.Value;

            // (Tu? ch?n) Clamp LifeSteal v? [0, 5] = [0%, 500%] ?? tránh âm/quá l?n:
            if (kv.Key == StatType.LifeSteal)
                statRef = Mathf.Clamp(statRef, 0f, 5f);

            handle.deltas.Add(new AppliedDelta { stat = kv.Key, delta = kv.Value });
        }

        return handle;
    }

    public void Remove(CharacterData target, AppliedTrait handle)
    {
        if (target == null || handle == null || handle.source != this) return;

        foreach (var d in handle.deltas)
        {
            ref float statRef = ref GetStatRef(target, d.stat);
            statRef -= d.delta;

            if (d.stat == StatType.LifeSteal)
                statRef = Mathf.Clamp(statRef, 0f, 5f);
        }

        handle.deltas.Clear();
    }

    private float GetStatValue(CharacterData t, StatType stat)
    {
        switch (stat)
        {
            case StatType.MaxHealth: return t.maxHealth;
            case StatType.MaxMana: return t.maxMana;
            case StatType.Attack: return t.attack;
            case StatType.Health: return t.health;
            case StatType.Mana: return t.mana;
            case StatType.Magic: return t.magic;
            case StatType.AttackSpeed: return t.attackSpeed;
            case StatType.MoveSpeed: return t.moveSpeed;
            case StatType.Armor: return t.armor;
            case StatType.MagicResist: return t.magicResist;
            case StatType.LifeSteal: return t.lifeSteal; // <-- M?I
            case StatType.STR: return t.STR;
            case StatType.INT: return t.INT;
            case StatType.DEX: return t.DEX;
            default: throw new System.ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }

    private ref float GetStatRef(CharacterData t, StatType stat)
    {
        switch (stat)
        {
            case StatType.MaxHealth: return ref t.maxHealth;
            case StatType.MaxMana: return ref t.maxMana;
            case StatType.Attack: return ref t.attack;
            case StatType.Health: return ref t.health;
            case StatType.Mana: return ref t.mana;
            case StatType.Magic: return ref t.magic;
            case StatType.AttackSpeed: return ref t.attackSpeed;
            case StatType.MoveSpeed: return ref t.moveSpeed;
            case StatType.Armor: return ref t.armor;
            case StatType.MagicResist: return ref t.magicResist;
            case StatType.LifeSteal: return ref t.lifeSteal; // <-- M?I
            case StatType.STR: return ref t.STR;
            case StatType.INT: return ref t.INT;
            case StatType.DEX: return ref t.DEX;
            default: throw new System.ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }
}

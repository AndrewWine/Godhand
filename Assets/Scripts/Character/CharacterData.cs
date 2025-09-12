using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Role
{
    Tank,       // Tiền tuyến, chịu đòn
    MeleeDPS,   // Cận chiến gây sát thương đơn mục tiêu
    Mage        // Pháp sư, gây sát thương phép
}

public enum CharacterClass
{
    Warrior,        // Tank
    Paladin,        // Tank
    Berserker,      // MeleeDPS
    Assassin,       // MeleeDPS
    Rogue,          // MeleeDPS
    Slayer,         // MeleeDPS
    Mage,           // Mage
    Healer,         // Mage
    ElementorMaster // Mage
}

// PERSONALITY TRAITS
// - Brave: +10% Armor & MagicResist
// - Insightful: +10% Mana (max & current)
// - Timid: +10% DEX, -2% Armor & MagicResist
// - Frenzied: +5% Attack, +5% AttackSpeed, -5% Mana (max & current)
// - Indomitable: +7% Armor & MagicResist, +5% Health (max & current)
// - Confident: +3% DEX, +2% AttackSpeed, +2% MoveSpeed
// - Intelligent: +5% Magic, +5% Mana (max & current)
// - Transcendent: +10% Attack, +15% AttackSpeed, +10% Armor & MagicResist, +5% MoveSpeed, -40% Health
public enum Personality
{
    Brave,
    Insightful,
    Timid,
    Frenzied,
    Indomitable,
    Confident,
    Intelligent,
    Transcendent
}

[System.Serializable]
public struct CharacterStats
{
    [Header("Vitals")]
    public float maxHealth;
    public float health;
    public float maxMana;
    public float mana;

    [Header("Combat")]
    public float attack;
    public float magic;
    public float armor;
    public float magicResist;
    public float evasion;

    [Header("Misc")]
    public float lifeSteal;
    public float attackSpeed;
    public float moveSpeed;

    [Header("Attributes")]
    public float STR;
    public float INT;
    public float DEX;
}

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identifiers")]
    public int characterID;

    [Header("Information")]
    public string characterName;
    public Role role;
    public CharacterClass classChar;
    public Personality personality;

    [Header("Stats (Base)")]
    public float maxHealth = 100f;
    public float maxMana   = 10f;
    public float attack    = 10f;
    public float health    = 100f;
    public float mana      = 10f;
    public float magic     = 1f;
    public float armor     = 1f;
    public float magicResist = 1f;
    public float evasion = 0f;
    public float lifeSteal   = 0f;
    public float attackSpeed = 1f;
    public float moveSpeed   = 5f;

    [Header("Attributes (Base)")]
    public float STR = 1f;
    public float INT = 1f;
    public float DEX = 1f;

    public ExperienceCurve overrideCurve;

    // Evasion cap để tránh né 100% (có thể chỉnh tùy game)
    private const float EVASION_CAP = 0.95f;

    /// <summary>
    /// Trả về chỉ số sau khi cộng thuộc tính (STR/DEX/INT) và áp dụng Personality.
    /// Thứ tự: Base -> Attribute (cộng phẳng) -> Personality (nhân %).
    /// </summary>
    public CharacterStats GetFinalStats()
    {
        CharacterStats s = new CharacterStats
        {
            maxHealth    = maxHealth,
            health       = health,
            maxMana      = maxMana,
            mana         = mana,
            attack       = attack,
            magic        = magic,
            armor        = armor,
            magicResist  = magicResist,
            evasion      = evasion,
            lifeSteal    = lifeSteal,
            attackSpeed  = attackSpeed,
            moveSpeed    = moveSpeed,
            STR          = STR,
            INT          = INT,
            DEX          = DEX
        };

        // 1) APPLY ATTRIBUTE BONUSES (cộng phẳng theo yêu cầu)
        // STR: +10 HP, +3 Attack mỗi 1 STR
        s.maxHealth += 10f * s.STR;
        s.health    += 10f * s.STR;  // để current không nhỏ hơn max mới
        s.attack    += 3f  * s.STR;

        // DEX: +0.5 evasion, +0.5 attackSpeed, +0.5 moveSpeed, +1 attack mỗi 1 DEX
        s.evasion     += 0.005f * s.DEX;  // 0.5% per DEX (scale 0–1)
        s.attackSpeed += 0.5f * s.DEX;
        s.moveSpeed   += 0.5f * s.DEX;
        s.attack      += 1f   * s.DEX;

        // INT: +5 mana, +3 magic mỗi 1 INT
        s.maxMana += 5f * s.INT;
        s.mana    += 5f * s.INT; // đồng bộ current
        s.magic   += 3f * s.INT;

        // 2) APPLY PERSONALITY MODIFIERS (nhân %)
        switch (personality)
        {
            case Personality.Brave:
                s.armor       *= 1.10f;
                s.magicResist *= 1.10f;
                break;

            case Personality.Insightful:
                s.maxMana *= 1.10f;
                s.mana    *= 1.10f;
                break;

            case Personality.Timid:
                s.DEX         *= 1.10f;
                s.armor       *= 0.98f;
                s.magicResist *= 0.98f;
                break;

            case Personality.Frenzied:
                s.attack      *= 1.05f;
                s.attackSpeed *= 1.05f;
                s.maxMana     *= 0.95f;
                s.mana        *= 0.95f;
                break;

            case Personality.Indomitable:
                s.armor       *= 1.07f;
                s.magicResist *= 1.07f;
                s.maxHealth   *= 1.05f;
                s.health      *= 1.05f;
                break;

            case Personality.Confident:
                s.DEX         *= 1.03f;
                s.attackSpeed *= 1.02f;
                s.moveSpeed   *= 1.02f;
                break;

            case Personality.Intelligent:
                s.magic   *= 1.05f;
                s.maxMana *= 1.05f;
                s.mana    *= 1.05f;
                break;

            case Personality.Transcendent:
                s.attack      *= 1.10f;
                s.attackSpeed *= 1.15f;
                s.armor       *= 1.10f;
                s.magicResist *= 1.10f;
                s.moveSpeed   *= 1.05f;

                // -40% Health (max & current)
                s.maxHealth *= 0.60f;
                s.health    *= 0.60f;
                break;
        }

        // 3) CLAMPS / VỆ SINH GIÁ TRỊ
        s.maxHealth   = Mathf.Max(1f, s.maxHealth);
        s.health      = Mathf.Clamp(s.health, 0f, s.maxHealth);
        s.maxMana     = Mathf.Max(0f, s.maxMana);
        s.mana        = Mathf.Clamp(s.mana, 0f, s.maxMana);
        s.attackSpeed = Mathf.Max(0.01f, s.attackSpeed);
        s.moveSpeed   = Mathf.Max(0f, s.moveSpeed);
        s.armor       = Mathf.Max(0f, s.armor);
        s.magicResist = Mathf.Max(0f, s.magicResist);
        s.evasion     = Mathf.Clamp(s.evasion, 0f, EVASION_CAP); // tránh né tối đa 95%

        return s;
    }

#if UNITY_EDITOR
    [SerializeField, HideInInspector] private CharacterStats _previewFinal;
    private void OnValidate()
    {
        _previewFinal = GetFinalStats();
    }
#endif
}

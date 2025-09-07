using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] protected CharacterData characterData;
    [SerializeField] protected Trait trait; // có thể null

    // ===== Runtime base (copy từ CharacterData + Trait) =====
    protected struct RuntimeBase
    {
        public float MaxHealth, MaxMana;
        public float Attack, Armor, MagicResist;
    }
    protected RuntimeBase _base;   // chỉ số gốc runtime (được "sửa đổi" từ CharacterData + Trait)

    // ===== Runtime state =====
    protected float currentHealth;
    protected float currentMana;

    // ===== Temporary modifiers (cộng rồi hoàn) =====
    protected struct Totals
    {
        public float Attack, Armor, MagicResist;
        public void Add(float atk, float ar, float mr) { Attack += atk; Armor += ar; MagicResist += mr; }
        public void Sub(float atk, float ar, float mr) { Attack -= atk; Armor -= ar; MagicResist -= mr; }
        public void Reset() { Attack = Armor = MagicResist = 0f; }
    }
    private Totals _mods; // tổng các buff tạm thời đang active

    private readonly List<ActiveBuff> _activeBuffs = new();
    private struct ActiveBuff
    {
        public float addAtk, addArmor, addMR;
        public Coroutine co;
    }

    // ===== IDamageable =====
    public int Id => characterData != null ? characterData.characterID : GetInstanceID();
    public float MaxHealth => _base.MaxHealth;       
    public float Health    => currentHealth;
    public bool  IsDead    => currentHealth <= 0f;

    // Chỉ số hiệu dụng = base + tổng modifiers
    public float Attack      => _base.Attack      + _mods.Attack;
    public float Armor       => _base.Armor       + _mods.Armor;
    public float MagicResist => _base.MagicResist + _mods.MagicResist;

    public event Action<DamageContext, float> OnDamaged;
    public event Action<float> OnHealed;
    public event Action OnDied;

    // ================= Lifecycle =================
    protected virtual void Start()
    {
        RebuildBaseFromData();     // tạo base từ CharacterData + Trait
        currentHealth = MaxHealth; // trạng thái khởi đầu
        currentMana   = _base.MaxMana;
    }

    // gọi hàm này nếu thay đổi CharacterData / Trait lúc runtime
    protected void RebuildBaseFromData()
    {
        if (characterData == null)
        {
            Debug.LogWarning($"{name}: missing CharacterData!");
            _base.MaxHealth = 100f; _base.MaxMana = 10f;
            _base.Attack = 10f; _base.Armor = 0f; _base.MagicResist = 0f;
            return;
        }

        // 1) copy từ CharacterData
        _base.MaxHealth  = characterData.maxHealth;
        _base.MaxMana    = characterData.maxMana;
        _base.Attack     = characterData.attack;
        _base.Armor      = characterData.armor;
        _base.MagicResist= characterData.magicResist;

        // 2) áp Trait vào BASE RUNTIME (không động vào SO gốc)
        if (trait != null)
        {

            ApplyTraitToRuntime(trait, ref _base);
        }

        // 3) clamp lại HP/Mana theo base mới
        if (currentHealth > 0f) currentHealth = Mathf.Min(currentHealth, _base.MaxHealth);
        currentMana = Mathf.Min(currentMana, _base.MaxMana);
    }

    // Ví dụ adapter Trait -> runtime base (chỉ minh hoạ vài stat chính)
    protected virtual void ApplyTraitToRuntime(Trait t, ref RuntimeBase rb)
    {
        foreach (var m in t.modifiers)
        {
            switch (m.stat)
            {
                case StatType.MaxHealth:   rb.MaxHealth   += rb.MaxHealth * m.addPercent + m.addFlat; break;
                case StatType.MaxMana:     rb.MaxMana     += rb.MaxMana   * m.addPercent + m.addFlat; break;
                case StatType.Attack:      rb.Attack      += rb.Attack    * m.addPercent + m.addFlat; break;
                case StatType.Armor:       rb.Armor       += rb.Armor     * m.addPercent + m.addFlat; break;
                case StatType.MagicResist: rb.MagicResist += rb.MagicResist*m.addPercent + m.addFlat; break;
         
            }
        }
    }

    // ================= Combat =================
    public virtual float ApplyDamage(in DamageContext ctx)
    {
        if (IsDead) return 0f;

        float actual = CalculateDamageAfterMitigation(ctx.Amount, ctx.Type);
        currentHealth = Mathf.Max(0f, currentHealth - actual);

        OnDamaged?.Invoke(ctx, actual);
        if (IsDead) { OnDied?.Invoke(); Die(); }

        return actual;
    }

    public virtual float Heal(float amount)
    {
        if (IsDead || amount <= 0f) return 0f;

        float before = currentHealth;
        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        float healed = currentHealth - before;
        if (healed > 0f) OnHealed?.Invoke(healed);
        return healed;
    }

    protected virtual float CalculateDamageAfterMitigation(float amount, DamageType type)
    {
        switch (type)
        {
            case DamageType.Physical: return amount / (1f + Mathf.Max(0f, Armor)       / 100f);
            case DamageType.Magical:  return amount / (1f + Mathf.Max(0f, MagicResist) / 100f);
            case DamageType.True:     return amount;
            default:                  return amount;
        }
    }

    protected virtual void Die() => Destroy(gameObject);

    // ================= Buff tạm thời (có hoàn) =================
    /// Hồi HP/Mana (giữ nguyên), còn ATK/Armor/MR chỉ cộng trong 'duration', hết hạn trả lại.
    public Coroutine BuffTemporary(
        float healHp = 0f,
        float healMana = 0f,
        float bonusAttack = 0f,
        float bonusArmor = 0f,
        float bonusMagicResist = 0f,
        float duration = 5f)
    {
        // 1) Heal ngay (không hoàn)
        if (healHp != 0f && !IsDead)
        {
            float before = currentHealth;
            currentHealth = Mathf.Min(MaxHealth, currentHealth + healHp);
            float healed = currentHealth - before;
            if (healed > 0f) OnHealed?.Invoke(healed);
        }
        if (healMana != 0f)
        {
            currentMana = Mathf.Min(_base.MaxMana, currentMana + healMana);
        }

        // 2) Cộng modifiers tạm thời
        if (bonusAttack != 0f || bonusArmor != 0f || bonusMagicResist != 0f)
        {
            _mods.Add(bonusAttack, bonusArmor, bonusMagicResist);

            var buff = new ActiveBuff
            {
                addAtk = bonusAttack,
                addArmor = bonusArmor,
                addMR = bonusMagicResist
            };
            buff.co = StartCoroutine(RevertBuffAfter(duration, buff));
            _activeBuffs.Add(buff);
        }

        return _activeBuffs.Count > 0 ? _activeBuffs[_activeBuffs.Count - 1].co : null;
    }

    private IEnumerator RevertBuffAfter(float duration, ActiveBuff buff)
    {
        if (duration > 0f) yield return new WaitForSeconds(duration);

        // Trả lại đúng phần đã cộng
        _mods.Sub(buff.addAtk, buff.addArmor, buff.addMR);
        _activeBuffs.Remove(buff);
    }

    /// Huỷ toàn bộ buff tạm thời và trả chỉ số hiệu dụng = base (HP/Mana giữ nguyên).
    public void CancelAllTemporaryBuffs()
    {
        foreach (var b in _activeBuffs) if (b.co != null) StopCoroutine(b.co);
        _mods.Reset();
        _activeBuffs.Clear();
    }
}

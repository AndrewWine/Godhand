using UnityEngine;
using System;

namespace GodhandSystem
{
        public class LevelSystem : MonoBehaviour, IProgression
    {
        [Header("Refs")]
        private CharacterData data;
        public ExperienceCurve defaultCurve;

        [Header("Runtime")]
        [Min(1)] public int level = 1;
        public int totalExp = 0;

        private ExperienceCurve Curve
            => data != null && data.overrideCurve != null ? data.overrideCurve : defaultCurve;

        public int Level => level;
        public int MaxLevel => Curve != null ? Curve.maxLevel : 1;
        public int TotalExp => totalExp;
        public int ExpRequiredThisLevel
            => (Curve != null && level < Curve.maxLevel) ? Curve.ExpRequiredForNextLevel(level) : 0;
        public int ExpInCurrentLevel
            => Curve != null ? totalExp - Curve.TotalExpToReachLevel(level) : 0;
        public float Progress01
            => ExpRequiredThisLevel <= 0 ? 1f : Mathf.Clamp01((float)ExpInCurrentLevel / ExpRequiredThisLevel);

        void Awake()
        {
            data = GetComponent<CharacterData>();
        }
        void Start()
        {
            PublishExpChanged();
        }

        public void AddExp(int amount)
        {
            if (Curve == null || amount <= 0) return;

            int maxLvl = Curve.maxLevel;
            totalExp += amount;

            int levelsGained = 0;
            while (level < maxLvl && totalExp >= Curve.TotalExpToReachLevel(level + 1))
            {
                level++;
                levelsGained++;
            }

            if (level >= maxLvl)
                totalExp = Mathf.Min(totalExp, Curve.TotalExpToReachLevel(maxLvl));

            if (levelsGained > 0)
                EventBus.Publish(new LevelUpEvent(data.characterID, level, levelsGained));

            PublishExpChanged();
        }

        private void PublishExpChanged()
        {
            EventBus.Publish(new ExpChangedEvent(
                data.characterID,
                totalExp,
                level,
                ExpInCurrentLevel,
                ExpRequiredThisLevel,
                Progress01
            ));
        }
    }

}
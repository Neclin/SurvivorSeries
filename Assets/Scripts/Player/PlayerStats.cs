using System;
using UnityEngine;
using SurvivorSeries.Characters.Data;

namespace SurvivorSeries.Player
{
    public enum StatType
    {
        MaxHealth,
        MoveSpeed,
        Damage,
        Area,
        ProjectileSpeed,
        CooldownReduction,
        Luck,
        Regen,
        Armor
    }

    [Serializable]
    public struct StatModifier
    {
        public StatType Stat;
        public float FlatBonus;
        public float PercentBonus; // additive percent, e.g. 0.1 = +10%
    }

    public class PlayerStats : MonoBehaviour
    {
        // Base values — set from CharacterDefinitionSO on run start
        [SerializeField] private float _baseMaxHealth = 100f;
        [SerializeField] private float _baseMoveSpeed = 5f;
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _baseArea = 1f;
        [SerializeField] private float _baseProjectileSpeed = 8f;
        [SerializeField] private float _baseCooldownReduction = 0f; // 0 = no reduction
        [SerializeField] private float _baseLuck = 1f;
        [SerializeField] private float _baseRegen = 0f;
        [SerializeField] private float _baseArmor = 0f;
        [SerializeField] private float _baseDetectionRange = 15f;

        // Accumulated modifiers (from passives, level-ups, shop)
        private float _flatMaxHealth, _percentMaxHealth;
        private float _flatMoveSpeed, _percentMoveSpeed;
        private float _flatDamage, _percentDamage;
        private float _flatArea, _percentArea;
        private float _flatProjectileSpeed, _percentProjectileSpeed;
        private float _flatCDR, _percentCDR;
        private float _flatLuck, _percentLuck;
        private float _flatRegen, _percentRegen;
        private float _flatArmor, _percentArmor;

        public event Action OnStatsChanged;

        // --- Computed properties ---

        public float MaxHealth => (_baseMaxHealth + _flatMaxHealth) * (1f + _percentMaxHealth);
        public float MoveSpeed => (_baseMoveSpeed + _flatMoveSpeed) * (1f + _percentMoveSpeed);
        public float Damage => (_baseDamage + _flatDamage) * (1f + _percentDamage);
        public float Area => (_baseArea + _flatArea) * (1f + _percentArea);
        public float ProjectileSpeed => (_baseProjectileSpeed + _flatProjectileSpeed) * (1f + _percentProjectileSpeed);
        public float CooldownMultiplier => Mathf.Max(0.1f, 1f - (_baseCooldownReduction + _flatCDR + _percentCDR));
        public float Luck => (_baseLuck + _flatLuck) * (1f + _percentLuck);
        public float Regen => (_baseRegen + _flatRegen) * (1f + _percentRegen);
        public float Armor => _baseArmor + _flatArmor;
        public float DetectionRange => _baseDetectionRange;

        public void ApplyCharacterBase(CharacterDefinitionSO def)
        {
            _baseMaxHealth = def.BaseMaxHealth;
            _baseMoveSpeed = def.BaseMoveSpeed;
            _baseDamage = def.BaseDamage;
            _baseArea = def.BaseArea;
            _baseProjectileSpeed = 8f;
            _baseCooldownReduction = def.BaseCooldownReduction;
            _baseLuck = def.BaseLuck;
            _baseRegen = 0f;
            _baseArmor = 0f;
            OnStatsChanged?.Invoke();
        }

        public void ApplyModifier(StatModifier mod)
        {
            switch (mod.Stat)
            {
                case StatType.MaxHealth:       _flatMaxHealth += mod.FlatBonus; _percentMaxHealth += mod.PercentBonus; break;
                case StatType.MoveSpeed:       _flatMoveSpeed += mod.FlatBonus; _percentMoveSpeed += mod.PercentBonus; break;
                case StatType.Damage:          _flatDamage += mod.FlatBonus; _percentDamage += mod.PercentBonus; break;
                case StatType.Area:            _flatArea += mod.FlatBonus; _percentArea += mod.PercentBonus; break;
                case StatType.ProjectileSpeed: _flatProjectileSpeed += mod.FlatBonus; _percentProjectileSpeed += mod.PercentBonus; break;
                case StatType.CooldownReduction: _flatCDR += mod.FlatBonus; _percentCDR += mod.PercentBonus; break;
                case StatType.Luck:            _flatLuck += mod.FlatBonus; _percentLuck += mod.PercentBonus; break;
                case StatType.Regen:           _flatRegen += mod.FlatBonus; _percentRegen += mod.PercentBonus; break;
                case StatType.Armor:           _flatArmor += mod.FlatBonus; break;
            }
            OnStatsChanged?.Invoke();
        }

        public float GetStat(StatType stat) => stat switch
        {
            StatType.MaxHealth       => MaxHealth,
            StatType.MoveSpeed       => MoveSpeed,
            StatType.Damage          => Damage,
            StatType.Area            => Area,
            StatType.ProjectileSpeed => ProjectileSpeed,
            StatType.CooldownReduction => CooldownMultiplier,
            StatType.Luck            => Luck,
            StatType.Regen           => Regen,
            StatType.Armor           => Armor,
            _ => 0f
        };
    }
}

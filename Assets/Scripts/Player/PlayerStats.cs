using System;
using System.Collections.Generic;
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
        public float PercentBonus;
    }

    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float _baseMaxHealth = 60f;
        [SerializeField] private float _baseMoveSpeed = 5f;
        [SerializeField] private float _baseDamage = 8f;
        [SerializeField] private float _baseArea = 1f;
        [SerializeField] private float _baseProjectileSpeed = 8f;
        [SerializeField] private float _baseCooldownReduction = 0f;
        [SerializeField] private float _baseLuck = 1f;
        [SerializeField] private float _baseRegen = 0f;
        [SerializeField] private float _baseArmor = 0f;
        [SerializeField] private float _baseDetectionRange = 15f;

        private float _flatMaxHealth, _percentMaxHealth;
        private float _flatMoveSpeed, _percentMoveSpeed;
        private float _flatDamage, _percentDamage;
        private float _flatArea, _percentArea;
        private float _flatProjectileSpeed, _percentProjectileSpeed;
        private float _flatCDR, _percentCDR;
        private float _flatLuck, _percentLuck;
        private float _flatRegen, _percentRegen;
        private float _flatArmor, _percentArmor;

        private readonly Dictionary<StatType, float> _growth = new()
        {
            { StatType.MaxHealth, 1f },
            { StatType.MoveSpeed, 1f },
            { StatType.Damage, 1f },
            { StatType.Area, 1f },
            { StatType.ProjectileSpeed, 1f },
            { StatType.CooldownReduction, 1f },
            { StatType.Luck, 1f },
            { StatType.Regen, 1f },
            { StatType.Armor, 1f },
        };

        public event Action OnStatsChanged;

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

            _growth[StatType.MaxHealth] = def.HealthGrowth;
            _growth[StatType.MoveSpeed] = def.MoveSpeedGrowth;
            _growth[StatType.Damage] = def.DamageGrowth;
            _growth[StatType.Area] = def.AreaGrowth;
            _growth[StatType.ProjectileSpeed] = def.ProjectileSpeedGrowth;
            _growth[StatType.CooldownReduction] = def.CooldownGrowth;
            _growth[StatType.Luck] = def.LuckGrowth;
            _growth[StatType.Regen] = def.RegenGrowth;
            _growth[StatType.Armor] = def.ArmorGrowth;

            OnStatsChanged?.Invoke();
        }

        public void ApplyModifier(StatModifier mod)
        {
            float g = _growth.TryGetValue(mod.Stat, out var v) ? v : 1f;
            float flat = mod.FlatBonus * g;
            float pct = mod.PercentBonus * g;

            switch (mod.Stat)
            {
                case StatType.MaxHealth:       _flatMaxHealth += flat; _percentMaxHealth += pct; break;
                case StatType.MoveSpeed:       _flatMoveSpeed += flat; _percentMoveSpeed += pct; break;
                case StatType.Damage:          _flatDamage += flat; _percentDamage += pct; break;
                case StatType.Area:            _flatArea += flat; _percentArea += pct; break;
                case StatType.ProjectileSpeed: _flatProjectileSpeed += flat; _percentProjectileSpeed += pct; break;
                case StatType.CooldownReduction: _flatCDR += flat; _percentCDR += pct; break;
                case StatType.Luck:            _flatLuck += flat; _percentLuck += pct; break;
                case StatType.Regen:           _flatRegen += flat; _percentRegen += pct; break;
                case StatType.Armor:           _flatArmor += flat; break;
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

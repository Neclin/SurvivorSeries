using UnityEngine;

namespace SurvivorSeries.Waves.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Difficulty Settings", fileName = "SO_Difficulty_")]
    public class DifficultySettingsSO : ScriptableObject
    {
        public string DifficultyName = "Normal";

        [Header("Enemy Scaling Exponents")]
        [Tooltip("EnemyHP = BaseHP * wave ^ HPScalingExponent")]
        public float HPScalingExponent = 1.3f;
        [Tooltip("EnemyDmg = BaseDmg * wave ^ DamageScalingExponent")]
        public float DamageScalingExponent = 1.2f;

        [Header("Wave Scaling Multipliers")]
        [Tooltip("WaveDuration = BaseDuration * (1 + (wave-1) * DurationScalingMultiplier)")]
        public float DurationScalingMultiplier = 0.15f;
        [Tooltip("EnemyCount = BaseCount * (1 + (wave-1) * EnemyCountScalingMultiplier)")]
        public float EnemyCountScalingMultiplier = 0.20f;

        [Header("Drop Rate Multipliers")]
        public float CurrencyDropMultiplier = 1.0f;
        public float XPDropMultiplier = 1.0f;

        [Header("Boss")]
        public float BossHPMultiplier = 1.0f;
    }
}

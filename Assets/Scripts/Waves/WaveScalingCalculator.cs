using UnityEngine;
using SurvivorSeries.Waves.Data;

namespace SurvivorSeries.Waves
{
    public static class WaveScalingCalculator
    {
        public static float GetEnemyHP(float baseHP, int wave, DifficultySettingsSO diff)
            => baseHP * Mathf.Pow(wave, diff.HPScalingExponent);

        public static float GetEnemyDamage(float baseDmg, int wave, DifficultySettingsSO diff)
            => baseDmg * Mathf.Pow(wave, diff.DamageScalingExponent);

        public static float GetWaveDuration(float baseDuration, int wave, DifficultySettingsSO diff)
            => baseDuration * (1f + (wave - 1) * diff.DurationScalingMultiplier);

        public static int GetEnemyCount(int baseCount, int wave, DifficultySettingsSO diff)
            => Mathf.RoundToInt(baseCount * (1f + (wave - 1) * diff.EnemyCountScalingMultiplier));
    }
}

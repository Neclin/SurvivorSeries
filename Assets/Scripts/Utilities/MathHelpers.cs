using UnityEngine;

namespace SurvivorSeries.Utilities
{
    public static class MathHelpers
    {
        /// <summary>XP threshold to reach level n. Formula: 10 * n^1.4</summary>
        public static float XPThreshold(int level)
        {
            return 10f * Mathf.Pow(level, 1.4f);
        }

        /// <summary>
        /// Enemy HP at wave w: BaseHP * w ^ exponent
        /// </summary>
        public static float ScaleHP(float baseHP, int wave, float exponent)
        {
            return baseHP * Mathf.Pow(wave, exponent);
        }

        /// <summary>
        /// Enemy damage at wave w: BaseDmg * w ^ exponent
        /// </summary>
        public static float ScaleDamage(float baseDmg, int wave, float exponent)
        {
            return baseDmg * Mathf.Pow(wave, exponent);
        }

        /// <summary>
        /// Wave duration at wave w: BaseDuration * (1 + (w-1) * durationMultiplier)
        /// </summary>
        public static float ScaleWaveDuration(float baseDuration, int wave, float durationMultiplier)
        {
            return baseDuration * (1f + (wave - 1) * durationMultiplier);
        }

        /// <summary>
        /// Enemy count at wave w: BaseCount * (1 + (w-1) * countMultiplier)
        /// </summary>
        public static float ScaleEnemyCount(float baseCount, int wave, float countMultiplier)
        {
            return baseCount * (1f + (wave - 1) * countMultiplier);
        }
    }
}

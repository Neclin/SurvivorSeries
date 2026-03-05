using UnityEngine;

namespace SurvivorSeries.Weapons
{
    public class FireProjectile : Projectile
    {
        private float _dotDamagePerTick;
        private float _dotInterval = 0.5f;
        private float _dotDuration;

        public void SetDot(float damagePerTick, float interval, float duration)
        {
            _dotDamagePerTick = damagePerTick;
            _dotInterval = interval;
            _dotDuration = duration;
        }

        protected override void OnHit(Enemies.EnemyHealth enemy)
        {
            if (_dotDuration <= 0f || _dotDamagePerTick <= 0f) return;
            var burn = enemy.GetComponent<BurnDOT>();
            if (burn == null) burn = enemy.gameObject.AddComponent<BurnDOT>();
            burn.Apply(_dotDamagePerTick, _dotInterval, _dotDuration);
        }
    }
}

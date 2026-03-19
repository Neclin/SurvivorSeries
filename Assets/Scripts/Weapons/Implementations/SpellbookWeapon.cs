using UnityEngine;
using SurvivorSeries.Audio;

namespace SurvivorSeries.Weapons.Implementations
{
    public class SpellbookWeapon : WeaponBase
    {
        [SerializeField] private float _dotDamagePerTick = 2f;
        [SerializeField] private float _dotInterval = 0.5f;
        [SerializeField] private float _dotDuration = 4f;

        protected override void Fire()
        {
            if (_projectilePool == null) return;
            Transform target = FindNearestEnemy();
            if (target == null) return;
            AudioManager.Play(SfxId.ProjectileShoot);

            Vector3 origin = GetPlayerPosition() + Vector3.up * 0.8f;
            Vector3 baseDir = target.position - origin;
            baseDir.y = 0f;
            baseDir.Normalize();

            int count = GetProjectileCount();
            float angleStep = count > 1 ? 18f : 0f;
            float startAngle = -angleStep * (count - 1) / 2f;
            float projSpeed = (_ownerStats != null ? _ownerStats.ProjectileSpeed : 8f) * 0.9f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;
                Projectile proj = _projectilePool.Get();
                if (proj is FireProjectile fire)
                    fire.SetDot(_dotDamagePerTick, _dotInterval, _dotDuration);
                proj.Initialize(origin, dir, GetDamage(), projSpeed, 4f, _projectilePool,
                                area: GetArea());
            }
        }
    }
}

using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    /// <summary>
    /// Fires N projectiles toward the nearest enemy.
    /// Multiple projectiles fan out with a small angle spread.
    /// </summary>
    public class OrbWeapon : WeaponBase
    {
        protected override void Fire()
        {
            if (_projectilePool == null) return;

            Transform target = FindNearestEnemy();
            if (target == null) return;
            Vector3 baseDir = (target.position - transform.position).normalized;

            int count = GetProjectileCount();
            float angleStep = count > 1 ? 15f : 0f;
            float startAngle = -angleStep * (count - 1) / 2f;
            float projSpeed = _ownerStats != null ? _ownerStats.ProjectileSpeed : 8f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;

                Projectile proj = _projectilePool.Get();
                proj.Initialize(transform.position + Vector3.up * 0.8f,
                                dir, GetDamage(), projSpeed, 5f, _projectilePool);
            }
        }
    }
}

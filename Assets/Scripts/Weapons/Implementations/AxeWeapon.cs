using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    /// <summary>
    /// Thrown axe weapon. Fires axes with an upward arc toward the nearest enemy.
    /// Uses gravity so axes follow a ballistic trajectory.
    /// Multiple axes are spaced 30 degrees apart.
    /// </summary>
    public class AxeWeapon : WeaponBase
    {
        protected override void Fire()
        {
            if (_projectilePool == null) return;

            Transform target = FindNearestEnemy();
            if (target == null) return;
            Vector3 baseDir = target.position - transform.position;
            baseDir.y = 0f;
            baseDir.Normalize();

            int count = GetProjectileCount();
            float spreadAngle = 30f;
            float startAngle = -spreadAngle * (count - 1) / 2f;
            float projSpeed = _ownerStats != null ? _ownerStats.ProjectileSpeed : 8f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + spreadAngle * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;

                Projectile proj = _projectilePool.Get();
                proj.Initialize(transform.position + Vector3.up * 0.8f,
                                dir, GetDamage(), projSpeed, 3f, _projectilePool);
            }
        }
    }
}

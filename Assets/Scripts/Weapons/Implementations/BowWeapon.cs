using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    public class BowWeapon : WeaponBase
    {
        protected override void Fire()
        {
            if (_projectilePool == null) return;
            Transform target = FindNearestEnemy();
            if (target == null) return;

            Vector3 origin = GetPlayerPosition() + Vector3.up * 0.8f;
            Vector3 baseDir = target.position - origin;
            baseDir.y = 0f;
            baseDir.Normalize();

            int count = GetProjectileCount();
            float angleStep = count > 1 ? 12f : 0f;
            float startAngle = -angleStep * (count - 1) / 2f;
            float projSpeed = _ownerStats != null ? _ownerStats.ProjectileSpeed : 12f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;
                Projectile proj = _projectilePool.Get();
                proj.Initialize(origin, dir, GetDamage(), projSpeed, 5f, _projectilePool,
                                area: GetArea());
            }
        }
    }
}

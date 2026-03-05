using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    public class DaggerWeapon : WeaponBase
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
            float spreadAngle = 30f;
            float startAngle = -spreadAngle * (count - 1) / 2f;
            float projSpeed = (_ownerStats != null ? _ownerStats.ProjectileSpeed : 8f) * 1.5f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + spreadAngle * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;
                Projectile proj = _projectilePool.Get();
                proj.Initialize(origin, dir, GetDamage(), projSpeed, 4f, _projectilePool, pierce: true);
            }
        }
    }
}

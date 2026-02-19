using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    /// <summary>
    /// Instant chain-lightning. Strikes the N nearest enemies within detection range.
    /// Uses Debug.DrawLine for development visualization. No projectile spawned.
    /// </summary>
    public class LightningWeapon : WeaponBase
    {
        protected override void Fire()
        {
            int count = GetProjectileCount();
            float damage = GetDamage();

            var enemies = FindEnemiesInRange(GetDetectionRange());
            if (enemies.Count == 0) return;

            enemies.Sort((a, b) =>
                (a.transform.position - transform.position).sqrMagnitude
                    .CompareTo((b.transform.position - transform.position).sqrMagnitude));

            Vector3 origin = transform.position + Vector3.up * 0.8f;
            int strikes = Mathf.Min(count, enemies.Count);

            for (int i = 0; i < strikes; i++)
            {
                enemies[i].TakeDamage(damage);
                Debug.DrawLine(origin, enemies[i].transform.position + Vector3.up * 0.5f,
                               Color.yellow, 0.3f);
            }
        }
    }
}

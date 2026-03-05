using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    public class StaffWeapon : WeaponBase
    {
        protected override void Fire()
        {
            int count = GetProjectileCount();
            float damage = GetDamage();

            var enemies = FindEnemiesInRange(GetDetectionRange());
            if (enemies.Count == 0) return;

            Vector3 playerPos = GetPlayerPosition();
            enemies.Sort((a, b) =>
                (a.transform.position - playerPos).sqrMagnitude
                    .CompareTo((b.transform.position - playerPos).sqrMagnitude));

            Vector3 origin = playerPos + Vector3.up * 0.8f;
            int strikes = Mathf.Min(count, enemies.Count);

            for (int i = 0; i < strikes; i++)
            {
                enemies[i].TakeDamage(damage);
                Debug.DrawLine(origin, enemies[i].transform.position + Vector3.up * 0.5f,
                               new Color(1f, 0.95f, 0.2f, 1f), 0.3f);
            }
        }
    }
}

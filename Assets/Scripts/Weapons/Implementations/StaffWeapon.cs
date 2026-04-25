using UnityEngine;
using SurvivorSeries.Audio;

namespace SurvivorSeries.Weapons.Implementations
{
    public class StaffWeapon : WeaponBase
    {
        private static readonly Color BoltColor = new(1f, 0.95f, 0.30f, 1f);
        private const float BoltWidth = 0.12f;
        private const float BoltDuration = 0.22f;

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
                SpawnBolt(origin, enemies[i].transform.position + Vector3.up * 0.5f);
            }

            AudioManager.Play(SfxId.LightningStrike);
        }

        private void SpawnBolt(Vector3 from, Vector3 to)
        {
            var go = new GameObject("StaffBolt");
            var line = go.AddComponent<LineRenderer>();
            line.numCapVertices = 2;
            var bolt = go.AddComponent<LightningBolt>();
            bolt.Initialize(from, to, BoltColor, BoltWidth, BoltDuration);
        }
    }
}

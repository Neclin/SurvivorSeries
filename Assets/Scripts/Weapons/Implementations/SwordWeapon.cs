using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    public class SwordWeapon : WeaponBase
    {
        [SerializeField] private float _arcAngle = 90f;
        [SerializeField] private float _baseRange = 2.5f;

        protected override void Fire()
        {
            float range = _baseRange * GetArea();
            var enemies = FindEnemiesInRange(range);
            if (enemies.Count == 0) return;

            Transform player = _ownerStats != null ? _ownerStats.transform : transform;
            Vector3 origin = player.position;
            Vector3 forward = ResolveSwingDirection(player);
            float halfArc = _arcAngle * 0.5f;

            foreach (var eh in enemies)
            {
                Vector3 toEnemy = eh.transform.position - origin;
                toEnemy.y = 0f;
                if (Vector3.Angle(forward, toEnemy) <= halfArc)
                    eh.TakeDamage(GetDamage());
            }
        }

        private Vector3 ResolveSwingDirection(Transform player)
        {
            Transform target = FindNearestEnemy();
            if (target != null)
            {
                Vector3 toTarget = target.position - player.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.001f) return toTarget.normalized;
            }
            return player.forward;
        }
    }
}

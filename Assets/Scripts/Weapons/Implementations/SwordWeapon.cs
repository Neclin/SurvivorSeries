using UnityEngine;
using SurvivorSeries.Audio;

namespace SurvivorSeries.Weapons.Implementations
{
    public class SwordWeapon : WeaponBase
    {
        [SerializeField] private float _arcAngle = 90f;
        [SerializeField] private float _baseRange = 2.5f;
        [SerializeField] private float _swingDuration = 0.22f;
        [SerializeField] private float _swingRadiusBoost = 0.6f;

        private float _swingStartTime = -999f;
        private Vector3 _swingDirection = Vector3.forward;

        protected override void Fire()
        {
            float range = _baseRange * GetArea();
            var enemies = FindEnemiesInRange(range);
            if (enemies.Count == 0) return;

            Transform player = _ownerStats != null ? _ownerStats.transform : transform;
            Vector3 origin = player.position;
            Vector3 forward = ResolveSwingDirection(player);

            _swingDirection = forward;
            _swingStartTime = Time.time;
            AudioManager.Play(SfxId.SwordSwing);

            float halfArc = _arcAngle * 0.5f;
            foreach (var eh in enemies)
            {
                Vector3 toEnemy = eh.transform.position - origin;
                toEnemy.y = 0f;
                if (Vector3.Angle(forward, toEnemy) <= halfArc)
                    eh.TakeDamage(GetDamage());
            }
        }

        protected override void Update()
        {
            base.Update();
            if (_displayModel == null) return;

            float t = Time.time - _swingStartTime;
            if (_swingStartTime < 0f || t < 0f || t > _swingDuration) return;

            float u = t / _swingDuration;
            float blend = Mathf.Sin(u * Mathf.PI);

            float arcAngle = Mathf.Lerp(-_arcAngle * 0.5f, _arcAngle * 0.5f, u);
            Vector3 dir = Quaternion.AngleAxis(arcAngle, Vector3.up) * _swingDirection;
            float reach = OrbitRadius + _swingRadiusBoost * blend;
            Vector3 swingPos = GetPlayerPosition() + dir * reach + Vector3.up * OrbitHeight;
            Quaternion swingRot = Quaternion.LookRotation(dir);

            _displayModel.position = Vector3.Lerp(_displayModel.position, swingPos, blend);
            _displayModel.rotation = Quaternion.Slerp(_displayModel.rotation, swingRot, blend);
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

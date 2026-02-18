using System.Collections.Generic;
using UnityEngine;

namespace SurvivorSeries.Weapons
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        private float _damage;
        private float _lifetime;
        private float _elapsed;
        private ProjectilePool _pool;
        private Rigidbody _rb;
        private bool _pierce;
        private HashSet<int> _hitEnemyIds;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            // None avoids the one-frame ghost render caused by interpolation when teleporting
            _rb.interpolation = RigidbodyInterpolation.None;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        /// <param name="spawnPosition">World-space position the projectile should start from.</param>
        public void Initialize(Vector3 spawnPosition, Vector3 direction, float damage, float speed,
                               float lifetime, ProjectilePool pool,
                               bool useGravity = false, bool pierce = false)
        {
            _damage = damage;
            _lifetime = lifetime;
            _pool = pool;
            _elapsed = 0f;
            _pierce = pierce;

            if (_pierce)
            {
                _hitEnemyIds ??= new HashSet<int>();
                _hitEnemyIds.Clear();
            }

            // Detach first so the position is in world space
            transform.SetParent(null);

            // Use rb.position so the physics engine is immediately in sync —
            // setting transform.position on a non-kinematic Rigidbody lags one physics frame
            _rb.position = spawnPosition;
            _rb.rotation = Quaternion.identity;
            _rb.useGravity = useGravity;
            _rb.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
                ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            var enemyHealth = other.GetComponentInParent<Enemies.EnemyHealth>();
            if (enemyHealth == null) return;

            if (_pierce)
            {
                // Skip enemies already hit by this projectile
                if (!_hitEnemyIds.Add(enemyHealth.EnemyId)) return;
                enemyHealth.TakeDamage(_damage);
                // Don't return to pool — continue through enemies until lifetime or Ground
            }
            else
            {
                enemyHealth.TakeDamage(_damage);
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = false;

            if (_pool != null)
            {
                transform.SetParent(_pool.transform);
                // Reset local position to zero so it doesn't ghost-collide on next activation
                transform.localPosition = Vector3.zero;
                _pool.Return(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}

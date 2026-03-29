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
            _rb.interpolation = RigidbodyInterpolation.None;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        public void Initialize(Vector3 spawnPosition, Vector3 direction, float damage, float speed,
                               float lifetime, ProjectilePool pool,
                               bool useGravity = false, bool pierce = false, float area = 1f)
        {
            _damage = damage;
            _lifetime = lifetime;
            _pool = pool;
            _elapsed = 0f;
            _pierce = pierce;
            transform.localScale = Vector3.one * Mathf.Max(0.1f, area);

            if (_pierce)
            {
                _hitEnemyIds ??= new HashSet<int>();
                _hitEnemyIds.Clear();
            }

            transform.SetParent(null);

            Vector3 normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            _rb.position = spawnPosition;
            _rb.rotation = Quaternion.LookRotation(normalized);
            _rb.useGravity = useGravity;
            _rb.linearVelocity = normalized * speed;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                ReturnToPool();
                return;
            }

            Vector3 velocity = _rb.linearVelocity;
            if (velocity.sqrMagnitude > 0.01f)
                _rb.rotation = Quaternion.LookRotation(velocity);
        }

        private static int _obstacleLayer = -1;

        private void OnTriggerEnter(Collider other)
        {
            if (_obstacleLayer < 0) _obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (other.gameObject.layer == _obstacleLayer)
            {
                ReturnToPool();
                return;
            }

            var enemyHealth = other.GetComponentInParent<Enemies.EnemyHealth>();
            if (enemyHealth == null) return;

            if (_pierce)
            {
                if (!_hitEnemyIds.Add(enemyHealth.EnemyId)) return;
                enemyHealth.TakeDamage(_damage);
                OnHit(enemyHealth);
            }
            else
            {
                enemyHealth.TakeDamage(_damage);
                OnHit(enemyHealth);
                ReturnToPool();
            }
        }

        protected virtual void OnHit(Enemies.EnemyHealth enemy) { }

        private void ReturnToPool()
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = false;

            if (_pool != null)
            {
                transform.SetParent(_pool.transform);
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
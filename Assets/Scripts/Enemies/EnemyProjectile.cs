using UnityEngine;

namespace SurvivorSeries.Enemies
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class EnemyProjectile : MonoBehaviour
    {
        private float _damage;
        private float _elapsed;
        private float _lifetime = 5f;
        private Rigidbody _rb;

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

        public void Fire(Vector3 position, Vector3 direction, float damage, float speed, float lifetime = 5f)
        {
            _rb.position = position;
            _damage = damage;
            _lifetime = lifetime;
            _elapsed = 0f;
            _rb.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var ph = other.GetComponentInParent<Player.PlayerHealth>();
            if (ph != null) ph.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
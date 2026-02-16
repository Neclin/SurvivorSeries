using UnityEngine;
using SurvivorSeries.Enemies.Data;

namespace SurvivorSeries.Enemies
{
    [RequireComponent(typeof(EnemyHealth), typeof(EnemyMovement), typeof(EnemyDropper))]
    public abstract class EnemyBase : MonoBehaviour
    {
        protected EnemyDataSO _data;
        protected EnemyHealth _health;
        protected EnemyMovement _movement;
        protected EnemyDropper _dropper;
        protected EnemyPool _pool;

        private float _contactDamageTimer;

        protected virtual void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _movement = GetComponent<EnemyMovement>();
            _dropper = GetComponent<EnemyDropper>();
        }

        public virtual void Initialize(EnemyDataSO data, Transform playerTarget,
                                       float hpMultiplier, float dmgMultiplier, EnemyPool pool)
        {
            _data = data;
            _pool = pool;
            _contactDamageTimer = 0f;

            _health.Initialize(data.BaseHealth * hpMultiplier);
            _health.OnDeath += OnDeath;

            _movement.Initialize(data.MoveSpeed);
            _movement.SetTarget(playerTarget);
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.OnDeath -= OnDeath;
        }

        private void Update()
        {
            _contactDamageTimer -= Time.deltaTime;
            UpdateBehavior();
        }

        protected abstract void UpdateBehavior();

        // Called when this enemy's trigger collider overlaps the player
        protected void HandleContactWithPlayer(Collider other)
        {
            if (_contactDamageTimer > 0f) return;
            if (!other.CompareTag("Player")) return;

            var playerHealth = other.GetComponentInParent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_data.BaseDamage);
                _contactDamageTimer = _data.ContactDamageInterval;
            }
        }

        protected virtual void OnDeath()
        {
            _dropper.SpawnDrops(transform.position, _data);
            _pool?.Return(this);
        }
    }
}

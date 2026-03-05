using UnityEngine;

namespace SurvivorSeries.Enemies.Variants
{
    public class RangedEnemy : EnemyBase
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _attackRange = 10f;
        [SerializeField] private float _fleeRange = 4f;
        [SerializeField] private float _projectileSpeed = 8f;
        [SerializeField] private float _attackCooldown = 2f;

        private Transform _playerTarget;
        private float _fireTimer;
        private float _scaledDamage;

        public override void Initialize(Data.EnemyDataSO data, Transform playerTarget,
                                        float hpMultiplier, float dmgMultiplier, EnemyPool pool)
        {
            base.Initialize(data, playerTarget, hpMultiplier, dmgMultiplier, pool);
            _playerTarget = playerTarget;
            _scaledDamage = data.BaseDamage * dmgMultiplier;
            _fireTimer = _attackCooldown;
        }

        protected override void UpdateBehavior()
        {
            if (_playerTarget == null) return;

            float dist = Vector3.Distance(transform.position, _playerTarget.position);
            _fireTimer -= Time.deltaTime;

            if (dist < _fleeRange)
            {
                Vector3 awayDir = (transform.position - _playerTarget.position).normalized;
                _movement.SetDestinationOverride(transform.position + awayDir * 8f);
            }
            else if (dist <= _attackRange)
            {
                _movement.Pause();
                if (_fireTimer <= 0f)
                {
                    FireAtPlayer();
                    _fireTimer = _attackCooldown;
                }
            }
            else
            {
                _movement.ClearOverride();
                _movement.Resume();
            }
        }

        private void FireAtPlayer()
        {
            if (_projectilePrefab == null || _playerTarget == null) return;

            Vector3 spawnPos = transform.position + Vector3.up * 0.8f;
            Vector3 dir = (_playerTarget.position + Vector3.up * 0.8f - spawnPos).normalized;

            var go = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
            var proj = go.GetComponent<EnemyProjectile>();
            proj?.Fire(spawnPos, dir, _scaledDamage, _projectileSpeed, 6f);
        }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);
    }
}
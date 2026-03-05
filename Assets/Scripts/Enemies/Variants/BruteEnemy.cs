using UnityEngine;

namespace SurvivorSeries.Enemies.Variants
{
    public class BruteEnemy : EnemyBase
    {
        [SerializeField] private float _chargeSpeedMultiplier = 4f;
        [SerializeField] private float _chargeDuration = 1.5f;
        [SerializeField] private float _chargeIntervalMin = 4f;
        [SerializeField] private float _chargeIntervalMax = 7f;

        private bool _isCharging;
        private float _chargeTimer;

        protected override void UpdateBehavior()
        {
            _chargeTimer -= Time.deltaTime;
            if (_chargeTimer > 0f) return;

            if (!_isCharging)
            {
                _isCharging = true;
                _movement.SetSpeed(_data.MoveSpeed * _chargeSpeedMultiplier);
                _chargeTimer = _chargeDuration;
            }
            else
            {
                _isCharging = false;
                _movement.SetSpeed(_data.MoveSpeed);
                _chargeTimer = Random.Range(_chargeIntervalMin, _chargeIntervalMax);
            }
        }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);
    }
}
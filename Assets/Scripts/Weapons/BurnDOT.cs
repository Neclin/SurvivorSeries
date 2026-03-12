using UnityEngine;
using SurvivorSeries.Enemies;

namespace SurvivorSeries.Weapons
{
    public class BurnDOT : MonoBehaviour
    {
        private float _tickDamage;
        private float _interval = 0.5f;
        private float _remainingDuration;
        private float _tickTimer;
        private EnemyHealth _hp;

        private void Awake() => _hp = GetComponent<EnemyHealth>();

        private void OnDisable() => Destroy(this);

        public void Apply(float damagePerTick, float interval, float duration)
        {
            _tickDamage = Mathf.Max(_tickDamage, damagePerTick);
            _interval = interval > 0f ? interval : 0.5f;
            _remainingDuration = Mathf.Max(_remainingDuration, duration);
            if (_tickTimer <= 0f) _tickTimer = _interval;
        }

        private void Update()
        {
            if (_hp == null || _hp.IsDead)
            {
                Destroy(this);
                return;
            }

            _remainingDuration -= Time.deltaTime;
            _tickTimer -= Time.deltaTime;

            if (_tickTimer <= 0f)
            {
                _hp.TakeDamage(_tickDamage);
                _tickTimer += _interval;
            }

            if (_remainingDuration <= 0f) Destroy(this);
        }
    }
}

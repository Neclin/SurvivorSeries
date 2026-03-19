using System;
using UnityEngine;
using SurvivorSeries.Audio;

namespace SurvivorSeries.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        private static int _nextId = 1;

        private float _maxHealth;
        private float _currentHealth;

        public int EnemyId { get; private set; }
        public bool IsDead => _currentHealth <= 0f;
        public float HealthPercent => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;

        public event Action OnDeath;
        public event Action<float> OnDamageTaken;
        public static event Action<EnemyHealth> OnAnyKilled;

        private Utilities.HitFlash _flash;

        private void Awake()
        {
            EnemyId = _nextId++;
            _flash = GetComponentInChildren<Utilities.HitFlash>();
        }

        public void Initialize(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            _flash?.Flash();
            OnDamageTaken?.Invoke(amount);
            AudioManager.Play(SfxId.EnemyHit);

            if (_currentHealth <= 0f)
            {
                AudioManager.Play(SfxId.EnemyDeath);
                OnDeath?.Invoke();
                OnAnyKilled?.Invoke(this);
            }
        }
    }
}
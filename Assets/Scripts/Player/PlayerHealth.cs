using System;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        private PlayerStats _stats;
        private float _currentHealth;
        private float _invincibilityTimer;
        private float _regenAccumulator;

        private const float InvincibilityDuration = 0.5f;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _stats != null ? _stats.MaxHealth : 100f;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action OnDeath;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            ServiceLocator.Register<PlayerHealth>(this);
        }

        private void Start()
        {
            _currentHealth = MaxHealth;
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        private void OnDestroy() => ServiceLocator.Unregister<PlayerHealth>();

        private void Update()
        {
            if (_invincibilityTimer > 0f)
                _invincibilityTimer -= Time.deltaTime;

            if (_stats != null && _stats.Regen > 0f)
            {
                _regenAccumulator += _stats.Regen * Time.deltaTime;
                if (_regenAccumulator >= 1f)
                {
                    float toHeal = Mathf.Floor(_regenAccumulator);
                    _regenAccumulator -= toHeal;
                    Heal(toHeal);
                }
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || _invincibilityTimer > 0f) return;

            float mitigated = Mathf.Max(0f, amount - (_stats != null ? _stats.Armor : 0f));
            _currentHealth = Mathf.Max(0f, _currentHealth - mitigated);
            _invincibilityTimer = InvincibilityDuration;
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

            if (_currentHealth <= 0f)
                OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        public void HealPercent(float percent) => Heal(MaxHealth * Mathf.Clamp01(percent));

        public void ResetHealth()
        {
            _currentHealth = MaxHealth;
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }
    }
}

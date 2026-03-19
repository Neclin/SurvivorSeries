using System;
using UnityEngine;
using SurvivorSeries.Audio;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        private PlayerStats _stats;
        private Utilities.HitFlash _flash;
        private float _currentHealth;
        private float _invincibilityTimer;
        private float _regenAccumulator;
        private float _lastMaxHealth;

        private const float InvincibilityDuration = 0.5f;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _stats != null ? _stats.MaxHealth : 100f;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _flash = GetComponentInChildren<Utilities.HitFlash>();
            ServiceLocator.Register<PlayerHealth>(this);
        }

        private void Start()
        {
            _currentHealth = MaxHealth;
            _lastMaxHealth = MaxHealth;
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

            if (_stats != null)
                _stats.OnStatsChanged += HandleStatsChanged;
        }

        private void OnDestroy()
        {
            if (_stats != null)
                _stats.OnStatsChanged -= HandleStatsChanged;
            ServiceLocator.Unregister<PlayerHealth>();
        }

        private void HandleStatsChanged()
        {
            float newMax = MaxHealth;
            float delta = newMax - _lastMaxHealth;
            if (delta > 0f)
                _currentHealth = Mathf.Min(newMax, _currentHealth + delta);
            else
                _currentHealth = Mathf.Min(_currentHealth, newMax);
            _lastMaxHealth = newMax;
            OnHealthChanged?.Invoke(_currentHealth, newMax);
            Debug.Log($"[PlayerHealth] Stats changed → max={newMax} cur={_currentHealth}");
        }

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
            _flash?.Flash();
            AudioManager.Play(SfxId.PlayerHurt);
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
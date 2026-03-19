using UnityEngine;
using SurvivorSeries.Audio;

namespace SurvivorSeries.Pickups
{
    public class CurrencyDrop : MonoBehaviour
    {
        [SerializeField] private float _magnetRadius = 5f;
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _collectDistance = 0.6f;

        private int _value;
        private Player.PlayerCurrencyHandler _currencyHandler;
        private Player.PlayerLevelSystem _levelSystem;
        private Transform _playerTransform;
        private CurrencyDropPool _pool;
        private bool _attracted;
        private bool _collected;

        public void Initialize(int value, Transform playerTransform,
                               Player.PlayerCurrencyHandler currencyHandler,
                               Player.PlayerLevelSystem levelSystem,
                               CurrencyDropPool pool)
        {
            _value = value;
            _playerTransform = playerTransform;
            _currencyHandler = currencyHandler;
            _levelSystem = levelSystem;
            _pool = pool;
            _attracted = false;
            _collected = false;
        }

        private void Update()
        {
            if (_playerTransform == null || _collected) return;

            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= _magnetRadius || _attracted)
            {
                _attracted = true;
                transform.position = Vector3.MoveTowards(
                    transform.position, _playerTransform.position,
                    _moveSpeed * Time.deltaTime);
            }

            if (dist <= _collectDistance) Collect();
        }

        private void Collect()
        {
            if (_collected) return;
            _collected = true;
            _currencyHandler?.AddCurrency(_value);
            _levelSystem?.AddXP(_value);
            AudioManager.Play(SfxId.CoinPickup);
            _pool?.Return(this);
        }
    }
}
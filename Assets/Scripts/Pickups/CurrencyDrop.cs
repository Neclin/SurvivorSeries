using UnityEngine;

namespace SurvivorSeries.Pickups
{
    /// <summary>
    /// Single pickup type. Grants both currency and XP when collected.
    /// Magnetizes toward the player when within range.
    /// </summary>
    public class CurrencyDrop : MonoBehaviour
    {
        [SerializeField] private float _magnetRadius = 5f;
        [SerializeField] private float _moveSpeed = 8f;

        private int _value;
        private Player.PlayerCurrencyHandler _currencyHandler;
        private Player.PlayerLevelSystem _levelSystem;
        private Transform _playerTransform;
        private CurrencyDropPool _pool;
        private bool _attracted;

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
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= _magnetRadius || _attracted)
            {
                _attracted = true;
                transform.position = Vector3.MoveTowards(
                    transform.position, _playerTransform.position,
                    _moveSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _currencyHandler?.AddCurrency(_value);
            _levelSystem?.AddXP(_value);
            _pool?.Return(this);
        }
    }
}

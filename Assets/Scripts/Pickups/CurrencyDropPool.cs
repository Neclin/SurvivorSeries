using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Pickups
{
    public class CurrencyDropPool : MonoBehaviour
    {
        [SerializeField] private CurrencyDrop _prefab;
        [SerializeField] private int _initialSize = 60;

        private ObjectPool<CurrencyDrop> _pool;
        private Transform _playerTransform;
        private Player.PlayerCurrencyHandler _currencyHandler;
        private Player.PlayerLevelSystem _levelSystem;

        private void Awake()
        {
            _pool = new ObjectPool<CurrencyDrop>(_prefab, _initialSize, transform);
            ServiceLocator.Register<CurrencyDropPool>(this);
        }

        private void OnDestroy() => ServiceLocator.Unregister<CurrencyDropPool>();

        public void SetPlayerReference(Transform player,
                                       Player.PlayerCurrencyHandler currencyHandler,
                                       Player.PlayerLevelSystem levelSystem)
        {
            _playerTransform = player;
            _currencyHandler = currencyHandler;
            _levelSystem = levelSystem;
        }

        public void Spawn(Vector3 position, int value)
        {
            CurrencyDrop drop = _pool.Get();
            drop.transform.position = position;
            drop.Initialize(value, _playerTransform, _currencyHandler, _levelSystem, this);
        }

        public void Return(CurrencyDrop drop) => _pool.Return(drop);
    }
}

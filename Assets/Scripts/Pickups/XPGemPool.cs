using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Pickups
{
    public class XPGemPool : MonoBehaviour
    {
        [SerializeField] private XPGem _prefab;
        [SerializeField] private int _initialSize = 50;

        private ObjectPool<XPGem> _pool;
        private Transform _playerTransform;
        private Player.PlayerLevelSystem _levelSystem;

        private void Awake()
        {
            _pool = new ObjectPool<XPGem>(_prefab, _initialSize, transform);
            ServiceLocator.Register<XPGemPool>(this);
        }

        private void OnDestroy() => ServiceLocator.Unregister<XPGemPool>();

        public void SetPlayerReference(Transform player, Player.PlayerLevelSystem levelSystem)
        {
            _playerTransform = player;
            _levelSystem = levelSystem;
        }

        public void Spawn(Vector3 position, float xpValue)
        {
            XPGem gem = _pool.Get();
            gem.transform.position = position;
            gem.Initialize(xpValue, _playerTransform, _levelSystem, this);
        }

        public void Return(XPGem gem) => _pool.Return(gem);
    }
}

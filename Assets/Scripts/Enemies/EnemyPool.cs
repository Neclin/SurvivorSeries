using UnityEngine;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Enemies
{
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private EnemyBase _prefab;
        [SerializeField] private int _initialSize = 20;

        private ObjectPool<EnemyBase> _pool;
        private Transform _playerTransform;

        private void Awake()
        {
            _pool = new ObjectPool<EnemyBase>(_prefab, _initialSize, transform);
        }

        public void SetPlayerTransform(Transform player) => _playerTransform = player;

        public EnemyBase Get(EnemyDataSO data, Vector3 position,
                             float hpMultiplier, float dmgMultiplier)
        {
            EnemyBase enemy = _pool.Get();
            enemy.transform.position = position;
            enemy.Initialize(data, _playerTransform, hpMultiplier, dmgMultiplier, this);
            return enemy;
        }

        public void Return(EnemyBase enemy) => _pool.Return(enemy);
    }
}

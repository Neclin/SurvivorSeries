using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Enemies
{
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private EnemyBase _prefab;
        [SerializeField] private int _initialSize = 12;

        private readonly Dictionary<EnemyBase, ObjectPool<EnemyBase>> _pools = new();
        private Transform _playerTransform;

        private void Awake()
        {
            if (_prefab != null)
                EnsurePool(_prefab);
        }

        public void SetPlayerTransform(Transform player) => _playerTransform = player;

        public EnemyBase Get(EnemyDataSO data, Vector3 position,
                             float hpMultiplier, float dmgMultiplier)
        {
            var prefab = ResolvePrefab(data);
            if (prefab == null)
            {
                Debug.LogWarning($"[EnemyPool] No prefab for data '{data?.name}'");
                return null;
            }
            var pool = EnsurePool(prefab);
            var enemy = pool.Get();
            enemy.OriginPrefab = prefab;
            enemy.transform.position = position;
            enemy.Initialize(data, _playerTransform, hpMultiplier, dmgMultiplier, this);
            return enemy;
        }

        public void Return(EnemyBase enemy)
        {
            if (enemy == null) return;
            var key = enemy.OriginPrefab != null ? enemy.OriginPrefab : _prefab;
            if (key != null && _pools.TryGetValue(key, out var pool))
            {
                pool.Return(enemy);
                return;
            }
            enemy.gameObject.SetActive(false);
        }

        public void DespawnAll()
        {
            foreach (var kv in _pools) kv.Value.ReturnAllActive();
        }

        private ObjectPool<EnemyBase> EnsurePool(EnemyBase prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool)) return pool;
            pool = new ObjectPool<EnemyBase>(prefab, _initialSize, transform);
            _pools[prefab] = pool;
            return pool;
        }

        private EnemyBase ResolvePrefab(EnemyDataSO data)
        {
            if (data != null && data.Prefab != null)
            {
                var p = data.Prefab.GetComponent<EnemyBase>();
                if (p != null) return p;
            }
            return _prefab;
        }
    }
}

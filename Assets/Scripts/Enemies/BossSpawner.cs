using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Enemies
{
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] private List<EnemyDataSO> _bossPool = new();
        [SerializeField] private float _spawnDistance = 12f;
        [SerializeField] private float _hpMultiplier = 1f;
        [SerializeField] private float _dmgMultiplier = 1f;

        private void Awake() => ServiceLocator.Register<BossSpawner>(this);
        private void OnDestroy() => ServiceLocator.Unregister<BossSpawner>();

        public EnemyBase SpawnBoss()
        {
            if (_bossPool == null || _bossPool.Count == 0)
            {
                Debug.LogError("[BossSpawner] No bosses configured.");
                return null;
            }
            if (!ServiceLocator.TryGet<Player.PlayerHealth>(out var ph))
            {
                Debug.LogError("[BossSpawner] No PlayerHealth.");
                return null;
            }
            if (!ServiceLocator.TryGet<EnemyPool>(out var pool))
            {
                pool = FindFirstObjectByType<EnemyPool>();
                if (pool == null) { Debug.LogError("[BossSpawner] No EnemyPool."); return null; }
            }

            var data = _bossPool[Random.Range(0, _bossPool.Count)];
            Vector3 origin = ph.transform.position;
            Vector3 offset = Random.insideUnitCircle.normalized * _spawnDistance;
            Vector3 pos = origin + new Vector3(offset.x, 0f, offset.y);
            pos.y = 0f;

            var boss = pool.Get(data, pos, _hpMultiplier, _dmgMultiplier);
            Debug.Log($"[BossSpawner] Spawned boss '{data.EnemyName}' at {pos}.");
            return boss;
        }
    }
}

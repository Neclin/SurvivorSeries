using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float _spawnRadius = 22f;

        private Transform _playerTransform;
        private EnemyPool _enemyPool;
        private CancellationTokenSource _cts;

        private void Awake() => ServiceLocator.Register<EnemySpawner>(this);
        private void OnDestroy() => ServiceLocator.Unregister<EnemySpawner>();

        public void Setup(Transform player, EnemyPool pool)
        {
            _playerTransform = player;
            _enemyPool = pool;
        }

        public void StartSpawning(EnemyDataSO data, int totalCount, float duration,
                                  float hpMultiplier, float dmgMultiplier)
        {
            StopSpawning();
            _cts = new CancellationTokenSource();
            _ = SpawnLoop(data, totalCount, duration, hpMultiplier, dmgMultiplier, _cts.Token);
        }

        public void StopSpawning()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async Awaitable SpawnLoop(EnemyDataSO data, int totalCount, float duration,
                                          float hpMult, float dmgMult, CancellationToken ct)
        {
            if (totalCount <= 0 || duration <= 0f) return;

            float interval = duration / totalCount;
            int spawned = 0;

            while (spawned < totalCount && !ct.IsCancellationRequested)
            {
                SpawnOne(data, hpMult, dmgMult);
                spawned++;
                await Awaitable.WaitForSecondsAsync(interval, ct);
            }
        }

        private void SpawnOne(EnemyDataSO data, float hpMult, float dmgMult)
        {
            if (_playerTransform == null || _enemyPool == null) return;

            const int MaxAttempts = 6;
            for (int i = 0; i < MaxAttempts; i++)
            {
                Vector3 candidate = _playerTransform.position
                                 + Extensions.RandomPointOnCircle(_spawnRadius);
                candidate.y = 0f;

                if (NavMesh.SamplePosition(candidate, out var hit, 3f, NavMesh.AllAreas))
                {
                    _enemyPool.Get(data, hit.position, hpMult, dmgMult);
                    return;
                }
            }
        }
    }
}
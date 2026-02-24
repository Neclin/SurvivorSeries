using System;
using System.Threading;
using UnityEngine;
using SurvivorSeries.Enemies;
using SurvivorSeries.Waves.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Waves
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private WaveConfigSO _config;
        [SerializeField] private bool _autoStartOnPlay = true;

        private DifficultySettingsSO _difficulty;
        private int _currentWave;
        private float _waveTimer;
        private bool _waveActive;
        private CancellationTokenSource _cts;

        public bool IsWaveActive => _waveActive;

        public int CurrentWave => _currentWave;
        public float WaveTimeRemaining => Mathf.Max(0f, _waveTimer);
        public bool IsFinalWave => _currentWave >= _config.TotalWaves;

        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveEnded;

        private void Awake() => ServiceLocator.Register<WaveManager>(this);
        private void OnDestroy() => ServiceLocator.Unregister<WaveManager>();

        private void Start()
        {
            if (_autoStartOnPlay && _config != null)
                StartNextWave();
        }

        private void Update()
        {
            if (!_waveActive) return;
            _waveTimer -= Time.deltaTime;
            if (_waveTimer <= 0f) EndWave();
        }

        public void SetDifficulty(DifficultySettingsSO diff) => _difficulty = diff;

        public void StartNextWave()
        {
            _currentWave++;
            StartWave(_currentWave);
        }

        public void StartWave(int waveNumber)
        {
            if (_config == null) { Debug.LogError("WaveManager: No WaveConfig assigned."); return; }

            _currentWave = waveNumber;
            _difficulty ??= CreateDefaultDifficulty();

            float duration = WaveScalingCalculator.GetWaveDuration(
                _config.BaseWaveDuration, waveNumber, _difficulty);
            int count = WaveScalingCalculator.GetEnemyCount(
                _config.BaseEnemyCount, waveNumber, _difficulty);

            _waveTimer = duration;
            _waveActive = true;

            // Pick a random enemy type from config weights
            var enemyData = PickEnemyType(waveNumber);
            if (enemyData != null && ServiceLocator.TryGet<EnemySpawner>(out var spawner))
            {
                float hpMult = WaveScalingCalculator.GetEnemyHP(1f, waveNumber, _difficulty);
                float dmgMult = WaveScalingCalculator.GetEnemyDamage(1f, waveNumber, _difficulty);
                spawner.StartSpawning(enemyData, count, duration, hpMult, dmgMult);
            }

            OnWaveStarted?.Invoke(_currentWave);
            Debug.Log($"[WaveManager] Wave {_currentWave} started — {count} enemies over {duration:F0}s");
        }

        private void EndWave()
        {
            _waveActive = false;
            if (ServiceLocator.TryGet<EnemySpawner>(out var spawner))
                spawner.StopSpawning();

            OnWaveEnded?.Invoke(_currentWave);
            Debug.Log($"[WaveManager] Wave {_currentWave} ended.");

            if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                gm.OnWaveEnded(IsFinalWave);
        }

        private Enemies.Data.EnemyDataSO PickEnemyType(int wave)
        {
            if (_config.EnemyWeights == null || _config.EnemyWeights.Count == 0) return null;

            float totalWeight = 0f;
            foreach (var w in _config.EnemyWeights)
                if (wave >= w.MinWave) totalWeight += w.Weight;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var w in _config.EnemyWeights)
            {
                if (wave < w.MinWave) continue;
                cumulative += w.Weight;
                if (roll <= cumulative) return w.EnemyData;
            }
            return _config.EnemyWeights[0].EnemyData;
        }

        private static DifficultySettingsSO CreateDefaultDifficulty()
        {
            var d = ScriptableObject.CreateInstance<DifficultySettingsSO>();
            d.DifficultyName = "Normal";
            return d;
        }
    }
}

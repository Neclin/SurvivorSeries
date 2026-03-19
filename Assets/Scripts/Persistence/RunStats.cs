using System;
using UnityEngine;
using SurvivorSeries.Utilities;
using SurvivorSeries.Waves;
using SurvivorSeries.Player;
using SurvivorSeries.Enemies;
using SurvivorSeries.Enemies.Variants;

namespace SurvivorSeries.Persistence
{
    public class RunStats : MonoBehaviour
    {
        public int Kills { get; private set; }
        public int GoldEarned { get; private set; }
        public float TimeSurvived { get; private set; }
        public int HighestWaveReached { get; private set; }
        public bool BossDefeated { get; private set; }
        public string CharacterName { get; private set; }
        public string DifficultyName { get; private set; }
        public bool RunActive { get; private set; }

        public event Action OnStatsChanged;
        public event Action<bool> OnRunEnded;

        private void Awake() => ServiceLocator.Register<RunStats>(this);
        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
            ServiceLocator.Unregister<RunStats>();
        }

        public void StartRun(string characterName, string difficultyName)
        {
            UnsubscribeFromGameEvents();
            Kills = 0;
            GoldEarned = 0;
            TimeSurvived = 0f;
            HighestWaveReached = 0;
            BossDefeated = false;
            CharacterName = characterName ?? "";
            DifficultyName = difficultyName ?? "";
            RunActive = true;
            SubscribeToGameEvents();
            OnStatsChanged?.Invoke();
        }

        public void EndRun(bool victory)
        {
            if (!RunActive) return;
            RunActive = false;
            UnsubscribeFromGameEvents();

            if (ServiceLocator.TryGet<UnlockRegistry>(out var unlocks))
                unlocks.RecordRunEnd(BossDefeated, HighestWaveReached, Kills, TimeSurvived);

            OnRunEnded?.Invoke(victory);
        }

        private void Update()
        {
            if (!RunActive) return;
            TimeSurvived += Time.deltaTime;
        }

        private void SubscribeToGameEvents()
        {
            EnemyHealth.OnAnyKilled += HandleEnemyKilled;
            BossEnemy.OnBossDefeated += HandleBossDefeated;

            if (ServiceLocator.TryGet<PlayerCurrencyHandler>(out var currency))
                currency.OnCurrencyAdded += HandleCurrencyAdded;
            if (ServiceLocator.TryGet<WaveManager>(out var waves))
                waves.OnWaveStarted += HandleWaveStarted;
            if (ServiceLocator.TryGet<PlayerHealth>(out var health))
                health.OnDeath += HandlePlayerDeath;
        }

        private void UnsubscribeFromGameEvents()
        {
            EnemyHealth.OnAnyKilled -= HandleEnemyKilled;
            BossEnemy.OnBossDefeated -= HandleBossDefeated;

            if (ServiceLocator.TryGet<PlayerCurrencyHandler>(out var currency))
                currency.OnCurrencyAdded -= HandleCurrencyAdded;
            if (ServiceLocator.TryGet<WaveManager>(out var waves))
                waves.OnWaveStarted -= HandleWaveStarted;
            if (ServiceLocator.TryGet<PlayerHealth>(out var health))
                health.OnDeath -= HandlePlayerDeath;
        }

        private void HandleEnemyKilled(EnemyHealth _)
        {
            Kills++;
            OnStatsChanged?.Invoke();
            if (ServiceLocator.TryGet<Achievements.AchievementsManager>(out var am)) am.CheckAll();
        }

        private void HandleCurrencyAdded(int amount)
        {
            GoldEarned += amount;
            OnStatsChanged?.Invoke();
        }

        private void HandleWaveStarted(int wave)
        {
            if (wave > HighestWaveReached) HighestWaveReached = wave;
            OnStatsChanged?.Invoke();
            if (ServiceLocator.TryGet<Achievements.AchievementsManager>(out var am)) am.CheckAll();
        }

        private void HandleBossDefeated()
        {
            BossDefeated = true;
            OnStatsChanged?.Invoke();
            if (ServiceLocator.TryGet<Achievements.AchievementsManager>(out var am)) am.CheckAll();
            EndRun(victory: true);
        }

        private void HandlePlayerDeath()
        {
            EndRun(victory: false);
        }
    }
}

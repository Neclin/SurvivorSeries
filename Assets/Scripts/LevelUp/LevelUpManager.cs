using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SurvivorSeries.Player;
using SurvivorSeries.Weapons.Evolution;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.LevelUp
{
    public class LevelUpManager : MonoBehaviour
    {
        [SerializeField] private WeaponEvolutionRegistry _evolutionRegistry;

        public event Action<List<UpgradeOption>> OnOptionsReady;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            ServiceLocator.Register<LevelUpManager>(this);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            ServiceLocator.Unregister<LevelUpManager>();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<PlayerLevelSystem>(out var levelSystem))
                levelSystem.OnLevelUp += HandleLevelUp;
        }

        private void HandleLevelUp(int newLevel)
        {
            Debug.Log($"[LevelUp] Reached level {newLevel}!");
            if (!ServiceLocator.TryGet<UpgradeOptionGenerator>(out var generator))
            {
                Debug.LogWarning("[LevelUpManager] No UpgradeOptionGenerator registered.");
                ApplyEvolutionCheck();
                if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                    gm.OnLevelUpComplete();
                return;
            }

            float luck = 1f;
            if (ServiceLocator.TryGet<Player.PlayerStats>(out var stats))
                luck = stats.Luck;

            var options = generator.GenerateOptions(3, luck);
            OnOptionsReady?.Invoke(options);

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _ = AutoApplyFirstOption(options, _cts.Token);
        }

        private async Awaitable AutoApplyFirstOption(List<UpgradeOption> options, CancellationToken ct)
        {
            await Awaitable.WaitForSecondsAsync(0.1f, ct);
            if (ct.IsCancellationRequested) return;

            if (options != null && options.Count > 0)
            {
                options[0].Apply();
                Debug.Log($"[LevelUp] Auto-applied: {options[0].Name} â€” {options[0].Description}");
                ApplyEvolutionCheck();
            }
            else
            {
                Debug.Log("[LevelUp] No upgrade options available (all slots full / all maxed).");
            }

            if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                gm.OnLevelUpComplete();
        }

        private void ApplyEvolutionCheck()
        {
            if (_evolutionRegistry != null)
                _evolutionRegistry.CheckAndApplyEvolutions();
        }

        public void ApplyOption(UpgradeOption option)
        {
            option?.Apply();
            ApplyEvolutionCheck();

            if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                gm.OnLevelUpComplete();
        }
    }
}
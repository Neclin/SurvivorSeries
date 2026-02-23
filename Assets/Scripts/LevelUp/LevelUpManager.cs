using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Player;
using SurvivorSeries.Weapons.Evolution;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.LevelUp
{
    public class LevelUpManager : MonoBehaviour
    {
        [SerializeField] private WeaponEvolutionRegistry _evolutionRegistry;

        private void Awake()
        {
            ServiceLocator.Register<LevelUpManager>(this);
        }

        private void OnDestroy()
        {
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

            float luck = 1f;
            if (ServiceLocator.TryGet<PlayerStats>(out var stats))
                luck = stats.Luck;

            List<UpgradeOption> options = new();
            if (ServiceLocator.TryGet<UpgradeOptionGenerator>(out var generator))
                options = generator.GenerateOptions(3, luck);

            if (options.Count > 0)
            {
                options[0].Apply();
                Debug.Log($"[LevelUp] Applied: {options[0].Name}");
                ApplyEvolutionCheck();
            }
        }

        private void ApplyEvolutionCheck()
        {
            if (_evolutionRegistry != null)
                _evolutionRegistry.CheckAndApplyEvolutions();
        }
    }
}

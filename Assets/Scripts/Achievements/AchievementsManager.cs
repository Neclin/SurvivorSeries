using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Achievements.Data;
using SurvivorSeries.Persistence;
using SurvivorSeries.Utilities;
using SurvivorSeries.Weapons;

namespace SurvivorSeries.Achievements
{
    public class AchievementsManager : MonoBehaviour
    {
        [SerializeField] private List<AchievementDefinitionSO> _definitions = new();

        public event Action<AchievementDefinitionSO> OnAchievementUnlocked;

        public IReadOnlyList<AchievementDefinitionSO> Definitions => _definitions;

        private void Awake() => ServiceLocator.Register<AchievementsManager>(this);
        private void OnDestroy() => ServiceLocator.Unregister<AchievementsManager>();

        public void CheckAll()
        {
            if (_definitions == null) return;
            if (!ServiceLocator.TryGet<UnlockRegistry>(out var unlocks)) return;
            if (!ServiceLocator.TryGet<RunStats>(out var stats)) return;

            foreach (var def in _definitions)
            {
                if (def == null) continue;
                if (unlocks.IsAchievementCompleted(def.AchievementID)) continue;
                if (def.RequiredCharacter != null && def.RequiredCharacter.CharacterName != stats.CharacterName) continue;
                if (!IsConditionMet(def, stats)) continue;

                unlocks.CompleteAchievement(def.AchievementID);
                if (def.UnlocksCharacter != null)
                    unlocks.UnlockCharacter(def.UnlocksCharacter.CharacterName);

                Debug.Log($"[Achievements] Unlocked: {def.Title}");
                OnAchievementUnlocked?.Invoke(def);
            }
        }

        private bool IsConditionMet(AchievementDefinitionSO def, RunStats stats)
        {
            switch (def.ConditionType)
            {
                case AchievementConditionType.ReachWave:
                    return stats.HighestWaveReached >= def.TargetValue;
                case AchievementConditionType.SurviveMinutes:
                    return stats.TimeSurvived >= def.TargetValue * 60f;
                case AchievementConditionType.KillCount:
                    return stats.Kills >= def.TargetValue;
                case AchievementConditionType.DefeatBoss:
                    return stats.BossDefeated;
                case AchievementConditionType.FillWeaponSlots:
                    if (!ServiceLocator.TryGet<WeaponSlotManager>(out var wsm)) return false;
                    int filled = 0;
                    foreach (var w in wsm.GetAllWeapons()) if (w != null) filled++;
                    return filled >= def.TargetValue;
                default:
                    return false;
            }
        }
    }
}

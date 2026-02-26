using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Persistence
{
    /// <summary>
    /// Loads save data on startup and provides runtime unlock checks.
    /// Lives in the Bootstrap scene.
    /// </summary>
    public class UnlockRegistry : MonoBehaviour
    {
        private SaveData _data;

        private void Awake()
        {
            _data = SaveSystem.Load();
            ServiceLocator.Register<UnlockRegistry>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<UnlockRegistry>();
        }

        public bool IsCharacterUnlocked(string characterName)
        {
            return _data.UnlockedCharacterIDs.Contains(characterName);
        }

        public bool IsAchievementCompleted(string achievementID)
        {
            return _data.CompletedAchievementIDs.Contains(achievementID);
        }

        public void UnlockCharacter(string characterName)
        {
            if (_data.UnlockedCharacterIDs.Contains(characterName)) return;
            _data.UnlockedCharacterIDs.Add(characterName);
            SaveNow();
        }

        public void CompleteAchievement(string achievementID)
        {
            if (_data.CompletedAchievementIDs.Contains(achievementID)) return;
            _data.CompletedAchievementIDs.Add(achievementID);
            SaveNow();
        }

        public void RecordRunEnd(bool bossDefeated, int waveReached, int kills, float playtimeSeconds)
        {
            _data.TotalRuns++;
            _data.TotalKills += kills;
            _data.TotalPlaytimeSeconds += playtimeSeconds;
            if (waveReached > _data.HighestWaveReached) _data.HighestWaveReached = waveReached;
            if (bossDefeated) _data.BossDefeated = true;
            SaveNow();
        }

        public SaveData GetData() => _data;

        public void SaveNow() => SaveSystem.Save(_data);
    }
}

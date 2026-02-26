using System;
using System.Collections.Generic;

namespace SurvivorSeries.Persistence
{
    [Serializable]
    public class SaveData
    {
        public List<string> UnlockedCharacterIDs = new();
        public List<string> CompletedAchievementIDs = new();
        public int TotalRuns;
        public int TotalKills;
        public int HighestWaveReached;
        public bool BossDefeated;
        public float TotalPlaytimeSeconds;
        public string LastSelectedCharacterID;
        public string LastSelectedDifficultyName;
    }
}

using UnityEngine;

namespace SurvivorSeries.Achievements.Data
{
    public enum AchievementConditionType
    {
        ReachWave,
        SurviveMinutes,
        KillCount,
        DefeatBoss,
        FillWeaponSlots
    }

    [CreateAssetMenu(menuName = "SurvivorSeries/Achievement", fileName = "SO_Achievement_")]
    public class AchievementDefinitionSO : ScriptableObject
    {
        public string AchievementID;
        public string Title;
        [TextArea] public string Description;

        public AchievementConditionType ConditionType;
        public float TargetValue;

        [Tooltip("Null means any character qualifies")]
        public Characters.Data.CharacterDefinitionSO RequiredCharacter;

        [Tooltip("Null if this achievement gives no character unlock")]
        public Characters.Data.CharacterDefinitionSO UnlocksCharacter;
    }
}

using UnityEngine;

namespace SurvivorSeries.Characters.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Character Definition", fileName = "SO_Character_")]
    public class CharacterDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string CharacterName;
        [TextArea] public string Description;
        public Sprite Portrait;
        public GameObject Prefab;

        [Header("Base Stats")]
        public float BaseMaxHealth = 100f;
        public float BaseMoveSpeed = 5f;
        public float BaseDamage = 10f;
        public float BaseArea = 1f;
        public float BaseCooldownReduction = 0f;
        public float BaseLuck = 1f;

        [Header("Starting Weapon")]
        public Weapons.Data.WeaponDataSO StartingWeapon;

        [Header("Unlock")]
        public bool IsUnlockedByDefault = false;
        public Achievements.Data.AchievementDefinitionSO UnlockAchievement;
    }
}

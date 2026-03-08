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
        public float BaseMaxHealth = 60f;
        public float BaseMoveSpeed = 5f;
        public float BaseDamage = 8f;
        public float BaseArea = 1f;
        public float BaseCooldownReduction = 0f;
        public float BaseLuck = 1f;

        [Header("Growth Multipliers (per stat passives are scaled by these)")]
        public float HealthGrowth = 1f;
        public float MoveSpeedGrowth = 1f;
        public float DamageGrowth = 1f;
        public float AreaGrowth = 1f;
        public float ProjectileSpeedGrowth = 1f;
        public float CooldownGrowth = 1f;
        public float LuckGrowth = 1f;
        public float RegenGrowth = 1f;
        public float ArmorGrowth = 1f;

        [Header("Starting Weapon")]
        public Weapons.Data.WeaponDataSO StartingWeapon;

        [Header("Unlock")]
        public bool IsUnlockedByDefault = false;
        public Achievements.Data.AchievementDefinitionSO UnlockAchievement;
    }
}

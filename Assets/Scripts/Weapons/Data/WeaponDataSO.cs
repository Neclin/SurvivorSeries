using UnityEngine;

namespace SurvivorSeries.Weapons.Data
{
    public enum WeaponType { Projectile, Melee, Aura, Thrown }

    [CreateAssetMenu(menuName = "SurvivorSeries/Weapon", fileName = "SO_Weapon_")]
    public class WeaponDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string WeaponName;
        public Sprite Icon;
        public WeaponType Type;

        [Header("Prefabs")]
        public GameObject WeaponPrefab;
        public GameObject ProjectilePrefab;

        [Header("Levels (8 entries each)")]
        public float[] BaseDamagePerLevel = new float[8];
        public float[] CooldownPerLevel = new float[8];
        public float[] AreaPerLevel = new float[8];
        public int[] ProjectileCountPerLevel = new int[8];
        public int MaxLevel = 8;

        [Header("Evolution")]
        public Passives.Data.PassiveItemDataSO EvolutionRequirement;
        public EvolvedWeaponDataSO EvolvedForm;

        [Header("Shop")]
        public int ShopPurchaseCost = 100;
    }
}

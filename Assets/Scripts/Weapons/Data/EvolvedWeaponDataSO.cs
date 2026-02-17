using UnityEngine;

namespace SurvivorSeries.Weapons.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Evolved Weapon", fileName = "SO_EvolvedWeapon_")]
    public class EvolvedWeaponDataSO : ScriptableObject
    {
        public string WeaponName;
        public Sprite Icon;
        public GameObject WeaponPrefab;
        public WeaponType Type;

        [Header("Stats (single fixed level)")]
        public float Damage;
        public float Cooldown;
        public float Area;
        public int ProjectileCount;
    }
}

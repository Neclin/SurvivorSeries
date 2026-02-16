using UnityEngine;

namespace SurvivorSeries.Enemies.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Enemy", fileName = "SO_Enemy_")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string EnemyName;
        public GameObject Prefab;

        [Header("Base Stats")]
        public float BaseHealth = 20f;
        public float BaseDamage = 5f;
        public float MoveSpeed = 3f;
        public float ContactDamageInterval = 0.5f; // seconds between contact damage ticks

        [Header("Drops")]
        public float XPDropAmount = 5f;
        public int CurrencyDropAmount = 5;
        [Range(0f, 1f)] public float CurrencyDropChance = 0.3f;
    }
}

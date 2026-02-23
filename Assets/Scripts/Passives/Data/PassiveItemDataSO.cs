using UnityEngine;
using SurvivorSeries.Player;

namespace SurvivorSeries.Passives.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Passive Item", fileName = "SO_Passive_")]
    public class PassiveItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string ItemName;
        public Sprite Icon;

        [Header("Effect")]
        public StatType AffectedStat;
        [Tooltip("Flat bonus added per level")]
        public float FlatBonusPerLevel;
        [Tooltip("Percent bonus added per level (0.1 = +10%)")]
        public float PercentBonusPerLevel;
        public int MaxLevel = 8;

        [Header("Shop")]
        public int ShopPurchaseCost = 80;
    }
}

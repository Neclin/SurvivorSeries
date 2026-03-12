using System;
using SurvivorSeries.Weapons.Data;
using SurvivorSeries.Passives.Data;

namespace SurvivorSeries.Shop
{
    public enum ShopItemType
    {
        WeaponNew,
        WeaponCombine,
        PassiveNew,
        PassiveLevelUp,
        Heal,
        Chest
    }

    public class ShopItem
    {
        public ShopItemType Type;
        public string Name;
        public string Description;
        public int Cost;
        public WeaponDataSO WeaponData;
        public PassiveItemDataSO PassiveData;
        public int CombineLevel;
        public Action OnPurchase;
    }
}
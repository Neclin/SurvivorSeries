using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Weapons;
using SurvivorSeries.Weapons.Data;
using SurvivorSeries.Passives;
using SurvivorSeries.Passives.Data;
using SurvivorSeries.Player;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Shop
{
    public class ShopInventoryGenerator : MonoBehaviour
    {
        [SerializeField] private WeaponDataSO[] _allWeapons;
        [SerializeField] private PassiveItemDataSO[] _allPassives;
        [SerializeField] private int _healCost = 40;
        [SerializeField] private int _chestCost = 120;

        private void Awake() => ServiceLocator.Register<ShopInventoryGenerator>(this);
        private void OnDestroy() => ServiceLocator.Unregister<ShopInventoryGenerator>();

        public List<ShopItem> Generate(float difficultyMultiplier = 1f)
        {
            ServiceLocator.TryGet<WeaponSlotManager>(out var wsm);
            ServiceLocator.TryGet<PassiveSlotManager>(out var psm);
            ServiceLocator.TryGet<PlayerHealth>(out var health);

            var pool = new List<ShopItem>();

            if (wsm != null)
                foreach (var w in _allWeapons)
                    if (w != null && wsm.CanBuyNew(w))
                        pool.Add(MakeWeaponNew(w, difficultyMultiplier));

            if (psm != null && psm.HasFreeSlot())
                foreach (var p in _allPassives)
                    if (!psm.HasPassive(p))
                        pool.Add(MakePassiveNew(p, difficultyMultiplier));

            if (psm != null)
                foreach (var p in _allPassives)
                    if (psm.HasPassive(p) && !psm.HasMaxLevel(p))
                        pool.Add(MakePassiveLevelUp(p, difficultyMultiplier));

            if (health != null && health.CurrentHealth < health.MaxHealth)
                pool.Add(MakeHealItem(difficultyMultiplier));

            Shuffle(pool);

            var items = new List<ShopItem>();
            int n = Mathf.Min(BuySlotCount, pool.Count);
            for (int i = 0; i < n; i++) items.Add(pool[i]);

            while (items.Count < BuySlotCount)
                items.Add(MakeChestItem(difficultyMultiplier));

            return items;
        }

        public const int BuySlotCount = 4;

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private ShopItem MakeWeaponNew(WeaponDataSO data, float mult)
        {
            int cost = Mathf.RoundToInt(data.ShopPurchaseCost * mult);
            string desc = !string.IsNullOrEmpty(data.Description)
                ? data.Description
                : "Lv. 1 weapon. Combine two of the same level to upgrade.";
            var item = new ShopItem
            {
                Type = ShopItemType.WeaponNew,
                Name = data.WeaponName,
                Description = desc,
                Cost = cost,
                WeaponData = data
            };
            item.OnPurchase = () =>
            {
                if (ServiceLocator.TryGet<WeaponSlotManager>(out var w)) w.TryBuyNew(data);
            };
            return item;
        }

        private ShopItem MakeWeaponCombine(WeaponDataSO data, int level)
        {
            var item = new ShopItem
            {
                Type = ShopItemType.WeaponCombine,
                Name = $"Combine {data.WeaponName} Lv.{level}",
                Description = $"Merge two Lv.{level} {data.WeaponName} into one Lv.{level + 1}.",
                Cost = 0,
                WeaponData = data,
                CombineLevel = level
            };
            item.OnPurchase = () =>
            {
                if (ServiceLocator.TryGet<WeaponSlotManager>(out var w)) w.TryCombine(data, level);
            };
            return item;
        }

        private ShopItem MakePassiveNew(PassiveItemDataSO data, float mult)
        {
            int cost = Mathf.RoundToInt(data.ShopPurchaseCost * mult);
            var item = new ShopItem
            {
                Type = ShopItemType.PassiveNew,
                Name = data.ItemName,
                Description = data.GetDescription(),
                Cost = cost,
                PassiveData = data
            };
            item.OnPurchase = () =>
            {
                if (ServiceLocator.TryGet<PassiveSlotManager>(out var p)) p.TryAddPassive(data);
            };
            return item;
        }

        private ShopItem MakePassiveLevelUp(PassiveItemDataSO data, float mult)
        {
            int cost = Mathf.RoundToInt(data.ShopPurchaseCost * 0.7f * mult);
            var item = new ShopItem
            {
                Type = ShopItemType.PassiveLevelUp,
                Name = $"{data.ItemName} +1",
                Description = data.GetDescription(),
                Cost = cost,
                PassiveData = data
            };
            item.OnPurchase = () =>
            {
                if (ServiceLocator.TryGet<PassiveSlotManager>(out var p)) p.TryLevelUpPassive(data);
            };
            return item;
        }

        private ShopItem MakeHealItem(float mult)
        {
            int cost = Mathf.RoundToInt(_healCost * mult);
            return new ShopItem
            {
                Type = ShopItemType.Heal,
                Name = "Potion",
                Description = "Restore 30% of max HP.",
                Cost = cost,
                OnPurchase = () =>
                {
                    if (ServiceLocator.TryGet<PlayerHealth>(out var h)) h.HealPercent(0.3f);
                }
            };
        }

        private ShopItem MakeChestItem(float mult)
        {
            int cost = Mathf.RoundToInt(_chestCost * mult);
            return new ShopItem
            {
                Type = ShopItemType.Chest,
                Name = "Mystery Chest",
                Description = "Randomly grants a weapon or passive.",
                Cost = cost,
                OnPurchase = ApplyChest
            };
        }

        private void ApplyChest()
        {
            ServiceLocator.TryGet<WeaponSlotManager>(out var wsm);
            ServiceLocator.TryGet<PassiveSlotManager>(out var psm);
            var options = new List<Action>();

            if (wsm != null && wsm.HasFreeSlot())
                foreach (var w in _allWeapons)
                    if (!wsm.HasWeapon(w))
                    {
                        var captured = w;
                        options.Add(() => wsm.TryAddWeapon(captured));
                    }

            if (psm != null && psm.HasFreeSlot())
                foreach (var p in _allPassives)
                    if (!psm.HasPassive(p))
                    {
                        var captured = p;
                        options.Add(() => psm.TryAddPassive(captured));
                    }

            if (options.Count > 0)
                options[UnityEngine.Random.Range(0, options.Count)]();
            else
                Debug.Log("[Shop] Chest: nothing to grant (all slots full).");
        }
    }
}
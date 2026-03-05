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

            var candidates = BuildCandidates(wsm, psm, health, difficultyMultiplier);

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            var items = new List<ShopItem>();
            for (int i = 0; i < Mathf.Min(6, candidates.Count); i++)
                items.Add(candidates[i]);

            while (items.Count < 6)
                items.Add(MakeChestItem(difficultyMultiplier));

            return items;
        }

        private List<ShopItem> BuildCandidates(WeaponSlotManager wsm, PassiveSlotManager psm,
                                                PlayerHealth health, float mult)
        {
            var list = new List<ShopItem>();

            if (wsm != null && wsm.HasFreeSlot())
                foreach (var w in _allWeapons)
                    if (!wsm.HasWeapon(w))
                        list.Add(MakeWeaponNew(w, mult));

            if (wsm != null)
                foreach (var w in _allWeapons)
                    if (wsm.HasWeapon(w) && !wsm.IsMaxLevel(w))
                        list.Add(MakeWeaponLevelUp(w, mult));

            if (psm != null && psm.HasFreeSlot())
                foreach (var p in _allPassives)
                    if (!psm.HasPassive(p))
                        list.Add(MakePassiveNew(p, mult));

            if (psm != null)
                foreach (var p in _allPassives)
                    if (psm.HasPassive(p) && !psm.HasMaxLevel(p))
                        list.Add(MakePassiveLevelUp(p, mult));

            if (health != null && health.CurrentHealth < health.MaxHealth)
                list.Add(MakeHealItem(mult));

            return list;
        }

        private ShopItem MakeWeaponNew(WeaponDataSO data, float mult)
        {
            int cost = Mathf.RoundToInt(data.ShopPurchaseCost * mult);
            var item = new ShopItem
            {
                Type = ShopItemType.WeaponNew,
                Name = data.WeaponName,
                Description = $"Add {data.WeaponName} to your arsenal.",
                Cost = cost,
                WeaponData = data
            };
            item.OnPurchase = () =>
            {
                if (ServiceLocator.TryGet<WeaponSlotManager>(out var w)) w.TryAddWeapon(data);
            };
            return item;
        }

        private ShopItem MakeWeaponLevelUp(WeaponDataSO data, float mult)
        {
            int cost = Mathf.RoundToInt(data.ShopPurchaseCost * 0.7f * mult);
            var item = new ShopItem
            {
                Type = ShopItemType.WeaponLevelUp,
                Name = $"{data.WeaponName} +1",
                Description = $"Upgrade {data.WeaponName} to the next level.",
                Cost = cost,
                WeaponData = data
            };
            item.OnPurchase = () =>
            {
                if (ServiceLocator.TryGet<WeaponSlotManager>(out var w)) w.TryLevelUpWeapon(data);
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
                Description = $"Gain the {data.ItemName} passive item.",
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
                Description = $"Level up {data.ItemName}.",
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
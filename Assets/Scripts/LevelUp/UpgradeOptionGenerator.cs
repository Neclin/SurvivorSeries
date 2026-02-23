using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Weapons;
using SurvivorSeries.Weapons.Data;
using SurvivorSeries.Passives;
using SurvivorSeries.Passives.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.LevelUp
{
    public class UpgradeOptionGenerator : MonoBehaviour
    {
        [SerializeField] private List<WeaponDataSO> _allWeapons = new();
        [SerializeField] private List<PassiveItemDataSO> _allPassives = new();

        private void Awake()
        {
            ServiceLocator.Register<UpgradeOptionGenerator>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<UpgradeOptionGenerator>();
        }

        /// <summary>
        /// Generates upgrade options weighted by type, filtered against current slot states.
        /// Higher luckMultiplier re-rolls duplicate picks once, favouring higher-weight options.
        /// </summary>
        public List<UpgradeOption> GenerateOptions(int count, float luckMultiplier)
        {
            bool hasWeaponSlot = ServiceLocator.TryGet<WeaponSlotManager>(out var wsm);
            bool hasPassiveSlot = ServiceLocator.TryGet<PassiveSlotManager>(out var psm);

            var pool = new List<(UpgradeOption option, int weight)>();

            // ── Weapon options ────────────────────────────────────────────────
            if (hasWeaponSlot)
            {
                bool weaponSlotFree = false;
                foreach (var slot in wsm.GetAllWeapons())
                {
                    if (slot == null) { weaponSlotFree = true; break; }
                }

                foreach (var weapon in _allWeapons)
                {
                    if (weapon == null) continue;

                    if (wsm.HasWeapon(weapon))
                    {
                        // Level-up option for owned, non-max weapons (weight 3)
                        bool isMax = false;
                        foreach (var w in wsm.GetAllWeapons())
                        {
                            if (w != null && w.Data == weapon && w.IsMaxLevel) { isMax = true; break; }
                        }
                        if (!isMax)
                        {
                            pool.Add((new UpgradeOption
                            {
                                Name = $"{weapon.WeaponName} (Level Up)",
                                Description = $"Upgrade your {weapon.WeaponName} to the next level.",
                                Type = UpgradeType.WeaponLevelUp,
                                WeaponData = weapon
                            }, 3));
                        }
                    }
                    else if (weaponSlotFree)
                    {
                        // New weapon for empty slots (weight 2)
                        pool.Add((new UpgradeOption
                        {
                            Name = weapon.WeaponName,
                            Description = $"Equip the {weapon.WeaponName}.",
                            Type = UpgradeType.WeaponNew,
                            WeaponData = weapon
                        }, 2));
                    }
                }
            }

            // ── Passive options ───────────────────────────────────────────────
            if (hasPassiveSlot)
            {
                bool passiveSlotFree = psm.HasFreeSlot();

                foreach (var passive in _allPassives)
                {
                    if (passive == null) continue;
                    if (psm.HasMaxLevel(passive)) continue;

                    if (psm.HasPassive(passive))
                    {
                        // Level-up option for owned, non-max passives (weight 3)
                        pool.Add((new UpgradeOption
                        {
                            Name = $"{passive.ItemName} (Level Up)",
                            Description = $"Increase {passive.ItemName} to the next level.",
                            Type = UpgradeType.PassiveLevelUp,
                            PassiveData = passive
                        }, 3));
                    }
                    else if (passiveSlotFree)
                    {
                        // New passive for empty slots (weight 2)
                        pool.Add((new UpgradeOption
                        {
                            Name = passive.ItemName,
                            Description = $"Gain the {passive.ItemName} passive item.",
                            Type = UpgradeType.PassiveNew,
                            PassiveData = passive
                        }, 2));
                    }
                }
            }

            if (pool.Count == 0) return new List<UpgradeOption>();

            // If pool has fewer options than requested, return all available
            if (pool.Count <= count)
            {
                var all = new List<UpgradeOption>(pool.Count);
                foreach (var (opt, _) in pool) all.Add(opt);
                return all;
            }

            // Weighted random selection without replacement
            var result = new List<UpgradeOption>(count);
            var remaining = new List<(UpgradeOption option, int weight)>(pool);
            bool useLuckReroll = luckMultiplier > 1f;

            for (int i = 0; i < count && remaining.Count > 0; i++)
            {
                int totalWeight = 0;
                foreach (var (_, w) in remaining) totalWeight += w;

                int roll = Random.Range(0, totalWeight);
                int pickedIndex = PickWeightedIndex(remaining, roll);

                if (useLuckReroll)
                {
                    // Re-roll once and take the higher-weight result
                    int roll2 = Random.Range(0, totalWeight);
                    int pickedIndex2 = PickWeightedIndex(remaining, roll2);
                    if (remaining[pickedIndex2].weight > remaining[pickedIndex].weight)
                        pickedIndex = pickedIndex2;
                }

                result.Add(remaining[pickedIndex].option);
                remaining.RemoveAt(pickedIndex);
            }

            return result;
        }

        private static int PickWeightedIndex(List<(UpgradeOption option, int weight)> pool, int roll)
        {
            int cumulative = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                cumulative += pool[i].weight;
                if (roll < cumulative) return i;
            }
            return pool.Count - 1;
        }
    }
}

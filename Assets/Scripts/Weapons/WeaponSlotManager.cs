using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Weapons.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Weapons
{
    public class WeaponSlotManager : MonoBehaviour
    {
        public const int MaxSlots = 6;
        private readonly WeaponBase[] _slots = new WeaponBase[MaxSlots];
        private Player.PlayerStats _stats;

        public event System.Action OnInventoryChanged;
        private void RaiseChanged() => OnInventoryChanged?.Invoke();

        private void Awake()
        {
            _stats = GetComponent<Player.PlayerStats>();
            ServiceLocator.Register<WeaponSlotManager>(this);
        }

        private void OnDestroy() => ServiceLocator.Unregister<WeaponSlotManager>();

        public WeaponBase[] GetAllWeapons() => _slots;

        public bool HasFreeSlot()
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] == null) return true;
            return false;
        }

        public bool HasWeapon(WeaponDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] != null && _slots[i].Data == data) return true;
            return false;
        }

        public int CountAt(WeaponDataSO data, int level)
        {
            int n = 0;
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] != null && _slots[i].Data == data && _slots[i].Level == level) n++;
            return n;
        }

        public List<int> GetCombinableLevels(WeaponDataSO data)
        {
            var levels = new Dictionary<int, int>();
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] == null || _slots[i].Data != data) continue;
                if (_slots[i].IsMaxLevel) continue;
                int lv = _slots[i].Level;
                levels.TryGetValue(lv, out var c);
                levels[lv] = c + 1;
            }
            var result = new List<int>();
            foreach (var kv in levels)
                if (kv.Value >= 2) result.Add(kv.Key);
            result.Sort();
            return result;
        }

        public bool CanBuyNew(WeaponDataSO data)
        {
            if (HasFreeSlot()) return true;
            if (CountAt(data, 1) >= 1) return true;
            return false;
        }

        public bool TryBuyNew(WeaponDataSO data)
        {
            if (data == null || data.WeaponPrefab == null) return false;

            if (HasFreeSlot())
            {
                bool ok = SpawnAtFreeSlot(data, level: 1);
                if (ok) RaiseChanged();
                return ok;
            }

            if (CountAt(data, 1) >= 1)
            {
                int idx = FindSlotAt(data, level: 1);
                if (idx < 0) return false;
                var existing = _slots[idx];
                if (existing.IsMaxLevel) return false;
                existing.LevelUp();
                RaiseChanged();
                return true;
            }

            return false;
        }

        public bool TryCombine(WeaponDataSO data, int level)
        {
            int firstIdx = -1, secondIdx = -1;
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] == null || _slots[i].Data != data || _slots[i].Level != level) continue;
                if (firstIdx < 0) firstIdx = i;
                else { secondIdx = i; break; }
            }
            if (firstIdx < 0 || secondIdx < 0) return false;
            if (_slots[firstIdx].IsMaxLevel) return false;

            DestroyAt(secondIdx);
            DestroyAt(firstIdx);
            bool ok = SpawnAtIndex(firstIdx, data, level: level + 1);
            if (ok) RaiseChanged();
            return ok;
        }

        public bool TryAddWeapon(WeaponDataSO data) => TryBuyNew(data);

        public bool TryLevelUpWeapon(WeaponDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] != null && _slots[i].Data == data && !_slots[i].IsMaxLevel)
                {
                    _slots[i].LevelUp();
                    return true;
                }
            }
            return false;
        }

        public bool IsMaxLevel(WeaponDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] != null && _slots[i].Data == data && _slots[i].IsMaxLevel) return true;
            return false;
        }

        public void ApplyEvolution(WeaponBase weapon, EvolvedWeaponDataSO evolved)
        {
            if (weapon == null || evolved == null) return;
            if (evolved.WeaponPrefab == null)
            {
                Debug.LogWarning($"[WeaponSlotManager] EvolvedWeaponDataSO '{evolved.WeaponName}' has no WeaponPrefab.");
                return;
            }

            int slotIndex = -1;
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] == weapon) { slotIndex = i; break; }
            if (slotIndex < 0)
            {
                Debug.LogWarning("[WeaponSlotManager] ApplyEvolution: weapon not found in any slot.");
                return;
            }

            DestroyAt(slotIndex);

            var go = Instantiate(evolved.WeaponPrefab, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var newWeapon = go.GetComponent<WeaponBase>();
            if (newWeapon == null) { Destroy(go); return; }

            newWeapon.SlotIndex = slotIndex;
            newWeapon.TotalSlots = MaxSlots;
            newWeapon.InitializeEvolved(evolved, _stats);
            _slots[slotIndex] = newWeapon;
        }

        private bool SpawnAtFreeSlot(WeaponDataSO data, int level)
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] == null) return SpawnAtIndex(i, data, level);
            return false;
        }

        private bool SpawnAtIndex(int slotIndex, WeaponDataSO data, int level)
        {
            if (data == null || data.WeaponPrefab == null) return false;

            var go = Instantiate(data.WeaponPrefab, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var weapon = go.GetComponent<WeaponBase>();
            if (weapon == null) { Destroy(go); return false; }

            weapon.SlotIndex = slotIndex;
            weapon.TotalSlots = MaxSlots;
            weapon.Initialize(data, _stats);
            weapon.SetLevel(level);
            _slots[slotIndex] = weapon;
            return true;
        }

        private void DestroyAt(int idx)
        {
            if (_slots[idx] == null) return;
            Destroy(_slots[idx].gameObject);
            _slots[idx] = null;
        }

        private int FindSlotAt(WeaponDataSO data, int level)
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] != null && _slots[i].Data == data && _slots[i].Level == level) return i;
            return -1;
        }
    }
}

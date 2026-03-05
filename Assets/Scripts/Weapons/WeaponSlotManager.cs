using UnityEngine;
using SurvivorSeries.Weapons.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Weapons
{
    public class WeaponSlotManager : MonoBehaviour
    {
        private const int MaxSlots = 6;
        private readonly WeaponBase[] _slots = new WeaponBase[MaxSlots];
        private Player.PlayerStats _stats;

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
            {
                if (_slots[i] == weapon) { slotIndex = i; break; }
            }
            if (slotIndex < 0)
            {
                Debug.LogWarning("[WeaponSlotManager] ApplyEvolution: weapon not found in any slot.");
                return;
            }

            Destroy(weapon.gameObject);
            _slots[slotIndex] = null;

            GameObject go = Instantiate(evolved.WeaponPrefab, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            WeaponBase newWeapon = go.GetComponent<WeaponBase>();
            if (newWeapon == null)
            {
                Debug.LogWarning($"[WeaponSlotManager] EvolvedWeaponDataSO '{evolved.WeaponName}' prefab has no WeaponBase.");
                Destroy(go);
                return;
            }

            newWeapon.SlotIndex = slotIndex;
            newWeapon.TotalSlots = MaxSlots;
            newWeapon.InitializeEvolved(evolved, _stats);
            _slots[slotIndex] = newWeapon;
            Debug.Log($"[WeaponSlotManager] Evolved weapon applied: {evolved.WeaponName}");
        }

        private void Awake()
        {
            _stats = GetComponent<Player.PlayerStats>();
            ServiceLocator.Register<WeaponSlotManager>(this);
        }

        private void OnDestroy() => ServiceLocator.Unregister<WeaponSlotManager>();

        public bool TryAddWeapon(WeaponDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] != null && _slots[i].Data == data)
                    return TryLevelUpWeapon(data);
            }

            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] != null) continue;

                if (data.WeaponPrefab == null)
                {
                    Debug.LogWarning($"WeaponDataSO '{data.WeaponName}' has no WeaponPrefab assigned.");
                    return false;
                }

                GameObject go = Instantiate(data.WeaponPrefab, transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                WeaponBase weapon = go.GetComponent<WeaponBase>();
                if (weapon == null)
                {
                    Debug.LogWarning($"WeaponPrefab for '{data.WeaponName}' has no WeaponBase component.");
                    Destroy(go);
                    return false;
                }

                weapon.SlotIndex = i;
                weapon.TotalSlots = MaxSlots;
                weapon.Initialize(data, _stats);
                _slots[i] = weapon;
                return true;
            }

            return false;
        }

        public bool TryLevelUpWeapon(WeaponDataSO data)
        {
            foreach (var weapon in _slots)
            {
                if (weapon != null && weapon.Data == data)
                {
                    weapon.LevelUp();
                    return true;
                }
            }
            return false;
        }

        public bool HasWeapon(WeaponDataSO data)
        {
            foreach (var w in _slots)
                if (w != null && w.Data == data) return true;
            return false;
        }

        public WeaponBase[] GetAllWeapons() => _slots;

        public bool HasFreeSlot()
        {
            foreach (var w in _slots)
                if (w == null) return true;
            return false;
        }

        public bool IsMaxLevel(WeaponDataSO data)
        {
            foreach (var w in _slots)
                if (w != null && w.Data == data) return w.IsMaxLevel;
            return false;
        }
    }
}
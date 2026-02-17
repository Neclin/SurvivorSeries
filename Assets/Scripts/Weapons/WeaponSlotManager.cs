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

        /// <summary>
        /// Destroys the given weapon's GameObject, instantiates the evolved weapon prefab
        /// in its place, and wires it up to the same slot index.
        /// </summary>
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

            // Destroy old weapon GameObject
            Destroy(weapon.gameObject);
            _slots[slotIndex] = null;

            // Instantiate evolved weapon
            GameObject go = Instantiate(evolved.WeaponPrefab, transform);
            WeaponBase newWeapon = go.GetComponent<WeaponBase>();
            if (newWeapon == null)
            {
                Debug.LogWarning($"[WeaponSlotManager] EvolvedWeaponDataSO '{evolved.WeaponName}' prefab has no WeaponBase.");
                Destroy(go);
                return;
            }

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
            // If already has this weapon, level it up instead
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] != null && _slots[i].Data == data)
                    return TryLevelUpWeapon(data);
            }

            // Find empty slot
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] != null) continue;

                if (data.WeaponPrefab == null)
                {
                    Debug.LogWarning($"WeaponDataSO '{data.WeaponName}' has no WeaponPrefab assigned.");
                    return false;
                }

                GameObject go = Instantiate(data.WeaponPrefab, transform);
                WeaponBase weapon = go.GetComponent<WeaponBase>();
                if (weapon == null)
                {
                    Debug.LogWarning($"WeaponPrefab for '{data.WeaponName}' has no WeaponBase component.");
                    Destroy(go);
                    return false;
                }

                weapon.Initialize(data, _stats);
                _slots[i] = weapon;
                return true;
            }

            return false; // all slots full
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

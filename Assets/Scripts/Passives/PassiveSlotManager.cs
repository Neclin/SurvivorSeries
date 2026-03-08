using UnityEngine;
using SurvivorSeries.Passives.Data;
using SurvivorSeries.Player;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Passives
{
    public class PassiveSlotManager : MonoBehaviour
    {
        private const int MaxSlots = 6;
        private readonly PassiveItemDataSO[] _slots = new PassiveItemDataSO[MaxSlots];
        private readonly int[] _levels = new int[MaxSlots];

        private void Awake() => ServiceLocator.Register<PassiveSlotManager>(this);
        private void OnDestroy() => ServiceLocator.Unregister<PassiveSlotManager>();

        public bool TryAddPassive(PassiveItemDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] == null) { _slots[i] = data; _levels[i] = 1; ApplyModifier(data, 1); return true; }
                if (_slots[i] == data) return TryLevelUpPassive(data);
            }
            return false;
        }

        public bool TryLevelUpPassive(PassiveItemDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] != data) continue;
                if (_levels[i] >= data.MaxLevel) return false;
                _levels[i]++;
                ApplyModifier(data, 1);
                return true;
            }
            return false;
        }

        public bool HasPassive(PassiveItemDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] == data) return true;
            return false;
        }

        public bool HasMaxLevel(PassiveItemDataSO data)
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] == data && _levels[i] >= data.MaxLevel) return true;
            return false;
        }

        public bool HasFreeSlot()
        {
            for (int i = 0; i < MaxSlots; i++)
                if (_slots[i] == null) return true;
            return false;
        }

        private void ApplyModifier(PassiveItemDataSO data, int levels)
        {
            if (!ServiceLocator.TryGet<PlayerStats>(out var stats)) return;
            if (data.ModifiersPerLevel == null) return;
            for (int m = 0; m < data.ModifiersPerLevel.Count; m++)
            {
                var src = data.ModifiersPerLevel[m];
                stats.ApplyModifier(new StatModifier
                {
                    Stat = src.Stat,
                    FlatBonus = src.FlatBonus * levels,
                    PercentBonus = src.PercentBonus * levels
                });
            }
        }
    }
}
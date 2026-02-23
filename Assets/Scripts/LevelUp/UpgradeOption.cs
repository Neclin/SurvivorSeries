using SurvivorSeries.Utilities;
using SurvivorSeries.Weapons;
using SurvivorSeries.Passives;

namespace SurvivorSeries.LevelUp
{
    public enum UpgradeType
    {
        WeaponNew,
        WeaponLevelUp,
        PassiveNew,
        PassiveLevelUp
    }

    public class UpgradeOption
    {
        public string Name;
        public string Description;
        public UpgradeType Type;
        public Weapons.Data.WeaponDataSO WeaponData;
        public Passives.Data.PassiveItemDataSO PassiveData;

        /// <summary>
        /// Applies this upgrade option via the ServiceLocator.
        /// Handles weapon add/level-up and passive add/level-up.
        /// </summary>
        public void Apply()
        {
            switch (Type)
            {
                case UpgradeType.WeaponNew:
                case UpgradeType.WeaponLevelUp:
                    if (WeaponData != null && ServiceLocator.TryGet<WeaponSlotManager>(out var wsm))
                        wsm.TryAddWeapon(WeaponData);
                    break;

                case UpgradeType.PassiveNew:
                case UpgradeType.PassiveLevelUp:
                    if (PassiveData != null && ServiceLocator.TryGet<PassiveSlotManager>(out var psm))
                        psm.TryAddPassive(PassiveData);
                    break;
            }
        }
    }
}

using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Core
{
    public class GameplayBootstrap : MonoBehaviour
    {
        [SerializeField] private Weapons.Data.WeaponDataSO _startingWeapon;

        private void Start()
        {
            var player = transform;

            // Give the single orb pool all three references it needs
            if (ServiceLocator.TryGet<Pickups.CurrencyDropPool>(out var orbPool))
                orbPool.SetPlayerReference(
                    player,
                    GetComponent<Player.PlayerCurrencyHandler>(),
                    GetComponent<Player.PlayerLevelSystem>());

            // Give EnemyPool the player transform
            var enemyPool = FindFirstObjectByType<Enemies.EnemyPool>();
            if (enemyPool != null)
                enemyPool.SetPlayerTransform(player);

            // Give EnemySpawner the player + pool references
            if (ServiceLocator.TryGet<Enemies.EnemySpawner>(out var spawner))
                spawner.Setup(player, enemyPool);

            // Add starting weapon to WeaponSlotManager
            if (_startingWeapon != null &&
                ServiceLocator.TryGet<Weapons.WeaponSlotManager>(out var weaponSlots))
                weaponSlots.TryAddWeapon(_startingWeapon);
        }
    }
}

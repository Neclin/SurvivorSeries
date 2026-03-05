using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Core
{
    public class GameplayBootstrap : MonoBehaviour
    {
        [SerializeField] private List<Weapons.Data.WeaponDataSO> _startingWeapons = new();

        private void Start()
        {
            var player = transform;

            if (ServiceLocator.TryGet<Pickups.CurrencyDropPool>(out var orbPool))
                orbPool.SetPlayerReference(
                    player,
                    GetComponent<Player.PlayerCurrencyHandler>(),
                    GetComponent<Player.PlayerLevelSystem>());

            var enemyPool = FindFirstObjectByType<Enemies.EnemyPool>();
            if (enemyPool != null)
                enemyPool.SetPlayerTransform(player);

            if (ServiceLocator.TryGet<Enemies.EnemySpawner>(out var spawner))
                spawner.Setup(player, enemyPool);

            if (_startingWeapons != null &&
                ServiceLocator.TryGet<Weapons.WeaponSlotManager>(out var weaponSlots))
            {
                foreach (var w in _startingWeapons)
                    if (w != null) weaponSlots.TryAddWeapon(w);
            }
        }
    }
}

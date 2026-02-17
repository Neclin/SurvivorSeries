using UnityEngine;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Pickups;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Enemies
{
    public class EnemyDropper : MonoBehaviour
    {
        public void SpawnDrops(Vector3 position, EnemyDataSO data)
        {
            // Single orb pickup — always drops, grants both currency and XP
            if (ServiceLocator.TryGet<CurrencyDropPool>(out var pool))
                pool.Spawn(position + Vector3.up * 0.2f, data.CurrencyDropAmount);
        }
    }
}

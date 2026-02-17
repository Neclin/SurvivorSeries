using UnityEngine;

namespace SurvivorSeries.Enemies.Variants
{
    public class WalkerEnemy : EnemyBase
    {
        // Walker just pursues the player — all logic is in EnemyMovement + EnemyBase

        protected override void UpdateBehavior() { }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);
    }
}

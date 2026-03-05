using UnityEngine;

namespace SurvivorSeries.Enemies.Variants
{
    public class WalkerEnemy : EnemyBase
    {

        protected override void UpdateBehavior() { }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);
    }
}
using UnityEngine;

namespace SurvivorSeries.Enemies.Variants
{
    public class EliteEnemy : EnemyBase
    {
        protected override void UpdateBehavior() { }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);
    }
}
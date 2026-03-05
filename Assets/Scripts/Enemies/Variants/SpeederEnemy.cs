using UnityEngine;

namespace SurvivorSeries.Enemies.Variants
{
    public class SpeederEnemy : EnemyBase
    {
        protected override void UpdateBehavior() { }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);
    }
}
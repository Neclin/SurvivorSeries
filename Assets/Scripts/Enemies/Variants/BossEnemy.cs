using System;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Enemies.Variants
{
    public class BossEnemy : EnemyBase
    {
        public static event Action OnBossDefeated;

        protected override void UpdateBehavior() { }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);

        protected override void OnDeath()
        {
            base.OnDeath();
            OnBossDefeated?.Invoke();

            if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                gm.OnBossDefeated();
            else if (ServiceLocator.TryGet<UI.Victory.VictoryUI>(out var ui))
                ui.Show();
        }
    }
}

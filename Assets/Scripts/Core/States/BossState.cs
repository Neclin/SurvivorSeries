using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Core.States
{
    public class BossState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Boss");
            if (ServiceLocator.TryGet<Enemies.BossSpawner>(out var bs))
                bs.SpawnBoss();
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
        }
    }
}
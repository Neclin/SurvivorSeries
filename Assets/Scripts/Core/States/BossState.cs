using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class BossState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Boss");
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
        }
    }
}
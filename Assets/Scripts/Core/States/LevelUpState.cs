using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class LevelUpState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] LevelUp");
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
        }
    }
}
using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class GameOverState : GameState
    {
        public override void Enter()
        {
            Time.timeScale = 0f;
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            Time.timeScale = 1f;
        }
    }
}

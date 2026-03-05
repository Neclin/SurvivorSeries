using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class PauseState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Pause");
            Time.timeScale = 0f;
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            Time.timeScale = 1f;
        }
    }
}
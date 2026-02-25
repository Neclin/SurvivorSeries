using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class VictoryState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Victory");
            Time.timeScale = 0f;
            // TODO: Show VictoryUI, trigger achievement checks, save unlocks
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            Time.timeScale = 1f;
            // TODO: Hide VictoryUI, clean up gameplay scene
        }
    }
}

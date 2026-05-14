using UnityEngine;
using SurvivorSeries.Utilities;
using SurvivorSeries.UI.Victory;

namespace SurvivorSeries.Core.States
{
    public class VictoryState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Victory");
            if (ServiceLocator.TryGet<VictoryUI>(out var ui))
                ui.Show();
            else
                Time.timeScale = 0f;
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            Time.timeScale = 1f;
        }
    }
}
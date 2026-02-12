using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class MainMenuState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] MainMenu");
            // TODO: Show MainMenuUI, disable player input map
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            // TODO: Hide MainMenuUI
        }
    }
}

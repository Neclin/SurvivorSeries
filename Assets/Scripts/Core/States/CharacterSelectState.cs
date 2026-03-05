using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class CharacterSelectState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] CharacterSelect");
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
        }
    }
}
using UnityEngine;

namespace SurvivorSeries.Core.States
{
    public class BossState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Boss");
            // TODO: Spawn boss, play boss music, show boss health bar
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            // TODO: Despawn any remaining boss minions, stop boss music
        }
    }
}

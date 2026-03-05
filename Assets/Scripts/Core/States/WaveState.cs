using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Core.States
{
    public class WaveState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Wave");
            if (ServiceLocator.TryGet<Waves.WaveManager>(out var wm) && !wm.IsWaveActive)
                wm.StartNextWave();
        }

        public override void Tick(float deltaTime) { }

        public override void Exit() { }
    }
}
namespace SurvivorSeries.Core
{
    public abstract class GameState
    {
        protected GameStateMachine FSM { get; private set; }

        public void SetFSM(GameStateMachine fsm) => FSM = fsm;

        public abstract void Enter();
        public abstract void Tick(float deltaTime);
        public abstract void Exit();
    }
}

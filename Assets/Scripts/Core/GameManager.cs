using UnityEngine;
using SurvivorSeries.Core.States;
using SurvivorSeries.Input;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Core
{
    /// <summary>
    /// Central game manager. Lives in the Bootstrap scene, never destroyed.
    /// Owns the GameStateMachine and coordinates high-level transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GameStateMachine _fsm;
        private GameState _preOverlayState;

        // Populated when a run starts
        public Characters.Data.CharacterDefinitionSO SelectedCharacter { get; private set; }
        public Waves.Data.DifficultySettingsSO SelectedDifficulty { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceLocator.Register<GameManager>(this);
            BuildFSM();
        }

        private void BuildFSM()
        {
            _fsm = new GameStateMachine();
            _fsm.RegisterState(new MainMenuState());
            _fsm.RegisterState(new CharacterSelectState());
            _fsm.RegisterState(new WaveState());
            _fsm.RegisterState(new LevelUpState());
            _fsm.RegisterState(new BossState());
            _fsm.RegisterState(new PauseState());
            _fsm.RegisterState(new GameOverState());
            _fsm.RegisterState(new VictoryState());
        }

        private void Start()
        {
            _fsm.Initialize<MainMenuState>();
        }

        private void Update()
        {
            _fsm.Tick(Time.deltaTime);
        }

        // --- Public API ---

        public void GoToCharacterSelect()
            => _fsm.TransitionTo<CharacterSelectState>();

        public void StartGame(Characters.Data.CharacterDefinitionSO character,
                              Waves.Data.DifficultySettingsSO difficulty)
        {
            SelectedCharacter = character;
            SelectedDifficulty = difficulty;
            _fsm.TransitionTo<WaveState>();
        }

        public void OnWaveEnded(bool isFinalWave)
        {
            if (isFinalWave)
                _fsm.TransitionTo<BossState>();
            else
                _fsm.TransitionTo<WaveState>();
        }

        public void OnBossDefeated() => _fsm.TransitionTo<VictoryState>();

        public void OnPlayerDied() => _fsm.TransitionTo<GameOverState>();

        public void OnLevelUp() => _fsm.TransitionTo<LevelUpState>();

        public void OnLevelUpComplete()
        {
            // Return to whichever active gameplay state was running
            if (_fsm.IsInState<LevelUpState>())
                _fsm.TransitionTo<WaveState>();
        }

        public void PauseGame()
        {
            if (_fsm.IsInState<PauseState>()) return;
            _preOverlayState = _fsm.CurrentState;
            _fsm.TransitionTo<PauseState>();
        }

        public void ResumeGame()
        {
            if (!_fsm.IsInState<PauseState>()) return;
            // Resume based on what was active before pause
            if (_preOverlayState is WaveState)
                _fsm.TransitionTo<WaveState>();
            else if (_preOverlayState is BossState)
                _fsm.TransitionTo<BossState>();
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            _fsm.TransitionTo<MainMenuState>();
        }

        public GameStateMachine GetFSM() => _fsm;

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameManager>();
        }
    }
}

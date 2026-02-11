using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorSeries.Core
{
    public class GameStateMachine
    {
        private GameState _currentState;
        private readonly Dictionary<Type, GameState> _states = new();

        public GameState CurrentState => _currentState;
        public event Action<GameState, GameState> OnStateChanged;

        public void RegisterState<T>(T state) where T : GameState
        {
            state.SetFSM(this);
            _states[typeof(T)] = state;
        }

        public void Initialize<T>() where T : GameState
        {
            if (!_states.TryGetValue(typeof(T), out GameState state))
            {
                Debug.LogError($"GameStateMachine: State {typeof(T).Name} not registered.");
                return;
            }
            _currentState = state;
            _currentState.Enter();
        }

        public void TransitionTo<T>() where T : GameState
        {
            if (!_states.TryGetValue(typeof(T), out GameState nextState))
            {
                Debug.LogError($"GameStateMachine: State {typeof(T).Name} not registered.");
                return;
            }

            if (nextState == _currentState) return;

            GameState previous = _currentState;
            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
            OnStateChanged?.Invoke(previous, _currentState);
        }

        public T GetState<T>() where T : GameState
        {
            _states.TryGetValue(typeof(T), out GameState state);
            return state as T;
        }

        public void Tick(float deltaTime)
        {
            _currentState?.Tick(deltaTime);
        }

        public bool IsInState<T>() where T : GameState
        {
            return _currentState is T;
        }
    }
}

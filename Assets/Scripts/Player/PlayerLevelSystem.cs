using System;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Player
{
    public class PlayerLevelSystem : MonoBehaviour
    {
        private int _level = 1;
        private float _currentXP;
        private float _xpToNextLevel;

        public int Level => _level;
        public float CurrentXP => _currentXP;
        public float XPToNextLevel => _xpToNextLevel;
        public float XPProgress => _xpToNextLevel > 0f ? _currentXP / _xpToNextLevel : 0f;

        public event Action<int> OnLevelUp;           // new level number
        public event Action<float, float> OnXPChanged; // (current, required)

        private void Awake()
        {
            _xpToNextLevel = MathHelpers.XPThreshold(_level);
            ServiceLocator.Register<PlayerLevelSystem>(this);
        }

        private void OnDestroy() => ServiceLocator.Unregister<PlayerLevelSystem>();

        public void AddXP(float amount)
        {
            _currentXP += amount;
            while (_currentXP >= _xpToNextLevel)
            {
                _currentXP -= _xpToNextLevel;
                _level++;
                _xpToNextLevel = MathHelpers.XPThreshold(_level);
                OnLevelUp?.Invoke(_level);
            }
            OnXPChanged?.Invoke(_currentXP, _xpToNextLevel);
        }

        public void Reset()
        {
            _level = 1;
            _currentXP = 0f;
            _xpToNextLevel = MathHelpers.XPThreshold(_level);
            OnXPChanged?.Invoke(_currentXP, _xpToNextLevel);
        }
    }
}

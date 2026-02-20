using System;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Player
{
    public class PlayerCurrencyHandler : MonoBehaviour
    {
        private int _currency;

        public int Currency => _currency;

        public event Action<int> OnCurrencyChanged;

        private void Awake() => ServiceLocator.Register<PlayerCurrencyHandler>(this);
        private void OnDestroy() => ServiceLocator.Unregister<PlayerCurrencyHandler>();

        public void AddCurrency(int amount)
        {
            _currency += amount;
            OnCurrencyChanged?.Invoke(_currency);
        }

        public bool SpendCurrency(int amount)
        {
            if (_currency < amount) return false;
            _currency -= amount;
            OnCurrencyChanged?.Invoke(_currency);
            return true;
        }

        public void Reset()
        {
            _currency = 0;
            OnCurrencyChanged?.Invoke(_currency);
        }
    }
}

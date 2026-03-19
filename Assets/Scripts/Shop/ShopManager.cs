using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private int _rerollBasePrice = 50;

        private int _rerollCost;
        private List<ShopItem> _inventory = new();
        private float _difficultyMultiplier = 1f;

        public IReadOnlyList<ShopItem> Inventory => _inventory;
        public int RerollCost => _rerollCost;

        public event Action OnInventoryChanged;

        private void Awake()
        {
            ServiceLocator.Register<ShopManager>(this);
            _rerollCost = _rerollBasePrice;
        }

        private void OnDestroy() => ServiceLocator.Unregister<ShopManager>();

        public void SetDifficultyMultiplier(float mult) => _difficultyMultiplier = mult;

        public void GenerateInventory()
        {
            _rerollCost = _rerollBasePrice;
            Regenerate();
        }

        public bool TryPurchase(int index)
        {
            if (index < 0 || index >= _inventory.Count) return false;
            var item = _inventory[index];

            if (!ServiceLocator.TryGet<Player.PlayerCurrencyHandler>(out var currency)) return false;
            if (!currency.SpendCurrency(item.Cost)) return false;

            item.OnPurchase?.Invoke();
            Audio.AudioManager.Play(Audio.SfxId.Purchase);

            _inventory.RemoveAt(index);
            var replacement = GenerateOne();
            if (replacement != null)
                _inventory.Insert(index, replacement);

            OnInventoryChanged?.Invoke();
            Debug.Log($"[Shop] Purchased: {item.Name} for {item.Cost}g");
            return true;
        }

        public bool TryReroll()
        {
            if (!ServiceLocator.TryGet<Player.PlayerCurrencyHandler>(out var currency)) return false;
            if (!currency.SpendCurrency(_rerollCost)) return false;

            _rerollCost *= 2;
            Regenerate();
            Debug.Log($"[Shop] Rerolled. Next reroll costs {_rerollCost}g");
            return true;
        }

        private void Regenerate()
        {
            _inventory.Clear();
            if (ServiceLocator.TryGet<ShopInventoryGenerator>(out var gen))
                _inventory = gen.Generate(_difficultyMultiplier);
            OnInventoryChanged?.Invoke();
        }

        private ShopItem GenerateOne()
        {
            if (!ServiceLocator.TryGet<ShopInventoryGenerator>(out var gen)) return null;
            var list = gen.Generate(_difficultyMultiplier);
            return list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count)] : null;
        }
    }
}
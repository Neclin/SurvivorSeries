using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Utilities;
using SurvivorSeries.Core;
using SurvivorSeries.Shop;
using SurvivorSeries.Player;
using SurvivorSeries.Waves;

namespace SurvivorSeries.UI.Shop
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _rerollCostText;
        [SerializeField] private Button _rerollButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private ShopItemUI[] _itemSlots;

        private ShopManager _shopManager;

        private void Awake()
        {
            ServiceLocator.Register<ShopUI>(this);
            _panel.SetActive(false);
            _rerollButton.onClick.AddListener(OnReroll);
            _continueButton.onClick.AddListener(OnContinue);
        }

        private void OnDestroy() => ServiceLocator.Unregister<ShopUI>();

        private void Start()
        {
            if (ServiceLocator.TryGet<ShopManager>(out _shopManager))
                _shopManager.OnInventoryChanged += Refresh;

            if (ServiceLocator.TryGet<PlayerCurrencyHandler>(out var currency))
                currency.OnCurrencyChanged += OnCurrencyChanged;
        }

        public void Show()
        {
            _panel.SetActive(true);

            if (ServiceLocator.TryGet<WaveManager>(out var wm))
                _titleText.text = $"SHOP — After Wave {wm.CurrentWave}";

            RefreshCurrency();
            Refresh();
        }

        public void Hide() => _panel.SetActive(false);

        private void Refresh()
        {
            if (_shopManager == null) return;
            var inventory = _shopManager.Inventory;

            for (int i = 0; i < _itemSlots.Length; i++)
            {
                if (i < inventory.Count)
                    _itemSlots[i].Populate(inventory[i], i, _shopManager);
                else
                    _itemSlots[i].SetEmpty();
            }

            if (_rerollCostText != null)
                _rerollCostText.text = $"Reroll ({_shopManager.RerollCost}g)";
        }

        private void RefreshCurrency()
        {
            if (ServiceLocator.TryGet<PlayerCurrencyHandler>(out var c))
                _currencyText.text = $"Gold: {c.Currency}";
        }

        private void OnCurrencyChanged(int amount)
        {
            _currencyText.text = $"Gold: {amount}";
            Refresh();
        }

        private void OnReroll()
        {
            _shopManager?.TryReroll();
        }

        private void OnContinue()
        {
            if (ServiceLocator.TryGet<GameManager>(out var gm))
                gm.OnShopClosed();
        }
    }
}
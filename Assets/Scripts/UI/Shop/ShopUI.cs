using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Utilities;
using SurvivorSeries.Core;
using SurvivorSeries.Shop;
using SurvivorSeries.Player;
using SurvivorSeries.Waves;
using SurvivorSeries.Weapons;
using SurvivorSeries.Passives;

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
        [SerializeField] private WeaponInventoryCard[] _weaponCards;
        [SerializeField] private PassiveInventoryCard[] _passiveCards;

        private ShopManager _shopManager;
        private WeaponSlotManager _wsm;
        private PassiveSlotManager _psm;

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
            if (ServiceLocator.TryGet(out _shopManager))
                _shopManager.OnInventoryChanged += Refresh;

            if (ServiceLocator.TryGet<PlayerCurrencyHandler>(out var currency))
                currency.OnCurrencyChanged += OnCurrencyChanged;

            if (ServiceLocator.TryGet(out _wsm))
                _wsm.OnInventoryChanged += RefreshInventory;
            if (ServiceLocator.TryGet(out _psm))
                _psm.OnInventoryChanged += RefreshInventory;
        }

        public void Show()
        {
            _panel.SetActive(true);

            if (ServiceLocator.TryGet<WaveManager>(out var wm))
                _titleText.text = $"SHOP — After Wave {wm.CurrentWave}";

            if (_shopManager == null && ServiceLocator.TryGet(out _shopManager))
                _shopManager.OnInventoryChanged += Refresh;
            if (_wsm == null && ServiceLocator.TryGet(out _wsm))
                _wsm.OnInventoryChanged += RefreshInventory;
            if (_psm == null && ServiceLocator.TryGet(out _psm))
                _psm.OnInventoryChanged += RefreshInventory;

            _shopManager?.GenerateInventory();

            RefreshCurrency();
            Refresh();
            RefreshInventory();

            Time.timeScale = 0f;
        }

        public void Hide()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void Refresh()
        {
            if (_shopManager == null) return;
            var inventory = _shopManager.Inventory;
            for (int i = 0; i < _itemSlots.Length; i++)
            {
                if (i < inventory.Count) _itemSlots[i].Populate(inventory[i], i, _shopManager);
                else _itemSlots[i].SetEmpty();
            }
            if (_rerollCostText != null)
                _rerollCostText.text = $"Reroll ({_shopManager.RerollCost}g)";
        }

        private void RefreshInventory()
        {
            if (_weaponCards != null && _wsm != null)
            {
                var slots = _wsm.GetAllWeapons();
                for (int i = 0; i < _weaponCards.Length; i++)
                    _weaponCards[i].Bind(i < slots.Length ? slots[i] : null);
            }

            if (_passiveCards != null && _psm != null)
            {
                for (int i = 0; i < _passiveCards.Length; i++)
                {
                    var data = _psm.GetSlotData(i);
                    int lvl = _psm.GetSlotLevel(i);
                    int max = data != null ? data.MaxLevel : 0;
                    _passiveCards[i].Bind(data, lvl, max);
                }
            }
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

        private void OnReroll() => _shopManager?.TryReroll();

        private void OnContinue()
        {
            Hide();
            if (ServiceLocator.TryGet<GameManager>(out var gm))
            {
                gm.OnShopClosed();
                return;
            }
            if (ServiceLocator.TryGet<WaveManager>(out var wm))
                wm.StartNextWave();
        }
    }
}

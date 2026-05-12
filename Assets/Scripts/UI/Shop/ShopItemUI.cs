using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Shop;
using SurvivorSeries.Utilities;
using SurvivorSeries.Player;

namespace SurvivorSeries.UI.Shop
{
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private GameObject _soldOutOverlay;
        [SerializeField] private Image _iconImage;

        private int _slotIndex;
        private ShopManager _shopManager;

        public void Populate(ShopItem item, int slotIndex, ShopManager shopManager)
        {
            _slotIndex = slotIndex;
            _shopManager = shopManager;

            _nameText.text = item.Name;
            _descriptionText.text = item.Description;
            _costText.text = $"{item.Cost}g";
            gameObject.SetActive(true);

            if (_iconImage != null)
            {
                Sprite icon = item.WeaponData != null ? item.WeaponData.Icon
                            : item.PassiveData != null ? item.PassiveData.Icon
                            : null;
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }

            if (_soldOutOverlay != null) _soldOutOverlay.SetActive(false);

            bool canAfford = true;
            if (ServiceLocator.TryGet<PlayerCurrencyHandler>(out var c))
                canAfford = c.Currency >= item.Cost;

            _buyButton.interactable = canAfford;
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnBuy);
        }

        public void SetEmpty()
        {
            _slotIndex = -1;
            _shopManager = null;

            _nameText.text = "—";
            _descriptionText.text = "";
            _costText.text = "";
            _buyButton.interactable = false;
            _buyButton.onClick.RemoveAllListeners();

            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }

            if (_soldOutOverlay != null) _soldOutOverlay.SetActive(true);
        }

        private void OnBuy() => _shopManager?.TryPurchase(_slotIndex);
    }
}
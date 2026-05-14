using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Weapons;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.Shop
{
    public class WeaponInventoryCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _mergeButton;
        [SerializeField] private TextMeshProUGUI _mergeLabel;
        [SerializeField] private GameObject _emptyOverlay;
        [SerializeField] private Image _iconImage;

        private WeaponBase _weapon;

        public void Bind(WeaponBase weapon)
        {
            _weapon = weapon;
            if (_emptyOverlay != null) _emptyOverlay.SetActive(weapon == null);

            if (weapon == null)
            {
                _nameText.text = "—";
                _levelText.text = "";
                _mergeButton.gameObject.SetActive(false);
                if (_iconImage != null) { _iconImage.sprite = null; _iconImage.enabled = false; }
                return;
            }

            if (_iconImage != null)
            {
                Sprite icon = weapon.Data != null ? weapon.Data.Icon : null;
                if (!_iconImage.gameObject.activeSelf) _iconImage.gameObject.SetActive(true);
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }

            _nameText.text = weapon.Data != null ? weapon.Data.WeaponName : weapon.name;
            _levelText.text = weapon.IsMaxLevel ? $"Lv. {weapon.Level} (MAX)" : $"Lv. {weapon.Level}";

            bool canMerge = false;
            if (!weapon.IsMaxLevel &&
                ServiceLocator.TryGet<WeaponSlotManager>(out var wsm))
            {
                canMerge = wsm.CountAt(weapon.Data, weapon.Level) >= 2;
            }

            _mergeButton.gameObject.SetActive(canMerge);
            _mergeButton.onClick.RemoveAllListeners();
            _mergeButton.onClick.AddListener(OnMerge);
            if (_mergeLabel != null) _mergeLabel.text = $"Merge → Lv.{weapon.Level + 1}";
        }

        private void OnMerge()
        {
            if (_weapon == null || _weapon.Data == null) return;
            if (ServiceLocator.TryGet<WeaponSlotManager>(out var wsm))
                wsm.TryCombine(_weapon.Data, _weapon.Level);
        }
    }
}

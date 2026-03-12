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
                return;
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

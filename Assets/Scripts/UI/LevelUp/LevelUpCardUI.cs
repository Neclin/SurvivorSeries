using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.LevelUp;

namespace SurvivorSeries.UI.LevelUp
{
    public class LevelUpCardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private Button _chooseButton;

        private UpgradeOption _option;
        private Action<UpgradeOption> _onChosen;

        public void Populate(UpgradeOption option, Action<UpgradeOption> onChosen)
        {
            _option = option;
            _onChosen = onChosen;

            gameObject.SetActive(true);

            _nameText.text = option.Name;
            _descriptionText.text = option.Description;

            if (_typeText != null)
            {
                _typeText.text = option.Type switch
                {
                    UpgradeType.WeaponNew => "NEW WEAPON",
                    UpgradeType.WeaponLevelUp => "WEAPON +1",
                    UpgradeType.PassiveNew => "NEW PASSIVE",
                    UpgradeType.PassiveLevelUp => "PASSIVE +1",
                    _ => ""
                };
            }

            _chooseButton.onClick.RemoveAllListeners();
            _chooseButton.onClick.AddListener(OnChoose);
            _chooseButton.interactable = true;
        }

        public void SetEmpty()
        {
            gameObject.SetActive(false);
        }

        private void OnChoose() => _onChosen?.Invoke(_option);
    }
}

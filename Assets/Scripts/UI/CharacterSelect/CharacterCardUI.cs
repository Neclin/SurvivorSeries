using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Characters.Data;

namespace SurvivorSeries.UI.CharacterSelect
{
    public class CharacterCardUI : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private GameObject _portraitPlaceholder;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Button _selectButton;

        private static readonly Color SelectedColor = new(0.95f, 0.75f, 0.30f, 1f);
        private static readonly Color UnselectedColor = new(0.20f, 0.20f, 0.25f, 1f);

        public CharacterDefinitionSO Data { get; private set; }

        public void Setup(CharacterDefinitionSO def, Action onSelected)
        {
            Data = def;
            if (def == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            if (_nameText != null) _nameText.text = def.CharacterName;
            if (_descriptionText != null) _descriptionText.text = def.Description;
            if (_statsText != null) _statsText.text = BuildStats(def);

            bool hasPortrait = def.Portrait != null;
            if (_portraitImage != null)
            {
                _portraitImage.sprite = def.Portrait;
                _portraitImage.enabled = hasPortrait;
                _portraitImage.preserveAspect = true;
                _portraitImage.color = hasPortrait ? Color.white : new Color(0.08f, 0.08f, 0.10f, 1f);
            }
            if (_portraitPlaceholder != null)
                _portraitPlaceholder.SetActive(!hasPortrait);

            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(() => onSelected?.Invoke());
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_background != null)
                _background.color = selected ? SelectedColor : UnselectedColor;
        }

        private static string BuildStats(CharacterDefinitionSO def)
        {
            return $"HP {def.BaseMaxHealth:0}   SPD {def.BaseMoveSpeed:0.0}   DMG {def.BaseDamage:0}\n" +
                   $"HP×{def.HealthGrowth:0.00}  SPD×{def.MoveSpeedGrowth:0.00}  DMG×{def.DamageGrowth:0.00}";
        }
    }
}

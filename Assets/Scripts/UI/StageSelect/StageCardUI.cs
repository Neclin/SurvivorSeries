using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Stages.Data;

namespace SurvivorSeries.UI.StageSelect
{
    public class StageCardUI : MonoBehaviour
    {
        [SerializeField] private Image _thumbnail;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _lockText;
        [SerializeField] private Button _selectButton;

        private static readonly Color SelectedColor = new(0.95f, 0.75f, 0.30f, 1f);
        private static readonly Color UnselectedColor = new(0.20f, 0.20f, 0.25f, 1f);
        private static readonly Color LockedTint = new(0.10f, 0.10f, 0.12f, 1f);

        public StageDefinitionSO Data { get; private set; }
        public bool IsLocked { get; private set; }

        public void Setup(StageDefinitionSO def, bool locked, string lockHint, Action onSelected)
        {
            Data = def;
            IsLocked = locked;
            if (def == null) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);

            if (_thumbnail != null) _thumbnail.color = def.PreviewTint;
            if (_nameText != null) _nameText.text = def.DisplayName;
            if (_descriptionText != null) _descriptionText.text = def.Description;

            if (_lockText != null)
            {
                _lockText.gameObject.SetActive(locked);
                if (locked) _lockText.text = string.IsNullOrEmpty(lockHint) ? "LOCKED" : $"LOCKED — {lockHint}";
            }

            if (_selectButton != null)
            {
                _selectButton.interactable = !locked;
                _selectButton.onClick.RemoveAllListeners();
                if (!locked) _selectButton.onClick.AddListener(() => onSelected?.Invoke());
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_background == null) return;
            if (IsLocked) { _background.color = LockedTint; return; }
            _background.color = selected ? SelectedColor : UnselectedColor;
        }
    }
}

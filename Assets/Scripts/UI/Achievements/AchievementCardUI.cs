using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Achievements.Data;

namespace SurvivorSeries.UI.Achievements
{
    public class AchievementCardUI : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _iconPlaceholder;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private GameObject _lockedOverlay;

        private static readonly Color UnlockedColor = new(0.20f, 0.45f, 0.20f, 1f);
        private static readonly Color LockedColor = new(0.18f, 0.18f, 0.22f, 1f);

        public void Setup(AchievementDefinitionSO def, bool unlocked, string rewardSummary)
        {
            if (def == null) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);

            if (_titleText != null) _titleText.text = def.Title;
            if (_descriptionText != null) _descriptionText.text = def.Description;
            if (_statusText != null) _statusText.text = unlocked ? "UNLOCKED" : "LOCKED";
            if (_rewardText != null)
            {
                bool hasReward = !string.IsNullOrEmpty(rewardSummary);
                _rewardText.gameObject.SetActive(hasReward);
                if (hasReward) _rewardText.text = unlocked ? $"Reward: {rewardSummary}" : $"Unlocks: {rewardSummary}";
            }
            if (_lockedOverlay != null) _lockedOverlay.SetActive(!unlocked);
            if (_background != null) _background.color = unlocked ? UnlockedColor : LockedColor;

            bool hasIcon = def.Icon != null;
            if (_icon != null)
            {
                _icon.sprite = def.Icon;
                _icon.enabled = hasIcon;
                _icon.preserveAspect = true;
                _icon.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
            if (_iconPlaceholder != null) _iconPlaceholder.SetActive(!hasIcon);
        }
    }
}

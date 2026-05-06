using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Achievements;
using SurvivorSeries.Achievements.Data;
using SurvivorSeries.Persistence;
using SurvivorSeries.Stages.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.Achievements
{
    public class AchievementsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Transform _cardParent;
        [SerializeField] private AchievementCardUI _cardPrefab;
        [SerializeField] private Button _backButton;
        [SerializeField] private StageRoster _stageRoster;
        [SerializeField] private List<AchievementDefinitionSO> _definitionsOverride = new();

        private readonly List<AchievementCardUI> _spawned = new();

        private void Awake()
        {
            ServiceLocator.Register<AchievementsUI>(this);
            if (_panel != null) _panel.SetActive(false);
            if (_titleText != null) _titleText.text = "ACHIEVEMENTS";
            if (_backButton != null) _backButton.onClick.AddListener(OnBack);
        }

        private void OnDestroy() => ServiceLocator.Unregister<AchievementsUI>();

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            Time.timeScale = 0f;
            Rebuild();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Rebuild()
        {
            var defs = GetDefinitions();
            ServiceLocator.TryGet<UnlockRegistry>(out var unlocks);

            EnsureCapacity(defs.Count);
            int unlockedCount = 0;

            for (int i = 0; i < _spawned.Count; i++)
            {
                if (i >= defs.Count) { _spawned[i].gameObject.SetActive(false); continue; }
                var def = defs[i];
                bool unlocked = unlocks != null && unlocks.IsAchievementCompleted(def.AchievementID);
                if (unlocked) unlockedCount++;
                _spawned[i].Setup(def, unlocked, BuildRewardSummary(def));
            }

            if (_progressText != null)
                _progressText.text = $"{unlockedCount} / {defs.Count} UNLOCKED";
        }

        private List<AchievementDefinitionSO> GetDefinitions()
        {
            if (_definitionsOverride != null && _definitionsOverride.Count > 0)
                return _definitionsOverride;

            if (ServiceLocator.TryGet<AchievementsManager>(out var mgr) && mgr.Definitions != null)
            {
                var list = new List<AchievementDefinitionSO>(mgr.Definitions.Count);
                foreach (var d in mgr.Definitions) if (d != null) list.Add(d);
                return list;
            }
            return new List<AchievementDefinitionSO>();
        }

        private string BuildRewardSummary(AchievementDefinitionSO def)
        {
            var sb = new StringBuilder();
            if (def.UnlocksCharacter != null)
                sb.Append(def.UnlocksCharacter.CharacterName);

            if (_stageRoster != null && _stageRoster.AllStages != null)
            {
                foreach (var stage in _stageRoster.AllStages)
                {
                    if (stage == null || string.IsNullOrEmpty(stage.UnlockAchievementID)) continue;
                    if (stage.UnlockAchievementID != def.AchievementID) continue;
                    if (sb.Length > 0) sb.Append(" + ");
                    sb.Append(stage.DisplayName);
                    sb.Append(" stage");
                }
            }
            return sb.ToString();
        }

        private void EnsureCapacity(int count)
        {
            if (_cardPrefab == null || _cardParent == null) return;
            while (_spawned.Count < count)
            {
                var card = Instantiate(_cardPrefab, _cardParent);
                _spawned.Add(card);
            }
        }

        private void OnBack()
        {
            Hide();
            if (ServiceLocator.TryGet<UI.MainMenu.MainMenuUI>(out var menu)) menu.Show();
        }
    }
}

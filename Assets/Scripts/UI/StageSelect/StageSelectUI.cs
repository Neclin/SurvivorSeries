using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Stages;
using SurvivorSeries.Stages.Data;
using SurvivorSeries.Persistence;
using SurvivorSeries.Achievements.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.StageSelect
{
    public class StageSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private StageRoster _roster;
        [SerializeField] private List<StageCardUI> _cards = new();
        [SerializeField] private List<AchievementDefinitionSO> _achievementLookup = new();
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _backButton;

        private StageDefinitionSO _selected;

        private void Awake()
        {
            ServiceLocator.Register<StageSelectUI>(this);
            if (_panel != null) _panel.SetActive(false);
            if (_titleText != null) _titleText.text = "SELECT STAGE";
            if (_selectButton != null) _selectButton.onClick.AddListener(OnSelect);
            if (_backButton != null) _backButton.onClick.AddListener(OnBack);
        }

        private void OnDestroy() => ServiceLocator.Unregister<StageSelectUI>();

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            Time.timeScale = 0f;
            BindCards();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void BindCards()
        {
            _selected = null;
            if (_roster == null || _roster.AllStages == null) return;
            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                if (card == null) continue;
                if (i >= _roster.AllStages.Count) { card.gameObject.SetActive(false); continue; }
                var def = _roster.AllStages[i];
                bool locked = !IsUnlocked(def);
                string hint = locked ? GetAchievementTitle(def.UnlockAchievementID) : null;
                card.Setup(def, locked, hint, () => SelectStage(def));
            }

            string preferredId = null;
            if (ServiceLocator.TryGet<UnlockRegistry>(out var unlocks)) preferredId = unlocks.GetData().LastSelectedStageID;
            foreach (var c in _cards)
            {
                if (c != null && c.Data != null && !c.IsLocked && c.Data.StageID == preferredId) { SelectStage(c.Data); return; }
            }
            foreach (var c in _cards)
            {
                if (c != null && c.Data != null && !c.IsLocked) { SelectStage(c.Data); return; }
            }
        }

        private void SelectStage(StageDefinitionSO def)
        {
            _selected = def;
            foreach (var c in _cards) if (c != null) c.SetSelected(c.Data == def);
        }

        private bool IsUnlocked(StageDefinitionSO def)
        {
            if (def == null) return false;
            if (string.IsNullOrEmpty(def.UnlockAchievementID)) return true;
            if (!ServiceLocator.TryGet<UnlockRegistry>(out var unlocks)) return true;
            return unlocks.IsAchievementCompleted(def.UnlockAchievementID);
        }

        private string GetAchievementTitle(string id)
        {
            foreach (var a in _achievementLookup)
                if (a != null && a.AchievementID == id) return a.Title;
            return id;
        }

        private void OnSelect()
        {
            if (_selected == null) return;
            if (ServiceLocator.TryGet<StageManager>(out var sm)) sm.PendingStage = _selected;
            if (ServiceLocator.TryGet<UnlockRegistry>(out var unlocks))
            {
                unlocks.GetData().LastSelectedStageID = _selected.StageID;
                unlocks.SaveNow();
            }
            Hide();
            if (ServiceLocator.TryGet<UI.CharacterSelect.CharacterSelectUI>(out var cs)) cs.Show();
        }

        private void OnBack()
        {
            Hide();
            if (ServiceLocator.TryGet<UI.MainMenu.MainMenuUI>(out var menu)) menu.Show();
        }
    }
}

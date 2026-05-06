using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Characters;
using SurvivorSeries.Characters.Data;
using SurvivorSeries.Waves;
using SurvivorSeries.Waves.Data;
using SurvivorSeries.Player;
using SurvivorSeries.Weapons;
using SurvivorSeries.Persistence;
using SurvivorSeries.Stages;
using SurvivorSeries.Audio;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.CharacterSelect
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private CharacterRoster _roster;
        [SerializeField] private List<DifficultySettingsSO> _difficulties = new();
        [SerializeField] private List<CharacterCardUI> _characterCards = new();
        [SerializeField] private List<Button> _difficultyButtons = new();
        [SerializeField] private List<TextMeshProUGUI> _difficultyLabels = new();
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _selectionSummary;
        [SerializeField] private CharacterPreviewRig _previewRig;

        private static readonly Color SelectedColor = new(0.95f, 0.75f, 0.30f, 1f);
        private static readonly Color UnselectedColor = new(0.20f, 0.20f, 0.25f, 1f);

        private CharacterDefinitionSO _selectedCharacter;
        private DifficultySettingsSO _selectedDifficulty;

        private void Awake()
        {
            ServiceLocator.Register<CharacterSelectUI>(this);
            if (_panel != null) _panel.SetActive(false);
            if (_titleText != null) _titleText.text = "CHOOSE YOUR HUNTER";
            if (_startButton != null) _startButton.onClick.AddListener(OnStart);
            if (_backButton != null) _backButton.onClick.AddListener(OnBack);

            BindDifficultyButtons();
            BindCharacterCards();
        }

        private void OnDestroy() => ServiceLocator.Unregister<CharacterSelectUI>();

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            Time.timeScale = 0f;

            _selectedCharacter = null;
            _selectedDifficulty = null;
            if (_roster != null && _roster.AllCharacters != null && _roster.AllCharacters.Count > 0)
                SelectCharacter(_roster.AllCharacters[0]);
            if (_difficulties.Count > 0)
                SelectDifficulty(_difficulties[Mathf.Min(1, _difficulties.Count - 1)]);

            RefreshSummary();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_previewRig != null) _previewRig.Clear();
        }

        private void BindCharacterCards()
        {
            if (_roster == null || _roster.AllCharacters == null) return;
            for (int i = 0; i < _characterCards.Count; i++)
            {
                var card = _characterCards[i];
                if (card == null) continue;
                if (i >= _roster.AllCharacters.Count) { card.gameObject.SetActive(false); continue; }
                var def = _roster.AllCharacters[i];
                card.Setup(def, () => SelectCharacter(def));
            }
        }

        private void BindDifficultyButtons()
        {
            for (int i = 0; i < _difficultyButtons.Count; i++)
            {
                if (_difficultyButtons[i] == null) continue;
                if (i >= _difficulties.Count) { _difficultyButtons[i].gameObject.SetActive(false); continue; }
                var diff = _difficulties[i];
                if (i < _difficultyLabels.Count && _difficultyLabels[i] != null)
                    _difficultyLabels[i].text = diff != null ? diff.DifficultyName : "?";
                _difficultyButtons[i].onClick.AddListener(() => SelectDifficulty(diff));
            }
        }

        private void SelectCharacter(CharacterDefinitionSO def)
        {
            _selectedCharacter = def;
            foreach (var card in _characterCards)
                if (card != null) card.SetSelected(card.Data == def);
            if (_previewRig != null) _previewRig.Show(def);
            RefreshSummary();
        }

        private void SelectDifficulty(DifficultySettingsSO diff)
        {
            _selectedDifficulty = diff;
            for (int i = 0; i < _difficultyButtons.Count && i < _difficulties.Count; i++)
            {
                var btn = _difficultyButtons[i];
                if (btn == null) continue;
                var img = btn.GetComponent<Image>();
                if (img != null)
                    img.color = _difficulties[i] == diff ? SelectedColor : UnselectedColor;
            }
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            if (_selectionSummary == null) return;
            string charName = _selectedCharacter != null ? _selectedCharacter.CharacterName : "—";
            string diffName = _selectedDifficulty != null ? _selectedDifficulty.DifficultyName : "—";
            _selectionSummary.text = $"{charName}  ·  {diffName}";
        }

        private void OnStart()
        {
            _ = StartRunAsync();
        }

        private async Awaitable StartRunAsync()
        {
            if (_selectedCharacter == null || _selectedDifficulty == null) return;

            AudioManager.Music(MusicMood.Gameplay);

            if (ServiceLocator.TryGet<PlayerStats>(out var stats))
                stats.ApplyCharacterBase(_selectedCharacter);

            if (ServiceLocator.TryGet<RunStats>(out var runStats))
                runStats.StartRun(_selectedCharacter.CharacterName, _selectedDifficulty.DifficultyName);

            if (ServiceLocator.TryGet<StageManager>(out var sm) && sm.PendingStage != null)
                await sm.LoadStage(sm.PendingStage);

            if (ServiceLocator.TryGet<WaveManager>(out var wm))
            {
                wm.SetDifficulty(_selectedDifficulty);
                if (_selectedCharacter.StartingWeapon != null &&
                    ServiceLocator.TryGet<WeaponSlotManager>(out var weapons) &&
                    !weapons.HasWeapon(_selectedCharacter.StartingWeapon))
                {
                    weapons.TryAddWeapon(_selectedCharacter.StartingWeapon);
                }
                Hide();
                Time.timeScale = 1f;
                if (!wm.IsWaveActive) wm.StartNextWave();
            }
            else
            {
                Hide();
                Time.timeScale = 1f;
            }
        }

        private void OnBack()
        {
            Hide();
            if (ServiceLocator.TryGet<StageSelect.StageSelectUI>(out var ss))
            {
                ss.Show();
                return;
            }
            if (ServiceLocator.TryGet<UI.MainMenu.MainMenuUI>(out var menu))
                menu.Show();
        }
    }
}

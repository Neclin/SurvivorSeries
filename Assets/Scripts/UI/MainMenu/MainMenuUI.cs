using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Audio;
using SurvivorSeries.Utilities;
using SurvivorSeries.Waves;

namespace SurvivorSeries.UI.MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        private void Awake()
        {
            ServiceLocator.Register<MainMenuUI>(this);
            if (_titleText != null) _titleText.text = "SURVIVOR SERIES";
            if (_playButton != null) _playButton.onClick.AddListener(OnPlay);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuit);
        }

        private void Start()
        {
            Show();
        }

        private void OnDestroy() => ServiceLocator.Unregister<MainMenuUI>();

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            Time.timeScale = 0f;
            AudioManager.Music(MusicMood.Menu);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnPlay()
        {
            Hide();
            if (ServiceLocator.TryGet<StageSelect.StageSelectUI>(out var ss))
            {
                ss.Show();
                return;
            }
            if (ServiceLocator.TryGet<CharacterSelect.CharacterSelectUI>(out var cs))
            {
                cs.Show();
                return;
            }
            if (ServiceLocator.TryGet<WaveManager>(out var wm) && !wm.IsWaveActive)
                wm.StartNextWave();
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

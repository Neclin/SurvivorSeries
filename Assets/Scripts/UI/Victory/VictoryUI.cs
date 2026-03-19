using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Audio;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.Victory
{
    public class VictoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            ServiceLocator.Register<VictoryUI>(this);
            if (_panel != null) _panel.SetActive(false);
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetry);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenu);
        }

        private void OnDestroy() => ServiceLocator.Unregister<VictoryUI>();

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            if (_titleText != null) _titleText.text = "VICTORY!";
            Time.timeScale = 0f;
            AudioManager.Music(MusicMood.None);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnRetry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

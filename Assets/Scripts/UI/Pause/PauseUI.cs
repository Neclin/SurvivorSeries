using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.Pause
{
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _menuButton;

        private bool _paused;

        private void Awake()
        {
            ServiceLocator.Register<PauseUI>(this);
            if (_panel != null) _panel.SetActive(false);
            if (_titleText != null) _titleText.text = "PAUSED";
            if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
            if (_menuButton != null) _menuButton.onClick.AddListener(Menu);
        }

        private void OnDestroy() => ServiceLocator.Unregister<PauseUI>();

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (_paused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            if (BlockedByOtherUI()) return;
            _paused = true;
            if (_panel != null) _panel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            _paused = false;
            if (_panel != null) _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void Menu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private bool BlockedByOtherUI()
        {
            if (ServiceLocator.TryGet<UI.GameOver.GameOverUI>(out _) == false) { }
            if (ServiceLocator.TryGet<UI.Victory.VictoryUI>(out var v) && v != null && v.gameObject.activeSelf)
            {
                var p = v.transform.Find("Panel");
                if (p != null && p.gameObject.activeSelf) return true;
            }
            if (ServiceLocator.TryGet<UI.Shop.ShopUI>(out var s))
            {
                var p = s.transform.Find("Panel");
                if (p != null && p.gameObject.activeSelf) return true;
            }
            if (ServiceLocator.TryGet<UI.LevelUp.LevelUpUI>(out var l))
            {
                var p = l.transform.Find("Panel");
                if (p != null && p.gameObject.activeSelf) return true;
            }
            if (ServiceLocator.TryGet<UI.MainMenu.MainMenuUI>(out var m))
            {
                var p = m.transform.Find("Panel");
                if (p != null && p.gameObject.activeSelf) return true;
            }
            if (ServiceLocator.TryGet<UI.CharacterSelect.CharacterSelectUI>(out var cs))
            {
                var p = cs.transform.Find("Panel");
                if (p != null && p.gameObject.activeSelf) return true;
            }
            if (ServiceLocator.TryGet<UI.StageSelect.StageSelectUI>(out var ss))
            {
                var p = ss.transform.Find("Panel");
                if (p != null && p.gameObject.activeSelf) return true;
            }
            return false;
        }
    }
}

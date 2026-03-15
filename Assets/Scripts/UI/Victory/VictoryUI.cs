using UnityEngine;
using TMPro;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.Victory
{
    public class VictoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;

        private void Awake()
        {
            ServiceLocator.Register<VictoryUI>(this);
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnDestroy() => ServiceLocator.Unregister<VictoryUI>();

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            if (_titleText != null) _titleText.text = "VICTORY!";
            Time.timeScale = 0f;
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}

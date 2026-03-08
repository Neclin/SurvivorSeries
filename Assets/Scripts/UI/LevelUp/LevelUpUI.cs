using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.LevelUp;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.LevelUp
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private LevelUpCardUI[] _cards;

        private LevelUpManager _manager;
        private List<UpgradeOption> _currentOptions;

        private void Awake()
        {
            ServiceLocator.Register<LevelUpUI>(this);
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnDestroy() => ServiceLocator.Unregister<LevelUpUI>();

        private void Start()
        {
            if (ServiceLocator.TryGet<LevelUpManager>(out _manager))
                _manager.OnOptionsReady += HandleOptionsReady;
        }

        private void HandleOptionsReady(List<UpgradeOption> options)
        {
            _currentOptions = options;

            if (options == null || options.Count == 0)
            {
                if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                    gm.OnLevelUpComplete();
                return;
            }

            Show();
        }

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            if (_titleText != null) _titleText.text = "LEVEL UP!";

            for (int i = 0; i < _cards.Length; i++)
            {
                if (i < _currentOptions.Count)
                    _cards[i].Populate(_currentOptions[i], OnCardChosen);
                else
                    _cards[i].SetEmpty();
            }

            Time.timeScale = 0f;

            if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                gm.OnLevelUp();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnCardChosen(UpgradeOption option)
        {
            Hide();
            _manager?.ApplyOption(option);
        }
    }
}

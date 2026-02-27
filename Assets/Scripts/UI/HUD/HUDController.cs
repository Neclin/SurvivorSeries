using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.UI.HUD
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _xpText;
        [SerializeField] private Image _xpFill;
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _levelUpNotification;

        private void Start()
        {
            if (ServiceLocator.TryGet<Player.PlayerLevelSystem>(out var levelSys))
            {
                levelSys.OnLevelUp += OnLevelUp;
                levelSys.OnXPChanged += OnXPChanged;
                UpdateLevel(levelSys.Level);
                UpdateXP(levelSys.CurrentXP, levelSys.XPToNextLevel);
            }

            if (ServiceLocator.TryGet<Waves.WaveManager>(out var waveMgr))
            {
                waveMgr.OnWaveStarted += OnWaveStarted;
                UpdateWave(waveMgr.CurrentWave);
            }

            if (ServiceLocator.TryGet<Player.PlayerCurrencyHandler>(out var currency))
            {
                currency.OnCurrencyChanged += UpdateCurrency;
                UpdateCurrency(currency.Currency);
            }

            if (_levelUpNotification != null)
                _levelUpNotification.gameObject.SetActive(false);
        }

        private void OnLevelUp(int level)
        {
            UpdateLevel(level);
            _ = ShowLevelUpNotification();
        }

        private void OnXPChanged(float current, float required) => UpdateXP(current, required);
        private void OnWaveStarted(int wave) => UpdateWave(wave);

        private void UpdateLevel(int level)
        {
            if (_levelText != null) _levelText.text = $"LVL {level}";
        }

        private void UpdateXP(float current, float required)
        {
            if (_xpFill != null)
                _xpFill.fillAmount = required > 0 ? current / required : 0f;
            if (_xpText != null)
                _xpText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(required)} XP";
        }

        private void UpdateWave(int wave)
        {
            if (_waveText != null) _waveText.text = $"WAVE {wave}";
        }

        private void UpdateCurrency(int amount)
        {
            if (_currencyText != null) _currencyText.text = $"{amount}g";
        }

        private async Awaitable ShowLevelUpNotification()
        {
            if (_levelUpNotification == null) return;
            _levelUpNotification.gameObject.SetActive(true);
            await Awaitable.WaitForSecondsAsync(2f);
            if (_levelUpNotification != null)
                _levelUpNotification.gameObject.SetActive(false);
        }
    }
}

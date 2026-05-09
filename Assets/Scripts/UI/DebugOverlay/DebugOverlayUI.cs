using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using SurvivorSeries.Achievements;
using SurvivorSeries.Enemies;
using SurvivorSeries.Persistence;
using SurvivorSeries.Stages;
using SurvivorSeries.Stages.Data;
using SurvivorSeries.Passives;
using SurvivorSeries.Passives.Data;
using SurvivorSeries.Utilities;
using SurvivorSeries.Waves;
using SurvivorSeries.Weapons;

namespace SurvivorSeries.UI.DebugOverlay
{
    public class DebugOverlayUI : MonoBehaviour
    {
        [SerializeField] private Key _toggleKey = Key.F1;
        [SerializeField] private StageRoster _stageRoster;

        private GameObject _panel;
        private TextMeshProUGUI _statusText;

        private void Awake()
        {
            BuildUI();
            SetVisible(false);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb[_toggleKey].wasPressedThisFrame)
                SetVisible(_panel != null && !_panel.activeSelf);
        }

        private void SetVisible(bool on)
        {
            if (_panel != null) _panel.SetActive(on);
            if (on) RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (_statusText == null) return;
            string wave = ServiceLocator.TryGet<WaveManager>(out var wm) ? $"Wave {wm.CurrentWave}/{wm.TotalWaves} active={wm.IsWaveActive}" : "Wave —";
            string stage = ServiceLocator.TryGet<StageManager>(out var sm) && sm.CurrentStage != null ? sm.CurrentStage.DisplayName : "—";
            _statusText.text = $"{wave}\nStage: {stage}";
        }

        private void BuildUI()
        {
            var canvasGO = new GameObject("DebugOverlay_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0f;

            _panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            _panel.transform.SetParent(canvasGO.transform, false);
            var panelRT = (RectTransform)_panel.transform;
            panelRT.anchorMin = new Vector2(0f, 1f);
            panelRT.anchorMax = new Vector2(0f, 1f);
            panelRT.pivot = new Vector2(0f, 1f);
            panelRT.anchoredPosition = new Vector2(20f, -20f);
            panelRT.sizeDelta = new Vector2(460f, 0f);
            _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
            var vlg = _panel.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 6f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            var fitter = _panel.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            AddLabel(_panel.transform, "DEBUG (F1)", 24, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f));
            _statusText = AddLabel(_panel.transform, "", 16, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f, 1f));

            AddSection(_panel.transform, "WAVES");
            AddButtonsInColumns(_panel.transform, new (string, System.Action)[]
            {
                ("Skip Wave", OnSkipWave),
                ("Skip to Boss", OnSkipToBoss),
                ("Spawn Boss", OnSpawnBoss),
                ("Kill All", OnKillAll),
            });

            AddSection(_panel.transform, "LOADOUT");
            AddButtonsInColumns(_panel.transform, new (string, System.Action)[]
            {
                ("Max Weapons", OnMaxAllWeapons),
                ("Max Passives", OnMaxAllPassives),
            });

            AddSection(_panel.transform, "PROGRESSION");
            AddButtonsInColumns(_panel.transform, new (string, System.Action)[]
            {
                ("Unlock All Ach.", OnUnlockAll),
                ("Reset Save", OnResetSave),
            });

            AddSection(_panel.transform, "STAGE");
            BuildStageRow(_panel.transform);
        }

        private void AddButtonsInColumns(Transform parent, (string label, System.Action action)[] items, int columns = 2)
        {
            for (int i = 0; i < items.Length; i += columns)
            {
                var row = CreateButtonRow(parent);
                for (int j = 0; j < columns; j++)
                {
                    int idx = i + j;
                    if (idx >= items.Length)
                    {
                        var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
                        spacer.transform.SetParent(row.transform, false);
                        spacer.GetComponent<LayoutElement>().flexibleWidth = 1f;
                        continue;
                    }
                    var item = items[idx];
                    AddButton(row.transform, item.label, item.action);
                }
            }
        }

        private GameObject CreateButtonRow(Transform parent)
        {
            var row = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;
            row.GetComponent<LayoutElement>().preferredHeight = 36f;
            return row;
        }

        private void BuildStageRow(Transform parent)
        {
            var row = new GameObject("StageRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandHeight = false;
            row.GetComponent<LayoutElement>().preferredHeight = 36f;

            if (_stageRoster != null && _stageRoster.AllStages != null)
            {
                foreach (var stage in _stageRoster.AllStages)
                {
                    if (stage == null) continue;
                    var captured = stage;
                    AddButton(row.transform, ShortStageName(stage.DisplayName), () => OnChangeStage(captured), 32);
                }
            }
        }

        private static string ShortStageName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?";
            int space = name.IndexOf(' ');
            return space > 0 ? name.Substring(0, space) : name;
        }

        private TextMeshProUGUI AddLabel(Transform parent, string text, int size, FontStyles style, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.raycastTarget = false;
            go.GetComponent<LayoutElement>().preferredHeight = size + 10;
            return tmp;
        }

        private void AddSection(Transform parent, string text)
        {
            AddLabel(parent, text, 14, FontStyles.Bold, new Color(1f, 0.7f, 0.3f, 1f));
        }

        private Button AddButton(Transform parent, string label, System.Action onClick, int height = 36)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.20f, 0.25f, 0.35f, 1f);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => { onClick?.Invoke(); RefreshStatus(); });

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.raycastTarget = false;
            return btn;
        }

        private void OnSkipWave()
        {
            if (!ServiceLocator.TryGet<WaveManager>(out var wm)) return;
            if (wm.IsWaveActive) wm.DebugEndWaveNow();
            else wm.StartNextWave();
        }

        private void OnSkipToBoss()
        {
            if (!ServiceLocator.TryGet<WaveManager>(out var wm)) return;
            wm.DebugSetCurrentWave(wm.TotalWaves - 1);
            if (wm.IsWaveActive) wm.DebugEndWaveNow();
            else wm.StartNextWave();
        }

        private void OnSpawnBoss()
        {
            if (ServiceLocator.TryGet<BossSpawner>(out var bs)) bs.SpawnBoss();
        }

        private void OnKillAll()
        {
            var pools = FindObjectsByType<EnemyPool>(FindObjectsSortMode.None);
            foreach (var p in pools) p.DespawnAll();
        }

        private void OnMaxAllWeapons()
        {
            if (!ServiceLocator.TryGet<WeaponSlotManager>(out var wsm)) return;
            const int safetyCap = 32;
            foreach (var w in wsm.GetAllWeapons())
            {
                if (w == null) continue;
                int guard = 0;
                while (!w.IsMaxLevel && guard++ < safetyCap) w.LevelUp();
            }
        }

        private void OnMaxAllPassives()
        {
            if (!ServiceLocator.TryGet<PassiveSlotManager>(out var psm)) return;
            const int safetyCap = 32;
            for (int i = 0; i < PassiveSlotManager.MaxSlots; i++)
            {
                var data = psm.GetSlotData(i);
                if (data == null) continue;
                int guard = 0;
                while (!psm.HasMaxLevel(data) && guard++ < safetyCap)
                    if (!psm.TryLevelUpPassive(data)) break;
            }
        }

        private void OnUnlockAll()
        {
            if (!ServiceLocator.TryGet<UnlockRegistry>(out var unlocks)) return;
            if (!ServiceLocator.TryGet<AchievementsManager>(out var mgr)) return;
            foreach (var def in mgr.Definitions)
            {
                if (def == null) continue;
                unlocks.CompleteAchievement(def.AchievementID);
                if (def.UnlocksCharacter != null)
                    unlocks.UnlockCharacter(def.UnlocksCharacter.CharacterName);
            }
            unlocks.SaveNow();
        }

        private void OnResetSave()
        {
            if (ServiceLocator.TryGet<UnlockRegistry>(out var unlocks)) unlocks.DebugReset();
        }

        private async void OnChangeStage(StageDefinitionSO stage)
        {
            if (stage == null) return;
            if (!ServiceLocator.TryGet<StageManager>(out var sm)) return;
            await sm.LoadStage(stage);
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.UI.MainMenu;
using SurvivorSeries.UI.GameOver;
using SurvivorSeries.UI.Pause;
using SurvivorSeries.UI.Victory;
using SurvivorSeries.Waves;

namespace SurvivorSeriesEditor
{
    public static class BuildMenuPanels
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Survivor Series/Build Menu Panels")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            BuildMainMenu();
            BuildGameOver();
            BuildPause();
            UpgradeVictory();
            DisableWaveAutoStart();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Menus] Main, GameOver, Pause, Victory canvases built.");
        }

        private static void DisableWaveAutoStart()
        {
            var wm = Object.FindAnyObjectByType<WaveManager>();
            if (wm == null) return;
            var so = new SerializedObject(wm);
            so.FindProperty("_autoStartOnPlay").boolValue = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(wm);
        }

        private static void BuildMainMenu()
        {
            var canvas = ResetCanvas("MainMenu_Canvas", sortingOrder: 120);
            canvas.AddComponent<MainMenuUI>();

            var panel = MakePanel(canvas.transform, "Panel", new Color(0.05f, 0.05f, 0.08f, 1f));

            var title = MakeText(panel.transform, "Title", "SURVIVOR SERIES",
                fontSize: 96, color: new Color(1f, 0.9f, 0.4f, 1f), bold: true);
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -180), new Vector2(1200, 140));

            var play = MakeButton(panel.transform, "PlayBtn", "PLAY",
                new Color(0.3f, 0.6f, 0.3f, 1f));
            SetRect(play.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(360, 90));

            var quit = MakeButton(panel.transform, "QuitBtn", "QUIT",
                new Color(0.55f, 0.25f, 0.25f, 1f));
            SetRect(quit.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(360, 90));

            var ui = canvas.GetComponent<MainMenuUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_playButton").objectReferenceValue = play;
            so.FindProperty("_quitButton").objectReferenceValue = quit;
            so.ApplyModifiedProperties();

            panel.SetActive(true);
        }

        private static void BuildGameOver()
        {
            var canvas = ResetCanvas("GameOver_Canvas", sortingOrder: 80);
            canvas.AddComponent<GameOverUI>();

            var panel = MakePanel(canvas.transform, "Panel", new Color(0f, 0f, 0f, 0.92f));

            var title = MakeText(panel.transform, "Title", "GAME OVER",
                fontSize: 96, color: new Color(0.9f, 0.25f, 0.25f, 1f), bold: true);
            SetRect(title, new Vector2(0, 0.6f), new Vector2(1, 0.6f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(0, 140));

            var retry = MakeButton(panel.transform, "RetryBtn", "RETRY",
                new Color(0.3f, 0.6f, 0.3f, 1f));
            SetRect(retry.gameObject, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f),
                    new Vector2(0.5f, 0.5f), new Vector2(-200, 0), new Vector2(360, 90));

            var menu = MakeButton(panel.transform, "MenuBtn", "MAIN MENU",
                new Color(0.45f, 0.30f, 0.55f, 1f));
            SetRect(menu.gameObject, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f),
                    new Vector2(0.5f, 0.5f), new Vector2(200, 0), new Vector2(360, 90));

            var ui = canvas.GetComponent<GameOverUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_retryButton").objectReferenceValue = retry;
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static void BuildPause()
        {
            var canvas = ResetCanvas("Pause_Canvas", sortingOrder: 70);
            canvas.AddComponent<PauseUI>();

            var panel = MakePanel(canvas.transform, "Panel", new Color(0f, 0f, 0f, 0.85f));

            var title = MakeText(panel.transform, "Title", "PAUSED",
                fontSize: 80, color: Color.white, bold: true);
            SetRect(title, new Vector2(0, 0.6f), new Vector2(1, 0.6f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(0, 120));

            var resume = MakeButton(panel.transform, "ResumeBtn", "RESUME",
                new Color(0.3f, 0.6f, 0.3f, 1f));
            SetRect(resume.gameObject, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f),
                    new Vector2(0.5f, 0.5f), new Vector2(-200, 0), new Vector2(360, 90));

            var menu = MakeButton(panel.transform, "MenuBtn", "MAIN MENU",
                new Color(0.45f, 0.30f, 0.55f, 1f));
            SetRect(menu.gameObject, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f),
                    new Vector2(0.5f, 0.5f), new Vector2(200, 0), new Vector2(360, 90));

            var ui = canvas.GetComponent<PauseUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_resumeButton").objectReferenceValue = resume;
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static void UpgradeVictory()
        {
            var canvas = GameObject.Find("Victory_Canvas");
            if (canvas == null) { Debug.LogWarning("[Menus] Victory_Canvas missing"); return; }
            var ui = canvas.GetComponent<VictoryUI>();
            var panel = canvas.transform.Find("Panel")?.gameObject;
            if (ui == null || panel == null) return;

            var existingRetry = panel.transform.Find("RetryBtn");
            if (existingRetry != null) Object.DestroyImmediate(existingRetry.gameObject);
            var existingMenu = panel.transform.Find("MenuBtn");
            if (existingMenu != null) Object.DestroyImmediate(existingMenu.gameObject);

            var retry = MakeButton(panel.transform, "RetryBtn", "PLAY AGAIN",
                new Color(0.3f, 0.6f, 0.3f, 1f));
            SetRect(retry.gameObject, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f),
                    new Vector2(0.5f, 0.5f), new Vector2(-200, 0), new Vector2(360, 90));

            var menu = MakeButton(panel.transform, "MenuBtn", "MAIN MENU",
                new Color(0.45f, 0.30f, 0.55f, 1f));
            SetRect(menu.gameObject, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f),
                    new Vector2(0.5f, 0.5f), new Vector2(200, 0), new Vector2(360, 90));

            var so = new SerializedObject(ui);
            so.FindProperty("_retryButton").objectReferenceValue = retry;
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.ApplyModifiedProperties();
        }

        private static GameObject ResetCanvas(string name, int sortingOrder)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(name,
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortingOrder;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            return go;
        }

        private static GameObject MakePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.sprite = GenerateFlatSprite.EnsureFlatSprite();
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return panel;
        }

        private static GameObject MakeText(Transform parent, string name, string text,
                                           int fontSize, Color color, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAlignmentOptions.Center;
            t.color = color;
            if (bold) t.fontStyle = FontStyles.Bold;
            return go;
        }

        private static Button MakeButton(Transform parent, string name, string label, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.sprite = GenerateFlatSprite.EnsureFlatSprite();
            var btn = go.AddComponent<Button>();
            var lbl = MakeText(go.transform, "Label", label, 32, Color.white, true);
            var lr = lbl.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.sizeDelta = Vector2.zero;
            return btn;
        }

        private static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
                                    Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = pivot;
            r.anchoredPosition = anchoredPos;
            r.sizeDelta = size;
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SurvivorSeries.Audio;

namespace SurvivorSeriesEditor
{
    public static class WireButtonSfx
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Survivor Series/Wire Button Click SFX")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            int added = 0;
            int already = 0;
            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in buttons)
            {
                if (btn.GetComponent<ButtonClickSfx>() != null) { already++; continue; }
                btn.gameObject.AddComponent<ButtonClickSfx>();
                added++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[ButtonSfx] {buttons.Length} buttons found. Added: {added}, already wired: {already}.");
        }
    }
}

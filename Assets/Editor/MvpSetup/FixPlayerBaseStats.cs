using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurvivorSeries.Player;

namespace SurvivorSeriesEditor
{
    public static class FixPlayerBaseStats
    {
        [MenuItem("Survivor Series/Fix Player Base Stats")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var player = GameObject.Find("Player");
            if (player == null) { Debug.LogError("Player not found"); return; }

            var stats = player.GetComponent<PlayerStats>();
            if (stats == null) { Debug.LogError("PlayerStats not found"); return; }

            var so = new SerializedObject(stats);
            so.FindProperty("_baseMaxHealth").floatValue = 60f;
            so.FindProperty("_baseDamage").floatValue = 8f;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(stats);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[PlayerStats] Reset to base 60 HP / 8 dmg.");
        }
    }
}

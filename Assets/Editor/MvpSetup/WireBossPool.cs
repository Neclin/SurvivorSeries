using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurvivorSeries.Enemies;
using SurvivorSeries.Enemies.Data;

namespace SurvivorSeriesEditor
{
    public static class WireBossPool
    {
        [MenuItem("Survivor Series/Wire Boss Pool")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var spawner = Object.FindAnyObjectByType<BossSpawner>();
            if (spawner == null) { Debug.LogError("[WireBoss] No BossSpawner"); return; }

            string[] paths =
            {
                "Assets/ScriptableObjects/Bosses/SO_Boss_MushroomKing.asset",
                "Assets/ScriptableObjects/Bosses/SO_Boss_Demon.asset",
                "Assets/ScriptableObjects/Bosses/SO_Boss_Yeti.asset",
                "Assets/ScriptableObjects/Bosses/SO_Boss_Orc.asset",
                "Assets/ScriptableObjects/Bosses/SO_Boss_Dragon.asset",
            };

            var so = new SerializedObject(spawner);
            var arr = so.FindProperty("_bossPool");
            arr.arraySize = paths.Length;
            int wired = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(paths[i]);
                arr.GetArrayElementAtIndex(i).objectReferenceValue = data;
                if (data != null) wired++;
                else Debug.LogWarning($"[WireBoss] Missing {paths[i]}");
            }
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(spawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[WireBoss] Wired {wired}/{paths.Length} bosses into BossSpawner.");
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeriesEditor
{
    public static class AddHitFlash
    {
        [MenuItem("Survivor Series/Add HitFlash to Player + Enemies")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var player = GameObject.Find("Player");
            if (player != null)
            {
                var visual = player.transform.Find("Visual");
                if (visual != null)
                {
                    if (visual.GetComponent<HitFlash>() == null)
                        visual.gameObject.AddComponent<HitFlash>();
                    EditorUtility.SetDirty(visual.gameObject);
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AddToPrefab("Assets/Prefabs/Enemies/Walker.prefab");
        }

        private static void AddToPrefab(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var visual = root.transform.Find("Visual");
                if (visual == null) return;
                if (visual.GetComponent<HitFlash>() == null)
                    visual.gameObject.AddComponent<HitFlash>();
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[HitFlash] Added to {path}");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}

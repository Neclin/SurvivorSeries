using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivorSeriesEditor
{
    public static class ConvertUiToFlat
    {
        [MenuItem("Survivor Series/Convert UI To Flat Colors")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var flat = GenerateFlatSprite.EnsureFlatSprite();

            int count = 0;
            foreach (var img in Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                img.sprite = flat;
                EditorUtility.SetDirty(img);
                count++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[FlatUI] Replaced sprite on {count} Image components.");
        }
    }
}

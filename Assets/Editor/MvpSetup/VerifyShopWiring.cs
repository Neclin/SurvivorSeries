using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurvivorSeries.Shop;

namespace SurvivorSeriesEditor
{
    public static class VerifyShopWiring
    {
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var gen = Object.FindAnyObjectByType<ShopInventoryGenerator>();
            if (gen == null) { Debug.LogError("[Verify] No ShopInventoryGenerator"); return; }

            var so = new SerializedObject(gen);
            var w = so.FindProperty("_allWeapons");
            var p = so.FindProperty("_allPassives");

            Debug.Log($"[Verify] _allWeapons size={w.arraySize}");
            for (int i = 0; i < w.arraySize; i++)
            {
                var v = w.GetArrayElementAtIndex(i).objectReferenceValue;
                Debug.Log($"  [{i}] {(v != null ? v.name : "<null>")}");
            }

            Debug.Log($"[Verify] _allPassives size={p.arraySize}");
            for (int i = 0; i < p.arraySize; i++)
            {
                var v = p.GetArrayElementAtIndex(i).objectReferenceValue;
                Debug.Log($"  [{i}] {(v != null ? v.name : "<null>")}");
            }
        }
    }
}

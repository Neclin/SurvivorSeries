using UnityEditor;
using UnityEngine;

namespace SurvivorSeriesEditor
{
    public static class AddObstacleLayer
    {
        public const string LayerName = "Obstacle";

        [MenuItem("Survivor Series/Add Obstacle Layer")]
        public static void Run()
        {
            int index = LayerMask.NameToLayer(LayerName);
            if (index >= 0) { Debug.Log($"[Layers] '{LayerName}' already exists at index {index}."); return; }

            var tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            var so = new SerializedObject(tagManager);
            var layers = so.FindProperty("layers");

            for (int i = 8; i < layers.arraySize; i++)
            {
                var element = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(element.stringValue))
                {
                    element.stringValue = LayerName;
                    so.ApplyModifiedProperties();
                    Debug.Log($"[Layers] Added '{LayerName}' at index {i}.");
                    return;
                }
            }
            Debug.LogError("[Layers] No free user layer slot found.");
        }
    }
}

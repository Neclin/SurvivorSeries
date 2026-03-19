using UnityEditor;
using UnityEngine;
using SurvivorSeries.Audio;

namespace SurvivorSeriesEditor
{
    public static class VerifyAudio
    {
        [MenuItem("Survivor Series/Verify Audio")]
        public static void Run()
        {
            var lib = AssetDatabase.LoadAssetAtPath<SfxLibrary>("Assets/ScriptableObjects/Audio/SO_SfxLibrary.asset");
            if (lib == null) { Debug.LogError("[Verify] SfxLibrary missing"); return; }
            var so = new SerializedObject(lib);
            var arr = so.FindProperty("_entries");
            for (int i = 0; i < arr.arraySize; i++)
            {
                var e = arr.GetArrayElementAtIndex(i);
                var idProp = e.FindPropertyRelative("Id");
                var clipsProp = e.FindPropertyRelative("Clips");
                Debug.Log($"[Verify] {(SfxId)idProp.enumValueIndex}: {clipsProp.arraySize} clips");
            }
        }
    }
}

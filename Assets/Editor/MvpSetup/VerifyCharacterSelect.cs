using UnityEditor;
using UnityEngine;
using SurvivorSeries.UI.CharacterSelect;

namespace SurvivorSeriesEditor
{
    public static class VerifyCharacterSelect
    {
        [MenuItem("Survivor Series/Verify Character Select")]
        public static void Run()
        {
            var canvas = GameObject.Find("CharacterSelect_Canvas");
            if (canvas == null) { Debug.LogError("[Verify] CharacterSelect_Canvas missing"); return; }
            var ui = canvas.GetComponent<CharacterSelectUI>();
            if (ui == null) { Debug.LogError("[Verify] CharacterSelectUI missing"); return; }

            var so = new SerializedObject(ui);
            DumpList(so, "_difficulties");
            DumpList(so, "_difficultyButtons");
            DumpList(so, "_difficultyLabels");
            DumpList(so, "_characterCards");
        }

        private static void DumpList(SerializedObject so, string name)
        {
            var p = so.FindProperty(name);
            if (p == null) { Debug.Log($"[Verify] {name}: PROPERTY NOT FOUND"); return; }
            Debug.Log($"[Verify] {name}: type={p.propertyType} isArray={p.isArray} size={p.arraySize}");
            for (int i = 0; i < p.arraySize; i++)
            {
                var e = p.GetArrayElementAtIndex(i);
                var refVal = e.objectReferenceValue;
                Debug.Log($"  [{i}] {(refVal == null ? "null" : refVal.name + " (" + refVal.GetType().Name + ")")}");
            }
        }
    }
}

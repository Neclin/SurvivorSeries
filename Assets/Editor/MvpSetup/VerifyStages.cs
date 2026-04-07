using UnityEditor;
using UnityEngine;
using SurvivorSeries.Stages;
using SurvivorSeries.Stages.Data;
using SurvivorSeries.UI.StageSelect;

namespace SurvivorSeriesEditor
{
    public static class VerifyStages
    {
        [MenuItem("Survivor Series/Verify Stages")]
        public static void Run()
        {
            var roster = AssetDatabase.LoadAssetAtPath<StageRoster>("Assets/ScriptableObjects/Stages/SO_StageRoster.asset");
            if (roster == null) { Debug.LogError("[Verify] StageRoster missing"); return; }
            Debug.Log($"[Verify] Roster has {roster.AllStages.Count} stages.");
            foreach (var s in roster.AllStages)
            {
                if (s == null) { Debug.LogWarning("[Verify] Null stage entry"); continue; }
                Debug.Log($"[Verify]   {s.StageID} '{s.DisplayName}': {s.ObstaclePrefabs?.Length ?? 0} prefabs, density={s.Density}, unlock={(string.IsNullOrEmpty(s.UnlockAchievementID) ? "always" : s.UnlockAchievementID)}, mat={(s.GroundMaterial != null ? s.GroundMaterial.name : "<missing>")}");
            }

            var sm = Object.FindAnyObjectByType<StageManager>();
            if (sm == null) Debug.LogError("[Verify] StageManager missing in scene");
            else Debug.Log($"[Verify] StageManager present, roster={(sm.Roster != null ? sm.Roster.name : "<missing>")}");

            var ssCanvas = GameObject.Find("StageSelect_Canvas");
            if (ssCanvas == null) Debug.LogError("[Verify] StageSelect_Canvas missing");
            else
            {
                var ui = ssCanvas.GetComponent<StageSelectUI>();
                Debug.Log($"[Verify] StageSelect_Canvas present, UI={(ui != null ? "wired" : "MISSING")}");
            }
        }
    }
}

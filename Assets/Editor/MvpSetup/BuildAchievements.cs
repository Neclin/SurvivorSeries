using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurvivorSeries.Achievements;
using SurvivorSeries.Achievements.Data;
using SurvivorSeries.Persistence;

namespace SurvivorSeriesEditor
{
    public static class BuildAchievements
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";
        private const string AchDir = "Assets/ScriptableObjects/Achievements";

        [MenuItem("Survivor Series/Build Achievements + Persistence")]
        public static void Run()
        {
            EnsureFolders();
            var defs = EnsureAchievements();

            var scene = EditorSceneManager.OpenScene(ScenePath);
            EnsureSceneSingleton<UnlockRegistry>("UnlockRegistry");
            EnsureSceneSingleton<RunStats>("RunStats");
            var am = EnsureSceneSingleton<AchievementsManager>("AchievementsManager");

            SetPrivateField(am, "_definitions", new List<AchievementDefinitionSO>(defs));
            EditorUtility.SetDirty(am);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Achievements] Built with {defs.Count} achievements and registered runtime singletons.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(AchDir))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Achievements");
        }

        private static List<AchievementDefinitionSO> EnsureAchievements()
        {
            var list = new List<AchievementDefinitionSO>
            {
                Upsert("SO_Achievement_Wave5", a =>
                {
                    a.AchievementID = "wave5";
                    a.Title = "First Steps";
                    a.Description = "Reach Wave 5 in any run.";
                    a.ConditionType = AchievementConditionType.ReachWave;
                    a.TargetValue = 5;
                }),
                Upsert("SO_Achievement_BossSlayer", a =>
                {
                    a.AchievementID = "boss_slayer";
                    a.Title = "Boss Slayer";
                    a.Description = "Defeat the boss for the first time.";
                    a.ConditionType = AchievementConditionType.DefeatBoss;
                    a.TargetValue = 1;
                }),
                Upsert("SO_Achievement_Survivor", a =>
                {
                    a.AchievementID = "survivor_10min";
                    a.Title = "Survivor";
                    a.Description = "Survive for 10 minutes in a single run.";
                    a.ConditionType = AchievementConditionType.SurviveMinutes;
                    a.TargetValue = 10;
                }),
                Upsert("SO_Achievement_Hunter", a =>
                {
                    a.AchievementID = "hunter_500";
                    a.Title = "Hunter";
                    a.Description = "Kill 500 enemies in a single run.";
                    a.ConditionType = AchievementConditionType.KillCount;
                    a.TargetValue = 500;
                }),
                Upsert("SO_Achievement_MaxedOut", a =>
                {
                    a.AchievementID = "maxed_out";
                    a.Title = "Maxed Out";
                    a.Description = "Fill all 6 weapon slots in one run.";
                    a.ConditionType = AchievementConditionType.FillWeaponSlots;
                    a.TargetValue = 6;
                }),
            };
            return list;
        }

        private static AchievementDefinitionSO Upsert(string fileName, System.Action<AchievementDefinitionSO> apply)
        {
            var path = $"{AchDir}/{fileName}.asset";
            var def = AssetDatabase.LoadAssetAtPath<AchievementDefinitionSO>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<AchievementDefinitionSO>();
                AssetDatabase.CreateAsset(def, path);
            }
            apply(def);
            EditorUtility.SetDirty(def);
            return def;
        }

        private static T EnsureSceneSingleton<T>(string name) where T : Component
        {
            var existing = Object.FindAnyObjectByType<T>();
            if (existing != null) return existing;
            var go = GameObject.Find(name) ?? new GameObject(name);
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        private static void SetPrivateField(Object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (f == null) { Debug.LogError($"[Achievements] Field '{fieldName}' not found"); return; }
            f.SetValue(target, value);
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurvivorSeries.Weapons.Data;

namespace SurvivorSeriesEditor
{
    public static class StartingWeaponPresets
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Survivor Series/Starting Weapons/None (character's weapon only)")]
        public static void None() => Apply(new string[0]);

        [MenuItem("Survivor Series/Starting Weapons/Two Bows (combine test)")]
        public static void TwoBows() => Apply(new[]
        {
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Bow.asset",
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Bow.asset",
        });

        [MenuItem("Survivor Series/Starting Weapons/Bow only")]
        public static void BowOnly() => Apply(new[]
        {
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Bow.asset",
        });

        [MenuItem("Survivor Series/Starting Weapons/All weapons")]
        public static void AllWeapons() => Apply(new[]
        {
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Bow.asset",
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Sword.asset",
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Dagger.asset",
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Axe.asset",
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Spellbook.asset",
            "Assets/ScriptableObjects/Weapons/SO_Weapon_Staff.asset",
        });

        private static void Apply(string[] paths)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var player = GameObject.Find("Player");
            if (player == null) { Debug.LogError("Player not found"); return; }
            var bs = player.GetComponent<SurvivorSeries.Core.GameplayBootstrap>();
            if (bs == null) { Debug.LogError("GameplayBootstrap missing"); return; }

            var so = new SerializedObject(bs);
            var list = so.FindProperty("_startingWeapons");
            list.arraySize = paths.Length;
            for (int i = 0; i < paths.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<WeaponDataSO>(paths[i]);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[StartingWeapons] Set to {paths.Length} weapon(s).");
        }
    }
}

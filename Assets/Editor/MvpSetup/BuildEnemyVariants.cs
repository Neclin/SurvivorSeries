using System.IO;
using UnityEditor;
using UnityEngine;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeriesEditor
{
    public static class BuildEnemyVariants
    {
        [MenuItem("Survivor Series/Build Enemy Variants")]
        public static void Run()
        {
            const string walkerPath = "Assets/Prefabs/Enemies/Walker.prefab";

            BuildVariant(walkerPath, "Speeder",
                "Assets/Models/KayKit_Skeletons_1.1_FREE/characters/fbx/Skeleton_Rogue.fbx",
                visualScale: 1f,
                replaceVariant: typeof(SurvivorSeries.Enemies.Variants.SpeederEnemy));

            BuildVariant(walkerPath, "Ranged",
                "Assets/Models/KayKit_Skeletons_1.1_FREE/characters/fbx/Skeleton_Mage.fbx",
                visualScale: 1f,
                replaceVariant: typeof(SurvivorSeries.Enemies.Variants.RangedEnemy));

            BuildVariant(walkerPath, "Brute",
                "Assets/Models/KayKit_Skeletons_1.1_FREE/characters/fbx/Skeleton_Warrior.fbx",
                visualScale: 1.25f,
                replaceVariant: typeof(SurvivorSeries.Enemies.Variants.BruteEnemy));

            BuildVariant(walkerPath, "Bomber",
                "Assets/Models/Ultimate Monsters/Blob/FBX/Mushnub.fbx",
                visualScale: 1f, visualY: 0f,
                replaceVariant: typeof(SurvivorSeries.Enemies.Variants.BomberEnemy));

            BuildVariant(walkerPath, "Elite",
                "Assets/Models/Ultimate Monsters/Big/FBX/Demon.fbx",
                visualScale: 1.4f, visualY: 0f,
                replaceVariant: typeof(SurvivorSeries.Enemies.Variants.EliteEnemy));

            WireDataPrefabs();
            AssetDatabase.SaveAssets();
            Debug.Log("[EnemyVariants] Built and wired.");
        }

        private static void BuildVariant(string sourcePrefabPath, string variantName,
                                         string fbxPath, float visualScale,
                                         System.Type replaceVariant,
                                         float visualY = -1f)
        {
            string outPath = $"Assets/Prefabs/Enemies/{variantName}.prefab";
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (src == null) { Debug.LogError($"[EnemyVariants] Source not found: {sourcePrefabPath}"); return; }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(outPath) != null)
                AssetDatabase.DeleteAsset(outPath);

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                inst.name = variantName;

                var existingScripts = inst.GetComponents<SurvivorSeries.Enemies.EnemyBase>();
                foreach (var s in existingScripts) Object.DestroyImmediate(s);

                var newComp = inst.AddComponent(replaceVariant);
                if (newComp == null)
                {
                    Debug.LogError($"[EnemyVariants] Failed to add {replaceVariant.Name}");
                    return;
                }

                var visualT = inst.transform.Find("Visual");
                if (visualT != null) Object.DestroyImmediate(visualT.gameObject);

                var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbx == null) Debug.LogError($"[EnemyVariants] FBX not found: {fbxPath}");
                else
                {
                    var v = (GameObject)PrefabUtility.InstantiatePrefab(fbx, inst.transform);
                    v.name = "Visual";
                    v.transform.localPosition = new Vector3(0f, visualY, 0f);
                    v.transform.localRotation = Quaternion.identity;
                    v.transform.localScale = Vector3.one * visualScale;
                    if (v.GetComponent<HitFlash>() == null) v.AddComponent<HitFlash>();
                }

                PrefabUtility.SaveAsPrefabAsset(inst, outPath);
                Debug.Log($"[EnemyVariants] Created {outPath}");
            }
            finally { Object.DestroyImmediate(inst); }
        }

        private static void WireDataPrefabs()
        {
            Wire("Walker",  "Assets/Prefabs/Enemies/Walker.prefab");
            Wire("Speeder", "Assets/Prefabs/Enemies/Speeder.prefab");
            Wire("Ranged",  "Assets/Prefabs/Enemies/Ranged.prefab");
            Wire("Brute",   "Assets/Prefabs/Enemies/Brute.prefab");
            Wire("Bomber",  "Assets/Prefabs/Enemies/Bomber.prefab");
            Wire("Elite",   "Assets/Prefabs/Enemies/Elite.prefab");
        }

        private static void Wire(string key, string prefabPath)
        {
            string soPath = $"Assets/ScriptableObjects/Enemies/SO_Enemy_{key}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(soPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (so == null || prefab == null)
            {
                Debug.LogWarning($"[EnemyVariants] Missing {soPath} or {prefabPath}");
                return;
            }
            so.Prefab = prefab;
            EditorUtility.SetDirty(so);
        }
    }
}

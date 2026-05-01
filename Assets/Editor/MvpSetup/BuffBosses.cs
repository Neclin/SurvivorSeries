using UnityEditor;
using UnityEngine;
using SurvivorSeries.Enemies;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Enemies.Variants;

namespace SurvivorSeriesEditor
{
    public static class BuffBosses
    {
        private const string ProjectilePrefabPath = "Assets/Prefabs/Enemies/EnemyProjectile.prefab";

        private struct BossStats
        {
            public string AssetName;
            public float Health;
            public float Damage;
            public float MoveSpeed;
        }

        private static readonly BossStats[] BossTable = new[]
        {
            new BossStats { AssetName = "SO_Boss_MushroomKing", Health = 2400f, Damage = 24f, MoveSpeed = 2.6f },
            new BossStats { AssetName = "SO_Boss_Demon",        Health = 2800f, Damage = 28f, MoveSpeed = 2.8f },
            new BossStats { AssetName = "SO_Boss_Orc",          Health = 2600f, Damage = 26f, MoveSpeed = 2.7f },
            new BossStats { AssetName = "SO_Boss_Yeti",         Health = 3000f, Damage = 30f, MoveSpeed = 2.4f },
            new BossStats { AssetName = "SO_Boss_Dragon",       Health = 3500f, Damage = 34f, MoveSpeed = 2.5f },
        };

        [MenuItem("Survivor Series/Buff Bosses")]
        public static void Run()
        {
            var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            if (projectilePrefab == null) Debug.LogWarning($"[Bosses] EnemyProjectile prefab missing at {ProjectilePrefabPath}");

            int soUpdated = 0, prefabUpdated = 0;

            foreach (var stats in BossTable)
            {
                var soPath = $"Assets/ScriptableObjects/Bosses/{stats.AssetName}.asset";
                var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(soPath);
                if (so == null) { Debug.LogWarning($"[Bosses] Missing {soPath}"); continue; }

                so.BaseHealth = stats.Health;
                so.BaseDamage = stats.Damage;
                so.MoveSpeed = stats.MoveSpeed;
                EditorUtility.SetDirty(so);
                soUpdated++;

                if (so.Prefab != null && projectilePrefab != null)
                {
                    var prefabRoot = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(so.Prefab));
                    var boss = prefabRoot.GetComponentInChildren<BossEnemy>();
                    if (boss != null)
                    {
                        var pso = new SerializedObject(boss);
                        var prop = pso.FindProperty("_projectilePrefab");
                        if (prop != null)
                        {
                            prop.objectReferenceValue = projectilePrefab;
                            pso.ApplyModifiedProperties();
                            PrefabUtility.SaveAsPrefabAsset(prefabRoot, AssetDatabase.GetAssetPath(so.Prefab));
                            prefabUpdated++;
                        }
                    }
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Bosses] Updated {soUpdated} stat SOs and wired {prefabUpdated} prefabs with projectile.");
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurvivorSeries.Audio;

namespace SurvivorSeriesEditor
{
    public static class BuildAudio
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";
        private const string AudioDir = "Assets/Audio";
        private const string SfxLibPath = "Assets/ScriptableObjects/Audio/SO_SfxLibrary.asset";

        [MenuItem("Survivor Series/Build Audio + SFX Library")]
        public static void Run()
        {
            EnsureFolders();
            var library = EnsureSfxLibrary();
            PopulateLibrary(library);

            var scene = EditorSceneManager.OpenScene(ScenePath);
            var am = EnsureSceneSingleton<AudioManager>("AudioManager");
            SetPrivateField(am, "_library", library);
            EditorUtility.SetDirty(am);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Audio] SfxLibrary populated and AudioManager wired.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Audio"))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Audio");
        }

        private static SfxLibrary EnsureSfxLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<SfxLibrary>(SfxLibPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<SfxLibrary>();
                AssetDatabase.CreateAsset(lib, SfxLibPath);
            }
            return lib;
        }

        private static void PopulateLibrary(SfxLibrary library)
        {
            var entries = new List<SfxLibrary.Entry>
            {
                BuildEntry(SfxId.CoinPickup,      0.18f, 0.12f, "coin3"),
                BuildEntry(SfxId.EnemyHit,        0.18f, 0.10f, "impactSoft_medium"),
                BuildEntry(SfxId.EnemyDeath,      0.40f, 0.08f, "impactWood_heavy"),
                BuildEntry(SfxId.PlayerHurt,      0.85f, 0.20f, "impactPunch_heavy"),
                BuildEntry(SfxId.SwordSwing,      0.30f, 0.15f, "swing"),
                BuildEntry(SfxId.ProjectileShoot, 0.22f, 0.08f, "swing"),
                BuildEntry(SfxId.BossSpawn,       1.00f, 0.20f, "impactMetal_heavy"),
                BuildEntry(SfxId.LevelUp,         0.80f, 0.20f, "interface"),
                BuildEntry(SfxId.UIClick,         0.45f, 0.05f, "click3"),
                BuildEntry(SfxId.ShopOpen,        0.70f, 0.20f, "doorOpen"),
                BuildEntry(SfxId.ShopClose,       0.70f, 0.20f, "doorClose"),
                BuildEntry(SfxId.Purchase,        0.55f, 0.05f, "coin"),
            };
            SetPrivateField(library, "_entries", entries.ToArray());
            library.InvalidateCache();
            EditorUtility.SetDirty(library);
        }

        private static SfxLibrary.Entry BuildEntry(SfxId id, float vol, float interval, params string[] prefixes)
        {
            var clips = LoadClipsMatching(prefixes);
            return new SfxLibrary.Entry
            {
                Id = id,
                Clips = clips.ToArray(),
                Volume = vol,
                PitchVariance = 0.08f,
                MinInterval = interval,
            };
        }

        private static List<AudioClip> LoadClipsMatching(string[] prefixes)
        {
            var result = new List<AudioClip>();
            if (!Directory.Exists(AudioDir)) return result;
            var seen = new HashSet<string>();
            foreach (var ext in new[] { "*.ogg", "*.wav" })
            {
                foreach (var f in Directory.GetFiles(AudioDir, ext, SearchOption.AllDirectories))
                {
                    if (f.Contains("__MACOSX")) continue;
                    string fileName = Path.GetFileNameWithoutExtension(f);
                    foreach (var p in prefixes)
                    {
                        if (!fileName.StartsWith(p, System.StringComparison.OrdinalIgnoreCase)) continue;
                        string assetPath = f.Replace('\\', '/');
                        if (!seen.Add(assetPath)) break;
                        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                        if (clip != null) result.Add(clip);
                        break;
                    }
                }
            }
            return result;
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
            if (f == null) { Debug.LogError($"[Audio] Field '{fieldName}' not found"); return; }
            f.SetValue(target, value);
        }
    }
}

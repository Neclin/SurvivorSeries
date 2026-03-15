using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Enemies;
using SurvivorSeries.Enemies.Data;
using SurvivorSeries.Enemies.Variants;
using SurvivorSeries.Utilities;
using SurvivorSeries.UI.Victory;

namespace SurvivorSeriesEditor
{
    public static class BuildBossSystem
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";
        private const string PrefabDir = "Assets/Prefabs/Bosses";
        private const string DataDir = "Assets/ScriptableObjects/Bosses";
        private const string AnimDir = "Assets/Animators";

        [MenuItem("Survivor Series/Build Boss System")]
        public static void Run()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(DataDir);
            Directory.CreateDirectory(AnimDir);

            var bosses = new List<EnemyDataSO>();
            bosses.Add(BuildBoss("MushroomKing",
                "Assets/Models/Ultimate Monsters/Big/FBX/MushroomKing.fbx",
                hp: 600f, dmg: 16f, speed: 1.8f, scale: 1.6f, xp: 0));
            bosses.Add(BuildBoss("Demon",
                "Assets/Models/Ultimate Monsters/Big/FBX/Demon.fbx",
                hp: 700f, dmg: 18f, speed: 2.0f, scale: 1.8f, xp: 0));
            bosses.Add(BuildBoss("Yeti",
                "Assets/Models/Ultimate Monsters/Big/FBX/Yeti.fbx",
                hp: 800f, dmg: 20f, speed: 1.6f, scale: 1.7f, xp: 0));
            bosses.Add(BuildBoss("Orc",
                "Assets/Models/Ultimate Monsters/Big/FBX/Orc.fbx",
                hp: 650f, dmg: 17f, speed: 1.9f, scale: 1.6f, xp: 0));
            bosses.Add(BuildBoss("Dragon",
                "Assets/Models/Ultimate Monsters/Flying/FBX/Dragon.fbx",
                hp: 900f, dmg: 22f, speed: 1.5f, scale: 2.0f, xp: 0));

            var scene = EditorSceneManager.OpenScene(ScenePath);
            EnsureBossSpawner(bosses);
            BuildVictoryCanvas();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Boss] Built {bosses.Count} bosses, BossSpawner + Victory canvas wired.");
        }

        private static EnemyDataSO BuildBoss(string key, string fbxPath,
                                             float hp, float dmg, float speed, float scale, int xp)
        {
            string prefabPath = $"{PrefabDir}/Boss_{key}.prefab";
            string dataPath = $"{DataDir}/SO_Boss_{key}.asset";
            string ctrlPath = $"{AnimDir}/Boss_{key}_Animator.controller";
            string avatarPath = $"{AnimDir}/Avatar_Boss_{key}.asset";

            var walkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Walker.prefab");
            if (walkerPrefab == null) { Debug.LogError("[Boss] Walker.prefab missing"); return null; }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                AssetDatabase.DeleteAsset(prefabPath);

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(walkerPrefab);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                inst.name = $"Boss_{key}";

                foreach (var s in inst.GetComponents<EnemyBase>()) Object.DestroyImmediate(s);
                inst.AddComponent<BossEnemy>();

                var visualT = inst.transform.Find("Visual");
                if (visualT != null) Object.DestroyImmediate(visualT.gameObject);

                var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbx == null) { Debug.LogError($"[Boss] FBX missing: {fbxPath}"); return null; }

                var v = (GameObject)PrefabUtility.InstantiatePrefab(fbx, inst.transform);
                v.name = "Visual";
                v.transform.localPosition = Vector3.zero;
                v.transform.localRotation = Quaternion.identity;
                v.transform.localScale = Vector3.one * scale;
                if (v.GetComponent<HitFlash>() == null) v.AddComponent<HitFlash>();

                var ctrl = BuildIdleWalkController(fbxPath, ctrlPath);
                var avatar = BuildAvatar(fbxPath, avatarPath);
                if (ctrl != null && avatar != null)
                {
                    var anim = v.GetComponent<Animator>();
                    if (anim == null) anim = v.AddComponent<Animator>();
                    anim.runtimeAnimatorController = ctrl;
                    anim.avatar = avatar;
                    anim.applyRootMotion = false;
                }

                PrefabUtility.SaveAsPrefabAsset(inst, prefabPath);
            }
            finally { Object.DestroyImmediate(inst); }

            if (AssetDatabase.LoadAssetAtPath<EnemyDataSO>(dataPath) != null)
                AssetDatabase.DeleteAsset(dataPath);

            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            data.EnemyName = $"{key} (Boss)";
            data.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            data.BaseHealth = hp;
            data.BaseDamage = dmg;
            data.MoveSpeed = speed;
            data.ContactDamageInterval = 0.5f;
            data.XPDropAmount = xp;
            data.CurrencyDropAmount = 0;
            data.CurrencyDropChance = 0f;
            AssetDatabase.CreateAsset(data, dataPath);
            Debug.Log($"[Boss] Created {prefabPath} + {dataPath}");
            return data;
        }

        private static AnimatorController BuildIdleWalkController(string fbxPath, string ctrlPath)
        {
            ConfigureGenericRig(fbxPath);
            SetClipsLooping(fbxPath, new[] { "idle", "walk", "run", "fly" });
            AssetDatabase.SaveAssets();

            AnimationClip idle = null, walk = null;
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                if (!(a is AnimationClip c) || c.name.StartsWith("__preview__")) continue;
                var lower = c.name.ToLower();
                if (idle == null && lower.Contains("idle")) idle = c;
                if (walk == null && (lower.Contains("walk") || lower.Contains("run") || lower.Contains("fly"))) walk = c;
            }
            if (idle == null || walk == null)
            {
                Debug.LogWarning($"[Boss] No idle/walk in {fbxPath}");
                return null;
            }

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath) != null)
                AssetDatabase.DeleteAsset(ctrlPath);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var sm = ctrl.layers[0].stateMachine;
            var s_idle = sm.AddState("Idle"); s_idle.motion = idle;
            var s_walk = sm.AddState("Walk"); s_walk.motion = walk;
            sm.defaultState = s_idle;
            var i2w = s_idle.AddTransition(s_walk);
            i2w.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            i2w.hasExitTime = false; i2w.duration = 0.1f;
            var w2i = s_walk.AddTransition(s_idle);
            w2i.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            w2i.hasExitTime = false; w2i.duration = 0.1f;
            return ctrl;
        }

        private static Avatar BuildAvatar(string fbxPath, string outPath)
        {
            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceSynchronousImport);
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                if (a is Avatar av) return av;

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            try
            {
                var avatar = AvatarBuilder.BuildGenericAvatar(instance, "");
                if (avatar == null || !avatar.isValid) return null;
                avatar.name = Path.GetFileNameWithoutExtension(outPath);
                if (AssetDatabase.LoadAssetAtPath<Avatar>(outPath) != null) AssetDatabase.DeleteAsset(outPath);
                AssetDatabase.CreateAsset(avatar, outPath);
                return AssetDatabase.LoadAssetAtPath<Avatar>(outPath);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        private static void ConfigureGenericRig(string path)
        {
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) return;
            if (imp.animationType != ModelImporterAnimationType.Generic)
            {
                imp.animationType = ModelImporterAnimationType.Generic;
                imp.SaveAndReimport();
            }
        }

        private static void SetClipsLooping(string fbxPath, string[] keywords)
        {
            var imp = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (imp == null) return;
            var src = imp.clipAnimations;
            if (src == null || src.Length == 0) src = imp.defaultClipAnimations;
            if (src == null || src.Length == 0) return;
            bool changed = false;
            for (int i = 0; i < src.Length; i++)
            {
                var lower = src[i].name.ToLower();
                bool match = false;
                foreach (var k in keywords) if (lower.Contains(k)) { match = true; break; }
                if (match && (!src[i].loopTime || !src[i].loopPose))
                {
                    src[i].loopTime = true;
                    src[i].loopPose = true;
                    changed = true;
                }
            }
            if (changed) { imp.clipAnimations = src; imp.SaveAndReimport(); }
        }

        private static void EnsureBossSpawner(List<EnemyDataSO> bosses)
        {
            var spawner = Object.FindAnyObjectByType<BossSpawner>();
            GameObject go = spawner != null ? spawner.gameObject : new GameObject("BossSpawner");
            if (spawner == null) spawner = go.AddComponent<BossSpawner>();

            var so = new SerializedObject(spawner);
            var arr = so.FindProperty("_bossPool");
            arr.arraySize = bosses.Count;
            for (int i = 0; i < bosses.Count; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = bosses[i];
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
        }

        private static void BuildVictoryCanvas()
        {
            var existing = GameObject.Find("Victory_Canvas");
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject("Victory_Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 60;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);

            go.AddComponent<VictoryUI>();

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(go.transform, false);
            var pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0f, 0f, 0f, 0.9f);
            pImg.sprite = GenerateFlatSprite.EnsureFlatSprite();
            var pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
            pr.sizeDelta = Vector2.zero;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(panel.transform, false);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = "VICTORY!";
            title.fontSize = 96;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(1f, 0.95f, 0.4f, 1f);
            var tr = titleGo.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0, 0.5f); tr.anchorMax = new Vector2(1, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = new Vector2(0, 140);

            var ui = go.GetComponent<VictoryUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }
    }
}

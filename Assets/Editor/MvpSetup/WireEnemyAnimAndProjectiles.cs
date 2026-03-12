using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SurvivorSeriesEditor
{
    public static class WireEnemyAnimAndProjectiles
    {
        [MenuItem("Survivor Series/Wire Enemy Anim + Projectiles")]
        public static void Run()
        {
            CreateEnemyProjectilePrefab();

            var skeletonCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/Animators/EnemyAnimator.controller");
            var skeletonAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(
                "Assets/Animators/Avatar_Skeleton_Minion.asset");

            WireAnimator("Assets/Prefabs/Enemies/Speeder.prefab", skeletonCtrl, skeletonAvatar);
            WireAnimator("Assets/Prefabs/Enemies/Ranged.prefab", skeletonCtrl, skeletonAvatar);
            WireAnimator("Assets/Prefabs/Enemies/Brute.prefab", skeletonCtrl, skeletonAvatar);

            BuildMonsterAnimator(
                fbxPath: "Assets/Models/Ultimate Monsters/Blob/FBX/Mushnub.fbx",
                ctrlPath: "Assets/Animators/MushnubAnimator.controller",
                avatarPath: "Assets/Animators/Avatar_Mushnub.asset",
                prefabPath: "Assets/Prefabs/Enemies/Bomber.prefab");

            BuildMonsterAnimator(
                fbxPath: "Assets/Models/Ultimate Monsters/Big/FBX/Demon.fbx",
                ctrlPath: "Assets/Animators/DemonAnimator.controller",
                avatarPath: "Assets/Animators/Avatar_Demon.asset",
                prefabPath: "Assets/Prefabs/Enemies/Elite.prefab");

            WireRangedProjectile();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemyAnim] Animations and ranged projectile wired.");
        }

        private static void CreateEnemyProjectilePrefab()
        {
            const string outPath = "Assets/Prefabs/Enemies/EnemyProjectile.prefab";
            Directory.CreateDirectory("Assets/Prefabs/Enemies");

            var root = new GameObject("EnemyProjectile");
            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            var col = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.25f;
            root.AddComponent<SurvivorSeries.Enemies.EnemyProjectile>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(visual.GetComponent<SphereCollider>());
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * 0.45f;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(0.85f, 0.25f, 0.85f);
            visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

            if (AssetDatabase.LoadAssetAtPath<GameObject>(outPath) != null)
                AssetDatabase.DeleteAsset(outPath);
            PrefabUtility.SaveAsPrefabAsset(root, outPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[EnemyAnim] Created {outPath}");
        }

        private static void WireRangedProjectile()
        {
            const string prefabPath = "Assets/Prefabs/Enemies/Ranged.prefab";
            var projectile = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Enemies/EnemyProjectile.prefab");
            if (projectile == null) { Debug.LogError("[EnemyAnim] EnemyProjectile prefab missing"); return; }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var ranged = root.GetComponent<SurvivorSeries.Enemies.Variants.RangedEnemy>();
                if (ranged == null) { Debug.LogError("[EnemyAnim] RangedEnemy component not on Ranged.prefab"); return; }
                var so = new SerializedObject(ranged);
                var prop = so.FindProperty("_projectilePrefab");
                prop.objectReferenceValue = projectile;
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[EnemyAnim] Wired EnemyProjectile to Ranged.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void WireAnimator(string prefabPath, AnimatorController ctrl, Avatar avatar)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var visualT = root.transform.Find("Visual");
                if (visualT == null) { Debug.LogError($"[EnemyAnim] No Visual in {prefabPath}"); return; }

                var anim = visualT.GetComponent<Animator>();
                if (anim == null) anim = visualT.gameObject.AddComponent<Animator>();
                anim.runtimeAnimatorController = ctrl;
                anim.avatar = avatar;
                anim.applyRootMotion = false;

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[EnemyAnim] Wired animator on {prefabPath}");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BuildMonsterAnimator(string fbxPath, string ctrlPath, string avatarPath,
                                                 string prefabPath)
        {
            ConfigureGenericRig(fbxPath);
            SetClipsLooping(fbxPath, new[] { "idle", "walk", "run" });
            AssetDatabase.SaveAssets();

            var clips = new List<AnimationClip>();
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                if (a is AnimationClip c && !c.name.StartsWith("__preview__")) clips.Add(c);

            AnimationClip idle = null, walk = null;
            foreach (var c in clips)
            {
                var lower = c.name.ToLower();
                if (idle == null && lower.Contains("idle")) idle = c;
                if (walk == null && (lower.Contains("walk") || lower.Contains("run"))) walk = c;
            }
            if (idle == null || walk == null)
            {
                Debug.LogError($"[EnemyAnim] {fbxPath} missing idle/walk clips");
                return;
            }

            Directory.CreateDirectory("Assets/Animators");

            var avatar = BuildAvatar(fbxPath, avatarPath);

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

            WireAnimator(prefabPath, ctrl, avatar);
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
                if (avatar == null || !avatar.isValid)
                {
                    Debug.LogError($"[EnemyAnim] Failed to build avatar for {fbxPath}");
                    return null;
                }
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
            if (changed)
            {
                imp.clipAnimations = src;
                imp.SaveAndReimport();
            }
        }
    }
}

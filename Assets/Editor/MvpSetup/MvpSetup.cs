using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using SurvivorSeries.Pickups;
using SurvivorSeries.UI.HUD;
using SurvivorSeries.Weapons;
using SurvivorSeries.Weapons.Data;

namespace SurvivorSeriesEditor
{
    public static class MvpSetup
    {
        public static void SetupHudExtensions()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var canvas = GameObject.Find("HUD_Canvas");
            if (canvas == null) { Debug.LogError("[MvpSetup] HUD_Canvas not found."); return; }

            var bgGo = FindOrCreateChild(canvas, "HealthBar_BG", typeof(RectTransform));
            EnsureComponent<CanvasRenderer>(bgGo);
            var bg = EnsureComponent<Image>(bgGo);
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            if (bg.sprite == null) bg.sprite = DefaultUiSprite();
            SetRect(bgGo, anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0, 0),
                          pivot: new Vector2(0, 0),
                          anchoredPos: new Vector2(20, 40), size: new Vector2(360, 36));

            var fillGo = FindOrCreateChild(bgGo, "HealthBar_Fill", typeof(RectTransform));
            EnsureComponent<CanvasRenderer>(fillGo);
            var fill = EnsureComponent<Image>(fillGo);
            fill.color = new Color(0.85f, 0.22f, 0.22f, 1f);
            if (fill.sprite == null) fill.sprite = DefaultUiSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 1f;
            SetRect(fillGo, anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 1),
                            pivot: new Vector2(0.5f, 0.5f),
                            anchoredPos: Vector2.zero, size: Vector2.zero);

            var healthTxtGo = FindOrCreateChild(bgGo, "HealthText", typeof(RectTransform));
            EnsureComponent<CanvasRenderer>(healthTxtGo);
            var legacy = healthTxtGo.GetComponent<Text>();
            if (legacy != null) Object.DestroyImmediate(legacy);
            var healthTxt = EnsureComponent<TextMeshProUGUI>(healthTxtGo);
            healthTxt.text = "100 / 100";
            healthTxt.fontSize = 22;
            healthTxt.fontStyle = FontStyles.Bold;
            healthTxt.alignment = TextAlignmentOptions.Center;
            healthTxt.color = Color.white;
            SetRect(healthTxtGo, anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 1),
                                  pivot: new Vector2(0.5f, 0.5f),
                                  anchoredPos: Vector2.zero, size: Vector2.zero);

            var timerGo = FindOrCreateChild(canvas, "WaveTimer", typeof(RectTransform));
            EnsureComponent<CanvasRenderer>(timerGo);
            var timerLegacy = timerGo.GetComponent<Text>();
            if (timerLegacy != null) Object.DestroyImmediate(timerLegacy);
            var timer = EnsureComponent<TextMeshProUGUI>(timerGo);
            timer.text = "00:00";
            timer.fontSize = 36;
            timer.alignment = TextAlignmentOptions.Center;
            timer.color = Color.white;
            SetRect(timerGo, anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f),
                              pivot: new Vector2(0.5f, 1f),
                              anchoredPos: new Vector2(0, -90), size: new Vector2(300, 50));

            var currGo = FindOrCreateChild(canvas, "CurrencyText", typeof(RectTransform));
            EnsureComponent<CanvasRenderer>(currGo);
            var currLegacy = currGo.GetComponent<Text>();
            if (currLegacy != null) Object.DestroyImmediate(currLegacy);
            var curr = EnsureComponent<TextMeshProUGUI>(currGo);
            curr.text = "0g";
            curr.fontSize = 36;
            curr.alignment = TextAlignmentOptions.Right;
            curr.color = new Color(1f, 0.85f, 0.2f, 1f);
            SetRect(currGo, anchorMin: new Vector2(1f, 1f), anchorMax: new Vector2(1f, 1f),
                            pivot: new Vector2(1f, 1f),
                            anchoredPos: new Vector2(-30, -30), size: new Vector2(280, 50));

            var hud = canvas.GetComponent<HUDController>();
            if (hud == null) { Debug.LogError("[MvpSetup] HUDController missing."); return; }
            var so = new SerializedObject(hud);
            SetRef(so, "_healthFill", fill);
            SetRef(so, "_healthText", healthTxt);
            SetRef(so, "_waveTimerText", timer);
            SetRef(so, "_currencyText", curr);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[MvpSetup] HUD extensions configured + saved.");
        }

        private static GameObject FindOrCreateChild(GameObject parent, string name, params System.Type[] components)
        {
            var existing = parent.transform.Find(name);
            if (existing != null) return existing.gameObject;
            var go = new GameObject(name, components);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
                                    Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = pivot;
            r.anchoredPosition = anchoredPos;
            r.sizeDelta = size;
        }

        private static Sprite DefaultUiSprite()
            => GenerateFlatSprite.EnsureFlatSprite();

        public static void SetupVisuals_FirstSlice()
        {
            SetupPlayerVisual();
            SetupWalkerVisual();
            SetupCoinDrop();
        }

        public static void PolishCoinAndAnimate()
        {
            UpgradeCoinDrop();
            SetupAnimations();
        }

        public static void AddSpinToThrownProjectiles()
        {
            AddSpinComponent("Assets/Prefabs/Projectiles/DaggerProjectile.prefab",
                             axis: new Vector3(1f, 0f, 0f), dps: 1080f);
            AddSpinComponent("Assets/Prefabs/Projectiles/AxeProjectile.prefab",
                             axis: new Vector3(0f, 0f, 1f), dps: 720f);
        }

        private static void AddSpinComponent(string prefabPath, Vector3 axis, float dps)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var visualT = root.transform.Find("Visual");
                if (visualT == null) { Debug.LogWarning($"[MvpSetup] No Visual in {prefabPath}"); return; }
                var spin = visualT.GetComponent<ProjectileSpin>();
                if (spin == null) spin = visualT.gameObject.AddComponent<ProjectileSpin>();
                var so = new SerializedObject(spin);
                so.FindProperty("_axis").vector3Value = axis;
                so.FindProperty("_degreesPerSecond").floatValue = dps;
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[MvpSetup] Added ProjectileSpin to {prefabPath} Visual.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        public static void BuildNewWeaponSystem()
        {
            CreateFireProjectilePrefab();
            CreateWeaponPrefab<SurvivorSeries.Weapons.Implementations.BowWeapon>("BowWeapon");
            CreateWeaponPrefab<SurvivorSeries.Weapons.Implementations.SwordWeapon>("SwordWeapon");
            CreateWeaponPrefab<SurvivorSeries.Weapons.Implementations.DaggerWeapon>("DaggerWeapon");
            CreateWeaponPrefab<SurvivorSeries.Weapons.Implementations.AxeWeapon>("AxeWeapon");
            CreateWeaponPrefab<SurvivorSeries.Weapons.Implementations.SpellbookWeapon>("SpellbookWeapon");
            CreateWeaponPrefab<SurvivorSeries.Weapons.Implementations.StaffWeapon>("StaffWeapon");

            CreateWeaponSO("Bow",       WeaponType.Projectile,
                "Assets/Prefabs/Weapons/BowWeapon.prefab",
                "Assets/Prefabs/Projectiles/ArrowProjectile.prefab",
                "Assets/Prefabs/WeaponDisplays/BowDisplay.prefab",
                description: "Fires arrows at the nearest enemy. Long range, fast cooldown.",
                damage: new[] {10f,12f,15f,18f,22f,27f,33f,40f},
                cooldown: new[] {0.9f,0.85f,0.8f,0.75f,0.7f,0.65f,0.6f,0.55f},
                projCount: new[] {1,1,1,2,2,2,3,3});

            CreateWeaponSO("Sword",     WeaponType.Melee,
                "Assets/Prefabs/Weapons/SwordWeapon.prefab",
                projectilePrefabPath: null,
                "Assets/Prefabs/WeaponDisplays/SwordDisplay.prefab",
                description: "Sweeps a 90 degree melee arc in front of the player. High damage, short range.",
                damage: new[] {15f,18f,22f,26f,31f,37f,44f,52f},
                cooldown: new[] {0.7f,0.65f,0.6f,0.55f,0.5f,0.45f,0.4f,0.35f},
                projCount: new[] {1,1,1,1,1,1,1,1});

            CreateWeaponSO("Dagger",    WeaponType.Thrown,
                "Assets/Prefabs/Weapons/DaggerWeapon.prefab",
                "Assets/Prefabs/Projectiles/DaggerProjectile.prefab",
                "Assets/Prefabs/WeaponDisplays/DaggerDisplay.prefab",
                description: "Throws fast piercing daggers that hit multiple enemies in a line.",
                damage: new[] {7f,9f,11f,14f,17f,21f,26f,32f},
                cooldown: new[] {0.5f,0.45f,0.42f,0.4f,0.38f,0.36f,0.34f,0.32f},
                projCount: new[] {1,1,2,2,2,3,3,4});

            CreateWeaponSO("Axe",       WeaponType.Thrown,
                "Assets/Prefabs/Weapons/AxeWeapon.prefab",
                "Assets/Prefabs/Projectiles/AxeProjectile.prefab",
                "Assets/Prefabs/WeaponDisplays/AxeDisplay.prefab",
                description: "Hurls a spinning axe with a slow cooldown. Highest single-hit damage.",
                damage: new[] {18f,22f,27f,33f,40f,48f,57f,68f},
                cooldown: new[] {1.2f,1.1f,1.05f,1.0f,0.95f,0.9f,0.85f,0.8f},
                projCount: new[] {1,1,1,1,2,2,2,3});

            CreateWeaponSO("Spellbook", WeaponType.Projectile,
                "Assets/Prefabs/Weapons/SpellbookWeapon.prefab",
                "Assets/Prefabs/Projectiles/FireProjectile.prefab",
                "Assets/Prefabs/WeaponDisplays/SpellbookDisplay.prefab",
                description: "Conjures fire bolts that ignite enemies, dealing damage over time.",
                damage: new[] {6f,7f,9f,11f,13f,16f,19f,23f},
                cooldown: new[] {1.1f,1.0f,0.95f,0.9f,0.85f,0.8f,0.75f,0.7f},
                projCount: new[] {1,1,1,2,2,2,3,3});

            CreateWeaponSO("Staff",     WeaponType.Aura,
                "Assets/Prefabs/Weapons/StaffWeapon.prefab",
                projectilePrefabPath: null,
                "Assets/Prefabs/WeaponDisplays/StaffDisplay.prefab",
                description: "Strikes nearby enemies with a chain of lightning.",
                damage: new[] {12f,15f,19f,23f,28f,34f,41f,49f},
                cooldown: new[] {1.4f,1.3f,1.2f,1.1f,1.0f,0.9f,0.8f,0.7f},
                projCount: new[] {1,2,2,3,3,4,4,5});

            Debug.Log("[MvpSetup] New weapon system built.");
        }

        private static void CreateFireProjectilePrefab()
        {
            Directory.CreateDirectory("Assets/Prefabs/Projectiles");
            const string outPath = "Assets/Prefabs/Projectiles/FireProjectile.prefab";

            var root = new GameObject("FireProjectile");
            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.None;
            var col = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.3f;
            root.AddComponent<SurvivorSeries.Weapons.FireProjectile>();

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Models/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/smokebomb.fbx");
            if (fbx != null)
            {
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx, root.transform);
                visual.name = "Visual";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                visual.transform.localScale = Vector3.one * 1.5f;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(outPath) != null) AssetDatabase.DeleteAsset(outPath);
            PrefabUtility.SaveAsPrefabAsset(root, outPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[MvpSetup] Created {outPath}");
        }

        private static void CreateWeaponPrefab<T>(string prefabName) where T : SurvivorSeries.Weapons.WeaponBase
        {
            Directory.CreateDirectory("Assets/Prefabs/Weapons");
            string outPath = $"Assets/Prefabs/Weapons/{prefabName}.prefab";
            var root = new GameObject(prefabName);
            root.AddComponent<T>();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(outPath) != null) AssetDatabase.DeleteAsset(outPath);
            PrefabUtility.SaveAsPrefabAsset(root, outPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[MvpSetup] Created {outPath}");
        }

        private static void CreateWeaponSO(string name, WeaponType type,
                                           string weaponPrefabPath, string projectilePrefabPath,
                                           string displayPrefabPath,
                                           string description,
                                           float[] damage, float[] cooldown, int[] projCount)
        {
            Directory.CreateDirectory("Assets/ScriptableObjects/Weapons");
            string outPath = $"Assets/ScriptableObjects/Weapons/SO_Weapon_{name}.asset";

            if (AssetDatabase.LoadAssetAtPath<WeaponDataSO>(outPath) != null)
                AssetDatabase.DeleteAsset(outPath);

            var so = ScriptableObject.CreateInstance<WeaponDataSO>();
            so.WeaponName = name;
            so.Description = description;
            so.Type = type;
            so.WeaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPrefabPath);
            so.ProjectilePrefab = projectilePrefabPath != null
                ? AssetDatabase.LoadAssetAtPath<GameObject>(projectilePrefabPath) : null;
            so.DisplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(displayPrefabPath);
            so.BaseDamagePerLevel = damage;
            so.CooldownPerLevel = cooldown;
            so.AreaPerLevel = new[] {1f,1f,1.05f,1.1f,1.15f,1.2f,1.25f,1.3f};
            so.ProjectileCountPerLevel = projCount;
            so.MaxLevel = 8;
            so.ShopPurchaseCost = 60;

            AssetDatabase.CreateAsset(so, outPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MvpSetup] Created {outPath}");
        }

        public static void GivePlayerAllWeapons()
        {
            string[] soPaths =
            {
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Bow.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Sword.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Dagger.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Axe.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Spellbook.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Staff.asset",
            };

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var player = GameObject.Find("Player");
            if (player == null) { Debug.LogError("[MvpSetup] Player not found"); return; }
            var bootstrap = player.GetComponent<SurvivorSeries.Core.GameplayBootstrap>();
            if (bootstrap == null) { Debug.LogError("[MvpSetup] GameplayBootstrap missing on Player"); return; }

            var so = new SerializedObject(bootstrap);
            var listProp = so.FindProperty("_startingWeapons");
            if (listProp == null) { Debug.LogError("[MvpSetup] _startingWeapons field missing"); return; }
            listProp.arraySize = soPaths.Length;
            for (int i = 0; i < soPaths.Length; i++)
            {
                var weaponSo = AssetDatabase.LoadAssetAtPath<SurvivorSeries.Weapons.Data.WeaponDataSO>(soPaths[i]);
                if (weaponSo == null) { Debug.LogError($"[MvpSetup] Cannot load {soPaths[i]}"); continue; }
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = weaponSo;
            }
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[MvpSetup] Wired {soPaths.Length} starting weapons onto Player.");
        }

        private static void SetSoFieldToAsset(string soPath, string fieldName, string assetPath)
        {
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(soPath);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (so == null) { Debug.LogError($"[MvpSetup] Cannot load SO {soPath}"); return; }
            if (asset == null) { Debug.LogError($"[MvpSetup] Cannot load asset {assetPath}"); return; }

            var serObj = new SerializedObject(so);
            var prop = serObj.FindProperty(fieldName);
            if (prop == null) { Debug.LogError($"[MvpSetup] No field {fieldName} on {soPath}"); return; }
            prop.objectReferenceValue = asset;
            serObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MvpSetup] {Path.GetFileName(soPath)}.{fieldName} → {Path.GetFileName(assetPath)}");
        }

        public static void UpgradeCoinDrop()
        {
            const string prefabPath = "Assets/Prefabs/Pickups/CurrencyDrop.prefab";
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var visualT = root.transform.Find("Visual");
                if (visualT == null)
                {
                    Debug.LogError("[MvpSetup] CurrencyDrop has no Visual child to upgrade.");
                    return;
                }
                visualT.localScale = Vector3.one * 4f;
                visualT.localRotation = Quaternion.Euler(90f, 0f, 0f);
                visualT.localPosition = new Vector3(0f, 0.4f, 0f);

                var visual = visualT.gameObject;
                var spin = visual.GetComponent<SpinAndBob>();
                if (spin == null) visual.AddComponent<SpinAndBob>();

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[MvpSetup] CurrencyDrop coin upgraded (4x scale, spin+bob).");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        public static void SetupAnimations()
        {
            const string knightFbx = "Assets/Models/KayKit_Adventurers_2.0_FREE/Characters/fbx/Knight.fbx";
            const string genFbx = "Assets/Models/KayKit_Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_General.fbx";
            const string movFbx = "Assets/Models/KayKit_Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx";
            const string skMinionFbx = "Assets/Models/KayKit_Skeletons_1.1_FREE/characters/fbx/Skeleton_Minion.fbx";
            const string skGenFbx = "Assets/Models/KayKit_Skeletons_1.1_FREE/Animations/fbx/Rig_Medium/Rig_Medium_General.fbx";
            const string skMovFbx = "Assets/Models/KayKit_Skeletons_1.1_FREE/Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx";

            ConfigureGenericRig(knightFbx);
            ConfigureGenericRig(genFbx);
            ConfigureGenericRig(movFbx);
            ConfigureGenericRig(skMinionFbx);
            ConfigureGenericRig(skGenFbx);
            ConfigureGenericRig(skMovFbx);

            string[] loopKeywords = { "idle", "walk", "run" };
            SetClipsLooping(genFbx, loopKeywords);
            SetClipsLooping(movFbx, loopKeywords);
            SetClipsLooping(skGenFbx, loopKeywords);
            SetClipsLooping(skMovFbx, loopKeywords);
            SetClipsLooping(knightFbx, loopKeywords);
            SetClipsLooping(skMinionFbx, loopKeywords);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var advClips = new List<AnimationClip>();
            advClips.AddRange(LoadClipsAt(genFbx));
            advClips.AddRange(LoadClipsAt(movFbx));
            advClips.AddRange(LoadClipsAt(knightFbx));

            var skClips = new List<AnimationClip>();
            skClips.AddRange(advClips);
            skClips.AddRange(LoadClipsAt(skGenFbx));
            skClips.AddRange(LoadClipsAt(skMovFbx));
            skClips.AddRange(LoadClipsAt(skMinionFbx));

            foreach (var c in advClips) Debug.Log($"[MvpSetup] Adv clip: {c.name}");
            foreach (var c in skClips)  Debug.Log($"[MvpSetup] Sk clip: {c.name}");

            var advIdle = PickClip(advClips, "idle");
            var advWalk = PickClip(advClips, "walking", "running", "walk", "run");
            var skIdle = PickClip(skClips, "idle");
            var skWalk = PickClip(skClips, "walking", "running", "walk", "run");

            if (advIdle == null || advWalk == null)
            {
                Debug.LogError($"[MvpSetup] Adventurer clips missing. idle={advIdle}, walk={advWalk}");
                return;
            }
            if (skIdle == null || skWalk == null)
            {
                Debug.LogError($"[MvpSetup] Skeleton clips missing. idle={skIdle}, walk={skWalk}");
                return;
            }

            Directory.CreateDirectory("Assets/Animators");
            var playerCtrl = BuildIdleWalkController("Assets/Animators/PlayerAnimator.controller", advIdle, advWalk);
            var enemyCtrl = BuildIdleWalkController("Assets/Animators/EnemyAnimator.controller", skIdle, skWalk);

            var knightAvatar = LoadAvatar(knightFbx);
            var skAvatar = LoadAvatar(skMinionFbx);

            WireAnimatorOnSceneVisual("Assets/Scenes/Gameplay.unity", "Player", playerCtrl, knightAvatar);
            WireAnimatorOnPrefabVisual("Assets/Prefabs/Enemies/Walker.prefab", enemyCtrl, skAvatar);

            Debug.Log($"[MvpSetup] Animations wired. Player(Idle={advIdle.name}, Walk={advWalk.name}); " +
                      $"Skeleton(Idle={skIdle.name}, Walk={skWalk.name}).");
        }

        private static void ConfigureGenericRig(string path)
        {
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) { Debug.LogWarning($"[MvpSetup] No ModelImporter for {path}"); return; }
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
                string lower = src[i].name.ToLower();
                bool match = keywords.Any(k => lower.Contains(k));
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

        private static List<AnimationClip> LoadClipsAt(string path)
        {
            var list = new List<AnimationClip>();
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                if (a is AnimationClip c && !c.name.StartsWith("__preview__")) list.Add(c);
            return list;
        }

        private static AnimationClip PickClip(List<AnimationClip> clips, params string[] keywords)
        {
            foreach (var kw in keywords)
            {
                var hit = clips.FirstOrDefault(c => c.name.ToLower().Contains(kw));
                if (hit != null) return hit;
            }
            return null;
        }

        private static Avatar LoadAvatar(string fbxPath)
        {
            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceSynchronousImport);
            var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (var a in assets)
                if (a is Avatar av) return av;
            return BuildAndSaveAvatar(fbxPath);
        }

        private static Avatar BuildAndSaveAvatar(string fbxPath)
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbx == null) { Debug.LogError($"[MvpSetup] Cannot load FBX {fbxPath}"); return null; }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            try
            {
                var avatar = AvatarBuilder.BuildGenericAvatar(instance, "");
                if (avatar == null || !avatar.isValid)
                {
                    Debug.LogError($"[MvpSetup] AvatarBuilder failed for {fbxPath}");
                    return null;
                }
                Directory.CreateDirectory("Assets/Animators");
                var outPath = $"Assets/Animators/Avatar_{Path.GetFileNameWithoutExtension(fbxPath)}.asset";
                avatar.name = Path.GetFileNameWithoutExtension(outPath);
                if (AssetDatabase.LoadAssetAtPath<Avatar>(outPath) != null) AssetDatabase.DeleteAsset(outPath);
                AssetDatabase.CreateAsset(avatar, outPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[MvpSetup] Built avatar at {outPath}");
                return AssetDatabase.LoadAssetAtPath<Avatar>(outPath);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        private static AnimatorController BuildIdleWalkController(string path, AnimationClip idle, AnimationClip walk)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);

            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var sm = ctrl.layers[0].stateMachine;
            var idleState = sm.AddState("Idle");
            idleState.motion = idle;
            var walkState = sm.AddState("Walk");
            walkState.motion = walk;
            sm.defaultState = idleState;

            var i2w = idleState.AddTransition(walkState);
            i2w.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            i2w.hasExitTime = false;
            i2w.duration = 0.1f;

            var w2i = walkState.AddTransition(idleState);
            w2i.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            w2i.hasExitTime = false;
            w2i.duration = 0.1f;

            return ctrl;
        }

        private static void WireAnimatorOnSceneVisual(string scenePath, string rootName,
                                                      AnimatorController ctrl, Avatar avatar)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            var root = GameObject.Find(rootName);
            if (root == null) { Debug.LogError($"[MvpSetup] {rootName} not found in scene"); return; }
            var visualT = root.transform.Find("Visual");
            if (visualT == null) { Debug.LogError($"[MvpSetup] {rootName}.Visual not found"); return; }
            var anim = EnsureComponent<Animator>(visualT.gameObject);
            anim.runtimeAnimatorController = ctrl;
            anim.avatar = avatar;
            anim.applyRootMotion = false;
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void WireAnimatorOnPrefabVisual(string prefabPath, AnimatorController ctrl, Avatar avatar)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var visualT = root.transform.Find("Visual");
                if (visualT == null) { Debug.LogError($"[MvpSetup] Visual missing on {prefabPath}"); return; }
                var anim = EnsureComponent<Animator>(visualT.gameObject);
                anim.runtimeAnimatorController = ctrl;
                anim.avatar = avatar;
                anim.applyRootMotion = false;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        public static void SetupPlayerVisual()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var player = GameObject.Find("Player");
            if (player == null) { Debug.LogError("[MvpSetup] Player not found in Gameplay scene"); return; }

            AttachKitVisual(player,
                "Assets/Models/KayKit_Adventurers_2.0_FREE/Characters/fbx/Knight.fbx",
                visualName: "Visual",
                localOffset: new Vector3(0, -1f, 0),
                scale: 1f);

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MvpSetup] Player visual set to Knight.");
        }

        public static void SetupWalkerVisual()
        {
            const string prefabPath = "Assets/Prefabs/Enemies/Walker.prefab";
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                AttachKitVisual(root,
                    "Assets/Models/KayKit_Skeletons_1.1_FREE/characters/fbx/Skeleton_Minion.fbx",
                    visualName: "Visual",
                    localOffset: new Vector3(0, -1f, 0),
                    scale: 1f);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[MvpSetup] Walker visual set to Skeleton_Minion.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        public static void SetupCoinDrop()
        {
            const string prefabPath = "Assets/Prefabs/Pickups/CurrencyDrop.prefab";
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                AttachKitVisual(root,
                    "Assets/Models/KayKit_DungeonRemastered_1.1_FREE/Assets/fbx(unity)/coin.fbx",
                    visualName: "Visual",
                    localOffset: Vector3.zero,
                    scale: 1.5f);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[MvpSetup] CurrencyDrop visual set to coin.fbx.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void AttachKitVisual(GameObject root, string fbxAssetPath,
                                            string visualName, Vector3 localOffset, float scale)
        {
            var mr = root.GetComponent<MeshRenderer>();
            if (mr != null) Object.DestroyImmediate(mr);
            var mf = root.GetComponent<MeshFilter>();
            if (mf != null) Object.DestroyImmediate(mf);

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxAssetPath);
            if (fbx == null)
            {
                Debug.LogError($"[MvpSetup] Could not load FBX at: {fbxAssetPath}");
                return;
            }

            var existing = root.transform.Find(visualName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx, root.transform);
            visual.name = visualName;
            visual.transform.localPosition = localOffset;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * scale;
        }

        private static void ReplaceWithTmp(GameObject canvas, string path,
                                           string text, int fontSize,
                                           TextAlignmentOptions align)
        {
            var t = canvas.transform.Find(path);
            if (t == null) { Debug.LogWarning($"[MvpSetup] Missing UI path: {path}"); return; }
            var go = t.gameObject;

            var legacy = go.GetComponent<Text>();
            if (legacy != null) Object.DestroyImmediate(legacy);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static void SetRef(SerializedObject so, string field, Object value)
        {
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning($"[MvpSetup] Field not found: {field}"); return; }
            prop.objectReferenceValue = value;
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Stages;
using SurvivorSeries.Stages.Data;
using SurvivorSeries.UI.StageSelect;
using SurvivorSeries.Achievements.Data;

namespace SurvivorSeriesEditor
{
    public static class BuildStagesAndObstacles
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";
        private const string StageDir = "Assets/ScriptableObjects/Stages";
        private const string MaterialDir = "Assets/Materials/Stages";
        private const string ObstacleDir = "Assets/Prefabs/Obstacles";
        private static readonly string[] NaturePackFbxDirs = {
            "Assets/Models/Ultimate Nature Pack - Jun 2019/FBX",
            "Assets/Models/Ultimate Stylized Nature - May 2022/FBX",
        };

        private static readonly string[] WastelandFbx = {
            "Rock_1","Rock_2","Rock_3","Rock_4","Rock_5","Rock_6","Rock_7",
            "DeadTree_1","DeadTree_2","DeadTree_3","DeadTree_4","DeadTree_5",
            "DeadTree_6","DeadTree_7","DeadTree_8","DeadTree_9","DeadTree_10",
            "Cactus_1","Cactus_2","Cactus_3","Cactus_4"
        };
        private static readonly string[] CryptFbx = {
            "Rock_Moss_1","Rock_Moss_2","Rock_Moss_3","Rock_Moss_4","Rock_Moss_5","Rock_Moss_6","Rock_Moss_7",
            "BirchTree_Dead_1","BirchTree_Dead_2","BirchTree_Dead_3","BirchTree_Dead_4","BirchTree_Dead_5"
        };
        private static readonly string[] TundraFbx = {
            "Rock_Snow_1","Rock_Snow_2","Rock_Snow_3","Rock_Snow_4","Rock_Snow_5","Rock_Snow_6",
            "BirchTree_Snow_1","BirchTree_Snow_2","BirchTree_Snow_3","BirchTree_Snow_4","BirchTree_Snow_5",
            "Bush_Snow_1","Bush_Snow_2"
        };

        [MenuItem("Survivor Series/Build Stages and Obstacles")]
        public static void Run()
        {
            EnsureFolders();
            EnsureObstacleLayer();

            var wastelandMat = EnsureMaterial("M_Ground_Wasteland", new Color(0.85f, 0.70f, 0.40f));
            var cryptMat     = EnsureMaterial("M_Ground_Crypt",     new Color(0.55f, 0.55f, 0.60f));
            var tundraMat    = EnsureMaterial("M_Ground_Tundra",    new Color(0.75f, 0.90f, 1.00f));

            var wastelandPrefabs = BuildPrefabVariants(WastelandFbx, "Wasteland");
            var cryptPrefabs     = BuildPrefabVariants(CryptFbx,     "Crypt");
            var tundraPrefabs    = BuildPrefabVariants(TundraFbx,    "Tundra");

            var wasteland = UpsertStage("SO_Stage_Wasteland", s => {
                s.StageID = "wasteland"; s.DisplayName = "Wasteland";
                s.Description = "Sun-bleached rocks and dead trees stretch to the horizon.";
                s.GroundMaterial = wastelandMat;
                s.PreviewTint = new Color(0.85f, 0.70f, 0.40f);
                s.ObstaclePrefabs = wastelandPrefabs.ToArray();
                s.Density = 0.30f; s.MinPlayerClearRadius = 5f; s.UnlockAchievementID = "";
            });
            var crypt = UpsertStage("SO_Stage_Crypt", s => {
                s.StageID = "crypt"; s.DisplayName = "Crypt";
                s.Description = "Mossy ruins under a leaden sky. The dead remember everything.";
                s.GroundMaterial = cryptMat;
                s.PreviewTint = new Color(0.55f, 0.55f, 0.60f);
                s.ObstaclePrefabs = cryptPrefabs.ToArray();
                s.Density = 0.40f; s.MinPlayerClearRadius = 5f; s.UnlockAchievementID = "boss_slayer";
            });
            var tundra = UpsertStage("SO_Stage_Tundra", s => {
                s.StageID = "tundra"; s.DisplayName = "Frozen Tundra";
                s.Description = "An endless ice plain. Frost bites and silence presses in.";
                s.GroundMaterial = tundraMat;
                s.PreviewTint = new Color(0.75f, 0.90f, 1.00f);
                s.ObstaclePrefabs = tundraPrefabs.ToArray();
                s.Density = 0.35f; s.MinPlayerClearRadius = 5f; s.UnlockAchievementID = "survivor_10min";
            });

            var roster = EnsureRoster(new[] { wasteland, crypt, tundra });

            var scene = EditorSceneManager.OpenScene(ScenePath);
            var sm = EnsureSceneSingleton<StageManager>("StageManager");
            WireStageManager(sm, roster);

            BuildStageSelectCanvas(roster);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Stages] Built {roster.AllStages.Count} stages, {wastelandPrefabs.Count + cryptPrefabs.Count + tundraPrefabs.Count} obstacle prefabs.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects")) AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(StageDir)) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Stages");
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder(MaterialDir)) AssetDatabase.CreateFolder("Assets/Materials", "Stages");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Obstacles")) AssetDatabase.CreateFolder("Assets/Prefabs", "Obstacles");
            foreach (var theme in new[] { "Wasteland", "Crypt", "Tundra" })
                if (!AssetDatabase.IsValidFolder($"{ObstacleDir}/{theme}"))
                    AssetDatabase.CreateFolder(ObstacleDir, theme);
        }

        private static void EnsureObstacleLayer()
        {
            if (LayerMask.NameToLayer(AddObstacleLayer.LayerName) < 0) AddObstacleLayer.Run();
        }

        private static Material EnsureMaterial(string fileName, Color color)
        {
            string path = $"{MaterialDir}/{fileName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static List<GameObject> BuildPrefabVariants(string[] fbxNames, string theme)
        {
            var result = new List<GameObject>();
            int obstacleLayer = LayerMask.NameToLayer(AddObstacleLayer.LayerName);
            foreach (var name in fbxNames)
            {
                GameObject fbx = null;
                foreach (var dir in NaturePackFbxDirs)
                {
                    fbx = AssetDatabase.LoadAssetAtPath<GameObject>($"{dir}/{name}.fbx");
                    if (fbx != null) break;
                }
                if (fbx == null) { Debug.LogWarning($"[Stages] FBX missing: {name}"); continue; }

                string prefabPath = $"{ObstacleDir}/{theme}/Obstacle_{name}.prefab";
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                instance.name = $"Obstacle_{name}";

                var bounds = ComputeRendererBounds(instance);
                float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                float targetMax = 2.0f;
                float scale = maxDim > 0.01f ? Mathf.Clamp(targetMax / maxDim, 0.5f, 3f) : 1f;
                instance.transform.localScale *= scale;

                Quaternion savedRot = instance.transform.rotation;
                instance.transform.rotation = Quaternion.identity;
                bounds = ComputeRendererBounds(instance);
                Vector3 localCenter = instance.transform.InverseTransformPoint(bounds.center);
                Vector3 ls = instance.transform.localScale;
                Vector3 localSize = new Vector3(
                    bounds.size.x / Mathf.Max(0.0001f, Mathf.Abs(ls.x)),
                    bounds.size.y / Mathf.Max(0.0001f, Mathf.Abs(ls.y)),
                    bounds.size.z / Mathf.Max(0.0001f, Mathf.Abs(ls.z))
                );
                instance.transform.rotation = savedRot;

                var box = instance.AddComponent<BoxCollider>();
                box.center = localCenter;
                box.size = localSize;

                var nmo = instance.AddComponent<NavMeshObstacle>();
                nmo.shape = NavMeshObstacleShape.Box;
                nmo.center = localCenter;
                nmo.size = localSize;
                nmo.carving = true;
                nmo.carveOnlyStationary = true;

                SetLayerRecursive(instance, obstacleLayer);

                var saved = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);
                if (saved != null) result.Add(saved);
            }
            return result;
        }

        private static Bounds ComputeRendererBounds(GameObject go)
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform) SetLayerRecursive(t.gameObject, layer);
        }

        private static StageDefinitionSO UpsertStage(string fileName, System.Action<StageDefinitionSO> apply)
        {
            string path = $"{StageDir}/{fileName}.asset";
            var def = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<StageDefinitionSO>();
                AssetDatabase.CreateAsset(def, path);
            }
            apply(def);
            EditorUtility.SetDirty(def);
            return def;
        }

        private static StageRoster EnsureRoster(StageDefinitionSO[] stages)
        {
            string path = $"{StageDir}/SO_StageRoster.asset";
            var roster = AssetDatabase.LoadAssetAtPath<StageRoster>(path);
            if (roster == null)
            {
                roster = ScriptableObject.CreateInstance<StageRoster>();
                AssetDatabase.CreateAsset(roster, path);
            }
            roster.AllStages = new List<StageDefinitionSO>(stages);
            EditorUtility.SetDirty(roster);
            return roster;
        }

        private static T EnsureSceneSingleton<T>(string name) where T : Component
        {
            var existing = Object.FindAnyObjectByType<T>();
            if (existing != null) return existing;
            var go = GameObject.Find(name) ?? new GameObject(name);
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        private static void WireStageManager(StageManager sm, StageRoster roster)
        {
            var groundGO = GameObject.Find("Ground");
            Renderer groundRenderer = groundGO != null ? groundGO.GetComponentInChildren<Renderer>() : null;

            var playerGO = GameObject.Find("Player");
            Transform spawnPoint = playerGO != null ? playerGO.transform : null;

            SetPrivateField(sm, "_roster", roster);
            SetPrivateField(sm, "_groundRenderer", groundRenderer);
            SetPrivateField(sm, "_playerSpawnPoint", spawnPoint);
            SetPrivateField(sm, "_arenaSize", 80f);
            SetPrivateField(sm, "_cellSize", 5f);
            EditorUtility.SetDirty(sm);
        }

        private static void BuildStageSelectCanvas(StageRoster roster)
        {
            var existing = GameObject.Find("StageSelect_Canvas");
            if (existing != null) Object.DestroyImmediate(existing);

            var canvas = new GameObject("StageSelect_Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = canvas.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 105;
            var s = canvas.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<StageSelectUI>();

            var panel = MakePanel(canvas.transform, "Panel", new Color(0.05f, 0.05f, 0.08f, 1f));

            var title = MakeText(panel.transform, "Title", "SELECT STAGE", 72,
                new Color(1f, 0.9f, 0.4f, 1f), bold: true);
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -90), new Vector2(1400, 110));

            var cardsRow = new GameObject("CardsRow", typeof(RectTransform));
            cardsRow.transform.SetParent(panel.transform, false);
            SetRect(cardsRow, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(1500, 520));

            var cards = new List<StageCardUI>();
            int n = roster.AllStages.Count;
            float cardWidth = 440f, spacing = 30f;
            float totalWidth = n * cardWidth + (n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                float x = -totalWidth / 2f + i * (cardWidth + spacing) + cardWidth / 2f;
                cards.Add(BuildStageCard(cardsRow.transform, i, new Vector2(x, 0), new Vector2(cardWidth, 520)));
            }

            var selectBtn = MakeButton(panel.transform, "SelectBtn", "SELECT", new Color(0.30f, 0.60f, 0.30f, 1f));
            SetRect(selectBtn.gameObject, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f),
                    new Vector2(0.5f, 0.5f), new Vector2(140, 0), new Vector2(320, 80));

            var backBtn = MakeButton(panel.transform, "BackBtn", "BACK", new Color(0.55f, 0.25f, 0.25f, 1f));
            SetRect(backBtn.gameObject, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f),
                    new Vector2(0.5f, 0.5f), new Vector2(-140, 0), new Vector2(220, 80));

            var ui = canvas.GetComponent<StageSelectUI>();
            SetPrivateField(ui, "_panel", panel);
            SetPrivateField(ui, "_titleText", title.GetComponent<TextMeshProUGUI>());
            SetPrivateField(ui, "_roster", roster);
            SetPrivateField(ui, "_selectButton", selectBtn);
            SetPrivateField(ui, "_backButton", backBtn);
            SetPrivateField(ui, "_cards", cards);
            SetPrivateField(ui, "_achievementLookup", LoadAllAchievements());
            EditorUtility.SetDirty(ui);

            panel.SetActive(false);
        }

        private static List<AchievementDefinitionSO> LoadAllAchievements()
        {
            var list = new List<AchievementDefinitionSO>();
            var guids = AssetDatabase.FindAssets("t:AchievementDefinitionSO");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var a = AssetDatabase.LoadAssetAtPath<AchievementDefinitionSO>(path);
                if (a != null) list.Add(a);
            }
            return list;
        }

        private static StageCardUI BuildStageCard(Transform parent, int idx, Vector2 pos, Vector2 size)
        {
            var go = new GameObject($"Card_{idx}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.20f, 0.25f, 1f);
            bg.sprite = GenerateFlatSprite.EnsureFlatSprite();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var card = go.AddComponent<StageCardUI>();
            var btn = go.AddComponent<Button>();

            var thumb = new GameObject("Thumbnail", typeof(RectTransform));
            thumb.transform.SetParent(go.transform, false);
            var thumbImg = thumb.AddComponent<Image>();
            thumbImg.color = Color.white;
            thumbImg.sprite = GenerateFlatSprite.EnsureFlatSprite();
            SetRect(thumb, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -30), new Vector2(size.x - 40, 200));

            var nameText = MakeText(go.transform, "Name", "Stage", 44, Color.white, true);
            SetRect(nameText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0, 70), new Vector2(size.x - 40, 60));

            var descText = MakeText(go.transform, "Desc", "", 22, new Color(0.85f, 0.85f, 0.85f, 1f), false);
            descText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            SetRect(descText, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(size.x - 40, 110));

            var lockText = MakeText(go.transform, "Lock", "", 22, new Color(1f, 0.5f, 0.5f, 1f), true);
            SetRect(lockText, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(size.x - 40, 60));

            SetPrivateField(card, "_background", bg);
            SetPrivateField(card, "_thumbnail", thumbImg);
            SetPrivateField(card, "_nameText", nameText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(card, "_descriptionText", descText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(card, "_lockText", lockText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(card, "_selectButton", btn);
            return card;
        }

        private static GameObject MakePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.sprite = GenerateFlatSprite.EnsureFlatSprite();
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return panel;
        }

        private static GameObject MakeText(Transform parent, string name, string text,
                                           int fontSize, Color color, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAlignmentOptions.Center;
            t.color = color;
            if (bold) t.fontStyle = FontStyles.Bold;
            return go;
        }

        private static Button MakeButton(Transform parent, string name, string label, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.sprite = GenerateFlatSprite.EnsureFlatSprite();
            var btn = go.AddComponent<Button>();
            var lbl = MakeText(go.transform, "Label", label, 30, Color.white, true);
            var lr = lbl.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.sizeDelta = Vector2.zero;
            return btn;
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

        private static void SetPrivateField(Object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (f == null) { Debug.LogError($"[Stages] Field '{fieldName}' not found on {target.GetType().Name}"); return; }
            f.SetValue(target, value);
        }
    }
}

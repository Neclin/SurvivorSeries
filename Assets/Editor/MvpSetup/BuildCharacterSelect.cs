using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorSeries.Characters;
using SurvivorSeries.Characters.Data;
using SurvivorSeries.UI.CharacterSelect;
using SurvivorSeries.Waves.Data;
using SurvivorSeries.Weapons.Data;

namespace SurvivorSeriesEditor
{
    public static class BuildCharacterSelect
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";
        private const string CharDir = "Assets/ScriptableObjects/Characters";
        private const string DiffDir = "Assets/ScriptableObjects/Difficulties";
        private const string WeaponDir = "Assets/ScriptableObjects/Weapons";

        [MenuItem("Survivor Series/Build Character Select")]
        public static void Run()
        {
            EnsureFolders();
            var roster = EnsureCharacters();
            var difficulties = LoadDifficulties();

            var scene = EditorSceneManager.OpenScene(ScenePath);
            BuildCanvas(roster, difficulties);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CharSelect] Built with {roster.AllCharacters.Count} characters and {difficulties.Count} difficulties.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(CharDir))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Characters");
        }

        private static CharacterRoster EnsureCharacters()
        {
            var sword = AssetDatabase.LoadAssetAtPath<WeaponDataSO>($"{WeaponDir}/SO_Weapon_Sword.asset");
            var dagger = AssetDatabase.LoadAssetAtPath<WeaponDataSO>($"{WeaponDir}/SO_Weapon_Dagger.asset");
            var spellbook = AssetDatabase.LoadAssetAtPath<WeaponDataSO>($"{WeaponDir}/SO_Weapon_Spellbook.asset");

            var warrior = UpsertCharacter("SO_Character_Warrior", c =>
            {
                c.CharacterName = "Warrior";
                c.Description = "A frontline brute. Soaks hits and deals heavy damage.";
                c.BaseMaxHealth = 90f;
                c.BaseMoveSpeed = 4.2f;
                c.BaseDamage = 10f;
                c.BaseArea = 1f;
                c.HealthGrowth = 1.25f;
                c.MoveSpeedGrowth = 0.85f;
                c.DamageGrowth = 1.15f;
                c.AreaGrowth = 1.0f;
                c.IsUnlockedByDefault = true;
                c.StartingWeapon = sword;
            });

            var rogue = UpsertCharacter("SO_Character_Rogue", c =>
            {
                c.CharacterName = "Rogue";
                c.Description = "Quick and agile. Low health, but blistering speed and cooldowns.";
                c.BaseMaxHealth = 55f;
                c.BaseMoveSpeed = 6.5f;
                c.BaseDamage = 8f;
                c.BaseArea = 1f;
                c.HealthGrowth = 0.85f;
                c.MoveSpeedGrowth = 1.25f;
                c.DamageGrowth = 1.0f;
                c.CooldownGrowth = 1.25f;
                c.AreaGrowth = 1.0f;
                c.IsUnlockedByDefault = true;
                c.StartingWeapon = dagger;
            });

            var mage = UpsertCharacter("SO_Character_Mage", c =>
            {
                c.CharacterName = "Mage";
                c.Description = "Channels arcane energy. Modest health but huge area scaling.";
                c.BaseMaxHealth = 65f;
                c.BaseMoveSpeed = 4.8f;
                c.BaseDamage = 9f;
                c.BaseArea = 1.1f;
                c.HealthGrowth = 1.0f;
                c.MoveSpeedGrowth = 1.0f;
                c.DamageGrowth = 1.05f;
                c.AreaGrowth = 1.35f;
                c.LuckGrowth = 1.15f;
                c.IsUnlockedByDefault = true;
                c.StartingWeapon = spellbook;
            });

            var rosterPath = $"{CharDir}/SO_CharacterRoster.asset";
            var roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>(rosterPath);
            if (roster == null)
            {
                roster = ScriptableObject.CreateInstance<CharacterRoster>();
                AssetDatabase.CreateAsset(roster, rosterPath);
            }
            roster.AllCharacters = new List<CharacterDefinitionSO> { warrior, rogue, mage };
            EditorUtility.SetDirty(roster);
            return roster;
        }

        private static CharacterDefinitionSO UpsertCharacter(string fileName, System.Action<CharacterDefinitionSO> apply)
        {
            var path = $"{CharDir}/{fileName}.asset";
            var def = AssetDatabase.LoadAssetAtPath<CharacterDefinitionSO>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<CharacterDefinitionSO>();
                AssetDatabase.CreateAsset(def, path);
            }
            apply(def);
            EditorUtility.SetDirty(def);
            return def;
        }

        private static List<DifficultySettingsSO> LoadDifficulties()
        {
            var list = new List<DifficultySettingsSO>();
            foreach (var name in new[] { "Easy", "Normal", "Hard", "Nightmare" })
            {
                var d = AssetDatabase.LoadAssetAtPath<DifficultySettingsSO>($"{DiffDir}/SO_Difficulty_{name}.asset");
                if (d != null) list.Add(d);
            }
            return list;
        }

        private static void BuildCanvas(CharacterRoster roster, List<DifficultySettingsSO> difficulties)
        {
            var canvas = ResetCanvas("CharacterSelect_Canvas", sortingOrder: 110);
            canvas.AddComponent<CharacterSelectUI>();

            var panel = MakePanel(canvas.transform, "Panel", new Color(0.05f, 0.05f, 0.08f, 1f));

            var title = MakeText(panel.transform, "Title", "CHOOSE YOUR HUNTER",
                fontSize: 72, color: new Color(1f, 0.9f, 0.4f, 1f), bold: true);
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -90), new Vector2(1400, 110));

            var cardsRow = new GameObject("CardsRow", typeof(RectTransform));
            cardsRow.transform.SetParent(panel.transform, false);
            SetRect(cardsRow, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(1500, 480));

            var cards = new List<CharacterCardUI>();
            int n = roster.AllCharacters.Count;
            float cardWidth = 440f;
            float spacing = 30f;
            float totalWidth = n * cardWidth + (n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                float x = -totalWidth / 2f + i * (cardWidth + spacing) + cardWidth / 2f;
                var card = BuildCharacterCard(cardsRow.transform, i, new Vector2(x, 0), new Vector2(cardWidth, 480));
                cards.Add(card);
            }

            var diffLabel = MakeText(panel.transform, "DifficultyLabel", "DIFFICULTY",
                fontSize: 36, color: Color.white, bold: true);
            SetRect(diffLabel, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f),
                    new Vector2(0, 60), new Vector2(600, 50));

            var diffRow = new GameObject("DifficultyRow", typeof(RectTransform));
            diffRow.transform.SetParent(panel.transform, false);
            SetRect(diffRow, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f),
                    new Vector2(0, -30), new Vector2(1200, 100));

            var diffButtons = new List<Button>();
            var diffLabels = new List<TextMeshProUGUI>();
            int dn = difficulties.Count;
            float dbW = 260f;
            float dbSpacing = 20f;
            float dbTotal = dn * dbW + (dn - 1) * dbSpacing;
            for (int i = 0; i < dn; i++)
            {
                float x = -dbTotal / 2f + i * (dbW + dbSpacing) + dbW / 2f;
                var btn = MakeButton(diffRow.transform, $"Diff{i}", difficulties[i].DifficultyName,
                    new Color(0.20f, 0.20f, 0.25f, 1f));
                SetRect(btn.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f), new Vector2(x, 0), new Vector2(dbW, 80));
                diffButtons.Add(btn);
                diffLabels.Add(btn.GetComponentInChildren<TextMeshProUGUI>());
            }

            var summary = MakeText(panel.transform, "Summary", "—",
                fontSize: 32, color: new Color(0.85f, 0.85f, 0.90f, 1f), bold: false);
            SetRect(summary, new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(900, 50));

            var startBtn = MakeButton(panel.transform, "StartBtn", "START",
                new Color(0.30f, 0.60f, 0.30f, 1f));
            SetRect(startBtn.gameObject, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f),
                    new Vector2(0.5f, 0.5f), new Vector2(140, 0), new Vector2(320, 80));

            var backBtn = MakeButton(panel.transform, "BackBtn", "BACK",
                new Color(0.55f, 0.25f, 0.25f, 1f));
            SetRect(backBtn.gameObject, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f),
                    new Vector2(0.5f, 0.5f), new Vector2(-140, 0), new Vector2(220, 80));

            var ui = canvas.GetComponent<CharacterSelectUI>();
            SetPrivateField(ui, "_panel", panel);
            SetPrivateField(ui, "_titleText", title.GetComponent<TextMeshProUGUI>());
            SetPrivateField(ui, "_roster", roster);
            SetPrivateField(ui, "_startButton", startBtn);
            SetPrivateField(ui, "_backButton", backBtn);
            SetPrivateField(ui, "_selectionSummary", summary.GetComponent<TextMeshProUGUI>());
            SetPrivateField(ui, "_difficulties", new List<DifficultySettingsSO>(difficulties));
            SetPrivateField(ui, "_difficultyButtons", new List<Button>(diffButtons));
            SetPrivateField(ui, "_difficultyLabels", new List<TextMeshProUGUI>(diffLabels));
            SetPrivateField(ui, "_characterCards", new List<CharacterCardUI>(cards));
            EditorUtility.SetDirty(ui);

            panel.SetActive(false);
        }

        private static void SetPrivateField(Object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (f == null) { Debug.LogError($"[CharSelect] Field '{fieldName}' not found on {target.GetType().Name}"); return; }
            f.SetValue(target, value);
        }

        private static CharacterCardUI BuildCharacterCard(Transform parent, int idx, Vector2 pos, Vector2 size)
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

            var card = go.AddComponent<CharacterCardUI>();
            var btn = go.AddComponent<Button>();

            var nameText = MakeText(go.transform, "Name", "Name", 44, Color.white, true);
            SetRect(nameText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -30), new Vector2(size.x - 40, 70));

            var descText = MakeText(go.transform, "Desc", "", 22, new Color(0.85f, 0.85f, 0.85f, 1f), false);
            descText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            SetRect(descText, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(size.x - 40, 200));

            var statsText = MakeText(go.transform, "Stats", "", 22, new Color(1f, 0.85f, 0.55f, 1f), true);
            SetRect(statsText, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(size.x - 40, 110));

            var so = new SerializedObject(card);
            so.FindProperty("_background").objectReferenceValue = bg;
            so.FindProperty("_nameText").objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_descriptionText").objectReferenceValue = descText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_statsText").objectReferenceValue = statsText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_selectButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            return card;
        }

        private static void FillList<T>(SerializedObject so, string propName, List<T> values) where T : Object
        {
            var prop = so.FindProperty(propName);
            prop.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static GameObject ResetCanvas(string name, int sortingOrder)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(name,
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortingOrder;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            return go;
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
    }
}

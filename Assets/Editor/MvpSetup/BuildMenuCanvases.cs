using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using SurvivorSeries.UI.Shop;
using SurvivorSeries.UI.LevelUp;
using SurvivorSeries.Shop;

namespace SurvivorSeriesEditor
{
    public static class BuildMenuCanvases
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Survivor Series/Build Shop + LevelUp Canvases")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            EnsureManagers();
            BuildShopCanvas();
            BuildLevelUpCanvas();
            WireShopGeneratorWeapons();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Canvases] Shop + LevelUp canvases built and saved.");
        }

        private static void EnsureManagers()
        {
            EnsureSceneSingleton<ShopManager>("ShopManager");
            EnsureSceneSingleton<ShopInventoryGenerator>("ShopInventoryGenerator");
            EnsureSceneSingleton<SurvivorSeries.LevelUp.UpgradeOptionGenerator>("UpgradeOptionGenerator");
            EnsureEventSystem();
        }

        private static void EnsureEventSystem()
        {
            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es != null) return;
            var go = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            Debug.Log("[Canvases] Added EventSystem with InputSystemUIInputModule.");
        }

        private static T EnsureSceneSingleton<T>(string name) where T : Component
        {
            var existing = Object.FindAnyObjectByType<T>();
            if (existing != null) return existing;
            var go = GameObject.Find(name) ?? new GameObject(name);
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        private static void BuildShopCanvas()
        {
            var canvas = FindOrCreateCanvas("Shop_Canvas", sortingOrder: 30);
            EnsureComp<ShopUI>(canvas);

            var panel = FindOrCreate(canvas.transform, "Panel");
            var pImg = EnsureComp<Image>(panel);
            pImg.color = new Color(0f, 0f, 0f, 0.85f);
            if (pImg.sprite == null) pImg.sprite = DefaultUiSprite();
            SetRect(panel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            EnsureComp<GraphicRaycaster>(canvas);

            var title = MakeText(panel.transform, "Title", "SHOP", 48, TextAlignmentOptions.Center, Color.white);
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -40), new Vector2(800, 70));

            var gold = MakeText(panel.transform, "Gold", "Gold: 0", 32, TextAlignmentOptions.Right,
                                new Color(1f, 0.85f, 0.2f, 1f));
            SetRect(gold, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-40, -40), new Vector2(280, 50));

            var grid = FindOrCreate(panel.transform, "ItemsGrid");
            var glg = EnsureComp<GridLayoutGroup>(grid);
            glg.cellSize = new Vector2(380, 200);
            glg.spacing = new Vector2(20, 20);
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            SetRect(grid, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0, 30), new Vector2(1180, 440));

            var slots = new ShopItemUI[6];
            for (int i = 0; i < 6; i++)
                slots[i] = BuildShopItemCard(grid.transform, $"ItemSlot_{i}");

            var rerollBtn = BuildButton(panel.transform, "RerollBtn", "Reroll (50g)",
                new Color(0.45f, 0.30f, 0.55f, 1f));
            SetRect(rerollBtn.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f), new Vector2(-160, 60), new Vector2(280, 70));

            var continueBtn = BuildButton(panel.transform, "ContinueBtn", "Continue",
                new Color(0.30f, 0.55f, 0.30f, 1f));
            SetRect(continueBtn.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f), new Vector2(160, 60), new Vector2(280, 70));

            var rerollLabel = rerollBtn.GetComponentInChildren<TextMeshProUGUI>();

            var ui = canvas.GetComponent<ShopUI>();
            var so = new SerializedObject(ui);
            SetRef(so, "_panel", panel);
            SetRef(so, "_titleText", title.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_currencyText", gold.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_rerollCostText", rerollLabel);
            SetRef(so, "_rerollButton", rerollBtn);
            SetRef(so, "_continueButton", continueBtn);
            var slotsProp = so.FindProperty("_itemSlots");
            slotsProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static ShopItemUI BuildShopItemCard(Transform parent, string name)
        {
            var card = FindOrCreate(parent, name);
            var bg = EnsureComp<Image>(card);
            bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
            if (bg.sprite == null) bg.sprite = DefaultUiSprite();

            var nameText = MakeText(card.transform, "Name", "Item", 26,
                TextAlignmentOptions.Center, Color.white);
            SetRect(nameText, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f),
                    new Vector2(0, -10), new Vector2(0, 36));

            var descText = MakeText(card.transform, "Desc", "Description", 18,
                TextAlignmentOptions.Center, new Color(0.85f, 0.85f, 0.85f, 1f));
            SetRect(descText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                    new Vector2(0, 0), new Vector2(-20, -100));

            var costText = MakeText(card.transform, "Cost", "0g", 28,
                TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f, 1f));
            SetRect(costText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                    new Vector2(-90, 18), new Vector2(120, 36));

            var buyGo = FindOrCreate(card.transform, "BuyBtn");
            var buyImg = EnsureComp<Image>(buyGo);
            buyImg.color = new Color(0.30f, 0.55f, 0.30f, 1f);
            if (buyImg.sprite == null) buyImg.sprite = DefaultUiSprite();
            var buyBtn = EnsureComp<Button>(buyGo);
            SetRect(buyGo, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                    new Vector2(80, 18), new Vector2(120, 36));
            var buyLabel = MakeText(buyGo.transform, "Label", "BUY", 20,
                TextAlignmentOptions.Center, Color.white);
            SetRect(buyLabel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);

            var soldGo = FindOrCreate(card.transform, "SoldOutOverlay");
            var soldImg = EnsureComp<Image>(soldGo);
            soldImg.color = new Color(0f, 0f, 0f, 0.6f);
            if (soldImg.sprite == null) soldImg.sprite = DefaultUiSprite();
            SetRect(soldGo, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);
            soldGo.SetActive(false);

            var ui = EnsureComp<ShopItemUI>(card);
            var so = new SerializedObject(ui);
            SetRef(so, "_nameText", nameText.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_descriptionText", descText.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_costText", costText.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_buyButton", buyBtn);
            SetRef(so, "_soldOutOverlay", soldGo);
            so.ApplyModifiedProperties();
            return ui;
        }

        private static void BuildLevelUpCanvas()
        {
            var canvas = FindOrCreateCanvas("LevelUp_Canvas", sortingOrder: 40);
            EnsureComp<LevelUpUI>(canvas);

            var panel = FindOrCreate(canvas.transform, "Panel");
            var pImg = EnsureComp<Image>(panel);
            pImg.color = new Color(0f, 0f, 0f, 0.85f);
            if (pImg.sprite == null) pImg.sprite = DefaultUiSprite();
            SetRect(panel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);
            EnsureComp<GraphicRaycaster>(canvas);

            var title = MakeText(panel.transform, "Title", "LEVEL UP!", 64,
                TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.4f, 1f));
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -80), new Vector2(800, 90));

            var row = FindOrCreate(panel.transform, "CardsRow");
            var hlg = EnsureComp<HorizontalLayoutGroup>(row);
            hlg.spacing = 30;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            SetRect(row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, 460));

            var cards = new LevelUpCardUI[3];
            for (int i = 0; i < 3; i++)
                cards[i] = BuildLevelUpCard(row.transform, $"Card_{i}");

            var ui = canvas.GetComponent<LevelUpUI>();
            var so = new SerializedObject(ui);
            SetRef(so, "_panel", panel);
            SetRef(so, "_titleText", title.GetComponent<TextMeshProUGUI>());
            var cardsProp = so.FindProperty("_cards");
            cardsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static LevelUpCardUI BuildLevelUpCard(Transform parent, string name)
        {
            var card = FindOrCreate(parent, name);
            var bg = EnsureComp<Image>(card);
            bg.color = new Color(0.18f, 0.18f, 0.25f, 0.95f);
            if (bg.sprite == null) bg.sprite = DefaultUiSprite();
            var rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(360, 440);

            var typeText = MakeText(card.transform, "Type", "TYPE", 20,
                TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f, 1f));
            SetRect(typeText, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f),
                    new Vector2(0, -16), new Vector2(0, 28));

            var nameText = MakeText(card.transform, "Name", "Upgrade", 30,
                TextAlignmentOptions.Center, Color.white);
            SetRect(nameText, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f),
                    new Vector2(0, -54), new Vector2(0, 44));

            var descText = MakeText(card.transform, "Desc", "Description", 18,
                TextAlignmentOptions.Center, new Color(0.85f, 0.85f, 0.85f, 1f));
            SetRect(descText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                    new Vector2(0, 0), new Vector2(-30, -200));

            var btnGo = FindOrCreate(card.transform, "ChooseBtn");
            var btnImg = EnsureComp<Image>(btnGo);
            btnImg.color = new Color(0.30f, 0.55f, 0.30f, 1f);
            if (btnImg.sprite == null) btnImg.sprite = DefaultUiSprite();
            var btn = EnsureComp<Button>(btnGo);
            SetRect(btnGo, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                    new Vector2(0, 30), new Vector2(280, 60));
            var btnLabel = MakeText(btnGo.transform, "Label", "CHOOSE", 26,
                TextAlignmentOptions.Center, Color.white);
            SetRect(btnLabel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);

            var ui = EnsureComp<LevelUpCardUI>(card);
            var so = new SerializedObject(ui);
            SetRef(so, "_nameText", nameText.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_descriptionText", descText.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_typeText", typeText.GetComponent<TextMeshProUGUI>());
            SetRef(so, "_chooseButton", btn);
            so.ApplyModifiedProperties();
            return ui;
        }

        private static void WireShopGeneratorWeapons()
        {
            string[] weaponPaths =
            {
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Bow.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Sword.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Dagger.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Axe.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Spellbook.asset",
                "Assets/ScriptableObjects/Weapons/SO_Weapon_Staff.asset",
            };
            string[] passivePaths = ScanAssetPaths("Assets/ScriptableObjects/Passives");

            var gen = Object.FindAnyObjectByType<ShopInventoryGenerator>();
            if (gen != null)
            {
                var so = new SerializedObject(gen);
                FillObjectArray(so, "_allWeapons", weaponPaths,
                    p => AssetDatabase.LoadAssetAtPath<SurvivorSeries.Weapons.Data.WeaponDataSO>(p));
                FillObjectArray(so, "_allPassives", passivePaths,
                    p => AssetDatabase.LoadAssetAtPath<SurvivorSeries.Passives.Data.PassiveItemDataSO>(p));
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(gen);
            }

            var lvlGen = Object.FindAnyObjectByType<SurvivorSeries.LevelUp.UpgradeOptionGenerator>();
            if (lvlGen != null)
            {
                var soL = new SerializedObject(lvlGen);
                FillObjectArray(soL, "_allWeapons", weaponPaths,
                    p => AssetDatabase.LoadAssetAtPath<SurvivorSeries.Weapons.Data.WeaponDataSO>(p));
                FillObjectArray(soL, "_allPassives", passivePaths,
                    p => AssetDatabase.LoadAssetAtPath<SurvivorSeries.Passives.Data.PassiveItemDataSO>(p));
                soL.ApplyModifiedProperties();
                EditorUtility.SetDirty(lvlGen);
            }
        }

        private static void FillObjectArray(SerializedObject so, string field, string[] paths,
                                            System.Func<string, Object> loader)
        {
            var arr = so.FindProperty(field);
            if (arr == null) { Debug.LogWarning($"[Canvases] No field {field}"); return; }
            arr.arraySize = paths.Length;
            for (int i = 0; i < paths.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = loader(paths[i]);
        }

        private static GameObject FindOrCreateCanvas(string name, int sortingOrder)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing;
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas),
                                          typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortingOrder;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            s.matchWidthOrHeight = 0.5f;
            return go;
        }

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) return t.gameObject;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static T EnsureComp<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static GameObject MakeText(Transform parent, string name, string text,
                                           int fontSize, TextAlignmentOptions align, Color color)
        {
            var go = FindOrCreate(parent, name);
            var t = EnsureComp<TextMeshProUGUI>(go);
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color;
            t.textWrappingMode = TextWrappingModes.Normal;
            return go;
        }

        private static Button BuildButton(Transform parent, string name, string label, Color color)
        {
            var go = FindOrCreate(parent, name);
            var img = EnsureComp<Image>(go);
            img.color = color;
            if (img.sprite == null) img.sprite = DefaultUiSprite();
            var btn = EnsureComp<Button>(go);
            var lbl = MakeText(go.transform, "Label", label, 26,
                TextAlignmentOptions.Center, Color.white);
            SetRect(lbl, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);
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

        private static void SetRef(SerializedObject so, string field, Object value)
        {
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"[Canvases] No field {field} on {so.targetObject.name}"); return; }
            p.objectReferenceValue = value;
        }

        private static Sprite DefaultUiSprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        private static string[] ScanAssetPaths(string dir)
        {
            if (!System.IO.Directory.Exists(dir)) return new string[0];
            var files = System.IO.Directory.GetFiles(dir, "*.asset");
            for (int i = 0; i < files.Length; i++) files[i] = files[i].Replace('\\', '/');
            System.Array.Sort(files);
            return files;
        }
    }
}

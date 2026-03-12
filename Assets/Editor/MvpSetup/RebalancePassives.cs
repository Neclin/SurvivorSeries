using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using SurvivorSeries.Player;
using SurvivorSeries.Passives.Data;
using SurvivorSeries.Enemies.Data;

namespace SurvivorSeriesEditor
{
    public static class RebalancePassives
    {
        private const string PassiveDir = "Assets/ScriptableObjects/Passives";
        private const string EnemyDir = "Assets/ScriptableObjects/Enemies";

        [MenuItem("Survivor Series/Rebalance Passives + Enemies")]
        public static void Run()
        {
            DeleteOldPassives();
            CreateNewPassives();
            BoostEnemies();
            BuildMenuCanvases.Build();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Rebalance] Passives rebuilt, enemies tuned, generators rewired.");
        }

        private static void DeleteOldPassives()
        {
            if (!Directory.Exists(PassiveDir)) return;
            foreach (var path in Directory.GetFiles(PassiveDir, "*.asset"))
                AssetDatabase.DeleteAsset(path.Replace('\\', '/'));
        }

        private static void CreateNewPassives()
        {
            Directory.CreateDirectory(PassiveDir);

            CreatePassive("IronHeart", "Iron Heart", maxLvl: 5, cost: 35,
                Mod(StatType.MaxHealth, flat: 15f));

            CreatePassive("Berserker", "Berserker's Rune", maxLvl: 5, cost: 45,
                Mod(StatType.Damage, pct: 0.20f),
                Mod(StatType.MoveSpeed, pct: -0.05f));

            CreatePassive("Boots", "Swift Boots", maxLvl: 5, cost: 40,
                Mod(StatType.MoveSpeed, pct: 0.10f),
                Mod(StatType.MaxHealth, pct: -0.05f));

            CreatePassive("Magnifier", "Magnifier", maxLvl: 5, cost: 35,
                Mod(StatType.Area, pct: 0.15f));

            CreatePassive("QuickCharge", "Quick Charge", maxLvl: 5, cost: 40,
                Mod(StatType.CooldownReduction, flat: 0.10f));

            CreatePassive("VelocityCrystal", "Velocity Crystal", maxLvl: 5, cost: 30,
                Mod(StatType.ProjectileSpeed, pct: 0.20f));

            CreatePassive("RabbitsFoot", "Rabbit's Foot", maxLvl: 5, cost: 35,
                Mod(StatType.Luck, pct: 0.20f));

            CreatePassive("PhoenixPlume", "Phoenix Plume", maxLvl: 5, cost: 50,
                Mod(StatType.Regen, flat: 0.5f));

            CreatePassive("PlateArmor", "Plate Armor", maxLvl: 5, cost: 50,
                Mod(StatType.Armor, flat: 2f),
                Mod(StatType.MoveSpeed, pct: -0.08f));

            CreatePassive("VampiricCharm", "Vampiric Charm", maxLvl: 5, cost: 55,
                Mod(StatType.Regen, flat: 1f),
                Mod(StatType.MaxHealth, flat: -8f));

            CreatePassive("GlassCannon", "Glass Cannon", maxLvl: 3, cost: 75,
                Mod(StatType.Damage, pct: 0.30f),
                Mod(StatType.ProjectileSpeed, pct: 0.20f),
                Mod(StatType.MaxHealth, pct: -0.20f));

            CreatePassive("WispsGrace", "Wisp's Grace", maxLvl: 3, cost: 70,
                Mod(StatType.MoveSpeed, pct: 0.10f),
                Mod(StatType.CooldownReduction, flat: 0.08f),
                Mod(StatType.Damage, pct: -0.12f));
        }

        private static StatModifier Mod(StatType s, float flat = 0f, float pct = 0f)
            => new StatModifier { Stat = s, FlatBonus = flat, PercentBonus = pct };

        private static void CreatePassive(string fileKey, string itemName, int maxLvl, int cost,
                                          params StatModifier[] mods)
        {
            string path = $"{PassiveDir}/SO_Passive_{fileKey}.asset";
            var so = ScriptableObject.CreateInstance<PassiveItemDataSO>();
            so.ItemName = itemName;
            so.MaxLevel = maxLvl;
            so.ShopPurchaseCost = cost;
            so.ModifiersPerLevel = new List<StatModifier>(mods);
            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[Rebalance] Created {path}: {so.GetDescription()}");
        }

        private static void BoostEnemies()
        {
            Tune("Walker",  hp: 16f, dmg: 8f,  speed: 3.0f, xp: 5);
            Tune("Speeder", hp: 10f, dmg: 6f,  speed: 5.5f, xp: 4);
            Tune("Ranged",  hp: 14f, dmg: 7f,  speed: 2.5f, xp: 6);
            Tune("Brute",   hp: 60f, dmg: 14f, speed: 1.8f, xp: 12);
            Tune("Bomber",  hp: 12f, dmg: 18f, speed: 3.2f, xp: 8);
            Tune("Elite",   hp: 90f, dmg: 16f, speed: 2.6f, xp: 18);
        }

        private static void Tune(string key, float hp, float dmg, float speed, int xp)
        {
            string path = $"{EnemyDir}/SO_Enemy_{key}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (so == null) { Debug.LogWarning($"[Rebalance] Missing {path}"); return; }
            so.BaseHealth = hp;
            so.BaseDamage = dmg;
            so.MoveSpeed = speed;
            so.XPDropAmount = xp;
            EditorUtility.SetDirty(so);
        }
    }
}

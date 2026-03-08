using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SurvivorSeries.Player;

namespace SurvivorSeries.Passives.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Passive Item", fileName = "SO_Passive_")]
    public class PassiveItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string ItemName;
        public Sprite Icon;

        [Header("Effect (applied each level)")]
        public List<StatModifier> ModifiersPerLevel = new();
        public int MaxLevel = 5;

        [Header("Shop")]
        public int ShopPurchaseCost = 80;

        public string GetDescription()
        {
            if (ModifiersPerLevel == null || ModifiersPerLevel.Count == 0)
                return "";

            var sb = new StringBuilder();
            for (int i = 0; i < ModifiersPerLevel.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(FormatModifier(ModifiersPerLevel[i]));
            }
            return sb.ToString();
        }

        private static string FormatModifier(StatModifier mod)
        {
            if (Mathf.Abs(mod.PercentBonus) > 0.0001f)
            {
                int pct = Mathf.RoundToInt(mod.PercentBonus * 100f);
                string sign = pct >= 0 ? "+" : "";
                return $"{sign}{pct}% {StatLabel(mod.Stat)}";
            }
            if (Mathf.Abs(mod.FlatBonus) > 0.0001f)
            {
                string sign = mod.FlatBonus >= 0 ? "+" : "";
                return $"{sign}{mod.FlatBonus:0.##} {StatLabel(mod.Stat)}";
            }
            return StatLabel(mod.Stat);
        }

        private static string StatLabel(StatType s) => s switch
        {
            StatType.MaxHealth => "Max Health",
            StatType.MoveSpeed => "Move Speed",
            StatType.Damage => "Damage",
            StatType.Area => "Area",
            StatType.ProjectileSpeed => "Proj. Speed",
            StatType.CooldownReduction => "Cooldown Red.",
            StatType.Luck => "Luck",
            StatType.Regen => "Regen",
            StatType.Armor => "Armor",
            _ => s.ToString()
        };
    }
}

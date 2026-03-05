using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Passives;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Weapons.Evolution
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Weapon Evolution Registry", fileName = "SO_WeaponEvolutionRegistry")]
    public class WeaponEvolutionRegistry : ScriptableObject
    {
        [Serializable]
        public struct EvolutionRecipe
        {
            public Data.WeaponDataSO Base;
            public Passives.Data.PassiveItemDataSO Passive;
            public Data.EvolvedWeaponDataSO Result;
        }

        [SerializeField] private List<EvolutionRecipe> _recipes = new();

        public IReadOnlyList<EvolutionRecipe> Recipes => _recipes;

        public Data.EvolvedWeaponDataSO TryGetEvolution(Data.WeaponDataSO weapon, Passives.Data.PassiveItemDataSO passive)
        {
            foreach (var recipe in _recipes)
            {
                if (recipe.Base == weapon && recipe.Passive == passive)
                    return recipe.Result;
            }
            return null;
        }

        public void CheckAndApplyEvolutions()
        {
            if (!ServiceLocator.TryGet<WeaponSlotManager>(out var wsm)) return;
            if (!ServiceLocator.TryGet<PassiveSlotManager>(out var psm)) return;

            foreach (WeaponBase weapon in wsm.GetAllWeapons())
            {
                if (weapon == null) continue;
                if (!weapon.CanEvolve()) continue;

                foreach (var recipe in _recipes)
                {
                    if (recipe.Base != weapon.Data) continue;
                    if (!psm.HasMaxLevel(recipe.Passive)) continue;

                    wsm.ApplyEvolution(weapon, recipe.Result);
                    break;
                }
            }
        }
    }
}
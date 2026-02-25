using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Characters.Data;

namespace SurvivorSeries.Characters
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Character Roster", fileName = "SO_CharacterRoster")]
    public class CharacterRoster : ScriptableObject
    {
        public List<CharacterDefinitionSO> AllCharacters;

        public List<CharacterDefinitionSO> GetUnlocked(Persistence.UnlockRegistry registry)
        {
            var result = new List<CharacterDefinitionSO>();
            foreach (var c in AllCharacters)
            {
                if (c.IsUnlockedByDefault || registry.IsCharacterUnlocked(c.CharacterName))
                    result.Add(c);
            }
            return result;
        }
    }
}

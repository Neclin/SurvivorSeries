using System.Collections.Generic;
using UnityEngine;

namespace SurvivorSeries.Stages.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Stage Roster", fileName = "SO_StageRoster")]
    public class StageRoster : ScriptableObject
    {
        public List<StageDefinitionSO> AllStages = new();
    }
}

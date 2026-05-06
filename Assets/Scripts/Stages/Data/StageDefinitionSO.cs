using UnityEngine;

namespace SurvivorSeries.Stages.Data
{
    [CreateAssetMenu(menuName = "SurvivorSeries/Stage Definition", fileName = "SO_Stage_")]
    public class StageDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string StageID;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Visuals")]
        public Material GroundMaterial;
        public Sprite PreviewImage;
        public Color PreviewTint = Color.white;

        [Header("Obstacles")]
        public GameObject[] ObstaclePrefabs;
        [Range(0f, 1f)] public float Density = 0.3f;
        public float MinPlayerClearRadius = 5f;

        [Header("Unlock")]
        public string UnlockAchievementID;
    }
}

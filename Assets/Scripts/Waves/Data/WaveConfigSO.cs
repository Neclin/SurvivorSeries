using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorSeries.Enemies.Data;

namespace SurvivorSeries.Waves.Data
{
    [Serializable]
    public struct EnemySpawnWeight
    {
        public EnemyDataSO EnemyData;
        [Range(0f, 1f)] public float Weight;
        [Tooltip("This enemy type only appears from this wave number onwards")]
        public int MinWave;
    }

    [CreateAssetMenu(menuName = "SurvivorSeries/Wave Config", fileName = "SO_WaveConfig")]
    public class WaveConfigSO : ScriptableObject
    {
        public int TotalWaves = 10;
        public float BaseWaveDuration = 60f;
        public int BaseEnemyCount = 30;
        public List<EnemySpawnWeight> EnemyWeights;
    }
}

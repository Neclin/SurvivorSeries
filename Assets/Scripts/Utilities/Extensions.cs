using UnityEngine;

namespace SurvivorSeries.Utilities
{
    public static class Extensions
    {
        public static Vector3 RandomPointOnCircle(float radius)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        public static Vector3 RandomSpawnPosition(Vector3 center, float spawnRadius)
        {
            return center + RandomPointOnCircle(spawnRadius);
        }

        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
        }

        public static bool HasFlag(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
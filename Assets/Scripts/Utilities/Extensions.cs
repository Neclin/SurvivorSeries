using UnityEngine;

namespace SurvivorSeries.Utilities
{
    public static class Extensions
    {
        /// <summary>Returns a random point on a circle of the given radius around the origin.</summary>
        public static Vector3 RandomPointOnCircle(float radius)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        /// <summary>Returns a random point on the circumference just outside a camera frustum (approximated as circle).</summary>
        public static Vector3 RandomSpawnPosition(Vector3 center, float spawnRadius)
        {
            return center + RandomPointOnCircle(spawnRadius);
        }

        /// <summary>Remaps a value from one range to another.</summary>
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

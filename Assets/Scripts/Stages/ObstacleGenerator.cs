using UnityEngine;
using SurvivorSeries.Stages.Data;

namespace SurvivorSeries.Stages
{
    public static class ObstacleGenerator
    {
        public static int Generate(StageDefinitionSO stage,
                                   Transform root,
                                   Vector3 origin,
                                   float arenaSize,
                                   float cellSize)
        {
            if (stage == null || stage.ObstaclePrefabs == null || stage.ObstaclePrefabs.Length == 0) return 0;
            if (root == null) return 0;

            int spawned = 0;
            float half = arenaSize * 0.5f;
            float clearSq = stage.MinPlayerClearRadius * stage.MinPlayerClearRadius;

            for (float cx = -half; cx < half; cx += cellSize)
            {
                for (float cz = -half; cz < half; cz += cellSize)
                {
                    Vector3 cellCenter = origin + new Vector3(cx + cellSize * 0.5f, 0f, cz + cellSize * 0.5f);
                    if ((cellCenter - origin).sqrMagnitude < clearSq) continue;
                    if (Random.value > stage.Density) continue;

                    var prefab = stage.ObstaclePrefabs[Random.Range(0, stage.ObstaclePrefabs.Length)];
                    if (prefab == null) continue;

                    Vector2 jitter2D = Random.insideUnitCircle * (cellSize * 0.35f);
                    Vector3 pos = cellCenter + new Vector3(jitter2D.x, 0f, jitter2D.y);
                    Quaternion yaw = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    float scale = Random.Range(0.7f, 1.45f);

                    var go = Object.Instantiate(prefab, root);
                    go.transform.position = pos;
                    go.transform.rotation = yaw * prefab.transform.rotation;
                    go.transform.localScale = prefab.transform.localScale * scale;

                    var rends = go.GetComponentsInChildren<Renderer>();
                    if (rends.Length > 0)
                    {
                        Bounds b = rends[0].bounds;
                        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                        float lift = origin.y - b.min.y;
                        go.transform.position += new Vector3(0f, lift, 0f);
                    }
                    spawned++;
                }
            }
            return spawned;
        }
    }
}

using UnityEngine;
using SurvivorSeries.Stages.Data;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Stages
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private StageRoster _roster;
        [SerializeField] private Renderer _groundRenderer;
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private float _arenaSize = 80f;
        [SerializeField] private float _cellSize = 5f;
        [SerializeField] private float _arenaBoundSize = 90f;
        [SerializeField] private float _wallHeight = 10f;

        public StageRoster Roster => _roster;
        public StageDefinitionSO PendingStage { get; set; }
        public StageDefinitionSO CurrentStage { get; private set; }

        private Transform _obstacleRoot;

        private void Awake()
        {
            ServiceLocator.Register<StageManager>(this);
            EnsureObstacleRoot();
            EnsureArenaWalls();
        }

        private void OnDestroy() => ServiceLocator.Unregister<StageManager>();

        private void EnsureObstacleRoot()
        {
            var existing = transform.Find("ObstacleRoot");
            if (existing != null) { _obstacleRoot = existing; return; }
            var go = new GameObject("ObstacleRoot");
            go.transform.SetParent(transform, false);
            _obstacleRoot = go.transform;
        }

        public async Awaitable LoadStage(StageDefinitionSO stage)
        {
            if (stage == null) return;
            CurrentStage = stage;
            ClearObstacles();
            ApplyGroundMaterial(stage);

            Random.InitState((int)System.DateTime.Now.Ticks);

            Vector3 origin = _playerSpawnPoint != null
                ? _playerSpawnPoint.position
                : (ServiceLocator.TryGet<Player.PlayerHealth>(out var ph) ? ph.transform.position : Vector3.zero);
            origin.y = _groundRenderer != null ? _groundRenderer.bounds.max.y : 0f;

            int spawned = ObstacleGenerator.Generate(stage, _obstacleRoot, origin, _arenaSize, _cellSize);
            Debug.Log($"[StageManager] Loaded '{stage.DisplayName}' — {spawned} obstacles spawned.");

            await Awaitable.NextFrameAsync();
        }

        private void EnsureArenaWalls()
        {
            if (transform.Find("ArenaWalls") != null) return;
            var wallsRoot = new GameObject("ArenaWalls");
            wallsRoot.transform.SetParent(transform, false);

            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            float half = _arenaBoundSize * 0.5f;
            float thickness = 2f;

            BuildWall(wallsRoot.transform, "N", new Vector3(0f, _wallHeight * 0.5f,  half), new Vector3(_arenaBoundSize + thickness * 2f, _wallHeight, thickness), obstacleLayer);
            BuildWall(wallsRoot.transform, "S", new Vector3(0f, _wallHeight * 0.5f, -half), new Vector3(_arenaBoundSize + thickness * 2f, _wallHeight, thickness), obstacleLayer);
            BuildWall(wallsRoot.transform, "E", new Vector3( half, _wallHeight * 0.5f, 0f), new Vector3(thickness, _wallHeight, _arenaBoundSize), obstacleLayer);
            BuildWall(wallsRoot.transform, "W", new Vector3(-half, _wallHeight * 0.5f, 0f), new Vector3(thickness, _wallHeight, _arenaBoundSize), obstacleLayer);
        }

        private static void BuildWall(Transform parent, string name, Vector3 pos, Vector3 size, int layer)
        {
            var go = new GameObject($"Wall_{name}");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            if (layer >= 0) go.layer = layer;
        }

        private void ClearObstacles()
        {
            if (_obstacleRoot == null) return;
            for (int i = _obstacleRoot.childCount - 1; i >= 0; i--)
                Destroy(_obstacleRoot.GetChild(i).gameObject);
        }

        private void ApplyGroundMaterial(StageDefinitionSO stage)
        {
            if (_groundRenderer == null || stage.GroundMaterial == null) return;
            _groundRenderer.sharedMaterial = stage.GroundMaterial;
        }
    }
}

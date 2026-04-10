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

        public StageRoster Roster => _roster;
        public StageDefinitionSO PendingStage { get; set; }
        public StageDefinitionSO CurrentStage { get; private set; }

        private Transform _obstacleRoot;

        private void Awake()
        {
            ServiceLocator.Register<StageManager>(this);
            EnsureObstacleRoot();
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

            int spawned = ObstacleGenerator.Generate(stage, _obstacleRoot, origin, _arenaSize, _cellSize);
            Debug.Log($"[StageManager] Loaded '{stage.DisplayName}' — {spawned} obstacles spawned.");

            await Awaitable.NextFrameAsync();
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

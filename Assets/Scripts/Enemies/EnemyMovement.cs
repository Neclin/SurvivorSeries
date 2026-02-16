using UnityEngine;
using UnityEngine.AI;

namespace SurvivorSeries.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Transform _target;

        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        public void Initialize(float speed)
        {
            _agent.speed = speed;
            _agent.angularSpeed = 360f;
            _agent.acceleration = 20f;
            _agent.stoppingDistance = 0.5f;
        }

        public void SetTarget(Transform target) => _target = target;

        public void Pause()
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        public void Resume() => _agent.isStopped = false;

        private bool _useOverride;
        private Vector3 _overrideDestination;

        /// <summary>Overrides target-following with a specific world position for this frame (and subsequent frames until cleared).</summary>
        public void SetDestinationOverride(Vector3 worldPos)
        {
            _useOverride = true;
            _overrideDestination = worldPos;
            _agent.isStopped = false;
        }

        public void ClearOverride()
        {
            _useOverride = false;
        }

        private void Update()
        {
            if (_target == null || !_agent.isOnNavMesh) return;
            _agent.SetDestination(_useOverride ? _overrideDestination : _target.position);
        }

        public void SetSpeed(float speed)
        {
            if (_agent != null) _agent.speed = speed;
        }
    }
}

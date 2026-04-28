using UnityEngine;
using UnityEngine.AI;

namespace SurvivorSeries.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Transform _target;
        private Animator _animator;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
        }

        public void Initialize(float speed)
        {
            _agent.speed = speed;
            _agent.angularSpeed = 360f;
            _agent.acceleration = 20f;
            _agent.stoppingDistance = 0.5f;
        }

        public void SetTarget(Transform target) => _target = target;

        public void Warp(Vector3 position)
        {
            if (_agent != null) _agent.Warp(position);
            else transform.position = position;
        }

        public void Pause()
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        public void Resume() => _agent.isStopped = false;

        private bool _useOverride;
        private Vector3 _overrideDestination;

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
            if (_animator != null)
                _animator.SetFloat(SpeedHash, _agent.velocity.magnitude);
        }

        public void SetSpeed(float speed)
        {
            if (_agent != null) _agent.speed = speed;
        }
    }
}
using UnityEngine;
using SurvivorSeries.Core;
using SurvivorSeries.Input;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 720f;

        private CharacterController _cc;
        private PlayerStats _stats;
        private PlayerHealth _health;
        private InputReader _input;
        private Animator _animator;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private Vector2 _moveInput;
        private Vector3 _velocity;
        private const float _gravity = -20f;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
            _health = GetComponent<PlayerHealth>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<InputReader>(out _input))
                _input.OnMove += HandleMove;
            else
                Debug.LogWarning("[PlayerController] InputReader not found in ServiceLocator.");

            if (_health != null)
                _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            if (_input != null)
                _input.OnMove -= HandleMove;

            if (_health != null)
                _health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (ServiceLocator.TryGet<GameManager>(out var gm))
                gm.OnPlayerDied();
        }

        private void Update()
        {
            Move();
            ApplyGravity();
            if (_animator != null)
                _animator.SetFloat(SpeedHash, _moveInput.magnitude);
        }

        private void HandleMove(Vector2 input) => _moveInput = input;

        private void Move()
        {
            if (_moveInput.sqrMagnitude < 0.01f) return;

            float speed = _stats != null ? _stats.MoveSpeed : 5f;
            Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

            _cc.Move(direction * speed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot,
                                                           _rotationSpeed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (_cc.isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;

            _velocity.y += _gravity * Time.deltaTime;
            _cc.Move(_velocity * Time.deltaTime);
        }
    }
}
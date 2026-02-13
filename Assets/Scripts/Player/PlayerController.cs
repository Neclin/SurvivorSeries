using UnityEngine;
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
        private InputReader _input;

        private Vector2 _moveInput;
        private Vector3 _velocity;
        private const float _gravity = -20f;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            // Subscribe in Start() — guaranteed to run after all Awake() calls,
            // so InputReader will already be registered in ServiceLocator.
            if (ServiceLocator.TryGet<InputReader>(out _input))
                _input.OnMove += HandleMove;
            else
                Debug.LogWarning("[PlayerController] InputReader not found in ServiceLocator.");
        }

        private void OnDisable()
        {
            if (_input != null)
                _input.OnMove -= HandleMove;
        }

        private void Update()
        {
            Move();
            ApplyGravity();
        }

        private void HandleMove(Vector2 input) => _moveInput = input;

        private void Move()
        {
            if (_moveInput.sqrMagnitude < 0.01f) return;

            float speed = _stats != null ? _stats.MoveSpeed : 5f;
            Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

            _cc.Move(direction * speed * Time.deltaTime);

            // Rotate player to face movement direction
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

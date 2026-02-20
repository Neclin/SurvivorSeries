using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Pickups
{
    public class XPGem : MonoBehaviour
    {
        [SerializeField] private float _magnetRadius = 5f;
        [SerializeField] private float _moveSpeed = 8f;

        private float _xpValue;
        private Transform _playerTransform;
        private Player.PlayerLevelSystem _levelSystem;
        private XPGemPool _pool;
        private bool _attracted;

        public void Initialize(float xpValue, Transform playerTransform,
                               Player.PlayerLevelSystem levelSystem, XPGemPool pool)
        {
            _xpValue = xpValue;
            _playerTransform = playerTransform;
            _levelSystem = levelSystem;
            _pool = pool;
            _attracted = false;
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            float dist = Vector3.Distance(transform.position, _playerTransform.position);

            if (dist <= _magnetRadius || _attracted)
            {
                _attracted = true;
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _playerTransform.position,
                    _moveSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _levelSystem?.AddXP(_xpValue);
            _pool?.Return(this);
        }
    }
}

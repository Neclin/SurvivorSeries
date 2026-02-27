using UnityEngine;

namespace SurvivorSeries.Camera
{
    /// <summary>
    /// Smooth overhead follow camera with configurable height and pitch angle.
    /// Attach to a Camera rig (empty parent GameObject); the Camera sits as a child.
    /// </summary>
    public class OverheadCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _height = 18f;
        [SerializeField] private float _pitchAngle = 55f;
        [SerializeField] private float _smoothSpeed = 8f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (_target == null) return;

            float rad = _pitchAngle * Mathf.Deg2Rad;
            float offsetZ = -Mathf.Cos(rad) * _height;
            float offsetY = Mathf.Sin(rad) * _height;

            Vector3 desiredPosition = _target.position + new Vector3(0f, offsetY, offsetZ);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition,
                                                    ref _velocity, 1f / _smoothSpeed);

            transform.LookAt(_target.position);
        }

        public void SetTarget(Transform target) => _target = target;

        public async Awaitable Shake(float duration, float magnitude,
                                     System.Threading.CancellationToken ct = default)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) break;

                Vector3 offset = Random.insideUnitSphere * magnitude;
                transform.localPosition = originalPos + offset;
                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(ct);
            }

            transform.localPosition = originalPos;
        }
    }
}

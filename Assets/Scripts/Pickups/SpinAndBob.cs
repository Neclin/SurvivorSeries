using UnityEngine;

namespace SurvivorSeries.Pickups
{
    public class SpinAndBob : MonoBehaviour
    {
        [SerializeField] private float _spinSpeed = 240f;
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobFrequency = 2.5f;

        private Vector3 _baseLocal;
        private float _phase;

        private void OnEnable()
        {
            _baseLocal = transform.localPosition;
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, _spinSpeed * Time.deltaTime, Space.World);
            float y = Mathf.Sin(Time.time * _bobFrequency + _phase) * _bobAmplitude;
            transform.localPosition = _baseLocal + Vector3.up * y;
        }
    }
}

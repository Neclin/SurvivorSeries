using UnityEngine;

namespace SurvivorSeries.Weapons
{
    [RequireComponent(typeof(LineRenderer))]
    public class LightningBolt : MonoBehaviour
    {
        private LineRenderer _line;
        private float _duration;
        private float _elapsed;
        private Color _baseColor;
        private static Material _sharedMaterial;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            if (_line.sharedMaterial == null) _line.sharedMaterial = GetSharedMaterial();
        }

        public void Initialize(Vector3 from, Vector3 to, Color color, float width, float duration, int segments = 10)
        {
            if (_line == null) _line = GetComponent<LineRenderer>();
            if (_line.sharedMaterial == null) _line.sharedMaterial = GetSharedMaterial();

            _duration = Mathf.Max(0.05f, duration);
            _elapsed = 0f;
            _baseColor = color;

            _line.useWorldSpace = true;
            _line.positionCount = segments + 1;
            _line.startWidth = width;
            _line.endWidth = width;
            _line.startColor = color;
            _line.endColor = color;

            Vector3 dir = to - from;
            float dist = dir.magnitude;
            Vector3 perp = Vector3.Cross(dir.normalized, Vector3.up).normalized;
            if (perp.sqrMagnitude < 0.01f) perp = Vector3.right;

            _line.SetPosition(0, from);
            _line.SetPosition(segments, to);
            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector3 along = Vector3.Lerp(from, to, t);
                float jitter = Random.Range(-1f, 1f) * dist * 0.06f;
                _line.SetPosition(i, along + perp * jitter);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration) { Destroy(gameObject); return; }
            float alpha = 1f - (_elapsed / _duration);
            Color c = _baseColor; c.a = alpha;
            _line.startColor = c;
            _line.endColor = c;
        }

        private static Material GetSharedMaterial()
        {
            if (_sharedMaterial != null) return _sharedMaterial;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            _sharedMaterial = new Material(shader);
            return _sharedMaterial;
        }
    }
}

using UnityEngine;

namespace SurvivorSeries.Utilities
{
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.1f;
        [SerializeField] private Color _flashColor = Color.red;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private Renderer[] _renderers;
        private MaterialPropertyBlock _block;
        private float _timer;
        private bool _flashing;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _block = new MaterialPropertyBlock();
        }

        public void Flash()
        {
            _timer = _duration;
            if (!_flashing)
            {
                _flashing = true;
                ApplyTint(_flashColor);
            }
        }

        private void Update()
        {
            if (!_flashing) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _flashing = false;
                ClearTint();
            }
        }

        private void OnDisable()
        {
            if (_flashing)
            {
                _flashing = false;
                ClearTint();
            }
        }

        private void ApplyTint(Color c)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_block);
                _block.SetColor(BaseColorId, c);
                _block.SetColor(ColorId, c);
                r.SetPropertyBlock(_block);
            }
        }

        private void ClearTint()
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
                if (r != null) r.SetPropertyBlock(null);
        }
    }
}

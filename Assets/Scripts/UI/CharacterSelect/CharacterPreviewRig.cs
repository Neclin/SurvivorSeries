using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using SurvivorSeries.Characters.Data;

namespace SurvivorSeries.UI.CharacterSelect
{
    public class CharacterPreviewRig : MonoBehaviour
    {
        [SerializeField] private Transform _mountPoint;
        [SerializeField] private Vector3 _rotationEulerPerSecond = new(0f, 45f, 0f);
        [SerializeField] private Vector3 _modelLocalEuler = Vector3.zero;
        [SerializeField] private Vector3 _modelLocalScale = Vector3.one;
        [SerializeField] private int _previewLayer = -1;
        [SerializeField] private AnimationClip _idleClip;
        [SerializeField, Range(0f, 1f)] private float _idleSampleTime = 0f;
        [SerializeField] private Avatar _idleAvatar;

        private GameObject _current;
        private CharacterDefinitionSO _currentDef;
        private PlayableGraph _graph;

        public void Show(CharacterDefinitionSO def)
        {
            if (_currentDef == def) return;
            ClearCurrent();
            _currentDef = def;
            if (def == null || def.Prefab == null || _mountPoint == null) return;

            _current = Instantiate(def.Prefab, _mountPoint);
            _current.transform.localPosition = Vector3.zero;
            _current.transform.localRotation = Quaternion.Euler(_modelLocalEuler);
            _current.transform.localScale = _modelLocalScale;
            StripGameplay(_current);
            if (_previewLayer >= 0)
                SetLayerRecursive(_current.transform, _previewLayer);
            ApplyIdlePose(_current);
        }

        private void ApplyIdlePose(GameObject go)
        {
            if (_idleClip == null) return;
            var animator = go.GetComponentInChildren<Animator>();
            if (animator == null) animator = go.AddComponent<Animator>();
            if (animator.avatar == null && _idleAvatar != null) animator.avatar = _idleAvatar;

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            DestroyGraph();
            AnimationPlayableUtilities.PlayClip(animator, _idleClip, out _graph);
            var output = (AnimationPlayableOutput)_graph.GetOutput(0);
            var clipPlayable = (AnimationClipPlayable)output.GetSourcePlayable();
            clipPlayable.SetApplyFootIK(false);

            float t = Mathf.Lerp(0f, Mathf.Max(0.0001f, _idleClip.length), _idleSampleTime);
            clipPlayable.SetTime(t);
            clipPlayable.SetSpeed(0d);
            _graph.Evaluate();
        }

        private void DestroyGraph()
        {
            if (_graph.IsValid()) _graph.Destroy();
        }

        private void OnDestroy() => DestroyGraph();

        public void Clear()
        {
            ClearCurrent();
            _currentDef = null;
        }

        private void ClearCurrent()
        {
            if (_current != null)
            {
                if (Application.isPlaying) Destroy(_current);
                else DestroyImmediate(_current);
                _current = null;
            }
        }

        private void Update()
        {
            if (_mountPoint == null) return;
            _mountPoint.Rotate(_rotationEulerPerSecond * Time.unscaledDeltaTime, Space.Self);
        }

        private static void StripGameplay(GameObject root)
        {
            foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
            foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            foreach (var b in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (b == null) continue;
                var ns = b.GetType().Namespace;
                if (ns != null && ns.StartsWith("SurvivorSeries"))
                    b.enabled = false;
            }
        }

        private static void SetLayerRecursive(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++) SetLayerRecursive(t.GetChild(i), layer);
        }
    }
}

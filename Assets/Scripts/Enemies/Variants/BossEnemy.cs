using System;
using System.Threading;
using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Enemies.Variants
{
    public class BossEnemy : EnemyBase
    {
        public static event Action OnBossDefeated;

        [Header("Ranged Attack")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _projectileSpeed = 11f;
        [SerializeField] private float _projectileDamage = 14f;
        [SerializeField] private float _rangedCooldown = 3.5f;
        [SerializeField] private int _projectilesPerVolley = 5;
        [SerializeField] private float _projectileSpreadDeg = 28f;

        [Header("AOE Slam")]
        [SerializeField] private float _aoeCooldown = 8f;
        [SerializeField] private float _aoeRadius = 5.5f;
        [SerializeField] private float _aoeDamage = 35f;
        [SerializeField] private float _aoeChargeTime = 1.25f;

        private float _rangedTimer;
        private float _aoeTimer;
        private Transform _playerTarget;
        private float _scaledProjectileDamage;
        private float _scaledAoeDamage;
        private CancellationTokenSource _cts;

        public override void Initialize(Data.EnemyDataSO data, Transform playerTarget,
                                        float hpMultiplier, float dmgMultiplier, EnemyPool pool)
        {
            base.Initialize(data, playerTarget, hpMultiplier, dmgMultiplier, pool);
            _playerTarget = playerTarget;
            _rangedTimer = 2.5f;
            _aoeTimer = 5f;
            _scaledProjectileDamage = _projectileDamage * dmgMultiplier;
            _scaledAoeDamage = _aoeDamage * dmgMultiplier;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        protected override void UpdateBehavior()
        {
            if (_playerTarget == null) return;
            _rangedTimer -= Time.deltaTime;
            _aoeTimer -= Time.deltaTime;

            if (_rangedTimer <= 0f)
            {
                FireVolley();
                _rangedTimer = _rangedCooldown;
            }
            if (_aoeTimer <= 0f)
            {
                _ = AoeSlamAsync(_cts.Token);
                _aoeTimer = _aoeCooldown;
            }
        }

        private void OnTriggerStay(Collider other) => HandleContactWithPlayer(other);

        private void FireVolley()
        {
            if (_projectilePrefab == null || _playerTarget == null) return;

            Vector3 origin = transform.position + Vector3.up * 1.4f;
            Vector3 baseDir = (_playerTarget.position + Vector3.up * 0.8f - origin).normalized;

            float step = _projectilesPerVolley > 1 ? _projectileSpreadDeg / (_projectilesPerVolley - 1) : 0f;
            float startAngle = -_projectileSpreadDeg * 0.5f;

            for (int i = 0; i < _projectilesPerVolley; i++)
            {
                float angle = startAngle + step * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;
                var go = Instantiate(_projectilePrefab, origin, Quaternion.identity);
                var proj = go.GetComponent<EnemyProjectile>();
                proj?.Fire(origin, dir, _scaledProjectileDamage, _projectileSpeed, 6f);
            }

            Audio.AudioManager.Play(Audio.SfxId.LightningStrike);
        }

        private async Awaitable AoeSlamAsync(CancellationToken ct)
        {
            var indicator = CreateAoeIndicator(transform.position, _aoeRadius);
            float elapsed = 0f;
            while (elapsed < _aoeChargeTime && !ct.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _aoeChargeTime);
                if (indicator != null)
                {
                    var rend = indicator.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        var c = rend.material.color;
                        c.a = Mathf.Lerp(0.25f, 0.75f, t);
                        rend.material.color = c;
                    }
                }
                await Awaitable.NextFrameAsync();
            }

            if (!ct.IsCancellationRequested && _playerTarget != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTarget.position);
                if (dist <= _aoeRadius)
                {
                    var ph = _playerTarget.GetComponentInParent<Player.PlayerHealth>();
                    if (ph != null) ph.TakeDamage(_scaledAoeDamage);
                }
                Audio.AudioManager.Play(Audio.SfxId.BossSpawn);
            }

            if (indicator != null) Destroy(indicator);
        }

        private GameObject CreateAoeIndicator(Vector3 center, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "BossAoeIndicator";
            Destroy(go.GetComponent<Collider>());
            go.transform.position = new Vector3(center.x, 0.05f, center.z);
            go.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);

            var rend = go.GetComponent<Renderer>();
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            mat.color = new Color(1f, 0.2f, 0.2f, 0.25f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            rend.material = mat;
            return go;
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            OnBossDefeated?.Invoke();

            if (ServiceLocator.TryGet<Core.GameManager>(out var gm))
                gm.OnBossDefeated();
            else if (ServiceLocator.TryGet<UI.Victory.VictoryUI>(out var ui))
                ui.Show();
        }
    }
}

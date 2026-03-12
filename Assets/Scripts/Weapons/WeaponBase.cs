using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SurvivorSeries.Passives;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        protected Data.WeaponDataSO _data;
        protected Data.EvolvedWeaponDataSO _evolvedData;
        protected int _level = 1;
        protected Player.PlayerStats _ownerStats;
        protected ProjectilePool _projectilePool;
        protected Transform _displayModel;

        public int SlotIndex { get; set; }
        public int TotalSlots { get; set; } = 6;

        private CancellationTokenSource _cts;
        private const float OrbitRadius = 1.4f;
        private const float OrbitHeight = 0.45f;
        private const float DisplayTurnSpeed = 540f;

        public Data.WeaponDataSO Data => _data;
        public bool IsEvolved => _evolvedData != null;
        public int Level => _level;
        public bool IsMaxLevel => _level >= (_data != null ? _data.MaxLevel : 8);

        public virtual void Initialize(Data.WeaponDataSO data, Player.PlayerStats stats)
        {
            _data = data;
            _ownerStats = stats;
            _level = 1;

            if (data != null && data.ProjectilePrefab != null)
            {
                var poolGO = new GameObject($"{data.WeaponName}_Pool");
                poolGO.transform.SetParent(transform, false);
                _projectilePool = poolGO.AddComponent<ProjectilePool>();

                var projPrefab = data.ProjectilePrefab.GetComponent<Projectile>();
                if (projPrefab != null)
                    _projectilePool.Initialize(projPrefab, 20);
            }

            CreateDisplayModel();

            _cts = new CancellationTokenSource();
            _ = FireLoop(_cts.Token);
        }

        public void InitializeEvolved(Data.EvolvedWeaponDataSO evolved, Player.PlayerStats stats)
        {
            _data = null;
            _evolvedData = evolved;
            _ownerStats = stats;
            _level = 8;

            _cts = new CancellationTokenSource();
            _ = FireLoop(_cts.Token);
        }

        private void CreateDisplayModel()
        {
            if (_data == null || _data.DisplayPrefab == null)
            {
                Debug.Log($"[Weapon] {(_data != null ? _data.WeaponName : "<null>")}: no DisplayPrefab assigned.");
                return;
            }
            var go = Instantiate(_data.DisplayPrefab, transform);
            go.name = "Display";
            _displayModel = go.transform;
            Debug.Log($"[Weapon] {_data.WeaponName} slot={SlotIndex} Display spawned (prefab={_data.DisplayPrefab.name}).");
        }

        protected virtual void Update()
        {
            if (_displayModel == null) return;

            float angleDeg = TotalSlots > 0 ? (360f * SlotIndex / TotalSlots) : 0f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector3 localOffset = new Vector3(Mathf.Sin(angleRad) * OrbitRadius,
                                              OrbitHeight,
                                              Mathf.Cos(angleRad) * OrbitRadius);
            _displayModel.localPosition = localOffset;

            var target = FindNearestEnemy();
            Vector3 desired;
            if (target != null)
            {
                desired = target.position - _displayModel.position;
            }
            else
            {
                desired = _displayModel.position - GetPlayerPosition();
            }
            desired.y = 0f;
            if (desired.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(desired);
                _displayModel.rotation = Quaternion.RotateTowards(
                    _displayModel.rotation, targetRot, DisplayTurnSpeed * Time.deltaTime);
            }
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        protected abstract void Fire();

        public void LevelUp()
        {
            if (IsMaxLevel) return;
            _level++;
        }

        public void SetLevel(int level)
        {
            int max = _data != null ? _data.MaxLevel : 8;
            _level = Mathf.Clamp(level, 1, max);
        }

        public bool CanEvolve()
        {
            if (_evolvedData != null) return false;
            if (!IsMaxLevel || _data == null || _data.EvolutionRequirement == null) return false;
            return ServiceLocator.TryGet<PassiveSlotManager>(out var pm)
                   && pm.HasMaxLevel(_data.EvolutionRequirement);
        }

        protected float GetDamage()
        {
            if (_data == null) return (_evolvedData?.Damage ?? 0f) * (_ownerStats != null ? _ownerStats.Damage : 1f);
            int idx = Mathf.Clamp(_level - 1, 0, _data.BaseDamagePerLevel.Length - 1);
            return _data.BaseDamagePerLevel[idx] * (_ownerStats != null ? _ownerStats.Damage : 1f);
        }

        protected float GetCooldown()
        {
            if (_data == null) return (_evolvedData?.Cooldown ?? 1f) * (_ownerStats != null ? _ownerStats.CooldownMultiplier : 1f);
            int idx = Mathf.Clamp(_level - 1, 0, _data.CooldownPerLevel.Length - 1);
            return _data.CooldownPerLevel[idx] * (_ownerStats != null ? _ownerStats.CooldownMultiplier : 1f);
        }

        protected float GetArea()
        {
            if (_data == null) return (_evolvedData?.Area ?? 1f) * (_ownerStats != null ? _ownerStats.Area : 1f);
            int idx = Mathf.Clamp(_level - 1, 0, _data.AreaPerLevel.Length - 1);
            return _data.AreaPerLevel[idx] * (_ownerStats != null ? _ownerStats.Area : 1f);
        }

        protected int GetProjectileCount()
        {
            if (_data == null) return _evolvedData?.ProjectileCount ?? 1;
            int idx = Mathf.Clamp(_level - 1, 0, _data.ProjectileCountPerLevel.Length - 1);
            return _data.ProjectileCountPerLevel[idx];
        }

        protected float GetDetectionRange() => _ownerStats != null ? _ownerStats.DetectionRange : 15f;

        protected Vector3 GetPlayerPosition()
        {
            if (_ownerStats != null) return _ownerStats.transform.position;
            if (ServiceLocator.TryGet<Player.PlayerController>(out var pc)) return pc.transform.position;
            return transform.position;
        }

        protected Transform FindNearestEnemy()
        {
            float closestSq = float.MaxValue;
            Transform result = null;
            var seen = new HashSet<int>();

            Vector3 origin = GetPlayerPosition();
            foreach (var col in Physics.OverlapSphere(origin, GetDetectionRange()))
            {
                var eh = col.GetComponentInParent<Enemies.EnemyHealth>();
                if (eh == null || eh.IsDead || !seen.Add(eh.EnemyId)) continue;

                float dsq = (eh.transform.position - origin).sqrMagnitude;
                if (dsq < closestSq) { closestSq = dsq; result = eh.transform; }
            }
            return result;
        }

        protected List<Enemies.EnemyHealth> FindEnemiesInRange(float range)
        {
            var list = new List<Enemies.EnemyHealth>();
            var seen = new HashSet<int>();

            foreach (var col in Physics.OverlapSphere(GetPlayerPosition(), range))
            {
                var eh = col.GetComponentInParent<Enemies.EnemyHealth>();
                if (eh == null || eh.IsDead || !seen.Add(eh.EnemyId)) continue;
                list.Add(eh);
            }
            return list;
        }

        private async Awaitable FireLoop(CancellationToken ct)
        {
            await Awaitable.NextFrameAsync(ct);
            while (!ct.IsCancellationRequested)
            {
                Fire();
                float cd = (_data != null || _evolvedData != null) ? Mathf.Max(0.1f, GetCooldown()) : 1f;
                await Awaitable.WaitForSecondsAsync(cd, ct);
            }
        }
    }
}
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

        private CancellationTokenSource _cts;

        public Data.WeaponDataSO Data => _data;
        public bool IsEvolved => _evolvedData != null;
        public int Level => _level;
        public bool IsMaxLevel => _level >= (_data != null ? _data.MaxLevel : 8);

        public virtual void Initialize(Data.WeaponDataSO data, Player.PlayerStats stats)
        {
            _data = data;
            _ownerStats = stats;
            _level = 1;

            // Create a projectile pool if this weapon uses projectiles
            if (data != null && data.ProjectilePrefab != null)
            {
                var poolGO = new GameObject($"{data.WeaponName}_Pool");
                poolGO.transform.SetParent(transform);
                _projectilePool = poolGO.AddComponent<ProjectilePool>();

                var projPrefab = data.ProjectilePrefab.GetComponent<Projectile>();
                if (projPrefab != null)
                    _projectilePool.Initialize(projPrefab, 20);
            }

            // Start fire loop here, after data is set
            _cts = new CancellationTokenSource();
            _ = FireLoop(_cts.Token);
        }

        /// <summary>
        /// Initializes this weapon from an evolved weapon definition (fixed single-level stats).
        /// Reads directly from EvolvedWeaponDataSO fields; _data remains null.
        /// </summary>
        public void InitializeEvolved(Data.EvolvedWeaponDataSO evolved, Player.PlayerStats stats)
        {
            _data = null;
            _evolvedData = evolved;
            _ownerStats = stats;
            _level = 8; // Max level by default

            // Start fire loop
            _cts = new CancellationTokenSource();
            _ = FireLoop(_cts.Token);
        }

        protected virtual void OnEnable()
        {
            // FireLoop is started by Initialize() instead, so the data is guaranteed
            // to be set before the first Fire() call.
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

        public bool CanEvolve()
        {
            if (_evolvedData != null) return false; // already evolved
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

        /// <summary>Returns the detection range from the owner's stats.</summary>
        protected float GetDetectionRange() => _ownerStats != null ? _ownerStats.DetectionRange : 15f;

        /// <summary>Finds the nearest active enemy within the player's detection range.</summary>
        protected Transform FindNearestEnemy()
        {
            float closestSq = float.MaxValue;
            Transform result = null;
            var seen = new HashSet<int>();

            foreach (var col in Physics.OverlapSphere(transform.position, GetDetectionRange()))
            {
                var eh = col.GetComponentInParent<Enemies.EnemyHealth>();
                if (eh == null || eh.IsDead || !seen.Add(eh.EnemyId)) continue;

                float dsq = (eh.transform.position - transform.position).sqrMagnitude;
                if (dsq < closestSq) { closestSq = dsq; result = eh.transform; }
            }
            return result;
        }

        /// <summary>
        /// Returns all living enemies whose colliders overlap a sphere of the given radius.
        /// Uses EnemyId to deduplicate enemies that have multiple colliders.
        /// </summary>
        protected List<Enemies.EnemyHealth> FindEnemiesInRange(float range)
        {
            var list = new List<Enemies.EnemyHealth>();
            var seen = new HashSet<int>();

            foreach (var col in Physics.OverlapSphere(transform.position, range))
            {
                var eh = col.GetComponentInParent<Enemies.EnemyHealth>();
                if (eh == null || eh.IsDead || !seen.Add(eh.EnemyId)) continue;
                list.Add(eh);
            }
            return list;
        }

        private async Awaitable FireLoop(CancellationToken ct)
        {
            // Small initial delay so Initialize() runs first
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

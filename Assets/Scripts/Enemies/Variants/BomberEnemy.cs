using UnityEngine;

namespace SurvivorSeries.Enemies.Variants
{
    public class BomberEnemy : EnemyBase
    {
        [SerializeField] private float _explosionTriggerRange = 2f;
        [SerializeField] private float _explosionRadius = 3f;

        private bool _hasExploded;

        public override void Initialize(Data.EnemyDataSO data, Transform playerTarget,
                                        float hpMultiplier, float dmgMultiplier, EnemyPool pool)
        {
            base.Initialize(data, playerTarget, hpMultiplier, dmgMultiplier, pool);
            _hasExploded = false;
        }

        protected override void UpdateBehavior()
        {
            if (_hasExploded || _data == null) return;

            Transform player = GetPlayerTransform();
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= _explosionTriggerRange)
                Explode();
        }

        private void Explode()
        {
            _hasExploded = true;

            Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius);
            foreach (var col in hits)
            {
                var ph = col.GetComponentInParent<Player.PlayerHealth>();
                if (ph != null) ph.TakeDamage(_data.BaseDamage);
            }

            Debug.Log($"[Bomber] Exploded at {transform.position}");
            OnDeath();
        }

        private Transform GetPlayerTransform()
        {
            if (Utilities.ServiceLocator.TryGet<Player.PlayerHealth>(out var ph))
                return ph.transform;
            return null;
        }

    }
}
using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    public class WhipWeapon : WeaponBase
    {
        private LineRenderer _arc;

        private void Awake() => SetupLineRenderer();

        private void SetupLineRenderer()
        {
            _arc = gameObject.GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
            _arc.loop = false;
            _arc.positionCount = 16;
            _arc.startWidth = 0.2f;
            _arc.endWidth = 0.05f;
            _arc.useWorldSpace = false;
            _arc.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                { color = new Color(1f, 0.4f, 0.1f, 1f) };
            _arc.enabled = false;
        }

        protected override void Fire()
        {
            float radius = GetArea() * 2f;
            var enemies = FindEnemiesInRange(radius);
            if (enemies.Count == 0) return;

            bool anyHit = false;
            foreach (var eh in enemies)
            {
                Vector3 toEnemy = eh.transform.position - transform.position;
                if (Vector3.Angle(transform.forward, toEnemy) <= 60f)
                {
                    eh.TakeDamage(GetDamage());
                    anyHit = true;
                }
            }

            if (anyHit) _ = FlashArc(radius);
        }

        private async Awaitable FlashArc(float radius)
        {
            if (_arc == null) return;

            // Draw a 120° arc in front of the weapon
            for (int i = 0; i < 16; i++)
            {
                float t = i / 15f;
                float angle = Mathf.Lerp(-60f, 60f, t) * Mathf.Deg2Rad;
                _arc.SetPosition(i, new Vector3(Mathf.Sin(angle) * radius, 0.1f, Mathf.Cos(angle) * radius));
            }
            _arc.enabled = true;
            await Awaitable.WaitForSecondsAsync(0.12f);
            if (_arc != null) _arc.enabled = false;
        }
    }
}

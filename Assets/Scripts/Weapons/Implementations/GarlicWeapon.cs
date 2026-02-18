using UnityEngine;

namespace SurvivorSeries.Weapons.Implementations
{
    public class GarlicWeapon : WeaponBase
    {
        private LineRenderer _ring;

        private void Awake() => SetupLineRenderer();

        private void SetupLineRenderer()
        {
            _ring = gameObject.GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
            _ring.loop = true;
            _ring.positionCount = 32;
            _ring.startWidth = 0.15f;
            _ring.endWidth = 0.15f;
            _ring.useWorldSpace = false;
            _ring.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                { color = new Color(0.6f, 0f, 0.9f, 1f) };
            _ring.enabled = false;
        }

        protected override void Fire()
        {
            float radius = GetArea() * 1.5f;
            var enemies = FindEnemiesInRange(radius);
            if (enemies.Count == 0) return;

            foreach (var eh in enemies)
                eh.TakeDamage(GetDamage());

            _ = PulseRing(radius);
        }

        private async Awaitable PulseRing(float radius)
        {
            if (_ring == null) return;

            // Draw circle at the aura radius
            for (int i = 0; i < 32; i++)
            {
                float angle = i * Mathf.PI * 2f / 32f;
                _ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.1f, Mathf.Sin(angle) * radius));
            }
            _ring.enabled = true;
            await Awaitable.WaitForSecondsAsync(0.18f);
            if (_ring != null) _ring.enabled = false;
        }
    }
}

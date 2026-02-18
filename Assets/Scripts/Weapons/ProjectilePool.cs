using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Weapons
{
    public class ProjectilePool : MonoBehaviour
    {
        private ObjectPool<Projectile> _pool;

        public void Initialize(Projectile prefab, int initialSize = 20)
        {
            _pool = new ObjectPool<Projectile>(prefab, initialSize, transform);
        }

        public Projectile Get() => _pool.Get();
        public void Return(Projectile p) => _pool.Return(p);
    }
}

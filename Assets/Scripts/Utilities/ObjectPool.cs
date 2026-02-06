using System.Collections.Generic;
using UnityEngine;

namespace SurvivorSeries.Utilities
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available = new();

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
                _available.Enqueue(CreateNew());
        }

        public T Get()
        {
            T obj = _available.Count > 0 ? _available.Dequeue() : CreateNew();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _available.Enqueue(obj);
        }

        public int AvailableCount => _available.Count;

        private T CreateNew()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            return obj;
        }
    }
}

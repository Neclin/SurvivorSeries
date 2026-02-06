using System;
using System.Collections.Generic;

namespace SurvivorSeries.Utilities
{
    /// <summary>
    /// Global service registry. Managers register themselves in Awake;
    /// consumers call Get<T>() instead of FindObjectOfType or static singletons.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out object service))
                return (T)service;

            throw new InvalidOperationException(
                $"ServiceLocator: No service of type {typeof(T).Name} is registered.");
        }

        public static bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out object raw))
            {
                service = (T)raw;
                return true;
            }
            service = default;
            return false;
        }

        public static void Unregister<T>()
        {
            _services.Remove(typeof(T));
        }

        /// <summary>Called during tests or scene reloads to clear state.</summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}

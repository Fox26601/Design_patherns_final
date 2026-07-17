using System;
using System.Collections.Generic;

namespace Shared
{
    /// <summary>
    /// Lightweight service locator for cross-scene service access.
    /// Pattern: Service Locator (https://www.unitydesignpatterns.com/patterns/servicelocator)
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<T>(T service) where T : class
        {
            Services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (Services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }

            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var value))
            {
                service = value as T;
                return service != null;
            }

            service = null;
            return false;
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}

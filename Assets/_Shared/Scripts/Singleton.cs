using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Generic singleton base for MonoBehaviour services.
  /// Pattern: Singleton (https://www.unitydesignpatterns.com/patterns/singleton)
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = FindFirstObjectByType<T>();
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            OnSingletonAwake();
        }

        protected virtual void OnSingletonAwake() { }
    }
}

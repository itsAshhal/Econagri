using UnityEngine;


namespace Econagri.Singleton
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        private static object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // Only search for instances in the current scene to avoid potential conflicts
                            _instance = FindObjectOfType<T>();

                            if (_instance == null)
                            {
                                // Create a new instance only if none exists
                                GameObject singletonGameObject = new GameObject($"{typeof(T).Name} (Singleton)");
                                _instance = singletonGameObject.AddComponent<T>();
                                DontDestroyOnLoad(singletonGameObject);
                            }
                        }
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // Handle duplicate instance gracefully, e.g., logging a warning or destroying this instance
                Debug.LogWarning($"Duplicate instance of {typeof(T)} found! Destroying this instance.");
                Destroy(gameObject);
            }
        }
    }

}
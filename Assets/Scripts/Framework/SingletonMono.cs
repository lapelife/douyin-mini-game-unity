using UnityEngine;

namespace DouyinMiniGame.Framework
{
    /// <summary>
    /// MonoBehaviour 单例基类，线程安全，自动管理生命周期。
    /// </summary>
    public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _appIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_appIsQuitting)
                {
                    Debug.LogWarning($"[SingletonMono] {typeof(T)} 已在退出时销毁，返回 null。");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            var go = new GameObject($"[{typeof(T).Name}]");
                            _instance = go.AddComponent<T>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _appIsQuitting = true;
        }
    }
}

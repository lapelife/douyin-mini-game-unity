using UnityEngine;
using DouyinMiniGame.Framework;
using DouyinMiniGame.Core;
using DouyinMiniGame.SDK;
using DouyinMiniGame.Gameplay;

namespace DouyinMiniGame.Bootstrap
{
    /// <summary>
    /// 游戏引导器 —— 通过 RuntimeInitializeOnLoadMethod 在运行时自动创建所有核心系统。
    /// 无需手动搭建场景，打开任何场景点击 Play 即可自动启动游戏。
    /// 初始化顺序: 配置相机 -> 框架系统 -> SDK -> 玩法系统 -> UI -> 进入菜单
    /// </summary>
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Debug.Log("====================================");
            Debug.Log("[GameBootstrapper] 抖音小游戏框架启动中...");
            Debug.Log($"[GameBootstrapper] 平台: {PlatformInfo.PlatformName}");
            Debug.Log("====================================");

            // 创建游戏根节点
            var rootGo = new GameObject("--- DouyinMiniGame Root ---");
            Object.DontDestroyOnLoad(rootGo);

            // 1. 创建主相机（如果场景中没有）
            SetupCamera(rootGo.transform);

            // 2. 初始化框架系统（Singleton 会自动创建 GameObject）
            var sdk = DouyinSDKBridge.Instance;       // SDK 桥接层
            var scoreMgr = ScoreManager.Instance;      // 分数管理
            var gameMgr = GameManager.Instance;        // 游戏管理器
            var spawner = ItemSpawner.Instance;        // 物体生成器
            var basket = BasketController.Instance;    // 篮子控制
            var uiMgr = UIManager.Instance;             // UI管理器

            // 3. 设置层级关系
            sdk.transform.SetParent(rootGo.transform);
            scoreMgr.transform.SetParent(rootGo.transform);
            gameMgr.transform.SetParent(rootGo.transform);
            spawner.transform.SetParent(rootGo.transform);
            basket.transform.SetParent(rootGo.transform);
            uiMgr.transform.SetParent(rootGo.transform);

            Debug.Log("[GameBootstrapper] 所有系统初始化完成!");
            Debug.Log("[GameBootstrapper] 游戏框架版本: 1.0.0");
        }

        /// <summary>配置主相机</summary>
        private static void SetupCamera(Transform parent)
        {
            var existingCam = Camera.main;
            if (existingCam != null)
            {
                // 配置已有相机
                existingCam.orthographic = true;
                existingCam.orthographicSize = 5f;
                existingCam.backgroundColor = new Color(0.2f, 0.6f, 0.9f, 1f);
                return;
            }

            // 创建新的主相机
            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(parent);

            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.2f, 0.6f, 0.9f, 1f);

            // AudioListener 在中国版 Unity 中可能缺少 Audio 模块，用反射安全添加
            try
            {
                var alType = System.Type.GetType("UnityEngine.AudioListener, UnityEngine.AudioModule");
                if (alType != null && camGo.GetComponent(alType) == null)
                {
                    camGo.AddComponent(alType);
                }
            }
            catch { /* Audio 模块不可用时静默跳过 */ }
        }
    }
}

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
    /// 使用 AfterSceneLoad 确保场景相机已存在后再进行配置。
    /// </summary>
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            // 防止重复执行：检查根节点是否已存在
            if (GameObject.Find("--- DouyinMiniGame Root ---") != null)
            {
                Debug.Log("[GameBootstrapper] 框架已初始化，跳过重复引导。");
                return;
            }

            Debug.Log("====================================");
            Debug.Log("[GameBootstrapper] 抖音小游戏框架启动中...");
            Debug.Log($"[GameBootstrapper] 平台: {PlatformInfo.PlatformName}");
            Debug.Log("====================================");

            // 创建游戏根节点
            var rootGo = new GameObject("--- DouyinMiniGame Root ---");
            Object.DontDestroyOnLoad(rootGo);

            // 1. 配置主相机（场景已加载，可以找到现有相机）
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

        /// <summary>配置主相机 —— 正交模式，纯色背景，正确位置</summary>
        private static void SetupCamera(Transform parent)
        {
            Camera mainCam = null;

            // 查找所有相机，找到 MainCamera 或任意相机
            var allCameras = Object.FindObjectsOfType<Camera>();
            foreach (var cam in allCameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    mainCam = cam;
                    break;
                }
            }

            // 如果没有 MainCamera，取第一个相机
            if (mainCam == null && allCameras.Length > 0)
            {
                mainCam = allCameras[0];
                mainCam.tag = "MainCamera";
            }

            // 如果场景中完全没有相机，创建一个新的
            if (mainCam == null)
            {
                Debug.Log("[GameBootstrapper] 场景中未找到相机，创建新相机。");
                var camGo = new GameObject("MainCamera");
                camGo.tag = "MainCamera";
                camGo.transform.SetParent(parent);
                mainCam = camGo.AddComponent<Camera>();

                // 安全添加 AudioListener
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

            // 配置相机：正交 + 纯色背景 + 正确位置
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.2f, 0.6f, 0.9f, 1f);
            mainCam.depth = 10;
            mainCam.nearClipPlane = 0.1f;
            mainCam.farClipPlane = 100f;

            // 设置相机位置和朝向（确保能看到 z=0 平面的物体）
            mainCam.transform.position = new Vector3(0, 0, -10f);
            mainCam.transform.rotation = Quaternion.identity;
            mainCam.transform.SetParent(parent);

            // 销毁场景中其他多余相机，防止覆盖渲染
            foreach (var cam in allCameras)
            {
                if (cam != null && cam != mainCam)
                {
                    Debug.Log($"[GameBootstrapper] 销毁多余相机: {cam.name}");
                    Object.Destroy(cam.gameObject);
                }
            }

            Debug.Log($"[GameBootstrapper] 相机配置完成: ortho={mainCam.orthographic}, size={mainCam.orthographicSize}, pos={mainCam.transform.position}");
        }
    }
}

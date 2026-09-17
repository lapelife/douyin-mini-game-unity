using UnityEngine;
using DouyinMiniGame.Framework;

namespace DouyinMiniGame.SDK
{
    /// <summary>
    /// 抖音SDK桥接层 —— 统一封装抖音平台能力。
    /// 在编辑器下提供模拟实现，在抖音平台下调用真实SDK。
    /// 需要在 Player Settings 中添加宏定义: DOUYIN_MINIGAME_SDK
    /// </summary>
    public class DouyinSDKBridge : SingletonMono<DouyinSDKBridge>
    {
        [Header("SDK 配置")]
        [Tooltip("抖音小游戏 AppID（在抖音开放平台创建游戏后获取）")]
        [SerializeField] private string _appId = "";

        /// <summary>抖音小游戏 AppID</summary>
        public string AppId => _appId;

        /// <summary>SDK 是否已初始化</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>当前玩家 OpenID</summary>
        public string OpenId { get; private set; } = "";

        #region 生命周期

        protected override void Awake()
        {
            base.Awake();
            InitializeSDK();
        }

        /// <summary>
        /// 初始化抖音 SDK。
        /// 在编辑器下模拟初始化流程，在抖音平台调用真实初始化。
        /// </summary>
        private void InitializeSDK()
        {
            if (PlatformInfo.IsDouyinPlatform)
            {
#if DOUYIN_MINIGAME_SDK
                // ====== 抖音平台真实初始化 ======
                Debug.Log("[DouyinSDK] 正在初始化抖音 SDK...");

                // StarkSDK 初始化
                // StarkSDK.API.StarkSDKBridge.Init();
                // StarkSDK.API.Stark.Login.OnLoginSuccess += OnLoginSuccess;
                // StarkSDK.API.Stark.Login.Login();

                IsInitialized = true;
                Debug.Log("[DouyinSDK] 抖音 SDK 初始化成功!");
#else
                Debug.LogWarning("[DouyinSDK] 已定义抖音平台但未安装 StarkSDK，请导入 SDK 资源包。");
#endif
            }
            else
            {
                // ====== 编辑器模拟初始化 ======
                Debug.Log("[DouyinSDK] 编辑器模式 - SDK 模拟初始化完成。");
                OpenId = "editor_mock_user";
                IsInitialized = true;
            }

            EventBus.Fire("SDKInitialized");
        }

        #endregion

        #region 平台能力封装

        /// <summary>
        /// 显示激励视频广告。
        /// </summary>
        public void ShowRewardedAd(System.Action onReward = null, System.Action onFail = null)
        {
            if (PlatformInfo.IsDouyinPlatform)
            {
#if DOUYIN_MINIGAME_SDK
                // var ad = StarkSDK.API.Stark.AdManager.CreateRewardedVideoAd("ad_unit_id");
                // ad.OnClose += (isReward) => { if (isReward) onReward?.Invoke(); };
                // ad.ShowAd();
                Debug.Log("[DouyinSDK] 激励视频广告已展示");
                onReward?.Invoke(); // 临时模拟
#else
                onReward?.Invoke();
#endif
            }
            else
            {
                Debug.Log("[DouyinSDK] 编辑器模拟 - 激励视频广告完成");
                onReward?.Invoke();
            }
        }

        /// <summary>
        /// 提交分数到排行榜。
        /// </summary>
        public void SubmitScore(int score, System.Action<bool> callback = null)
        {
            if (PlatformInfo.IsDouyinPlatform)
            {
#if DOUYIN_MINIGAME_SDK
                // StarkSDK.API.Stark.OpenWindow.ShowRankingWindow();
                // 或者调用云函数提交分数
                Debug.Log($"[DouyinSDK] 分数已提交到排行榜: {score}");
#else
                Debug.Log($"[DouyinSDK] 编辑器模拟 - 分数提交: {score}");
#endif
            }
            else
            {
                Debug.Log($"[DouyinSDK] 编辑器模拟 - 分数提交: {score}");
            }
            callback?.Invoke(true);
        }

        /// <summary>
        /// 分享游戏到抖音。
        /// </summary>
        public void ShareGame(string title = "来玩接金币吧！", System.Action onShareComplete = null)
        {
            if (PlatformInfo.IsDouyinPlatform)
            {
#if DOUYIN_MINIGAME_SDK
                // StarkSDK.API.Stark.OpenWindow.ShareAppMessage(title);
                Debug.Log("[DouyinSDK] 已触发抖音分享");
#endif
            }
            else
            {
                Debug.Log($"[DouyinSDK] 编辑器模拟 - 分享: {title}");
            }
            onShareComplete?.Invoke();
        }

        /// <summary>
        /// 短震动反馈。
        /// </summary>
        public void Vibrate()
        {
            if (PlatformInfo.IsDouyinPlatform)
            {
#if DOUYIN_MINIGAME_SDK
                // StarkSDK.API.Stark.Device.VibrateShort();
#endif
            }
            else
            {
                Handheld.Vibrate();
            }
        }

        #endregion
    }
}

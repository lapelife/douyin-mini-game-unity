using UnityEngine;

namespace DouyinMiniGame.SDK
{
    /// <summary>
    /// 平台检测工具，判断当前运行环境。
    /// 通过条件编译宏 DOUYIN_MINIGAME_SDK 区分抖音环境和编辑器。
    /// </summary>
    public static class PlatformInfo
    {
        /// <summary>是否运行在抖音小游戏平台</summary>
        public static bool IsDouyinPlatform
        {
            get
            {
#if DOUYIN_MINIGAME_SDK
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>是否在 Unity 编辑器中运行</summary>
        public static bool IsEditor => Application.isEditor;

        /// <summary>获取平台名称</summary>
        public static string PlatformName => IsDouyinPlatform ? "抖音小游戏" : (IsEditor ? "Unity编辑器" : "其他平台");

        /// <summary>获取设备唯一标识（抖音平台使用 openId，编辑器使用设备ID）</summary>
        public static string DeviceId
        {
            get
            {
                if (IsDouyinPlatform)
                {
#if DOUYIN_MINIGAME_SDK
                    // 在抖音平台可调用 StarkSDK 获取 openId
                    // return StarkSDK.API.Stark.Login.GetOpenId();
#endif
                    return "douyin_player";
                }
                return SystemInfo.deviceUniqueIdentifier;
            }
        }
    }
}

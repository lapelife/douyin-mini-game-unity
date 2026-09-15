#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DouyinMiniGame.EditorTools
{
    /// <summary>
    /// 场景设置工具 —— 通过菜单一键创建游戏主场景。
    /// 菜单: Tools > 抖音小游戏 > 创建主场景
    /// </summary>
    public static class SceneSetupTool
    {
        [MenuItem("Tools/抖音小游戏/创建主场景 (Create Main Scene)")]
        public static void CreateMainScene()
        {
            // 创建新场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建一个空 GameObject 作为场景占位（Bootstrapper 会自动创建所有系统）
            var placeholder = new GameObject("--- 场景已自动配置 ---");
            Debug.Log("[SceneSetupTool] 主场景已创建。点击 Play 即可运行游戏!");

            // 保存场景
            string scenePath = "Assets/Scenes/Main.unity";
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            EditorSceneManager.SaveScene(scene, scenePath);

            // 将场景添加到 Build Settings
            var scenes = EditorBuildSettings.scenes;
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(newScenes, 0);
            newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;

            EditorUtility.DisplayDialog(
                "场景创建成功",
                "主场景已创建并添加到 Build Settings。\n\n点击 Play 按钮即可运行游戏！\n\nGameBootstrapper 会在运行时自动初始化所有系统。",
                "确定"
            );
        }

        [MenuItem("Tools/抖音小游戏/配置 Player Settings")]
        public static void ConfigurePlayerSettings()
        {
            // 设置公司名和产品名
            PlayerSettings.companyName = "DouyinMiniGame";
            PlayerSettings.productName = "接金币";

            // 设置竖屏（接金币游戏适合竖屏）
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // 设置目标 API
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel_24;

            // 添加抖音 SDK 宏定义
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (!defines.Contains("DOUYIN_MINIGAME_SDK"))
            {
                defines += ";DOUYIN_MINIGAME_SDK";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
                Debug.Log("[SceneSetupTool] 已添加宏定义: DOUYIN_MINIGAME_SDK");
            }

            var definesIOS = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            if (!definesIOS.Contains("DOUYIN_MINIGAME_SDK"))
            {
                definesIOS += ";DOUYIN_MINIGAME_SDK";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, definesIOS);
            }

            EditorUtility.DisplayDialog(
                "Player Settings 配置完成",
                "已设置:\n" +
                "- 产品名称: 接金币\n" +
                "- 横屏模式\n" +
                "- Android minSdk 24\n" +
                "- 宏定义: DOUYIN_MINIGAME_SDK\n\n" +
                "注意: 导入抖音 StarkSDK 后宏定义才会生效。",
                "确定"
            );
        }

        [MenuItem("Tools/抖音小游戏/查看框架说明")]
        public static void ShowHelp()
        {
            EditorUtility.DisplayDialog(
                "抖音小游戏 Unity 框架 v1.0",
                "框架架构:\n" +
                "  - Framework: 单例基类、事件总线、状态机\n" +
                "  - SDK: 抖音SDK桥接层（广告/分享/排行榜）\n" +
                "  - Core: GameManager、UIManager\n" +
                "  - Gameplay: ItemSpawner、FallingItem、BasketController\n" +
                "  - Bootstrap: 自动启动引导器\n\n" +
                "游戏玩法: 接金币\n" +
                "  - 左右滑动控制篮子\n" +
                "  - 接住金色金币 +10分\n" +
                "  - 避开红色炸弹（扣分扣命）\n" +
                "  - 3条生命，用完游戏结束\n\n" +
                "快捷操作:\n" +
                "  1. Tools > 抖音小游戏 > 创建主场景\n" +
                "  2. Tools > 抖音小游戏 > 配置 Player Settings\n" +
                "  3. 点击 Play 运行",
                "了解"
            );
        }
    }
}
#endif

# 抖音小游戏 Unity 框架 - 接金币

> 基于 Unity 2022.3 的抖音小游戏框架，含完整游戏"接金币"，零预制体、零外部资源依赖。

## 快速开始（3步）

### 1. 打开项目
用 Unity Hub 打开 `DouyinMiniGame` 文件夹（需 Unity 2022.3 LTS）。

### 2. 一键配置
打开 Unity 后，点击菜单栏：
```
Tools → 抖音小游戏 → 创建主场景
Tools → 抖音小游戏 → 配置 Player Settings
```

### 3. 运行游戏
点击 Play 按钮即可运行！游戏框架会通过 `GameBootstrapper` 自动初始化所有系统。

## 游戏玩法

- **操作**: 左右滑动屏幕 / 鼠标拖拽 / 方向键 A/D 控制篮子
- **金币**(金色): 接住 +10分
- **炸弹**(红色): 接住扣15分 + 扣1命，避开则无惩罚
- **漏接金币**: 扣1命
- **生命**: 3条，用完游戏结束
- 随时间推进，生成速度逐渐加快

## 项目结构

```
DouyinMiniGame/
├── Assets/
│   ├── Scripts/
│   │   ├── Framework/          # 框架核心
│   │   │   ├── SingletonMono.cs   # MonoBehaviour 单例基类
│   │   │   ├── EventBus.cs        # 轻量级事件总线
│   │   │   └── GameState.cs       # 游戏状态枚举
│   │   ├── SDK/                # 抖音SDK桥接
│   │   │   ├── DouyinSDKBridge.cs  # SDK封装(广告/分享/排行榜)
│   │   │   └── PlatformInfo.cs    # 平台检测
│   │   ├── Core/              # 游戏核心
│   │   │   ├── GameManager.cs      # 状态机+流程控制
│   │   │   └── UIManager.cs        # 全UI代码动态生成
│   │   ├── Gameplay/          # 玩法逻辑
│   │   │   ├── ScoreManager.cs    # 分数管理
│   │   │   ├── ItemSpawner.cs     # 物体生成器
│   │   │   ├── FallingItem.cs     # 下落物品
│   │   │   └── BasketController.cs # 玩家篮子
│   │   └── Bootstrap/        # 启动引导
│   │       └── GameBootstrapper.cs # 自动初始化
│   └── Editor/
│       └── SceneSetupTool.cs     # 一键配置工具
├── Packages/
│   └── manifest.json
├── ProjectSettings/
│   └── ProjectVersion.txt
└── README.md
```

## 架构设计

### 事件总线通信
各模块通过 `EventBus` 解耦通信，不互相直接引用：
```
CoinCaught ──→ ScoreManager(+10), GameManager(加速)
BombCaught  ──→ ScoreManager(-15), GameManager(扣命)
ItemMissed  ──→ GameManager(扣命)
ScoreChanged──→ UIManager(更新显示)
GameStateChanged ──→ UIManager(切换面板)
```

### 自动启动
`GameBootstrapper` 使用 `[RuntimeInitializeOnLoadMethod]` 属性，在运行时自动：
1. 配置正交相机 (orthographicSize=5)
2. 创建 SDK 桥接 → 分数管理 → 游戏管理器 → 生成器 → 篮子 → UI
3. 所有系统挂载到 DontDestroyOnLoad 根节点

### 零预制体设计
所有视觉元素（金币、炸弹、篮子、UI）均由代码动态生成：
- 金币/炸弹: `Texture2D` 程序化绘制圆形精灵
- 篮子: 程序化绘制梯形篮子形状
- UI: `Canvas` + `Text` + `Image` + `Button` 全代码创建

## 接入抖音 SDK

### 方式一: 使用 StarkSDK (推荐)

1. 在抖音开放平台 (developer.open-douyin.com) 注册开发者账号
2. 创建小游戏，引擎选择 **Unity引擎**
3. 下载 [StarkSDK Unity 资源包](https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/game-engine/rd-to-SCgame/unity-game-access/sc_stark_sdk)
4. 导入 `.unitypackage` 到项目
5. 在 Player Settings → Scripting Define Symbols 添加: `DOUYIN_MINIGAME_SDK`
6. 取消注释 `DouyinSDKBridge.cs` 和 `PlatformInfo.cs` 中的 `#if DOUYIN_MINIGAME_SDK` 代码块
7. 填入你的 AppID

### 方式二: 使用团结引擎 (Tuanjie Engine)

1. 下载 [团结引擎](https://unity.cn/tuanjie/releases) (1.5+)
2. Build Settings → Install Support 安装抖音 SDK 插件
3. 直接构建导出抖音小游戏

### 方式三: WebGL 方案

适用于 iOS 抖音小游戏，详见:
[Unity 小游戏 WebGL 方案说明](https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/game-engine/rd-to-SCgame/open-capacity/overview-and-compatibility/sc_webgl_compatibility)

## 发布到抖音

### 构建步骤
1. 确认已安装 StarkSDK
2. 菜单: `ByteGame → TTSDKTools → Build Tool`
3. 取消勾选「采用旧格式打包」
4. 填入 AppID，点击 Build
5. 使用抖音开发者工具上传包体

### 包体限制
- Native方案: 首包 < 100M (推荐 20M)
- WebGL方案: 首包 < 100M (推荐 10M)

## 自定义扩展

### 添加新事件
```csharp
// 定义
EventBus.On<int>("MyEvent", (value) => { ... });
// 触发
EventBus.Fire("MyEvent", 42);
```

### 添加新平台能力
在 `DouyinSDKBridge.cs` 中添加方法，使用 `#if DOUYIN_MINIGAME_SDK` 条件编译。

### 添加新玩法元素
1. 在 `ItemSpawner.SpawnItem()` 中添加新类型
2. 在 `FallingItem.Initialize()` 中配置行为
3. 在 `ScoreManager` 中添加分数逻辑
4. 通过 `EventBus` 广播事件

## 参考文档
- [抖音Unity小游戏接入指南](https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/game-engine/rd-to-SCgame/unity-game-access/install-connect/sc_access_guide)
- [抖音Unity C# SDK](https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/game-engine/rd-to-SCgame/unity-game-access/sc_stark_sdk)
- [团结引擎抖音SDK](https://docs.unity.cn/cn/tuanjiemanual/Manual/DouyinSDK.html)
- [抖音Unity小游戏发布](https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/game-engine/rd-to-SCgame/unity-game-access/sc_publish)

## 技术规格
- Unity 版本: 2022.3.20f1 LTS
- 渲染管线: Built-in
- 相机: 正交 (orthographicSize=5)
- UI: uGUI (Screen Space Overlay)
- 编译宏: `DOUYIN_MINIGAME_SDK` (接入SDK时启用)

using UnityEngine;
using DouyinMiniGame.Framework;
using DouyinMiniGame.SDK;
using DouyinMiniGame.Gameplay;

namespace DouyinMiniGame.Core
{
    /// <summary>
    /// 游戏核心管理器 —— 控制游戏流程状态机，协调各子系统。
    /// 负责: 状态切换、游戏开始/暂停/结束、难度递增。
    /// </summary>
    public class GameManager : SingletonMono<GameManager>
    {
        [Header("游戏配置")]
        [SerializeField] private int _maxLives = 3;
        [SerializeField] private float _initialSpawnInterval = 1.5f;
        [SerializeField] private float _minSpawnInterval = 0.5f;
        [SerializeField] private float _spawnSpeedup = 0.02f; // 每次生成加速

        /// <summary>当前游戏状态</summary>
        public GameState CurrentState { get; private set; } = GameState.Initializing;

        /// <summary>当前生成交错间隔（秒）</summary>
        public float CurrentSpawnInterval { get; private set; }

        /// <summary>最大生命值</summary>
        public int MaxLives => _maxLives;

        /// <summary>当前剩余生命</summary>
        public int CurrentLives { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            CurrentSpawnInterval = _initialSpawnInterval;
            CurrentLives = _maxLives;
        }

        private void Start()
        {
            // 进入主菜单
            ChangeState(GameState.Menu);
        }

        private void OnEnable()
        {
            EventBus.On<int>("CoinCaught", OnCoinCaught);
            EventBus.On<int>("ItemMissed", OnItemMissed);
            EventBus.On("RequestStartGame", StartGame);
            EventBus.On("RequestRestart", StartGame);
        }

        private void OnDisable()
        {
            EventBus.Off<int>("CoinCaught", OnCoinCaught);
            EventBus.Off<int>("ItemMissed", OnItemMissed);
            EventBus.Off("RequestStartGame", StartGame);
            EventBus.Off("RequestRestart", StartGame);
        }

        #region 状态管理

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            var oldState = CurrentState;
            CurrentState = newState;
            EventBus.Fire("GameStateChanged", newState);
            EventBus.Fire("GameStateChangedDetailed", (oldState, newState));

            Debug.Log($"[GameManager] 状态切换: {oldState} -> {newState}");
        }

        /// <summary>开始游戏</summary>
        public void StartGame()
        {
            CurrentLives = _maxLives;
            CurrentSpawnInterval = _initialSpawnInterval;
            ItemSpawner.Instance?.ClearAllItems();
            ChangeState(GameState.Playing);
            EventBus.Fire("ScoreReset");
        }

        /// <summary>暂停游戏</summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
                ChangeState(GameState.Paused);
        }

        /// <summary>恢复游戏</summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
                ChangeState(GameState.Playing);
        }

        /// <summary>游戏结束</summary>
        public void GameOver()
        {
            ChangeState(GameState.GameOver);

            // 保存最高分
            ScoreManager.Instance?.CheckAndSaveHighScore();

            // 提交分数到排行榜
            int finalScore = ScoreManager.Instance?.Score ?? 0;
            DouyinSDKBridge.Instance?.SubmitScore(finalScore);
        }

        #endregion

        #region 事件回调

        private void OnCoinCaught(int scoreGain)
        {
            if (CurrentState != GameState.Playing) return;

            // 加速生成
            CurrentSpawnInterval = Mathf.Max(_minSpawnInterval, CurrentSpawnInterval - _spawnSpeedup);

            // 震动反馈
            DouyinSDKBridge.Instance?.Vibrate();
        }

        private void OnItemMissed(int livesLost)
        {
            if (CurrentState != GameState.Playing) return;

            CurrentLives -= livesLost;
            EventBus.Fire("LivesChanged", CurrentLives);

            if (CurrentLives <= 0)
            {
                GameOver();
            }
        }

        #endregion
    }
}

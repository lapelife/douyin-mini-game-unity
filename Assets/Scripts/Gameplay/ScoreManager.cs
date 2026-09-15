using DouyinMiniGame.Framework;

namespace DouyinMiniGame.Gameplay
{
    /// <summary>
    /// 分数管理器 —— 记录当前分数和历史最高分。
    /// 通过 EventBus 广播分数变化。
    /// </summary>
    public class ScoreManager : SingletonMono<ScoreManager>
    {
        /// <summary>当前分数</summary>
        public int Score { get; private set; }

        /// <summary>历史最高分（本地存储）</summary>
        public int HighScore { get; private set; }

        /// <summary>每次接住金币获得的分数</summary>
        private const int ScorePerCoin = 10;

        /// <summary>接住炸弹扣的分数</summary>
        private const int BombPenalty = 15;

        /// <summary>本地存储 Key</summary>
        private const string HighScoreKey = "DouyinMiniGame_HighScore";

        protected override void Awake()
        {
            base.Awake();
            HighScore = UnityEngine.PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        private void OnEnable()
        {
            EventBus.On("CoinCaught", OnCoinCaught);
            EventBus.On("BombCaught", OnBombCaught);
            EventBus.On("ScoreReset", ResetScore);
        }

        private void OnDisable()
        {
            EventBus.Off("CoinCaught", OnCoinCaught);
            EventBus.Off("BombCaught", OnBombCaught);
            EventBus.Off("ScoreReset", ResetScore);
        }

        /// <summary>接住金币</summary>
        private void OnCoinCaught(int _)
        {
            Score += ScorePerCoin;
            EventBus.Fire("ScoreChanged", Score);
        }

        /// <summary>接到炸弹（扣分但不扣命）</summary>
        private void OnBombCaught(int _)
        {
            Score = UnityEngine.Mathf.Max(0, Score - BombPenalty);
            EventBus.Fire("ScoreChanged", Score);
        }

        /// <summary>重置分数</summary>
        private void ResetScore()
        {
            Score = 0;
            EventBus.Fire("ScoreChanged", Score);
        }

        /// <summary>检查并更新最高分</summary>
        public void CheckAndSaveHighScore()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
                UnityEngine.PlayerPrefs.SetInt(HighScoreKey, HighScore);
                UnityEngine.PlayerPrefs.Save();
            }
        }
    }
}

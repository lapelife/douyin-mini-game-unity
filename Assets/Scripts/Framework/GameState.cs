namespace DouyinMiniGame.Framework
{
    /// <summary>
    /// 游戏状态枚举，用于有限状态机管理。
    /// </summary>
    public enum GameState
    {
        /// <summary>初始化中</summary>
        Initializing,
        /// <summary>主菜单/开始界面</summary>
        Menu,
        /// <summary>游戏进行中</summary>
        Playing,
        /// <summary>暂停</summary>
        Paused,
        /// <summary>游戏结束</summary>
        GameOver
    }
}

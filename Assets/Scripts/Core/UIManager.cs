using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DouyinMiniGame.Framework;
using DouyinMiniGame.Gameplay;

namespace DouyinMiniGame.Core
{
    /// <summary>
    /// UI 管理器 —— 通过代码动态创建所有 UI，无需预制体。
    /// 管理: 分数显示、生命值、开始/结束面板、按钮交互。
    /// </summary>
    public class UIManager : SingletonMono<UIManager>
    {
        private Canvas _canvas;
        private Text _scoreText;
        private Text _livesText;
        private GameObject _startPanel;
        private GameObject _gameOverPanel;
        private Text _finalScoreText;

        /// <summary>UI 使用的字体大小</summary>
        private const int FontSizeLarge = 48;
        private const int FontSizeMedium = 32;
        private const int FontSizeSmall = 24;

        protected override void Awake()
        {
            base.Awake();
            CreateCanvas();
            CreateHUD();
            CreateStartPanel();
            CreateGameOverPanel();
        }

        private void OnEnable()
        {
            EventBus.On<int>("ScoreChanged", OnScoreChanged);
            EventBus.On<int>("LivesChanged", OnLivesChanged);
            EventBus.On("ScoreReset", OnScoreReset);
            EventBus.On<GameState>("GameStateChanged", OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Off<int>("ScoreChanged", OnScoreChanged);
            EventBus.Off<int>("LivesChanged", OnLivesChanged);
            EventBus.Off("ScoreReset", OnScoreReset);
            EventBus.Off<GameState>("GameStateChanged", OnGameStateChanged);
        }

        #region UI 创建

        private void CreateCanvas()
        {
            var canvasGo = new GameObject("UICanvas");
            canvasGo.transform.SetParent(transform);

            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720, 1280);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
        }

        private Text CreateText(string name, string content, int fontSize, TextAnchor anchor, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;

            // 添加描边效果
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            return text;
        }

        private void CreateHUD()
        {
            // 顶部 HUD 容器
            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(_canvas.transform, false);

            var hudRect = hudGo.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(1, 1);
            hudRect.pivot = new Vector2(0.5f, 1);
            hudRect.sizeDelta = new Vector2(0, 80);
            hudRect.anchoredPosition = new Vector2(0, -10);

            // 分数文本（左上）
            _scoreText = CreateText("ScoreText", "分数: 0", FontSizeMedium, TextAnchor.MiddleLeft, hudGo.transform);
            var scoreRect = _scoreText.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 0.5f);
            scoreRect.anchorMax = new Vector2(0.4f, 0.5f);
            scoreRect.pivot = new Vector2(0, 0.5f);
            scoreRect.anchoredPosition = new Vector2(20, 0);

            // 生命值文本（右上）
            _livesText = CreateText("LivesText", "生命: 3", FontSizeMedium, TextAnchor.MiddleRight, hudGo.transform);
            var livesRect = _livesText.GetComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(0.6f, 0.5f);
            livesRect.anchorMax = new Vector2(1, 0.5f);
            livesRect.pivot = new Vector2(1, 0.5f);
            livesRect.anchoredPosition = new Vector2(-20, 0);
        }

        private Button CreateButton(string name, string label, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var btnRect = go.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(300, 80);

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 0.85f, 0.2f, 1f);

            var text = CreateText("Label", label, FontSizeMedium, TextAnchor.MiddleCenter, go.transform);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            text.color = new Color(0.15f, 0.15f, 0.15f);
            text.GetComponent<Outline>().enabled = false;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 0.95f, 0.4f);
            colors.pressedColor = new Color(0.9f, 0.75f, 0.1f);
            btn.colors = colors;

            return btn;
        }

        private void CreateStartPanel()
        {
            _startPanel = CreatePanel("StartPanel");

            // 标题
            var title = CreateText("Title", "接金币", FontSizeLarge, TextAnchor.MiddleCenter, _startPanel.transform);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 80);
            titleRect.sizeDelta = new Vector2(400, 80);

            // 副标题
            var subtitle = CreateText("Subtitle", "左右滑动移动篮子\n接住金币，避开炸弹!", FontSizeSmall, TextAnchor.MiddleCenter, _startPanel.transform);
            var subRect = subtitle.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 0);
            subRect.sizeDelta = new Vector2(400, 60);

            // 开始按钮
            var startBtn = CreateButton("StartBtn", "开始游戏", _startPanel.transform);
            var btnRect = startBtn.GetComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, -100);
            startBtn.onClick.AddListener(() =>
            {
                EventBus.Fire("RequestStartGame");
            });
        }

        private void CreateGameOverPanel()
        {
            _gameOverPanel = CreatePanel("GameOverPanel");
            _gameOverPanel.SetActive(false);

            // 标题
            var title = CreateText("Title", "游戏结束", FontSizeLarge, TextAnchor.MiddleCenter, _gameOverPanel.transform);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 80);
            titleRect.sizeDelta = new Vector2(400, 80);

            // 最终分数
            _finalScoreText = CreateText("FinalScore", "最终分数: 0", FontSizeMedium, TextAnchor.MiddleCenter, _gameOverPanel.transform);
            var scoreRect = _finalScoreText.GetComponent<RectTransform>();
            scoreRect.anchoredPosition = new Vector2(0, 0);
            scoreRect.sizeDelta = new Vector2(400, 60);

            // 重新开始按钮
            var restartBtn = CreateButton("RestartBtn", "再来一局", _gameOverPanel.transform);
            var btnRect = restartBtn.GetComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, -100);
            restartBtn.onClick.AddListener(() =>
            {
                EventBus.Fire("RequestRestart");
            });
        }

        private GameObject CreatePanel(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.75f);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            return go;
        }

        #endregion

        #region 事件回调

        private void OnScoreChanged(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"分数: {score}";
        }

        private void OnLivesChanged(int lives)
        {
            if (_livesText != null)
                _livesText.text = $"生命: {lives}";
        }

        private void OnScoreReset()
        {
            if (_scoreText != null)
                _scoreText.text = "分数: 0";
            if (_livesText != null)
                _livesText.text = $"生命: {GameManager.Instance.MaxLives}";
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    _startPanel?.SetActive(true);
                    _gameOverPanel?.SetActive(false);
                    break;
                case GameState.Playing:
                    _startPanel?.SetActive(false);
                    _gameOverPanel?.SetActive(false);
                    break;
                case GameState.GameOver:
                    _startPanel?.SetActive(false);
                    _gameOverPanel?.SetActive(true);
                    if (_finalScoreText != null)
                        _finalScoreText.text = $"最终分数: {ScoreManager.Instance?.Score ?? 0}";
                    break;
            }
        }

        #endregion
    }
}

using UnityEngine;
using DouyinMiniGame.Framework;
using DouyinMiniGame.Core;

namespace DouyinMiniGame.Gameplay
{
    /// <summary>
    /// 篮子控制器 —— 玩家控制的接物篮子。
    /// 支持: 触屏滑动、鼠标拖拽、键盘方向键。
    /// 篮子由代码动态生成，无需预制体。
    /// 相机 orthographicSize=5，篮子宽度约1.5世界单位。
    /// </summary>
    public class BasketController : SingletonMono<BasketController>
    {
        [Header("篮子配置")]
        [SerializeField] private float _basketWidth = 1.6f;   // 世界单位
        [SerializeField] private float _basketHeight = 0.7f;  // 世界单位
        [SerializeField] private float _moveSpeed = 8f;       // 移动速度
        [SerializeField] private float _smoothing = 0.25f;    // 平滑插值系数

        private SpriteRenderer _renderer;
        private Camera _mainCamera;
        private float _leftBound, _rightBound;
        private float _basketY;

        protected override void Awake()
        {
            base.Awake();
            CreateBasketVisual();
            SetupCameraBounds();
        }

        private void Start()
        {
            transform.position = new Vector3(0, _basketY, 0);
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState != GameState.Playing) return;

            HandleInput();
            CheckCatch();
        }

        #region 视觉创建

        private void CreateBasketVisual()
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = CreateBasketSprite();
            _renderer.sortingOrder = 5;
            _renderer.color = Color.white;

            // 直接使用世界单位作为缩放
            // 精灵创建时 pixelsPerUnit=1，纹理128x64
            // 所以缩放 = 目标世界尺寸 / 纹理像素数
            transform.localScale = new Vector3(_basketWidth / 128f, _basketHeight / 64f, 1);
        }

        private Sprite CreateBasketSprite()
        {
            int w = 128;
            int h = 64;
            var tex = new Texture2D(w, h);
            tex.alphaIsTransparency = true;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 归一化坐标 -1 to 1
                    float nx = (x / (float)w) * 2f - 1f;
                    float ny = (y / (float)h) * 2f - 1f;

                    // 梯形篮子形状：上宽下窄
                    float topWidth = 1f;
                    float bottomWidth = 0.55f;
                    float widthAtY = Mathf.Lerp(bottomWidth, topWidth, (ny + 1f) * 0.5f);

                    bool inside = Mathf.Abs(nx) <= widthAtY && ny >= -1f && ny <= 1f;

                    if (inside)
                    {
                        bool isEdge = Mathf.Abs(nx) > widthAtY - 0.06f || ny < -0.88f || ny > 0.88f;
                        if (isEdge)
                            tex.SetPixel(x, y, new Color(0.45f, 0.3f, 0.12f, 1f)); // 深棕色边缘
                        else
                            tex.SetPixel(x, y, new Color(0.78f, 0.55f, 0.28f, 1f)); // 浅棕色
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1);
        }

        #endregion

        #region 相机边界

        private void SetupCameraBounds()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            float halfHeight = _mainCamera.orthographicSize;
            float halfWidth = halfHeight * _mainCamera.aspect;

            _leftBound = -halfWidth + _basketWidth * 0.5f;
            _rightBound = halfWidth - _basketWidth * 0.5f;
            _basketY = -halfHeight + _basketHeight * 0.5f + 0.3f; // 距底部留点空间
        }

        #endregion

        #region 输入处理

        private void HandleInput()
        {
            float targetX = transform.position.x;

            // 触屏输入
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    Vector3 touchWorld = _mainCamera.ScreenToWorldPoint(touch.position);
                    targetX = touchWorld.x;
                }
            }

            // 鼠标输入（编辑器测试用）
            if (Input.GetMouseButton(0))
            {
                Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                targetX = mouseWorld.x;
            }

            // 键盘输入（备用）
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                targetX -= _moveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                targetX += _moveSpeed * Time.deltaTime;

            targetX = Mathf.Clamp(targetX, _leftBound, _rightBound);

            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, targetX, _smoothing);
            transform.position = pos;
        }

        #endregion

        #region 碰撞检测

        private void CheckCatch()
        {
            Vector2 basketPos = transform.position;
            ItemSpawner.Instance?.CheckCatch(basketPos, _basketWidth, _basketHeight);
        }

        #endregion
    }
}

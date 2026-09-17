using UnityEngine;
using System.Collections.Generic;
using DouyinMiniGame.Framework;
using DouyinMiniGame.Core;

namespace DouyinMiniGame.Gameplay
{
    /// <summary>
    /// 物体生成器 —— 定时生成下落物品（金币/炸弹）。
    /// 物品类型由概率决定，速度随游戏进程递增。
    /// 所有视觉对象均由代码动态创建，无需预制体。
    /// 相机 orthographicSize=5，可见区域约 10x6 世界单位（竖屏）。
    /// </summary>
    public class ItemSpawner : SingletonMono<ItemSpawner>
    {
        [Header("生成配置")]
        [SerializeField] private float _itemRadius = 0.35f;    // 物品半径（世界单位）
        [SerializeField] private float _coinFallSpeed = 3.5f;  // 金币下落速度
        [SerializeField] private float _bombFallSpeed = 3.0f; // 炸弹下落速度
        [SerializeField] private float _bombProbability = 0.2f; // 炸弹概率

        private Transform _itemContainer;
        private readonly List<FallingItem> _activeItems = new List<FallingItem>();
        private float _nextSpawnTime;

        private Camera _mainCamera;
        private float _leftBound, _rightBound;
        private float _topY, _bottomY;

        protected override void Awake()
        {
            base.Awake();
            SetupCameraBounds();
            _itemContainer = new GameObject("ItemContainer").transform;
            _itemContainer.SetParent(transform);
        }

        private void SetupCameraBounds()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            float halfHeight = _mainCamera.orthographicSize;
            float halfWidth = halfHeight * _mainCamera.aspect;

            _leftBound = -halfWidth + _itemRadius;
            _rightBound = halfWidth - _itemRadius;
            _topY = halfHeight + _itemRadius;
            _bottomY = -halfHeight - _itemRadius;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState != GameState.Playing) return;

            if (Time.time >= _nextSpawnTime)
            {
                SpawnItem();
                _nextSpawnTime = Time.time + gm.CurrentSpawnInterval;
            }

            UpdateActiveItems();
        }

        private void SpawnItem()
        {
            // 确保边界是最新的（处理屏幕旋转）
            SetupCameraBounds();

            bool isBomb = Random.value < _bombProbability;
            float x = Random.Range(_leftBound, _rightBound);

            var go = new GameObject(isBomb ? "Bomb" : "Coin");
            go.transform.SetParent(_itemContainer);
            go.transform.position = new Vector3(x, _topY, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            // 精灵纹理64x64，PPU=64 → 原始世界尺寸1x1，乘以直径得到目标大小
            float diameter = _itemRadius * 2f;
            float spriteScale = diameter / 64f; // 纹理64px，PPU=64 → 1单位，需再缩放
            if (isBomb)
            {
                sr.sprite = CreateCircleSprite(new Color(0.85f, 0.12f, 0.12f));
                sr.transform.localScale = Vector3.one * spriteScale;
            }
            else
            {
                sr.sprite = CreateCircleSprite(new Color(1f, 0.82f, 0.15f));
                sr.transform.localScale = Vector3.one * spriteScale;
            }

            var item = go.AddComponent<FallingItem>();
            item.Initialize(isBomb, isBomb ? _bombFallSpeed : _coinFallSpeed, _bottomY);
            _activeItems.Add(item);
        }

        /// <summary>创建程序化圆形精灵</summary>
        private Sprite CreateCircleSprite(Color color)
        {
            int size = 64;
            var tex = new Texture2D(size, size);
            tex.alphaIsTransparency = true;

            float center = size * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, dist <= radius ? color : new Color(0, 0, 0, 0));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 1);
        }

        private void UpdateActiveItems()
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                var item = _activeItems[i];
                if (item == null)
                {
                    _activeItems.RemoveAt(i);
                    continue;
                }

                // 必须用 Space.World，否则 Spinner 旋转会改变局部下方向导致物品乱飘
                item.transform.Translate(Vector3.down * item.FallSpeed * Time.deltaTime, Space.World);

                if (item.transform.position.y < _bottomY)
                {
                    if (!item.IsBomb)
                        EventBus.Fire("ItemMissed", 1);

                    _activeItems.RemoveAt(i);
                    Destroy(item.gameObject);
                }
            }
        }

        /// <summary>检测篮子是否接住了物品</summary>
        public void CheckCatch(Vector2 basketPos, float basketWidth, float basketHeight)
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                var item = _activeItems[i];
                if (item == null) continue;

                var itemPos = (Vector2)item.transform.position;
                float dx = Mathf.Abs(itemPos.x - basketPos.x);
                float dy = Mathf.Abs(itemPos.y - basketPos.y);

                if (dx < basketWidth * 0.5f && dy < basketHeight * 0.5f)
                {
                    if (item.IsBomb)
                    {
                        EventBus.Fire("BombCaught", 0);
                        EventBus.Fire("ItemMissed", 1);
                    }
                    else
                    {
                        EventBus.Fire("CoinCaught", 0);
                    }

                    _activeItems.RemoveAt(i);
                    Destroy(item.gameObject);
                }
            }
        }

        /// <summary>清除所有活跃物品</summary>
        public void ClearAllItems()
        {
            foreach (var item in _activeItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _activeItems.Clear();
        }
    }
}

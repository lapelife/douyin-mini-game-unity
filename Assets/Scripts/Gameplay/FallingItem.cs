using UnityEngine;

namespace DouyinMiniGame.Gameplay
{
    /// <summary>
    /// 下落物品组件 —— 记录物品类型和速度。
    /// 由 ItemSpawner 初始化和管理。
    /// </summary>
    public class FallingItem : MonoBehaviour
    {
        /// <summary>是否是炸弹</summary>
        public bool IsBomb { get; private set; }

        /// <summary>下落速度（像素/秒）</summary>
        public float FallSpeed { get; private set; }

        /// <summary>下落底线 Y（超出后判定为miss）</summary>
        public float BottomY { get; private set; }

        /// <summary>
        /// 初始化下落物品。
        /// </summary>
        public void Initialize(bool isBomb, float fallSpeed, float bottomY)
        {
            IsBomb = isBomb;
            FallSpeed = fallSpeed;
            BottomY = bottomY;

            // 添加旋转动画效果
            if (TryGetComponent<Spinner>(out var spinner) == false)
            {
                gameObject.AddComponent<Spinner>();
            }
        }
    }

    /// <summary>
    /// 简单旋转动画组件，让下落物品有旋转效果。
    /// </summary>
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float _rotateSpeed = 180f;

        private void Update()
        {
            transform.Rotate(0, 0, _rotateSpeed * Time.deltaTime);
        }
    }
}

using System;
using System.Collections.Generic;

namespace DouyinMiniGame.Framework
{
    /// <summary>
    /// 轻量级事件总线，实现模块间松耦合通信。
    /// 用法: EventBus.Fire("ScoreChanged", 100);
    ///       EventBus.On<int>("ScoreChanged", handler);
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<string, Delegate> _handlers = new Dictionary<string, Delegate>();

        /// <summary>订阅事件（无参数）</summary>
        public static void On(string eventName, Action handler)
        {
            AddHandler(eventName, handler);
        }

        /// <summary>订阅事件（带参数）</summary>
        public static void On<T>(string eventName, Action<T> handler)
        {
            AddHandler(eventName, handler);
        }

        /// <summary>触发事件（无参数）</summary>
        public static void Fire(string eventName)
        {
            if (_handlers.TryGetValue(eventName, out var del) && del is Action action)
            {
                action.Invoke();
            }
        }

        /// <summary>触发事件（带参数）</summary>
        public static void Fire<T>(string eventName, T arg)
        {
            if (_handlers.TryGetValue(eventName, out var del) && del is Action<T> action)
            {
                action.Invoke(arg);
            }
        }

        /// <summary>取消订阅</summary>
        public static void Off(string eventName, Action handler)
        {
            RemoveHandler(eventName, handler);
        }

        public static void Off<T>(string eventName, Action<T> handler)
        {
            RemoveHandler(eventName, handler);
        }

        /// <summary>清空所有事件</summary>
        public static void Clear()
        {
            _handlers.Clear();
        }

        private static void AddHandler(string eventName, Delegate handler)
        {
            if (_handlers.TryGetValue(eventName, out var existing))
            {
                _handlers[eventName] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[eventName] = handler;
            }
        }

        private static void RemoveHandler(string eventName, Delegate handler)
        {
            if (_handlers.TryGetValue(eventName, out var existing))
            {
                var newDel = Delegate.Remove(existing, handler);
                if (newDel == null)
                    _handlers.Remove(eventName);
                else
                    _handlers[eventName] = newDel;
            }
        }
    }
}

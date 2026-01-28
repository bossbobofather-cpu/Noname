using System;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// 전역/씬 이벤트 버스에 대한 간단 접근자입니다.
    /// </summary>
    public static class GameEventHub
    {
        public static GameEventBus.Scope Global => GameEventBus.Global;
        public static GameEventBus.Scope Scene => GameEventBus.Scene;

        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            GameEventBus.Subscribe(handler);
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEventContext
        {
            GameEventBus.Unsubscribe(handler);
        }

        public static void Publish<TEvent>(TEvent context) where TEvent : GameEventContext
        {
            GameEventBus.Publish(context);
        }
    }
}

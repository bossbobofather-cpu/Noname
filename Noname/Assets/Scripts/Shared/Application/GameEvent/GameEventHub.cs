using System;

namespace MyProject.Common.GameEvent
{
    /// <summary>
    /// 전역/씬 이벤트 버스를 간단히 접근하기 위한 허브입니다.
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

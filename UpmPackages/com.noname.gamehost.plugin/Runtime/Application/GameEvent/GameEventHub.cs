using System;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ?�역/???�벤??버스�?간단???�근?�기 ?�한 ?�브?�니??
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

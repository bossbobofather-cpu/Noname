using System;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ?�벤??버스??공용 ?�터?�이?�입?�다.
    /// </summary>
    public interface IEventBus<TEventBase>
        where TEventBase : class
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : TEventBase;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : TEventBase;
        void Publish<TEvent>(TEvent context) where TEvent : TEventBase;
        void Clear();
    }
}

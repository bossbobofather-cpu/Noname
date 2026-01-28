using System;

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// 이벤트 버스 공용 인터페이스입니다.
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

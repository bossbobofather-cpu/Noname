using MyProject.Common.Host;

namespace MyProject.Common.GameEvent
{
    /// <summary>
    /// 호스트에서 발생한 이벤트를 전달하는 이벤트입니다.
    /// </summary>
    public class GameHostEventRaisedEvent : SceneGameEventContext
    {
        public GameEventBase EventData { get; }

        public GameHostEventRaisedEvent(object source, GameEventBase eventData)
            : base(source)
        {
            EventData = eventData;
        }
    }
}

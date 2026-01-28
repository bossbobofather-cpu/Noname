namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// 호스트에서 발생한 이벤트를 전달하는 이벤트입니다.
    /// </summary>
    public class GameHostEventRaisedEvent : SceneGameEventContext
    {
        /// <summary>
        /// 호스트 이벤트 데이터입니다.
        /// </summary>
        public GameEventBase EventData { get; }

        public GameHostEventRaisedEvent(object source, GameEventBase eventData)
            : base(source)
        {
            EventData = eventData;
        }
    }
}

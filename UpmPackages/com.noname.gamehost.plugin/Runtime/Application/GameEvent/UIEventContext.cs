namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// UI 이벤트 종류입니다.
    /// </summary>
    public enum UIEventType
    {
        Button_Click,
    }

    /// <summary>
    /// UI 이벤트 컨텍스트 베이스입니다.
    /// </summary>
    public abstract class UIEventContext
    {
        /// <summary>
        /// UI 이벤트 종류입니다.
        /// </summary>
        public UIEventType EventType { get; }

        /// <summary>
        /// 이벤트 발신자입니다.
        /// </summary>
        public object Source { get; }

        protected UIEventContext(UIEventType eventType, object source)
        {
            EventType = eventType;
            Source = source;
        }
    }
}
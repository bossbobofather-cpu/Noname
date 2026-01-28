namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// UI 컨텍스트를 전달하는 이벤트입니다.
    /// </summary>
    public sealed class GameUIContextSupplyEvent : SceneGameEventContext
    {
        /// <summary>
        /// 전달된 UI 컨텍스트입니다.
        /// </summary>
        public UIEventContext UIEventCtx { get; }

        public GameUIContextSupplyEvent(UIEventContext eventContext, object source)
            : base(source)
        {
            UIEventCtx = eventContext;
        }
    }
}

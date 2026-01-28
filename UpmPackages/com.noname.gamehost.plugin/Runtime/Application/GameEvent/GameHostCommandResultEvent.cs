namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// 호스트 커맨드 처리 결과를 전달하는 이벤트입니다.
    /// </summary>
    public class GameHostCommandResultEvent : SceneGameEventContext
    {
        /// <summary>
        /// 커맨드 결과입니다.
        /// </summary>
        public GameCommandResultBase Result { get; }

        public GameHostCommandResultEvent(object source, GameCommandResultBase result)
            : base(source)
        {
            Result = result;
        }
    }
}

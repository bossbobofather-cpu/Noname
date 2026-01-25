using MyProject.Common.Host;

namespace MyProject.Common.GameEvent
{
    /// <summary>
    /// 호스트 명령 처리 결과를 전달하는 이벤트입니다.
    /// </summary>
    public class GameHostCommandResultEvent : SceneGameEventContext
    {
        public GameCommandResultBase Result { get; }

        public GameHostCommandResultEvent(object source, GameCommandResultBase result)
            : base(source)
        {
            Result = result;
        }
    }
}

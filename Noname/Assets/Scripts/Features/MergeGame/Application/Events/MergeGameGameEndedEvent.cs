using MyProject.Common.GameEvent;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 게임 종료 결과를 알리는 이벤트입니다.
    /// </summary>
    public sealed class MergeGameGameEndedEvent : SceneGameEventContext
    {
        /// <summary>
        /// 승리 여부입니다.
        /// </summary>
        public bool IsWin { get; }

        public MergeGameGameEndedEvent(object source, bool isWin)
            : base(source)
        {
            IsWin = isWin;
        }
    }
}

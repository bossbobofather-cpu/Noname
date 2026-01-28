using System.Collections.Generic;

namespace Noname.GameHost
{
    /// <summary>
    /// 커맨드 처리 결과와 전/후 이벤트를 묶어 전달하는 구조체입니다.
    /// </summary>
    public readonly struct GameCommandOutcome<TResult, TEvent>
    {
        /// <summary>
        /// 결과 이전에 발행할 이벤트 목록입니다.
        /// </summary>
        public IReadOnlyList<TEvent> PreEvents { get; }

        /// <summary>
        /// 커맨드 처리 결과입니다.
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// 결과 이후에 발행할 이벤트 목록입니다.
        /// </summary>
        public IReadOnlyList<TEvent> PostEvents { get; }

        public GameCommandOutcome(TResult result, IReadOnlyList<TEvent> postEvents = null)
        {
            PreEvents = null;
            Result = result;
            PostEvents = postEvents;
        }

        public GameCommandOutcome(
            IReadOnlyList<TEvent> preEvents,
            TResult result,
            IReadOnlyList<TEvent> postEvents = null)
        {
            PreEvents = preEvents;
            Result = result;
            PostEvents = postEvents;
        }
    }
}

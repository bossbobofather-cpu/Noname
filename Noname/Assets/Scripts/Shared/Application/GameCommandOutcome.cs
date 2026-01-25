using System.Collections.Generic;

namespace MyProject.Common.Host
{
    /// <summary>
    /// 명령 처리의 결과와 전후 이벤트를 묶어 전달합니다.
    /// </summary>
    public readonly struct GameCommandOutcome<TResult, TEvent>
    {
        public IReadOnlyList<TEvent> PreEvents { get; }
        public TResult Result { get; }
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
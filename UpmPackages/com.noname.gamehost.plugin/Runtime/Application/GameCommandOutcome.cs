using System.Collections.Generic;

namespace Noname.GameHost
{
    /// <summary>
    /// 명령 처리??결과?� ?�후 ?�벤?��? 묶어 ?�달?�니??
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

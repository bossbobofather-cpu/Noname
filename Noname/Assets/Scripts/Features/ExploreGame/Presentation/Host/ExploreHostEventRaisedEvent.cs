using MyProject.Common.GameEvent;
using MyProject.ExploreGame.Application;

namespace MyProject.ExploreGame.Presentation
{
    /// <summary>
    /// Host에서 이벤트가 발생했을 때 GameMode로 전달되는 이벤트입니다.
    /// 씬 스코프에서 처리됩니다.
    /// </summary>
    public sealed class ExploreHostEventRaisedEvent : SceneGameEventContext
    {
        public ExploreHostEvent Event { get; }

        public ExploreHostEventRaisedEvent(object source, ExploreHostEvent hostEvent)
            : base(source)
        {
            Event = hostEvent;
        }
    }
}

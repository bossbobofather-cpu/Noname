namespace MyProject.Common.GameEvent
{
    /// <summary>
    /// 게임 이벤트의 기본 컨텍스트입니다.
    /// </summary>
    public abstract class GameEventContext
    {
        /// <summary>
        /// 이벤트 발신자입니다.
        /// </summary>
        public object Source { get; }

        protected GameEventContext(object source)
        {
            Source = source;
        }
    }

    /// <summary>
    /// 씬 스코프에서 처리되는 이벤트 컨텍스트입니다.
    /// </summary>
    public abstract class SceneGameEventContext : GameEventContext
    {
        protected SceneGameEventContext(object source)
            : base(source)
        {
        }
    }

    /// <summary>
    /// 전역 스코프에서 처리되는 이벤트 컨텍스트입니다.
    /// </summary>
    public abstract class GlobalGameEventContext : GameEventContext
    {
        protected GlobalGameEventContext(object source)
            : base(source)
        {
        }
    }
}

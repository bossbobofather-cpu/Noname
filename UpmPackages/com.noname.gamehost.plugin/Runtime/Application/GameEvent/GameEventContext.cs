namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// 게임 ?�벤?�의 기본 컨텍?�트?�니??
    /// </summary>
    public abstract class GameEventContext
    {
        /// <summary>
        /// ?�벤??발신?�입?�다.
        /// </summary>
        public object Source { get; }

        protected GameEventContext(object source)
        {
            Source = source;
        }
    }

    /// <summary>
    /// ???�코?�에??처리?�는 ?�벤??컨텍?�트?�니??
    /// </summary>
    public abstract class SceneGameEventContext : GameEventContext
    {
        protected SceneGameEventContext(object source)
            : base(source)
        {
        }
    }

    /// <summary>
    /// ?�역 ?�코?�에??처리?�는 ?�벤??컨텍?�트?�니??
    /// </summary>
    public abstract class GlobalGameEventContext : GameEventContext
    {
        protected GlobalGameEventContext(object source)
            : base(source)
        {
        }
    }
}

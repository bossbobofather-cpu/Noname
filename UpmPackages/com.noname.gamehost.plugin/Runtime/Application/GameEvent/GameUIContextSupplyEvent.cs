
namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// UI ���ؽ�Ʈ�� �����ϴ� �̺�Ʈ�Դϴ�.
    /// </summary>
    public sealed class GameUIContextSupplyEvent : SceneGameEventContext
    {
        /// <summary>
        /// ������ UI ���ؽ�Ʈ�Դϴ�.
        /// </summary>
        public UIEventContext UIEventCtx { get; }

        public GameUIContextSupplyEvent(UIEventContext eventContext, object source)
            : base(source)
        {
            UIEventCtx = eventContext;
        }
    }
}

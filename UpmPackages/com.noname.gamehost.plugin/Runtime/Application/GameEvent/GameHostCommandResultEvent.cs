namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ȣ��Ʈ ���� ó�� ����� �����ϴ� �̺�Ʈ�Դϴ�.
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

namespace Noname.GameHost.GameEvent
{
    /// <summary>
    /// ȣ��Ʈ���� �߻��� �̺�Ʈ�� �����ϴ� �̺�Ʈ�Դϴ�.
    /// </summary>
    public class GameHostEventRaisedEvent : SceneGameEventContext
    {
        public GameEventBase EventData { get; }

        public GameHostEventRaisedEvent(object source, GameEventBase eventData)
            : base(source)
        {
            EventData = eventData;
        }
    }
}

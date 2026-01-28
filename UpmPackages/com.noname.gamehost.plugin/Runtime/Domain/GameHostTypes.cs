using System;

namespace Noname.GameHost
{
    /// <summary>
    /// ���� ���� ��û�� �ּ� �����Դϴ�.
    /// </summary>
    public abstract class GameCommandBase
    {
        /// <summary>
        /// ��û�� �ĺ��ϱ� ���� ���� ID�Դϴ�.
        /// </summary>
        public Guid CommandId { get; }

        /// <summary>
        /// ��û�� ���� �÷��̾�/������ ID�Դϴ�.
        /// </summary>
        public long SenderUid { get; }

        protected GameCommandBase(long senderUid = 0)
        {
            CommandId = Guid.NewGuid();
            SenderUid = senderUid;
        }
    }

    /// <summary>
    /// ���� ó�� �����?�⺻ Ÿ���Դϴ�.
    /// </summary>
    public abstract class GameCommandResultBase
    {
        /// <summary>
        /// �����?������ ȣ��Ʈ ƽ�Դϴ�.
        /// </summary>
        public long Tick { get; }

        /// <summary>
        /// ��û�� ���� �÷��̾�/������ ID�Դϴ�.
        /// </summary>
        public long SenderUid { get; }

        /// <summary>
        /// ó�� ���� �����Դϴ�.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// ���� ���� �Ǵ� �߰� �޽����Դϴ�.
        /// </summary>
        public string ErrorMessage { get; }

        protected GameCommandResultBase(long tick, long senderUid, bool success, string errorMessage = null)
        {
            Tick = tick;
            SenderUid = senderUid;
            Success = success;
            ErrorMessage = errorMessage ?? string.Empty;
        }
    }

    /// <summary>
    /// ���� ���� ��ȭ �̺�Ʈ�� �⺻ Ÿ���Դϴ�.
    /// </summary>
    public abstract class GameEventBase
    {
        /// <summary>
        /// �̺�Ʈ�� �߻��� ȣ��Ʈ ƽ�Դϴ�.
        /// </summary>
        public long Tick { get; }

        protected GameEventBase(long tick)
        {
            Tick = tick;
        }
    }

    /// <summary>
    /// ����ȭ�� �������� �⺻ Ÿ���Դϴ�.
    /// </summary>
    public abstract class GameSnapshotBase
    {
        /// <summary>
        /// �������� ������ ȣ��Ʈ ƽ�Դϴ�.
        /// </summary>
        public long Tick { get; }

        protected GameSnapshotBase(long tick)
        {
            Tick = tick;
        }
    }

    /// <summary>
    /// ȣ��Ʈ���� �����?���� ���� �������̽��Դϴ�.
    /// </summary>
    public interface IRandomSource
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();
    }

    public interface IHostCommandBus<TCommand, TResult, TEvent>
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
    {
        event Action<TResult> ResultProduced;
        event Action<TEvent> EventRaised;

        void SendCommand(TCommand command);
    }    /// <summary>
    /// ���� ȣ��Ʈ�� �⺻ ����Դϴ�?
    /// </summary>
    internal interface IGameHost<TCommand, TResult, TEvent, TSnapshot>
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        long Tick { get; }

        // event Action<TResult> ResultProduced;
        // event Action<TEvent> EventRaised;

        void Submit(TCommand command);
        void Advance(float deltaSeconds);
        TSnapshot BuildSnapshot();
    }
}

using System;

namespace Noname.GameHost
{
    /// <summary>
    /// 커맨드 요청의 기본 타입입니다.
    /// </summary>
    public abstract class GameCommandBase
    {
        /// <summary>
        /// 커맨드 고유 ID입니다.
        /// </summary>
        public Guid CommandId { get; }

        /// <summary>
        /// 요청을 보낸 사용자/플레이어 ID입니다.
        /// </summary>
        public long SenderUid { get; }

        protected GameCommandBase(long senderUid = 0)
        {
            CommandId = Guid.NewGuid();
            SenderUid = senderUid;
        }
    }

    /// <summary>
    /// 커맨드 처리 결과의 기본 타입입니다.
    /// </summary>
    public abstract class GameCommandResultBase
    {
        /// <summary>
        /// 결과가 만들어진 호스트 틱입니다.
        /// </summary>
        public long Tick { get; }

        /// <summary>
        /// 요청을 보낸 사용자/플레이어 ID입니다.
        /// </summary>
        public long SenderUid { get; }

        /// <summary>
        /// 처리 성공 여부입니다.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 실패 사유 메시지입니다.
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
    /// 게임 이벤트의 기본 타입입니다.
    /// </summary>
    public abstract class GameEventBase
    {
        /// <summary>
        /// 이벤트가 발생한 호스트 틱입니다.
        /// </summary>
        public long Tick { get; }

        protected GameEventBase(long tick)
        {
            Tick = tick;
        }
    }

    /// <summary>
    /// 게임 스냅샷의 기본 타입입니다.
    /// </summary>
    public abstract class GameSnapshotBase
    {
        /// <summary>
        /// 스냅샷이 생성된 호스트 틱입니다.
        /// </summary>
        public long Tick { get; }

        protected GameSnapshotBase(long tick)
        {
            Tick = tick;
        }
    }

    /// <summary>
    /// 호스트에서 사용할 랜덤 소스 인터페이스입니다.
    /// </summary>
    public interface IRandomSource
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();
    }

    /// <summary>
    /// 외부에서 사용할 커맨드 전송 버스입니다. //View에 제공할 내용만
    /// </summary>
    public interface IGameHost<TCommand, TResult, TEvent, TSnapshot>
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        event Action<TResult> ResultProduced;
        event Action<TEvent> EventRaised;

        void StartSimulation();
        void StopSimulation();
        void SendCommand(TCommand command);
        void FlushEvents();

        /// <summary>
        /// View/클라이언트에서 최신 스냅샷을 조회합니다.
        /// 내부 빌드는 Host 루프에서 수행됩니다.
        /// </summary>
        TSnapshot GetLatestSnapshot();
    }

    /// <summary>
    /// 호스트 내부 동작용 인터페이스입니다.
    /// </summary>
    internal interface IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        long Tick { get; }

        void Submit(TCommand command);
        void Advance(float deltaSeconds);
        TSnapshot BuildSnapshot();
    }
}

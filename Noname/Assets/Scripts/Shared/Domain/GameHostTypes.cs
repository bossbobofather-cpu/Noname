using System;

namespace MyProject.Common.Host
{
    /// <summary>
    /// 게임 로직 요청의 최소 단위입니다.
    /// </summary>
    public abstract class GameCommandBase
    {
        /// <summary>
        /// 요청을 식별하기 위한 고유 ID입니다.
        /// </summary>
        public Guid CommandId { get; }

        /// <summary>
        /// 요청을 보낸 플레이어/유저의 ID입니다.
        /// </summary>
        public long SenderUid { get; }

        protected GameCommandBase(long senderUid = 0)
        {
            CommandId = Guid.NewGuid();
            SenderUid = senderUid;
        }
    }

    /// <summary>
    /// 명령 처리 결과의 기본 타입입니다.
    /// </summary>
    public abstract class GameCommandResultBase
    {
        /// <summary>
        /// 결과가 생성된 호스트 틱입니다.
        /// </summary>
        public long Tick { get; }

        /// <summary>
        /// 요청을 보낸 플레이어/유저의 ID입니다.
        /// </summary>
        public long SenderUid { get; }

        /// <summary>
        /// 처리 성공 여부입니다.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 실패 사유 또는 추가 메시지입니다.
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
    /// 게임 상태 변화 이벤트의 기본 타입입니다.
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
    /// 동기화용 스냅샷의 기본 타입입니다.
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
    /// 호스트에서 사용할 난수 공급 인터페이스입니다.
    /// </summary>
    public interface IRandomSource
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();
    }

    /// <summary>
    /// 게임 호스트의 기본 계약입니다.
    /// </summary>
    public interface IGameHost<TCommand, TResult, TEvent, TSnapshot>
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        long Tick { get; }

        event Action<TResult> ResultProduced;
        event Action<TEvent> EventRaised;

        void Submit(TCommand command);
        void Advance(float deltaSeconds);
        TSnapshot BuildSnapshot();
    }
}

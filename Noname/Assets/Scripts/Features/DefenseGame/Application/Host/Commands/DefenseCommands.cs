using MyProject.Common.Host;

namespace MyProject.DefenseGame.Application.Commands
{
    /// <summary>
    /// 디펜스 게임 Command 기본 타입입니다.
    /// </summary>
    public abstract class DefenseCommand : GameCommandBase
    {
        protected DefenseCommand(long senderUid = 0) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 시작 Command입니다.
    /// </summary>
    public sealed class StartGameCommand : DefenseCommand
    {
        public StartGameCommand(long senderUid = 0) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 시작 Result입니다.
    /// </summary>
    public sealed class StartGameResult : DefenseCommandResult
    {
        private StartGameResult(long tick, long senderUid, bool success, string errorMessage)
            : base(tick, senderUid, success, errorMessage)
        {
        }

        public static StartGameResult Ok(long tick, long senderUid)
            => new(tick, senderUid, true, null);

        public static StartGameResult Fail(long tick, long senderUid, string message)
            => new(tick, senderUid, false, message);
    }

    /// <summary>
    /// 레벨업 능력 선택 Command입니다.
    /// </summary>
    public sealed class SelectLevelUpAbilityCommand : DefenseCommand
    {
        /// <summary>
        /// 선택할 능력 인덱스입니다.
        /// </summary>
        public int AbilityIndex { get; }

        public SelectLevelUpAbilityCommand(int abilityIndex, long senderUid = 0)
            : base(senderUid)
        {
            AbilityIndex = abilityIndex;
        }
    }

    /// <summary>
    /// 레벨업 능력 선택 Result입니다.
    /// </summary>
    public sealed class SelectLevelUpAbilityResult : DefenseCommandResult
    {
        /// <summary>
        /// 선택된 능력 ID입니다.
        /// </summary>
        public string SelectedAbilityId { get; }

        /// <summary>
        /// 선택된 능력 이름입니다.
        /// </summary>
        public string SelectedAbilityName { get; }

        private SelectLevelUpAbilityResult(
            long tick,
            long senderUid,
            bool success,
            string errorMessage,
            string abilityId,
            string abilityName)
            : base(tick, senderUid, success, errorMessage)
        {
            SelectedAbilityId = abilityId;
            SelectedAbilityName = abilityName;
        }

        public static SelectLevelUpAbilityResult Ok(
            long tick,
            long senderUid,
            string abilityId,
            string abilityName)
            => new(tick, senderUid, true, null, abilityId, abilityName);

        public static SelectLevelUpAbilityResult Fail(long tick, long senderUid, string message)
            => new(tick, senderUid, false, message, null, null);
    }

    /// <summary>
    /// 게임 종료 Command입니다.
    /// </summary>
    public sealed class EndGameCommand : DefenseCommand
    {
        public EndGameCommand(long senderUid = 0) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 종료 Result입니다.
    /// </summary>
    public sealed class EndGameResult : DefenseCommandResult
    {
        private EndGameResult(long tick, long senderUid, bool success, string errorMessage)
            : base(tick, senderUid, success, errorMessage)
        {
        }

        public static EndGameResult Ok(long tick, long senderUid)
            => new(tick, senderUid, true, null);

        public static EndGameResult Fail(long tick, long senderUid, string message)
            => new(tick, senderUid, false, message);
    }

    /// <summary>
    /// 디펜스 게임 Command 처리 결과 기본 타입입니다.
    /// </summary>
    public abstract class DefenseCommandResult : GameCommandResultBase
    {
        protected DefenseCommandResult(long tick, long senderUid, bool success, string errorMessage)
            : base(tick, senderUid, success, errorMessage)
        {
        }
    }
}

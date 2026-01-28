using Noname.GameHost;

namespace MyProject.DefenseGame.Application.Commands
{
    /// <summary>
    /// DefenseGame용 Command 기본 타입입니다.
    /// </summary>
    public abstract class DefenseCommand : GameCommandBase
    {
        public DefenseCommand(long senderUid) 
            : base(senderUid)
        {
        }

    }

    /// <summary>
    /// 게임 시작 Command입니다.
    /// </summary>
    public sealed class StartGameCommand : DefenseCommand
    {
        public StartGameCommand(long senderUid)
            : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 시작 Result입니다.
    /// </summary>
    public sealed class StartGameResult : DefenseCommandResult
    {
        private StartGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }

        public static StartGameResult Ok(long tick, long senderUid)
        {
            return new StartGameResult(tick, senderUid, true, null);
        }

        public static StartGameResult Fail(long tick, long senderUid, string reason)
        {
            return new StartGameResult(tick, senderUid, false, reason);
        }
    }

    /// <summary>
    /// 레벨업 선택 Command입니다.
    /// </summary>
    public sealed class SelectLevelUpAbilityCommand : DefenseCommand
    {
        public SelectLevelUpAbilityCommand(long senderUid, int abilityIndex)
            : base(senderUid)
        {
            AbilityIndex = abilityIndex;
        }

        /// <summary>
        /// 레벨업 선택 인덱스입니다.
        /// </summary>
        public int AbilityIndex { get; }
    }

    /// <summary>
    /// 레벨업 선택 Result입니다.
    /// </summary>
    public sealed class SelectLevelUpAbilityResult : DefenseCommandResult
    {
        private SelectLevelUpAbilityResult(long tick, long senderUid, bool success, string reason, string abilityId, string abilityName)
            : base(tick, senderUid, success, reason)
        {
            AbilityId = abilityId;
            AbilityName = abilityName;
        }

        /// <summary>
        /// 선택한 스킬 ID입니다.
        /// </summary>
        public string AbilityId { get; }

        /// <summary>
        /// 선택한 스킬 이름입니다.
        /// </summary>
        public string AbilityName { get; }

        public static SelectLevelUpAbilityResult Ok(long tick, long senderUid, string abilityId, string abilityName)
        {
            return new SelectLevelUpAbilityResult(tick, senderUid, true, null, abilityId, abilityName);
        }

        public static SelectLevelUpAbilityResult Fail(long tick, long senderUid, string reason)
        {
            return new SelectLevelUpAbilityResult(tick, senderUid, false, reason, null, null);
        }
    }

    /// <summary>
    /// 게임 종료 Command입니다.
    /// </summary>
    public sealed class EndGameCommand : DefenseCommand
    {
        public EndGameCommand(long senderUid)
            : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 종료 Result입니다.
    /// </summary>
    public sealed class EndGameResult : DefenseCommandResult
    {
        private EndGameResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }

        public static EndGameResult Ok(long tick, long senderUid)
        {
            return new EndGameResult(tick, senderUid, true, null);
        }

        public static EndGameResult Fail(long tick, long senderUid, string reason)
        {
            return new EndGameResult(tick, senderUid, false, reason);
        }
    }

    /// <summary>
    /// DefenseGame용 Command 처리 결과 기본 타입입니다.
    /// </summary>
    public abstract class DefenseCommandResult : GameCommandResultBase
    {
        protected DefenseCommandResult(long tick, long senderUid, bool success, string reason)
            : base(tick, senderUid, success, reason)
        {
        }
    }
}

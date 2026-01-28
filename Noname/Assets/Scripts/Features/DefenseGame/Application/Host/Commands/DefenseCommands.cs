using Noname.GameHost;

namespace MyProject.DefenseGame.Application.Commands
{
    /// <summary>
    /// ���潺 ���� Command �⺻ Ÿ���Դϴ�.
    /// </summary>
    public abstract class DefenseCommand : GameCommandBase
    {
        protected DefenseCommand(long senderUid = 0) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// ���� ���� Command�Դϴ�.
    /// </summary>
    public sealed class StartGameCommand : DefenseCommand
    {
        public StartGameCommand(long senderUid = 0) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// ���� ���� Result�Դϴ�.
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
    /// ������ �ɷ� ���� Command�Դϴ�.
    /// </summary>
    public sealed class SelectLevelUpAbilityCommand : DefenseCommand
    {
        /// <summary>
        /// ������ �ɷ� �ε����Դϴ�.
        /// </summary>
        public int AbilityIndex { get; }

        public SelectLevelUpAbilityCommand(int abilityIndex, long senderUid = 0)
            : base(senderUid)
        {
            AbilityIndex = abilityIndex;
        }
    }

    /// <summary>
    /// ������ �ɷ� ���� Result�Դϴ�.
    /// </summary>
    public sealed class SelectLevelUpAbilityResult : DefenseCommandResult
    {
        /// <summary>
        /// ���õ� �ɷ� ID�Դϴ�.
        /// </summary>
        public string SelectedAbilityId { get; }

        /// <summary>
        /// ���õ� �ɷ� �̸��Դϴ�.
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
    /// ���� ���� Command�Դϴ�.
    /// </summary>
    public sealed class EndGameCommand : DefenseCommand
    {
        public EndGameCommand(long senderUid = 0) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// ���� ���� Result�Դϴ�.
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
    /// ���潺 ���� Command ó�� ��� �⺻ Ÿ���Դϴ�.
    /// </summary>
    public abstract class DefenseCommandResult : GameCommandResultBase
    {
        protected DefenseCommandResult(long tick, long senderUid, bool success, string errorMessage)
            : base(tick, senderUid, success, errorMessage)
        {
        }
    }
}

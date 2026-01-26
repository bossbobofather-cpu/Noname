using MyProject.Common.Host;

namespace MyProject.ExploreGame.Application
{
    // ==================== Commands ====================

    /// <summary>
    /// 탐험 게임 커맨드의 기본 클래스입니다.
    /// </summary>
    public abstract class ExploreCommand : GameCommandBase
    {
        protected ExploreCommand(long senderUid) : base(senderUid)
        {
        }
    }

    /// <summary>
    /// 게임 참가 커맨드입니다.
    /// </summary>
    public sealed class ExploreJoinCommand : ExploreCommand
    {
        public string CharacterName { get; }

        public ExploreJoinCommand(long senderUid, string characterName) : base(senderUid)
        {
            CharacterName = characterName;
        }
    }

    /// <summary>
    /// 던전 시작 커맨드입니다.
    /// </summary>
    public sealed class ExploreStartDungeonCommand : ExploreCommand
    {
        public string DungeonId { get; }

        public ExploreStartDungeonCommand(long senderUid, string dungeonId) : base(senderUid)
        {
            DungeonId = dungeonId;
        }
    }

    // ==================== Command Results ====================

    /// <summary>
    /// 탐험 게임 커맨드 결과의 기본 클래스입니다.
    /// </summary>
    public abstract class ExploreCommandResult : GameCommandResultBase
    {
        protected ExploreCommandResult(long tick, long senderUid, bool success, string errorMessage = null)
            : base(tick, senderUid, success, errorMessage)
        {
        }
    }

    /// <summary>
    /// 게임 참가 결과입니다.
    /// </summary>
    public sealed class ExploreJoinResult : ExploreCommandResult
    {
        public long CharacterUid { get; }

        public ExploreJoinResult(long tick, long senderUid, bool success, long characterUid, string errorMessage = null)
            : base(tick, senderUid, success, errorMessage)
        {
            CharacterUid = characterUid;
        }
    }

    /// <summary>
    /// 던전 시작 결과입니다.
    /// </summary>
    public sealed class ExploreStartDungeonResult : ExploreCommandResult
    {
        public string DungeonId { get; }

        public ExploreStartDungeonResult(long tick, long senderUid, bool success, string dungeonId, string errorMessage = null)
            : base(tick, senderUid, success, errorMessage)
        {
            DungeonId = dungeonId;
        }
    }

    // ==================== Events ====================

    /// <summary>
    /// 탐험 게임 이벤트의 기본 클래스입니다.
    /// </summary>
    public abstract class ExploreHostEvent : GameEventBase
    {
        protected ExploreHostEvent(long tick) : base(tick)
        {
        }
    }

    /// <summary>
    /// 플레이어가 게임에 참가했을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExplorePlayerJoinedEvent : ExploreHostEvent
    {
        public long CharacterUid { get; }
        public string CharacterName { get; }

        public ExplorePlayerJoinedEvent(long tick, long characterUid, string characterName) : base(tick)
        {
            CharacterUid = characterUid;
            CharacterName = characterName;
        }
    }

    /// <summary>
    /// 던전 탐험이 시작되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreDungeonStartedEvent : ExploreHostEvent
    {
        public string DungeonId { get; }
        public int TotalStages { get; }

        public ExploreDungeonStartedEvent(long tick, string dungeonId, int totalStages) : base(tick)
        {
            DungeonId = dungeonId;
            TotalStages = totalStages;
        }
    }

    /// <summary>
    /// 스테이지가 변경되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreStageChangedEvent : ExploreHostEvent
    {
        public int Stage { get; }

        public ExploreStageChangedEvent(long tick, int stage) : base(tick)
        {
            Stage = stage;
        }
    }

    /// <summary>
    /// 전투가 시작되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreCombatStartedEvent : ExploreHostEvent
    {
        public string MonsterType { get; }
        public int MonsterCount { get; }

        public ExploreCombatStartedEvent(long tick, string monsterType, int monsterCount) : base(tick)
        {
            MonsterType = monsterType;
            MonsterCount = monsterCount;
        }
    }

    /// <summary>
    /// 전투 행동이 발생했을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreCombatActionEvent : ExploreHostEvent
    {
        public string ActorName { get; }
        public string TargetName { get; }
        public int Damage { get; }
        public bool IsTargetDead { get; }

        public ExploreCombatActionEvent(long tick, string actorName, string targetName, int damage, bool isTargetDead)
            : base(tick)
        {
            ActorName = actorName;
            TargetName = targetName;
            Damage = damage;
            IsTargetDead = isTargetDead;
        }
    }

    /// <summary>
    /// 어빌리티가 사용되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreAbilityUsedEvent : ExploreHostEvent
    {
        public string ActorName { get; }
        public string AbilityName { get; }
        public string TargetName { get; }
        public int Damage { get; }
        public bool IsTargetDead { get; }

        public ExploreAbilityUsedEvent(long tick, string actorName, string abilityName, string targetName, int damage, bool isTargetDead)
            : base(tick)
        {
            ActorName = actorName;
            AbilityName = abilityName;
            TargetName = targetName;
            Damage = damage;
            IsTargetDead = isTargetDead;
        }
    }

    /// <summary>
    /// 전투가 종료되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreCombatEndedEvent : ExploreHostEvent
    {
        public bool Victory { get; }

        public ExploreCombatEndedEvent(long tick, bool victory) : base(tick)
        {
            Victory = victory;
        }
    }

    /// <summary>
    /// 보상이 지급되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreRewardGrantedEvent : ExploreHostEvent
    {
        public int Gold { get; }
        public int Experience { get; }

        public ExploreRewardGrantedEvent(long tick, int gold, int experience) : base(tick)
        {
            Gold = gold;
            Experience = experience;
        }
    }

    /// <summary>
    /// 캐릭터가 레벨업했을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreCharacterLevelUpEvent : ExploreHostEvent
    {
        public long CharacterUid { get; }
        public int NewLevel { get; }

        public ExploreCharacterLevelUpEvent(long tick, long characterUid, int newLevel) : base(tick)
        {
            CharacterUid = characterUid;
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// 던전 탐험이 완료되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreDungeonCompletedEvent : ExploreHostEvent
    {
        public string DungeonId { get; }

        public ExploreDungeonCompletedEvent(long tick, string dungeonId) : base(tick)
        {
            DungeonId = dungeonId;
        }
    }

    /// <summary>
    /// 던전 탐험이 실패했을 때 발생하는 이벤트입니다.
    /// </summary>
    public sealed class ExploreDungeonFailedEvent : ExploreHostEvent
    {
        public string DungeonId { get; }
        public int ReachedStage { get; }

        public ExploreDungeonFailedEvent(long tick, string dungeonId, int reachedStage) : base(tick)
        {
            DungeonId = dungeonId;
            ReachedStage = reachedStage;
        }
    }
}

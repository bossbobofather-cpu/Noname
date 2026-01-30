using System.Collections.Generic;
using Noname.GameHost;
using MyProject.DefenseGame.Domain.LevelUp;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// 디펜스 게임 이벤트의 기본 타입입니다.
    /// </summary>
    public abstract class DefenseHostEvent : GameEventBase
    {
        protected DefenseHostEvent(long tick) : base(tick)
        {
        }
    }

    /// <summary>
    /// 게임 시작 이벤트입니다.
    /// </summary>
    public sealed class DefenseGameStartedEvent : DefenseHostEvent
    {
        public DefenseGameStartedEvent(long tick) : base(tick) { }
    }

    /// <summary>
    /// 몬스터 스폰 이벤트입니다.
    /// </summary>
    public sealed class DefenseMonsterSpawnedEvent : DefenseHostEvent
    {
        public long MonsterUid { get; }
        public string MonsterType { get; }
        public bool IsBoss { get; }
        public Point2D Position { get; }

        public DefenseMonsterSpawnedEvent(
            long tick,
            long monsterUid,
            string monsterType,
            bool isBoss,
            Point2D position) : base(tick)
        {
            MonsterUid = monsterUid;
            MonsterType = monsterType;
            IsBoss = isBoss;
            Position = position;
        }
    }

    /// <summary>
    /// 몬스터 처치 이벤트입니다.
    /// </summary>
    public sealed class DefenseMonsterKilledEvent : DefenseHostEvent
    {
        public long MonsterUid { get; }
        public string MonsterType { get; }
        public bool IsBoss { get; }
        public int ExpGained { get; }

        public DefenseMonsterKilledEvent(
            long tick,
            long monsterUid,
            string monsterType,
            bool isBoss,
            int expGained) : base(tick)
        {
            MonsterUid = monsterUid;
            MonsterType = monsterType;
            IsBoss = isBoss;
            ExpGained = expGained;
        }
    }

    /// <summary>
    /// 플레이어 능력 발동 이벤트 입니다.
    /// </summary>
    public sealed class DefensePlayerActivateAbilityEvent : DefenseHostEvent
    {
        public List<long> TargetUids { get; }
        public GameplayAbility Ability { get; }

        public DefensePlayerActivateAbilityEvent(
            long tick,
            List<long> targetUids,
            GameplayAbility ability
        ) : base(tick)
        {
            TargetUids = targetUids;
            Ability = ability;
        }
    }

    /// <summary>
    /// 몬스터 능력 발동 이벤트 입니다.
    /// </summary>
    public sealed class DefenseMonsterActivateAbilityEvent : DefenseHostEvent
    {
        public long MonsterUid { get; }
        public GameplayAbility Ability { get; }

        public bool IsBoss { get; }
        public DefenseMonsterActivateAbilityEvent(
            long tick,
            long monsterUid,
            GameplayAbility ability,
            bool isBoss = false
        ) : base(tick)
        {
            MonsterUid = monsterUid;
            Ability = ability;
            IsBoss = isBoss;
        }
    }

    /// <summary>
    /// 레벨 업 이벤트입니다.
    /// </summary>
    public sealed class DefenseLevelUpEvent : DefenseHostEvent
    {
        public int NewLevel { get; }

        public DefenseLevelUpEvent(long tick, int newLevel) : base(tick)
        {
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// 웨이브 변경 이벤트입니다.
    /// </summary>
    public sealed class DefenseWaveChangedEvent : DefenseHostEvent
    {
        public int Wave { get; }

        public DefenseWaveChangedEvent(long tick, int wave) : base(tick)
        {
            Wave = wave;
        }
    }

    /// <summary>
    /// 게임 종료 이벤트입니다.
    /// </summary>
    public sealed class DefenseGameOverEvent : DefenseHostEvent
    {
        public bool IsVictory { get; }
        public float SurvivalTime { get; }
        public int TotalKills { get; }

        public DefenseGameOverEvent(
            long tick,
            bool isVictory,
            float survivalTime,
            int totalKills) : base(tick)
        {
            IsVictory = isVictory;
            SurvivalTime = survivalTime;
            TotalKills = totalKills;
        }
    }

    /// <summary>
    /// 플레이어 사망 이벤트입니다.
    /// </summary>
    public sealed class DefensePlayerDeathEvent : DefenseHostEvent
    {
        public float SurvivalTime { get; }
        public int TotalKills { get; }

        public DefensePlayerDeathEvent(
            long tick,
            float survivalTime,
            int totalKills) : base(tick)
        {
            SurvivalTime = survivalTime;
            TotalKills = totalKills;
        }
    }

    /// <summary>
    /// 레벨업 선택지 이벤트입니다.
    /// </summary>
    public sealed class DefenseLevelUpOptionsEvent : DefenseHostEvent
    {
        public IReadOnlyList<LevelUpAbilityDefinition> Options { get; }

        public DefenseLevelUpOptionsEvent(
            long tick,
            IReadOnlyList<LevelUpAbilityDefinition> options) : base(tick)
        {
            Options = options;
        }
    }

    /// <summary>
    /// 어빌리티 선택 완료 이벤트입니다.
    /// </summary>
    public sealed class DefenseAbilitySelectedEvent : DefenseHostEvent
    {
        public FGameplayTag AbilityTag { get; }
        public string AbilityName { get; }

        public DefenseAbilitySelectedEvent(
            long tick,
            FGameplayTag abilityTag,
            string abilityName) : base(tick)
        {
            AbilityTag = abilityTag;
            AbilityName = abilityName;
        }
    }
}

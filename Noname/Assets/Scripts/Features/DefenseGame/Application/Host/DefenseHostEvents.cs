using System.Collections.Generic;
using Noname.GameHost;
using MyProject.DefenseGame.Domain.LevelUp;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// µğÆæ½º °ÔÀÓ ÀÌº¥Æ®ÀÇ ±âº» Å¸ÀÔÀÔ´Ï´Ù.
    /// </summary>
    public abstract class DefenseHostEvent : GameEventBase
    {
        protected DefenseHostEvent(long tick) : base(tick)
        {
        }
    }
public sealed class DefenseGameStartedEvent : DefenseHostEvent
    {
        public DefenseGameStartedEvent(long tick) : base(tick) { }
    }

    /// <summary>
    /// ëª¬ìŠ¤???¤í° ?´ë²¤?¸ì…?ˆë‹¤.
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
    /// ëª¬ìŠ¤???¬ë§ ?´ë²¤?¸ì…?ˆë‹¤.
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
    /// ?Œë ˆ?´ì–´ ê³µê²© ?´ë²¤?¸ì…?ˆë‹¤.
    /// </summary>
    public sealed class DefensePlayerAttackEvent : DefenseHostEvent
    {
        public long TargetUid { get; }
        public int Damage { get; }
        public bool TargetKilled { get; }

        public DefensePlayerAttackEvent(
            long tick,
            long targetUid,
            int damage,
            bool targetKilled) : base(tick)
        {
            TargetUid = targetUid;
            Damage = damage;
            TargetKilled = targetKilled;
        }
    }

    /// <summary>
    /// ëª¬ìŠ¤??ê³µê²© ?´ë²¤?¸ì…?ˆë‹¤.
    /// </summary>
    public sealed class DefenseMonsterAttackEvent : DefenseHostEvent
    {
        public long MonsterUid { get; }
        public int Damage { get; }
        public int PlayerHpRemaining { get; }

        public DefenseMonsterAttackEvent(
            long tick,
            long monsterUid,
            int damage,
            int playerHpRemaining) : base(tick)
        {
            MonsterUid = monsterUid;
            Damage = damage;
            PlayerHpRemaining = playerHpRemaining;
        }
    }

    /// <summary>
    /// ?ˆë²¨???´ë²¤?¸ì…?ˆë‹¤.
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
    /// ?¨ì´ë¸?ë³€ê²??´ë²¤?¸ì…?ˆë‹¤.
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
    /// ê²Œì„ ?¤ë²„ ?´ë²¤?¸ì…?ˆë‹¤.
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
    /// ?Œë ˆ?´ì–´ ?¬ë§ ?´ë²¤?¸ì…?ˆë‹¤.
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
    /// ?ˆë²¨??? íƒì§€ ?´ë²¤?¸ì…?ˆë‹¤.
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
    /// ?´ë¹Œë¦¬í‹° ? íƒ ?„ë£Œ ?´ë²¤?¸ì…?ˆë‹¤.
    /// </summary>
    public sealed class DefenseAbilitySelectedEvent : DefenseHostEvent
    {
        public LevelUpAbilityId AbilityId { get; }
        public string AbilityName { get; }

        public DefenseAbilitySelectedEvent(
            long tick,
            LevelUpAbilityId abilityId,
            string abilityName) : base(tick)
        {
            AbilityId = abilityId;
            AbilityName = abilityName;
        }
    }
}




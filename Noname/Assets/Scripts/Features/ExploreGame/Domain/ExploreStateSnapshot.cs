using System.Collections.Generic;
using MyProject.Common.Host;

namespace MyProject.ExploreGame.Domain
{
    /// <summary>
    /// 탐험 게임 상태의 불변 스냅샷입니다.
    /// </summary>
    public sealed class ExploreStateSnapshot : GameSnapshotBase
    {
        public ExploreSessionPhase SessionPhase { get; }
        public IReadOnlyList<ExploreCharacterSnapshot> Characters { get; }
        public ExploreDungeonSnapshot CurrentDungeon { get; }
        public ExploreCombatSnapshot CurrentCombat { get; }

        public ExploreStateSnapshot(
            long tick,
            ExploreSessionPhase sessionPhase,
            IReadOnlyDictionary<long, ExploreCharacterState> characters,
            ExploreDungeonState dungeon,
            ExploreCombatState combat)
            : base(tick)
        {
            SessionPhase = sessionPhase;

            // 캐릭터 스냅샷 생성
            var characterList = new List<ExploreCharacterSnapshot>();
            if (characters != null)
            {
                foreach (var kvp in characters)
                {
                    characterList.Add(new ExploreCharacterSnapshot(kvp.Value));
                }
            }
            Characters = characterList;

            // 던전 스냅샷 생성
            CurrentDungeon = dungeon != null ? new ExploreDungeonSnapshot(dungeon) : null;

            // 전투 스냅샷 생성
            CurrentCombat = combat != null ? new ExploreCombatSnapshot(combat) : null;
        }
    }

    /// <summary>
    /// 캐릭터 상태의 불변 스냅샷입니다.
    /// </summary>
    public sealed class ExploreCharacterSnapshot
    {
        public long Uid { get; }
        public string Name { get; }
        public int Level { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int AttackPower { get; }
        public int Defense { get; }
        public int Speed { get; }
        public int Gold { get; }
        public int Experience { get; }
        public float TurnGauge { get; }
        public bool IsAlive { get; }

        public ExploreCharacterSnapshot(ExploreCharacterState state)
        {
            Uid = state.Uid;
            Name = state.Name;
            Level = state.Level;
            CurrentHp = state.CurrentHp;
            MaxHp = state.MaxHp;
            AttackPower = state.AttackPower;
            Defense = state.Defense;
            Speed = state.Speed;
            Gold = state.Gold;
            Experience = state.Experience;
            TurnGauge = state.TurnGauge;
            IsAlive = state.IsAlive;
        }
    }

    /// <summary>
    /// 던전 상태의 불변 스냅샷입니다.
    /// </summary>
    public sealed class ExploreDungeonSnapshot
    {
        public string DungeonId { get; }
        public int TotalStages { get; }
        public int CurrentStage { get; }
        public DungeonPhase Phase { get; }
        public bool IsCompleted { get; }
        public bool IsFailed { get; }

        public ExploreDungeonSnapshot(ExploreDungeonState state)
        {
            DungeonId = state.DungeonId;
            TotalStages = state.TotalStages;
            CurrentStage = state.CurrentStage;
            Phase = state.Phase;
            IsCompleted = state.IsCompleted;
            IsFailed = state.IsFailed;
        }
    }

    /// <summary>
    /// 전투 상태의 불변 스냅샷입니다.
    /// </summary>
    public sealed class ExploreCombatSnapshot
    {
        public CombatPhase Phase { get; }
        public int TurnCount { get; }
        public int MaxTurns { get; }
        public bool IsVictory { get; }
        public IReadOnlyList<ExploreMonsterSnapshot> Monsters { get; }

        public ExploreCombatSnapshot(ExploreCombatState state)
        {
            Phase = state.Phase;
            TurnCount = state.TurnCount;
            MaxTurns = state.MaxTurns;
            IsVictory = state.IsVictory;

            var monsters = state.GetAllMonsters();
            var monsterList = new List<ExploreMonsterSnapshot>();
            for (var i = 0; i < monsters.Count; i++)
            {
                monsterList.Add(new ExploreMonsterSnapshot(monsters[i]));
            }
            Monsters = monsterList;
        }
    }

    /// <summary>
    /// 몬스터 상태의 불변 스냅샷입니다.
    /// </summary>
    public sealed class ExploreMonsterSnapshot
    {
        public long Uid { get; }
        public string MonsterType { get; }
        public int Level { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int AttackPower { get; }
        public int Defense { get; }
        public int Speed { get; }
        public float TurnGauge { get; }
        public bool IsDead { get; }

        public ExploreMonsterSnapshot(ExploreMonsterState state)
        {
            Uid = state.Uid;
            MonsterType = state.MonsterType;
            Level = state.Level;
            CurrentHp = state.CurrentHp;
            MaxHp = state.MaxHp;
            AttackPower = state.AttackPower;
            Defense = state.Defense;
            Speed = state.Speed;
            TurnGauge = state.TurnGauge;
            IsDead = state.IsDead;
        }
    }
}

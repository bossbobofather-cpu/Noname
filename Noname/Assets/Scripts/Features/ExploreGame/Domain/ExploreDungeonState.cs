using System;

namespace MyProject.ExploreGame.Domain
{
    /// <summary>
    /// 던전 탐험 단계입니다.
    /// </summary>
    public enum DungeonPhase
    {
        /// <summary>탐험 준비 중</summary>
        Preparing,
        /// <summary>탐험 진행 중</summary>
        Exploring,
        /// <summary>전투 중</summary>
        InCombat,
        /// <summary>탐험 완료</summary>
        Completed,
        /// <summary>탐험 실패</summary>
        Failed
    }

    /// <summary>
    /// 던전 스테이지 데이터입니다.
    /// </summary>
    public sealed class DungeonStageData
    {
        public int StageNumber { get; }
        public string MonsterType { get; }
        public int MonsterLevel { get; }
        public int MonsterCount { get; }

        public DungeonStageData(int stageNumber, string monsterType, int monsterLevel, int monsterCount)
        {
            if (stageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stageNumber), "스테이지 번호는 1 이상이어야 합니다.");
            }

            if (string.IsNullOrEmpty(monsterType))
            {
                throw new ArgumentException("몬스터 타입은 비어있을 수 없습니다.", nameof(monsterType));
            }

            if (monsterCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(monsterCount), "몬스터 수는 1 이상이어야 합니다.");
            }

            StageNumber = stageNumber;
            MonsterType = monsterType;
            MonsterLevel = monsterLevel;
            MonsterCount = monsterCount;
        }
    }

    /// <summary>
    /// 던전 탐험 상태를 나타냅니다.
    /// </summary>
    public sealed class ExploreDungeonState
    {
        public string DungeonId { get; }
        public int TotalStages { get; }
        public int CurrentStage { get; private set; }
        public DungeonPhase Phase { get; private set; }
        public DungeonStageData[] Stages { get; }

        public bool IsCompleted => Phase == DungeonPhase.Completed;
        public bool IsFailed => Phase == DungeonPhase.Failed;

        public ExploreDungeonState(string dungeonId, int totalStages, DungeonStageData[] stages)
        {
            if (string.IsNullOrEmpty(dungeonId))
            {
                throw new ArgumentException("던전 ID는 비어있을 수 없습니다.", nameof(dungeonId));
            }

            if (totalStages < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(totalStages), "총 스테이지 수는 1 이상이어야 합니다.");
            }

            if (stages == null || stages.Length != totalStages)
            {
                throw new ArgumentException("스테이지 데이터가 올바르지 않습니다.", nameof(stages));
            }

            DungeonId = dungeonId;
            TotalStages = totalStages;
            CurrentStage = 0;
            Phase = DungeonPhase.Preparing;
            Stages = stages;
        }

        /// <summary>
        /// 던전 탐험을 시작합니다.
        /// </summary>
        public void StartExploration()
        {
            if (Phase != DungeonPhase.Preparing)
            {
                throw new InvalidOperationException("이미 탐험이 시작되었습니다.");
            }

            Phase = DungeonPhase.Exploring;
            CurrentStage = 1;
        }

        /// <summary>
        /// 다음 스테이지로 이동합니다.
        /// </summary>
        public bool AdvanceStage()
        {
            if (Phase != DungeonPhase.Exploring)
            {
                throw new InvalidOperationException("탐험 중이 아닙니다.");
            }

            if (CurrentStage >= TotalStages)
            {
                Phase = DungeonPhase.Completed;
                return false;
            }

            CurrentStage++;

            if (CurrentStage > TotalStages)
            {
                Phase = DungeonPhase.Completed;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 전투 단계로 전환합니다.
        /// </summary>
        public void EnterCombat()
        {
            if (Phase != DungeonPhase.Exploring)
            {
                throw new InvalidOperationException("탐험 중이 아닙니다.");
            }

            Phase = DungeonPhase.InCombat;
        }

        /// <summary>
        /// 전투 종료 후 탐험 단계로 복귀합니다.
        /// </summary>
        public void ExitCombat(bool victory)
        {
            if (Phase != DungeonPhase.InCombat)
            {
                throw new InvalidOperationException("전투 중이 아닙니다.");
            }

            if (victory)
            {
                Phase = DungeonPhase.Exploring;
            }
            else
            {
                Phase = DungeonPhase.Failed;
            }
        }

        /// <summary>
        /// 현재 스테이지 데이터를 반환합니다.
        /// </summary>
        public DungeonStageData GetCurrentStageData()
        {
            if (CurrentStage < 1 || CurrentStage > TotalStages)
            {
                return null;
            }

            return Stages[CurrentStage - 1];
        }
    }
}

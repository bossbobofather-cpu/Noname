using System;
using System.Collections.Generic;

namespace MyProject.DefenseGame.Domain.LevelUp
{
    /// <summary>
    /// 레벨업 어빌리티 ID입니다.
    /// </summary>
    public enum LevelUpAbilityId
    {
        None = 0,

        // 기본 어빌리티 (선행조건 없음)
        FullHealthRestore,      // 체력 완전 회복
        AttackSpeedUp,          // 공격속도 증가 (쿨다운 감소)
        ExpGainUp,              // 경험치 획득량 증가
        AreaAttack,             // 범위 공격 (가장 가까운 3기 공격)
        LifeStealOnKill,        // 몬스터 처치 시 체력 회복

        // 종속 어빌리티 (선행조건 필요)
        AreaAttackTargetUp,     // 범위 공격 대상 수 증가 (AreaAttack 필요)
        LifeStealAmountUp,      // 체력 회복량 증가 (LifeStealOnKill 필요)
    }

    /// <summary>
    /// 레벨업 어빌리티 정의입니다.
    /// </summary>
    public sealed class LevelUpAbilityDefinition
    {
        /// <summary>
        /// 어빌리티 ID입니다.
        /// </summary>
        public LevelUpAbilityId Id { get; }

        /// <summary>
        /// 표시 이름입니다.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 설명입니다.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 선행 조건 어빌리티 ID입니다. None이면 선행조건 없음.
        /// </summary>
        public LevelUpAbilityId PrerequisiteId { get; }

        /// <summary>
        /// 중복 획득 가능 여부입니다.
        /// </summary>
        public bool IsStackable { get; }

        /// <summary>
        /// 어빌리티 적용 액션입니다.
        /// </summary>
        public Action<DefensePlayer> ApplyAction { get; }

        public LevelUpAbilityDefinition(
            LevelUpAbilityId id,
            string displayName,
            string description,
            LevelUpAbilityId prerequisiteId,
            bool isStackable,
            Action<DefensePlayer> applyAction)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            PrerequisiteId = prerequisiteId;
            IsStackable = isStackable;
            ApplyAction = applyAction;
        }
    }
}

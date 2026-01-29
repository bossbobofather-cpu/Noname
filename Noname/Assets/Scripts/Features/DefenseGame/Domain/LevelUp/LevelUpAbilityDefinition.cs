using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;

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
        AreaAttack,             // 범위 공격 (가장 가까운 3기 공격)

        // 종속 어빌리티 (선행조건 필요)
        AreaAttackTargetUp,     // 범위 공격 대상 수 증가 (AreaAttack 필요)
    }

    /// <summary>
    /// 레벨업 어빌리티 정의입니다.
    /// </summary>
    public sealed class LevelUpAbilityDefinition
    {
        /// <summary>
        /// 어빌리티 ID입니다.
        /// </summary>
        public FGameplayTag AbilityTag { get; }

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
        public FGameplayTag PrerequisiteTag { get; }

        /// <summary>
        /// 중복 획득 가능 여부입니다.
        /// </summary>
        public bool IsStackable { get; }

        /// <summary>
        /// 어빌리티 적용 액션입니다.
        /// </summary>
        public Action<DefensePlayer> ApplyAction { get; }

        public LevelUpAbilityDefinition(
            FGameplayTag abilityTag,
            string displayName,
            string description,
            bool isStackable,
            Action<DefensePlayer> applyAction,
            FGameplayTag prerequisiteTag = default)
        {
            AbilityTag = abilityTag;
            DisplayName = displayName;
            Description = description;
            PrerequisiteTag = prerequisiteTag;
            IsStackable = isStackable;
            ApplyAction = applyAction;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 게임플레이 어빌리티 설정입니다 (순수 C# 모델).
    /// </summary>
    [Serializable]
    public sealed class GameplayAbility
    {
        /// <summary>
        /// 어빌리티 태그입니다.
        /// </summary>
        public FGameplayTag AbilityTag { get; set; }

        /// <summary>
        /// 어빌리티 ID입니다.
        /// </summary>
        public string AbilityId { get; set; }

        /// <summary>
        /// 표시 이름입니다.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 설명입니다.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 쿨다운 효과입니다.
        /// 활성화 시 자신에게 적용되어 쿨다운 태그를 부여합니다.
        /// </summary>
        public GameplayEffect CooldownEffect { get; set; }

        /// <summary>
        /// 비용으로 소모되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffect> CostEffects { get; set; }

        /// <summary>
        /// 적용되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffect> AppliedEffects { get; set; }

        /// <summary>
        /// 활성화 필수 태그 목록입니다.
        /// </summary>
        public GameplayTagContainer  ActivationRequiredTags { get; set; }

        /// <summary>
        /// 활성화 차단 태그 목록입니다.
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags { get; set; }

        /// <summary>
        /// 타겟 선정 전략입니다.
        /// </summary>
        public ITargetingStrategy TargetingStrategy { get; set; }

        public GameplayAbility()
        {
            AbilityId = "";
            DisplayName = "";
            Description = "";
            CooldownEffect = null;
            CostEffects = new List<GameplayEffect>();
            AppliedEffects = new List<GameplayEffect>();
            ActivationRequiredTags = new GameplayTagContainer();
            ActivationBlockedTags = new GameplayTagContainer();
            TargetingStrategy = null;
        }
    }
}

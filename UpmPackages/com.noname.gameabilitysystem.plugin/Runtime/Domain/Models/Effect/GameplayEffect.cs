using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 게임플레이 효과 설정입니다 (순수 C# 모델).
    /// </summary>
    [Serializable]
    public sealed class GameplayEffect
    {
        /// <summary>
        /// 효과 ID입니다.
        /// </summary>
        public string EffectId { get; set; }

        /// <summary>
        /// 표시 이름입니다.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 설명입니다.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 지속 타입입니다.
        /// </summary>
        public EffectDurationType DurationType { get; set; }

        /// <summary>
        /// 지속 시간입니다 (초).
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// 주기 시간입니다 (초).
        /// </summary>
        public float Period { get; set; }

        /// <summary>
        /// 최대 스택 수입니다.
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// 수정자 그룹 목록입니다.
        /// </summary>
        public List<AttributeModifierGroup> ModifierGroups { get; set; }

        /// <summary>
        /// 부여되는 태그 목록입니다.
        /// </summary>
        public GameplayTagContainer GrantedTags { get; set; }

        /// <summary>
        /// 적용 필수 태그 목록입니다.
        /// </summary>
        public GameplayTagContainer RequiredTags { get; set; }

        /// <summary>
        /// 적용 차단 태그 목록입니다.
        /// </summary>
        public GameplayTagContainer BlockedTags { get; set; }

        public GameplayEffect()
        {
            ModifierGroups = new List<AttributeModifierGroup>();
            GrantedTags = new GameplayTagContainer();
            RequiredTags = new GameplayTagContainer();
            BlockedTags = new GameplayTagContainer();
            DurationType = EffectDurationType.Instant;
            MaxStack = 1;
        }
    }
}

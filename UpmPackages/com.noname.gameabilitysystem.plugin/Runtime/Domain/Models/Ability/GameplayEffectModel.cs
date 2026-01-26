using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 게임플레이 효과 설정입니다 (순수 C# 모델).
    /// </summary>
    [Serializable]
    public sealed class GameplayEffectModel
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
        public List<GameplayModifierGroup> ModifierGroups { get; set; }

        /// <summary>
        /// 부여되는 태그 목록입니다.
        /// </summary>
        public List<string> GrantedTags { get; set; }

        /// <summary>
        /// 적용 필수 태그 목록입니다.
        /// </summary>
        public List<string> RequiredTags { get; set; }

        /// <summary>
        /// 적용 차단 태그 목록입니다.
        /// </summary>
        public List<string> BlockedTags { get; set; }

        public GameplayEffectModel()
        {
            ModifierGroups = new List<GameplayModifierGroup>();
            GrantedTags = new List<string>();
            RequiredTags = new List<string>();
            BlockedTags = new List<string>();
            DurationType = EffectDurationType.Instant;
            MaxStack = 1;
        }
    }
}

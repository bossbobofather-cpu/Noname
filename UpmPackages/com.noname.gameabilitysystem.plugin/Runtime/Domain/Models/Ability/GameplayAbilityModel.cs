using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 게임플레이 어빌리티 설정입니다 (순수 C# 모델).
    /// </summary>
    [Serializable]
    public sealed class GameplayAbilityModel
    {
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
        /// 쿨다운 시간입니다 (초).
        /// </summary>
        public float Cooldown { get; set; }

        /// <summary>
        /// 비용으로 소모되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffectModel> CostEffects { get; set; }

        /// <summary>
        /// 적용되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffectModel> AppliedEffects { get; set; }

        /// <summary>
        /// 활성화 필수 태그 목록입니다.
        /// </summary>
        public List<string> ActivationRequiredTags { get; set; }

        /// <summary>
        /// 활성화 차단 태그 목록입니다.
        /// </summary>
        public List<string> ActivationBlockedTags { get; set; }

        public GameplayAbilityModel()
        {
            CostEffects = new List<GameplayEffectModel>();
            AppliedEffects = new List<GameplayEffectModel>();
            ActivationRequiredTags = new List<string>();
            ActivationBlockedTags = new List<string>();
        }
    }
}

using System;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 능력 사양 정보를 담는 구조입니다.
    /// ASC에 부여된 능력의 런타임 상태를 관리합니다.
    /// </summary>
    public sealed class GameplayAbilitySpec
    {
        private readonly GameplayAbility _ability;
        private readonly FGameplayAbilitySpecHandle _handle;
        private int _level;
        private int _activeCount;

        public GameplayAbilitySpec(GameplayAbility ability, FGameplayAbilitySpecHandle handle, int level = 1)
        {
            _ability = ability ?? throw new ArgumentNullException(nameof(ability));
            _handle = handle;
            _level = level;
            _activeCount = 0;
        }

        public GameplayAbilitySpec(GameplayAbility ability, int handleId)
            : this(ability, new FGameplayAbilitySpecHandle { Id = handleId })
        {
        }

        /// <summary>
        /// 능력 정의입니다.
        /// </summary>
        public GameplayAbility Ability => _ability;

        /// <summary>
        /// 능력 핸들입니다.
        /// </summary>
        public FGameplayAbilitySpecHandle Handle => _handle;

        /// <summary>
        /// 능력 태그입니다.
        /// </summary>
        public FGameplayTag AbilityTag => _ability.AbilityTag;

        /// <summary>
        /// 활성화에 필수로 필요한 태그 목록입니다.
        /// </summary>
        public GameplayTagContainer ActivationRequiredTags => _ability.ActivationRequiredTags;

        /// <summary>
        /// 활성화를 차단하는 태그 목록입니다.
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags => _ability.ActivationBlockedTags;

        /// <summary>
        /// 능력 ID입니다.
        /// </summary>
        public string AbilityId => _ability.AbilityId;

        /// <summary>
        /// 표시 이름입니다.
        /// </summary>
        public string DisplayName => _ability.DisplayName;

        /// <summary>
        /// 능력 레벨입니다.
        /// </summary>
        public int Level
        {
            get => _level;
            set => _level = Math.Max(1, value);
        }

        /// <summary>
        /// 활성화 중인 횟수입니다.
        /// </summary>
        public int ActiveCount => _activeCount;

        /// <summary>
        /// 활성화 카운트를 증가시킵니다.
        /// </summary>
        public void IncrementActiveCount()
        {
            _activeCount++;
        }

        /// <summary>
        /// 활성화 카운트를 감소시킵니다.
        /// </summary>
        public void DecrementActiveCount()
        {
            _activeCount = Math.Max(0, _activeCount - 1);
        }
    }
}

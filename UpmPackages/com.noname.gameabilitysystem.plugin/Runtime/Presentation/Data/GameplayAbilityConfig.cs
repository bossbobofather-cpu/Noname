using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;
using UnityEngine;

namespace Noname.GameAbilitySystem.Presentation
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayAbilityDefinition")]
    /// <summary>
    /// 어빌리티 정의 데이터를 담는 에셋입니다.
    /// </summary>
    public sealed class GameplayAbilityConfig : ScriptableObject
    {
        [SerializeField] private FGameplayTag _abilityTag;
        [SerializeField] private string _abilityId;
        [SerializeField] private string _dpName;
        [SerializeField] private string _dpDesc;
        [SerializeField] private float _cooldown;        
        [SerializeField] private List<GameplayEffectConfig> _appliedEffects;
        [SerializeField] private List<GameplayEffectConfig> _costEffects;
        [SerializeField] private GameplayTagContainerView _activationRequiredTags;
        [SerializeField] private GameplayTagContainerView _activationBlockedTags;

        /// <summary>
        /// 어빌리티 태그입니다.
        /// </summary>
        public FGameplayTag AbilityTag => _abilityTag;

        /// <summary>
        /// 어빌리티 ID입니다.
        /// </summary>
        public string AbilityID => _abilityId;

        /// <summary>
        /// 표시되는 이름입니다.
        /// </summary>
        public string DisplayName => _dpName;

        /// <summary>
        /// 표시되는 설명입니다.
        /// </summary>
        public string Description => _dpDesc;    
        
        /// <summary>
        /// 비용으로 소모되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffectConfig> CostEffects => _costEffects;

        /// <summary>
        /// 적용되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffectConfig> AppliedEffects => _appliedEffects;

        /// <summary>
        /// 활성화 필수 태그 목록입니다.
        /// </summary>
        public GameplayTagContainerView  ActivationRequiredTags => _activationRequiredTags;

        /// <summary>
        /// 활성화 차단 태그 목록입니다.
        /// </summary>
        public GameplayTagContainerView ActivationBlockedTags => _activationBlockedTags;        
    }
}

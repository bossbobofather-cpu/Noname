using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayEffectConfig")]
    /// <summary>
    /// 게임플레이 효과 설정을 담는 에셋입니다.
    /// </summary>
    public class GameplayEffectConfig : GameplayConfig
    {
        // 지속 타입
        [SerializeField] private EGameplayEffectDurationType _durationType = EGameplayEffectDurationType.Instant;

        // 지속 시간
        [SerializeField] private float _duration = 0f;

        // 주기 시간
        [SerializeField] private float _period = 0f;

        // 부여되는 태그
        [SerializeField] private GameplayTagContainer _grantedTags = new GameplayTagContainer();
        [SerializeField] private GameplayTagContainer _activationRequiredTags = new GameplayTagContainer();
        [SerializeField] private GameplayTagContainer _activationBlockedTags = new GameplayTagContainer();
        [SerializeField, HideInInspector] private EGameplayEffectDurationType _lastDurationType;
        [SerializeField, HideInInspector] private bool _durationTypeInitialized;

        [SerializeField] private List<AttributeModifier> _modifiers = new List<AttributeModifier>();

        /// <summary>
        /// 지속 타입입니다.
        /// </summary>
        public EGameplayEffectDurationType DurationType => _durationType;

        /// <summary>
        /// 지속 시간입니다. 지속 타입이 고정 시간일 때만 의미가 있습니다.
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 주기 시간입니다. 지속 타입이 고정 시간 또는 무한일 때만 의미가 있습니다.
        /// </summary>
        public float Period => _period;

        /// <summary>
        /// 효과로 부여되는 태그입니다.
        /// </summary>
        public GameplayTagContainer GrantedTags => _grantedTags;

        /// <summary>
        /// 적용 대상이 반드시 가지고 있어야 하는 태그입니다.
        /// </summary>
        public GameplayTagContainer ActivationRequiredTags => _activationRequiredTags;

        /// <summary>
        /// 적용 대상이 가지고 있으면 적용이 막히는 태그입니다.
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags => _activationBlockedTags;

        /// <summary>
        /// 부여되는 속성 수정자 목록입니다.
        /// </summary>
        public IReadOnlyList<AttributeModifier> Modifiers => _modifiers;
        
        private void OnValidate()
        {
            if (!_durationTypeInitialized)
            {
                // 첫 초기화 시에는 이전 값을 기록만 한다.
                _lastDurationType = _durationType;
                _durationTypeInitialized = true;
                return;
            }

            if (_lastDurationType != _durationType)
            {
                // 타입이 바뀌면 값 충돌을 피하려고 초기화한다.
                _duration = 0f;
                _period = 0f;
                _lastDurationType = _durationType;
            }
        }
    }
}

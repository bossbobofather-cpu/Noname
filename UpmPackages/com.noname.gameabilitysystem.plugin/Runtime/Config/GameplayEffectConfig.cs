using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayEffectConfig")]
    public sealed class GameplayEffectConfig : GameplayConfig
    {
        // 지속 타입
        [SerializeField] private EGameplayEffectDurationType _durationType = EGameplayEffectDurationType.Instant;

        // 지속 시간 
        [SerializeField] private float _duration = 0f;

        // 주기 시간
        [SerializeField] private float _period = 0f;

        // 부여되는 태그
        [SerializeField] private GameplayTagContainer _grantedTags = new GameplayTagContainer();
        [SerializeField, HideInInspector] private EGameplayEffectDurationType _lastDurationType;
        [SerializeField, HideInInspector] private bool _durationTypeInitialized;

        /// <summary>
        /// 지속 타입
        /// </summary>
        public EGameplayEffectDurationType DurationType => _durationType;

        /// <summary>
        /// 지속 시간 (_durationType이 HasDuration일때만 의미 있음)
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 주기 시간 (0이면 비발동) (_durationType이 HasDuration 또는 Infinite일때만 의미 있음)
        /// </summary>
        public float Period => _period;

        /// <summary>
        /// 부여되는 태그
        /// </summary>
        public GameplayTagContainer GrantedTags => _grantedTags;

        private void OnValidate()
        {
            if (!_durationTypeInitialized)
            {
                _lastDurationType = _durationType;
                _durationTypeInitialized = true;
                return;
            }

            if (_lastDurationType != _durationType)
            {
                _duration = 0f;
                _period = 0f;
                _lastDurationType = _durationType;
            }
        }
    }
}

using System;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame에서 공통으로 사용하는 GAS 기반 액터입니다.
    /// </summary>
    public abstract class MergeGameActorBase : MonoBehaviour, IAbilitySystemProvider
    {
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private AttributeId _healthAttribute = AttributeId.Health;

        private bool _isDead;

        /// <summary>
        /// 이 액터의 AbilitySystemComponent입니다.
        /// </summary>
        public AbilitySystemComponent AbilitySystem
        {
            get
            {
                if (_abilitySystem == null)
                {
                    _abilitySystem = GetComponentInChildren<AbilitySystemComponent>();
                }

                return _abilitySystem;
            }
        }

        /// <summary>
        /// 사망 상태 여부입니다.
        /// </summary>
        public bool IsDead => _isDead;

        /// <summary>
        /// 사망 처리 시 호출됩니다.
        /// </summary>
        public event Action<MergeGameActorBase> Died;

        /// <inheritdoc />
        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return AbilitySystem;
        }

        protected virtual void OnEnable()
        {
            if (AbilitySystem == null)
            {
                return;
            }

            // 능력 시스템에서 속성 변경 이벤트를 구독한다.
            AbilitySystem.onChangedAttributeModifier += HandleAttributeChanged;
        }

        protected virtual void OnDisable()
        {
            if (AbilitySystem == null)
            {
                return;
            }

            AbilitySystem.onChangedAttributeModifier -= HandleAttributeChanged;
        }

        private void HandleAttributeChanged(
            AbilitySystemComponent sender,
            AttributeModifier modifier,
            AttributeValue prevValue,
            AttributeValue currentValue)
        {
            if (_isDead)
            {
                return;
            }

            if (currentValue == null || currentValue.Definition == null)
            {
                return;
            }

            if (currentValue.Definition.Id != _healthAttribute)
            {
                return;
            }

            // 체력이 0 이하로 내려가면 사망 처리한다.
            if (currentValue.CurrentValue <= 0f)
            {
                HandleDeath();
            }
        }

        /// <summary>
        /// 사망 처리를 수행합니다.
        /// </summary>
        protected virtual void HandleDeath()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            Died?.Invoke(this);
        }
    }
}

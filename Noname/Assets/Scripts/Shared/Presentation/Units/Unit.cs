using System;
using UnityEngine;
using Noname.GameAbilitySystem;
using Common.Interface;

namespace MyProject.Common.Units
{
    /// <summary>
    /// 게임 내 유닛의 기본 클래스입니다.
    /// 이동(IMovement), 애니메이션 이벤트 수신(IAnimEventReceiver), 능력 시스템 제공(IAbilitySystemProvider) 인터페이스를 구현합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class Unit : MonoBehaviour, IMovement, IAnimEventReceiver, IAbilitySystemProvider
    {
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        
        /// <summary>
        /// 이 유닛이 소유한 AbilitySystemComponent입니다.
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

        [SerializeField] private Animator _animator;
        
        /// <summary>
        /// 이 유닛의 애니메이터 컴포넌트입니다.
        /// </summary>
        public Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponentInChildren<Animator>();
                }

                return _animator;
            }
        }

        /// <inheritdoc />
        public event Action<string> OnAnimEventReceived;

        /// <inheritdoc />
        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return AbilitySystem;
        }

        /// <inheritdoc />
        public void OnAnimationEventReceive(string eventData)
        {
            if (string.IsNullOrEmpty(eventData))
            {
                return;
            }

            OnAnimEventReceived?.Invoke(eventData);
        }
    }
}

using System;
using UnityEngine;
using Noname.GameAbilitySystem;
using Common.Interface;

namespace MyProject.Common.Units
{
    [DisallowMultipleComponent]
    public sealed partial class Unit : MonoBehaviour, IMovement, IAnimEventReceiver, IAbilitySystemProvider
    {
        [SerializeField] private AbilitySystemComponent _abilitySystem;
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

        public event Action<string> OnAnimEventReceived;

        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return AbilitySystem;
        }

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

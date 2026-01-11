using MergeGame.Config;
using MergeGame.Debug;
using MergeGame.Provider;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MergeGame.Unit
{
    [DisallowMultipleComponent]
    public sealed class Unit : MonoBehaviour, IAbilitySystemProvider, IDebugUtilObject
    {
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        public AbilitySystemComponent AbilitySystem
        {
            get
            {
                if (_abilitySystem == null)
                {
                    _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
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

        [SerializeField] private AnimationEventReceiver _eventReceiver;
        public AnimationEventReceiver EventReceiver
        {
            get
            {
                if (_eventReceiver == null)
                {
                    _eventReceiver = GetComponentInChildren<AnimationEventReceiver>();
                }

                return _eventReceiver;
            }
        }

        [SerializeField] private ILocomotionStateProvider _locomotionStateProvider;
        public ILocomotionStateProvider LocomotionStateProvider
        {
            get
            {
                if (_locomotionStateProvider == null)
                {
                    _locomotionStateProvider = GetComponentInParent<ILocomotionStateProvider>();
                }

                return _locomotionStateProvider;
            }
        }   

        //무기 발사 위치 트랜스폼
        [SerializeField] private Transform _muzzleTr;

        [Header("Locomotion Tags")]
        [SerializeField] private bool _applyLocomotionTags = true;
        [SerializeField] private FGameplayTag _groundedTag;
        [SerializeField] private FGameplayTag _jumpTag;
        [SerializeField] private FGameplayTag _moveTag;
        [SerializeField] private FGameplayTag _fallingTag;

        public Transform MuzzleTr
        {
            get
            {
                if (_muzzleTr != null)
                {
                    return _muzzleTr;
                }

                return transform;
            }
        }

        public LocomotionState CurrentState => LocomotionStateProvider?.CurrentState ?? default;


        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return AbilitySystem;
        }

        private void OnEnable()
        {
            if (EventReceiver != null)
            {
                EventReceiver.OnEventReceived += HandleAnimationEvent;
            }

            var locomotion = LocomotionStateProvider;
            if (locomotion != null)
            {
                locomotion.OnLocomotionStateChanged += HandleLocomotionStateChanged;
                ApplyLocomotionState(default, locomotion.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (EventReceiver != null)
            {
                EventReceiver.OnEventReceived -= HandleAnimationEvent;
            }

            var locomotion = LocomotionStateProvider;
            if (locomotion != null)
            {
                locomotion.OnLocomotionStateChanged -= HandleLocomotionStateChanged;
            }

            if (_applyLocomotionTags)
            {
                RevokeTag(_groundedTag);
                RevokeTag(_jumpTag);
                RevokeTag(_moveTag);
                RevokeTag(_fallingTag);
            }
        }

        private void HandleAnimationEvent(AnimationEventDataConfig eventData)
        {
            if (eventData == null)
            {
                return;
            }

            var abilitySystem = AbilitySystem;
            if (abilitySystem == null)
            {
                return;
            }

            foreach (var tag in eventData.GrantedTags.Tags)
            {
                abilitySystem.HandleGameplayEvent(new GameplayEventData(tag));
            }
        }

        public void ConfigureLocomotionTags(FGameplayTag grounded, FGameplayTag jump, FGameplayTag move, FGameplayTag falling)
        {
            _groundedTag = grounded;
            _jumpTag = jump;
            _moveTag = move;
            _fallingTag = falling;
            _applyLocomotionTags = true;

            if (isActiveAndEnabled && LocomotionStateProvider != null)
            {
                ApplyLocomotionState(default, LocomotionStateProvider.CurrentState);
            }
        }

        private void HandleLocomotionStateChanged(LocomotionState previous, LocomotionState next)
        {
            ApplyLocomotionState(previous, next);
        }

        private void ApplyLocomotionState(LocomotionState previous, LocomotionState next)
        {
            if (Animator != null)
            {
                Animator.SetBool("IsMoving", next.IsMoving);
                Animator.SetBool("IsJumping", next.IsJumping);
                Animator.SetBool("IsFalling", next.IsFalling);
            }

            if (_applyLocomotionTags)
            {
                ApplyLocomotionTags(previous, next);
            }
        }

        private void ApplyLocomotionTags(LocomotionState previous, LocomotionState current)
        {
            if (AbilitySystem == null)
            {
                return;
            }

            if (current.IsGrounded)
            {
                current.IsAirMoving = false;
                if (!current.IsJumping)
                {
                    current.IsFalling = false;
                }
            }

            UpdateTag(previous.IsGrounded, current.IsGrounded, _groundedTag);
            UpdateTag(previous.IsMoving, current.IsMoving, _moveTag);
            UpdateTag(previous.IsJumping, current.IsJumping, _jumpTag);
            UpdateTag(previous.IsFalling, current.IsFalling, _fallingTag);

            if (current.IsGrounded)
            {
                if (!current.IsJumping)
                {
                    RevokeTag(_jumpTag);
                }

                RevokeTag(_fallingTag);
            }
        }

        private void UpdateTag(bool wasActive, bool isActive, FGameplayTag tag)
        {
            if (wasActive == isActive)
            {
                return;
            }

            if (isActive)
            {
                AddLooseTag(tag);
            }
            else
            {
                RevokeTag(tag);
            }
        }

        private void AddLooseTag(FGameplayTag tag)
        {
            if (AbilitySystem == null || !tag.IsValid)
            {
                return;
            }

            AbilitySystem.AddLooseTag(tag);
        }

        private void RevokeTag(FGameplayTag tag)
        {
            if (AbilitySystem == null || !tag.IsValid)
            {
                return;
            }

            AbilitySystem.RemoveLooseTag(tag);
        }
    }
}

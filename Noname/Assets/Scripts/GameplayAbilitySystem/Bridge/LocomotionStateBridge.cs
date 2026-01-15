using MyProject.Common.Units;
using MyProject.Common.Units.Locomotion;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.GameplayAbilitySystem.Bridge
{
    [DisallowMultipleComponent]
    public sealed class LocomotionStateBridge : MonoBehaviour
    {
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private ILocomotionStateProvider _stateProvider;
        [SerializeField] private Animator _animator;

        [Header("State Tags")]
        [SerializeField] private FGameplayTag _groundedTag;
        [SerializeField] private FGameplayTag _jumpTag;
        [SerializeField] private FGameplayTag _moveTag;
        [SerializeField] private FGameplayTag _fallingTag;

        private void Awake()
        {
            _stateProvider = GetComponent<ILocomotionStateProvider>();

            //Bridge인데 ILocomotionStateProvider 없으면 의미가 없으니 자동 추가
            if (_stateProvider == null)
            {
                Debug.LogError($"[ILocomotionStateProvider] Missing on {gameObject.name}.");
                enabled = false;
                return;
            }

            if (_abilitySystem == null)
            {
                _abilitySystem = GetComponent<AbilitySystemComponent>();
            }

            //Bridge인데 AbilitySystemComponent가 없으면 의미가 없으니 자동 추가
            if (_abilitySystem == null)
            {
                _abilitySystem = gameObject.AddComponent<AbilitySystemComponent>();
                Debug.LogWarning($"ASC가 없어서 추가되었습니다. {gameObject.name}.");
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        private void OnEnable()
        {
            if (_stateProvider != null)
            {
                _stateProvider.OnLocomotionStateChanged += HandleLocomotionStateChanged;
                ApplyLocomotionState(default, _stateProvider.CurrentState);
                ApplyLocomotionTags(default, _stateProvider.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (_stateProvider != null)
            {
                _stateProvider.OnLocomotionStateChanged -= HandleLocomotionStateChanged;
            }

            RevokeTag(_groundedTag);
            RevokeTag(_jumpTag);
            RevokeTag(_moveTag);
            RevokeTag(_fallingTag);
        }

        private void HandleLocomotionStateChanged(LocomotionState previous, LocomotionState current)
        {
            ApplyLocomotionState(previous, current);
            ApplyLocomotionTags(previous, current);
        }

        private void ApplyLocomotionState(LocomotionState previous, LocomotionState next)
        {
            if (_animator != null)
            {
                _animator.SetBool("IsMoving", next.IsMoving);
                _animator.SetBool("IsJumping", next.IsJumping);
                _animator.SetBool("IsFalling", next.IsFalling);
            }
        }

        private void ApplyLocomotionTags(LocomotionState previous, LocomotionState current)
        {
            if (_abilitySystem == null)
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
            if (_abilitySystem == null || !tag.IsValid) return;
            _abilitySystem.AddLooseTag(tag);
        }

        private void RevokeTag(FGameplayTag tag)
        {
            if (_abilitySystem == null || !tag.IsValid) return;
            _abilitySystem.RemoveLooseTag(tag);
        }
    }
}

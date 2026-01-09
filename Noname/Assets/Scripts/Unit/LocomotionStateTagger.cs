using MergeGame.Provider;
using MergeGame.Unit;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Platformer.GameAbilitySystem
{
    [DisallowMultipleComponent]
    public sealed class LocomotionStateTagger : MonoBehaviour
    {
        [SerializeField] private Unit _unit;
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private ILocomotionStateProvider _stateProvider;

        [Header("State Tags")]
        [SerializeField] private FGameplayTag _groundedTag;
        [SerializeField] private FGameplayTag _jumpTag;
        [SerializeField] private FGameplayTag _moveTag;
        [SerializeField] private FGameplayTag _fallingTag;

        private bool _delegateToUnit;

        private void Awake()
        {
            if (_unit == null)
            {
                _unit = GetComponent<Unit>() ?? GetComponentInParent<Unit>();
            }

            if (_unit != null)
            {
                _unit.ConfigureLocomotionTags(_groundedTag, _jumpTag, _moveTag, _fallingTag);
                _delegateToUnit = true;
                return;
            }

            var ascInterface = GetComponent<IAbilitySystemProvider>() ?? GetComponentInParent<IAbilitySystemProvider>();
            if (ascInterface != null)
            {
                _abilitySystem = ascInterface.GetAbilitySystemComponent();
            }
            
            _stateProvider = GetComponent<ILocomotionStateProvider>() ?? GetComponentInParent<ILocomotionStateProvider>();
        }

        private void OnEnable()
        {
            if (_delegateToUnit)
            {
                return;
            }

            if (_stateProvider != null)
            {
                _stateProvider.OnLocomotionStateChanged += HandleLocomotionStateChanged;
                ApplyState(default, _stateProvider.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (_delegateToUnit)
            {
                return;
            }

            if (_stateProvider != null)
            {
                _stateProvider.OnLocomotionStateChanged -= HandleLocomotionStateChanged;
            }

            if (_abilitySystem != null)
            {
                RevokeTag(_groundedTag);
                RevokeTag(_jumpTag);
                RevokeTag(_moveTag);
                RevokeTag(_fallingTag);
            }
        }

        private void HandleLocomotionStateChanged(LocomotionState previous, LocomotionState current)
        {
            ApplyState(previous, current);
        }

        private void ApplyState(LocomotionState previous, LocomotionState current)
        {
            if (current.IsGrounded)
            {
                current.IsJumping = false;
                current.IsAirMoving = false;
                current.IsFalling = false;
            }

            UpdateTag(previous.IsGrounded, current.IsGrounded, _groundedTag);
            UpdateTag(previous.IsMoving, current.IsMoving, _moveTag);
            UpdateTag(previous.IsJumping, current.IsJumping, _jumpTag);
            UpdateTag(previous.IsFalling, current.IsFalling, _fallingTag);

            if (current.IsGrounded)
            {
                RevokeTag(_jumpTag);
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

using MyProject.Common.Units;
using MyProject.Common.Units.Locomotion;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.GameplayAbilitySystem.Bridge
{
    /// <summary>
    /// 유닛의 이동 상태(LocomotionState)를 ASC 태그 및 애니메이터 파라미터와 동기화하는 브리지 컴포넌트입니다.
    /// </summary>
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

            // 필수 인터페이스 부재 시 비활성화
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

            // ASC 자동 추가
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
                // 초기 상태 적용
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

            // 비활성화 시 관련 태그 제거
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

            // 지면에 닿아있는 상태에서는 공중 이동 및 낙하 상태를 보정
            if (current.IsGrounded)
            {
                current.IsAirMoving = false;
                if (!current.IsJumping)
                {
                    current.IsFalling = false;
                }
            }

            // 상태 변경에 따라 태그 업데이트 (추가/제거)
            UpdateTag(previous.IsGrounded, current.IsGrounded, _groundedTag);
            UpdateTag(previous.IsMoving, current.IsMoving, _moveTag);
            UpdateTag(previous.IsJumping, current.IsJumping, _jumpTag);
            UpdateTag(previous.IsFalling, current.IsFalling, _fallingTag);

            // 상태 충돌 방지를 위한 안전 장치
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

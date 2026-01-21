using System;
using Common.Interface;
using MyProject.Common.Units;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.GameplayAbilitySystem.Bridge
{
    /// <summary>
    /// ASC의 속성 및 태그 변화를 유닛의 이동 컴포넌트(IMovement)에 반영하는 브리지입니다.
    /// 이동 속도, 점프력 변경 및 이동 차단 상태 등을 동기화합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class MovementBridge : MonoBehaviour
    {
        [SerializeField] private IMovement _movement;
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        
        /// <summary>
        /// 이동 속도 속성 ID입니다.
        /// </summary>
        [SerializeField] private AttributeId _moveSpeedAttributeId = AttributeId.MoveSpeed;
        
        /// <summary>
        /// 점프력 속성 ID입니다.
        /// </summary>
        [SerializeField] private AttributeId _jumpSpeedAttributeId = AttributeId.JumpSpeed;
        
        /// <summary>
        /// 이동 차단 시 적용될 태그입니다.
        /// </summary>
        [SerializeField] private FGameplayTag _blockMoveTag = new FGameplayTag("Block.Move");

        private void Awake()
        {
            if (_movement == null)
            {
                _movement = GetComponent<IMovement>();
            }

            // Bridge인데 IMovement가 없으면 기능 수행 불가
            if (_movement == null)
            {
                Debug.LogError($"[IMovement] Missing on {gameObject.name}.");
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

            // 초기 속성 값 적용
            if (TryGetAttributeValue(_moveSpeedAttributeId, out var moveSpeed))
            {
                SetUnitMovementMoveSpeed(moveSpeed);
            }

            if (TryGetAttributeValue(_jumpSpeedAttributeId, out var jumpSpeed))
            {
                SetUnitMovementJumpSpeed(jumpSpeed);
            }

            SetUnitMoveBlocked(IsMoveBlocked());
        }

        private void OnEnable()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.onChangedAttributeModifier += OnChangedAttributeModifier;
                _abilitySystem.onAddedTag += OnAddedTag;
                _abilitySystem.onRemovedTag += OnRemovedTag;
            }
        }

        private void OnDisable()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.onChangedAttributeModifier -= OnChangedAttributeModifier;
                _abilitySystem.onAddedTag -= OnAddedTag;
                _abilitySystem.onRemovedTag -= OnRemovedTag;
            }
        }

        /// <summary>
        /// 속성 수정자 변경 시 호출됩니다. 이동/점프 속도를 갱신합니다.
        /// </summary>
        private void OnChangedAttributeModifier(AbilitySystemComponent component, AttributeModifier modifier, AttributeValue prevValue, AttributeValue value)
        {
            if (modifier.Attribute == null || value == null)
            {
                return;
            }

            var id = modifier.Attribute.Id;
            if (id == _moveSpeedAttributeId)
            {
                SetUnitMovementMoveSpeed(value.CurrentValue);
            }
            else if (id == _jumpSpeedAttributeId)
            {
                SetUnitMovementJumpSpeed(value.CurrentValue);
            }
        }

        private void OnAddedTag(AbilitySystemComponent component, FGameplayTag tag)
        {
            if (tag.Equals(_blockMoveTag))
            {
                SetUnitMoveBlocked(true);
            }
        }

        private void OnRemovedTag(AbilitySystemComponent component, FGameplayTag tag)
        {
            if (tag.Equals(_blockMoveTag))
            {
                SetUnitMoveBlocked(false);
            }
        }

        private void SetUnitMovementMoveSpeed(float moveSpeed)
        {
            if (_movement == null)
            {
                return;
            }

            _movement.SetMoveSpeed(moveSpeed);
        }

        private void SetUnitMovementJumpSpeed(float jumpSpeed)
        {
            if (_movement == null)
            {
                return;
            }

            _movement.SetJumpSpeed(jumpSpeed);
        }

        private void SetUnitMoveBlocked(bool isBlocked)
        {
            if (_movement == null)
            {
                return;
            }

            _movement.SetMoveBlocked(isBlocked);
        }

        private bool TryGetAttributeValue(AttributeId id, out float value)
        {
            value = 0f;
            if (_abilitySystem == null)
            {
                _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
            }

            if (_abilitySystem == null || !_abilitySystem.Attributes.TryGet(id, out var attribute))
            {
                return false;
            }

            value = attribute.CurrentValue;
            return true;
        }

        private bool IsMoveBlocked()
        {
            if (!_blockMoveTag.IsValid)
            {
                return false;
            }

            if (_abilitySystem == null)
            {
                _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
            }

            var tags = _abilitySystem != null ? _abilitySystem.OwnedTags : null;
            return tags != null && tags.HasTag(_blockMoveTag);
        }
    }
}

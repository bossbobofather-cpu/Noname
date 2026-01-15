using System;
using Common.Interface;
using MyProject.Common.Units;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.GameplayAbilitySystem.Bridge
{
    /// <summary>
    /// Unit Movement 관련 기능과 Ability System Component 간의 브리지 역할을 하는 클래스
    /// </summary>
    [DisallowMultipleComponent]
    public class MovementBridge : MonoBehaviour
    {
        [SerializeField] private IMovement _movement;
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private AttributeId _moveSpeedAttributeId = AttributeId.MoveSpeed;
        [SerializeField] private AttributeId _jumpSpeedAttributeId = AttributeId.JumpSpeed;
        [SerializeField] private FGameplayTag _blockMoveTag = new FGameplayTag("Block.Move");

        private void Awake()
        {
            if (_movement == null)
            {
                _movement = GetComponent<IMovement>();
            }

            //Bridge인데 IMovement 없으면 의미가 없으니 자동 추가
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

            //Bridge인데 AbilitySystemComponent가 없으면 의미가 없으니 자동 추가
            if (_abilitySystem == null)
            {
                _abilitySystem = gameObject.AddComponent<AbilitySystemComponent>();
                Debug.LogWarning($"ASC가 없어서 추가되었습니다. {gameObject.name}.");
            }

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

        //MoveSpeed, JumpSpeed 속성 변경시 호출되는 콜백
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

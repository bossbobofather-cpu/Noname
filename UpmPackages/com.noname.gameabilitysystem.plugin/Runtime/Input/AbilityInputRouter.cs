using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Platformer.GameAbilitySystem
{
    /// <summary>
    /// 입력을 어빌리티 이벤트로 전달하는 라우터입니다.
    /// </summary>
    public sealed class AbilityInputRouter : MonoBehaviour
    {
        [SerializeField] private AbilityInputMap _inputMap;
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private bool _autoEnableActions = true;

        private readonly List<RuntimeBinding> _runtimeBindings = new();

        private void Awake()
        {
            if (_abilitySystem == null)
            {
                // 부모에서 능력 시스템 컴포넌트를 찾는다.
                _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
            }

            // 입력 바인딩을 구성한다.
            BuildRuntimeBindings();
        }

        private void OnEnable()
        {
            if (_autoEnableActions)
            {
                // 자동 활성화 옵션이 켜져 있으면 활성화한다.
                SetActionsEnabled(true);
            }
        }

        private void OnDisable()
        {
            if (_autoEnableActions)
            {
                // 비활성화 시 입력을 끈다.
                SetActionsEnabled(false);
            }
        }

        private void Update()
        {
            if (_abilitySystem == null || _runtimeBindings.Count == 0)
            {
                // 준비되지 않았으면 처리하지 않는다.
                return;
            }

            var pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            for (var i = 0; i < _runtimeBindings.Count; i++)
            {
                var binding = _runtimeBindings[i];
                if (binding.Action == null)
                {
                    continue;
                }

                var shouldTrigger = binding.Trigger switch
                {
                    AbilityInputTrigger.Pressed => binding.Action.WasPressedThisFrame(),
                    AbilityInputTrigger.Released => binding.Action.WasReleasedThisFrame(),
                    AbilityInputTrigger.Performed => binding.Action.triggered,
                    _ => false
                };

                if (shouldTrigger)
                {
                    if (pointerOverUi && IsLeftClickAction(binding.Action))
                    {
                        // UI 위에서의 좌클릭은 막는다.
                        continue;
                    }

                    var eventData = new GameplayEventData
                    {
                        EventTag = binding.EventTag,
                        Payload = binding.Action.ReadValueAsObject()
                    };

                    // 입력 이벤트를 어빌리티 시스템으로 전달한다.
                    _abilitySystem.HandleGameplayEvent(eventData);
                }
            }
        }

        private static bool IsLeftClickAction(InputAction action)
        {
            if (action == null)
            {
                return false;
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return false;
            }

            var active = action.activeControl;
            if (active != null)
            {
                // 현재 활성 컨트롤이 왼쪽 버튼인지 확인한다.
                return active == mouse.leftButton
                    || (active.device is Mouse && string.Equals(active.name, "leftButton", StringComparison.Ordinal));
            }

            foreach (var control in action.controls)
            {
                if (control == mouse.leftButton)
                {
                    return true;
                }

                if (control.device is Mouse && string.Equals(control.name, "leftButton", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void BuildRuntimeBindings()
        {
            // 이전 바인딩을 초기화한다.
            _runtimeBindings.Clear();

            if (_inputMap == null)
            {
                return;
            }

            var asset = _actions != null ? _actions : InputSystem.actions;
            var bindings = _inputMap.Bindings;
            for (var i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding == null)
                {
                    continue;
                }

                var action = binding.ResolveAction(asset);
                if (action == null)
                {
                    Debug.LogWarning($"입력 액션을 찾을 수 없습니다: {binding}", this);
                    continue;
                }

                var eventTag = binding.EventTag;
                if (!eventTag.IsValid)
                {
                    Debug.LogWarning($"이벤트 태그가 유효하지 않습니다: {binding}", this);
                    continue;
                }

                // 유효한 바인딩만 등록한다.
                _runtimeBindings.Add(new RuntimeBinding(action, eventTag, binding.Trigger));
            }
        }

        private void SetActionsEnabled(bool enabled)
        {
            for (var i = 0; i < _runtimeBindings.Count; i++)
            {
                var action = _runtimeBindings[i].Action;
                if (action == null)
                {
                    continue;
                }

                if (enabled)
                {
                    // 입력을 활성화한다.
                    action.Enable();
                }
                else
                {
                    // 입력을 비활성화한다.
                    action.Disable();
                }
            }
        }


        private readonly struct RuntimeBinding
        {
            public RuntimeBinding(InputAction action, FGameplayTag eventTag, AbilityInputTrigger trigger)
            {
                // 바인딩 정보를 저장한다.
                Action = action;
                EventTag = eventTag;
                Trigger = trigger;
            }

            public InputAction Action { get; }
            public FGameplayTag EventTag { get; }
            public AbilityInputTrigger Trigger { get; }
        }
    }
}

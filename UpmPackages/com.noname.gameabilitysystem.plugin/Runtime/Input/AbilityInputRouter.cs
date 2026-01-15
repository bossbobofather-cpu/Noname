using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Platformer.GameAbilitySystem
{
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
                _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
            }

            BuildRuntimeBindings();
        }

        private void OnEnable()
        {
            if (_autoEnableActions)
            {
                SetActionsEnabled(true);
            }
        }

        private void OnDisable()
        {
            if (_autoEnableActions)
            {
                SetActionsEnabled(false);
            }
        }

        private void Update()
        {
            if (_abilitySystem == null || _runtimeBindings.Count == 0)
            {
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
                        continue;
                    }

                    var eventData = new GameplayEventData
                    {
                        EventTag = binding.EventTag,
                        Payload = binding.Action.ReadValueAsObject()
                    };

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
                    action.Enable();
                }
                else
                {
                    action.Disable();
                }
            }
        }


        private readonly struct RuntimeBinding
        {
            public RuntimeBinding(InputAction action, FGameplayTag eventTag, AbilityInputTrigger trigger)
            {
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

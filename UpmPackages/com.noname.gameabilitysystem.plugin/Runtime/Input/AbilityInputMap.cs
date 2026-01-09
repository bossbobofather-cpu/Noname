using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Noname.GameAbilitySystem
{
    public enum AbilityInputTrigger
    {
        Pressed,
        Released,
        Performed,
    }

    [Serializable]
    public sealed class AbilityInputBinding
    {
        [SerializeField] private InputActionReference _action;
        [SerializeField] private string _actionName;
        [SerializeField] private FGameplayTag _eventTag;
        [SerializeField] private AbilityInputTrigger _trigger = AbilityInputTrigger.Pressed;

        public FGameplayTag EventTag => _eventTag;
        public AbilityInputTrigger Trigger => _trigger;

        public InputAction ResolveAction(InputActionAsset asset)
        {
            if (_action != null)
            {
                return _action.action;
            }

            if (string.IsNullOrWhiteSpace(_actionName) || asset == null)
            {
                return null;
            }

            return asset.FindAction(_actionName, false);
        }
    }

    [CreateAssetMenu(menuName = "GameAbilitySystem/Input/AbilityInputMap")]
    public sealed class AbilityInputMap : ScriptableObject
    {
        [SerializeField] private List<AbilityInputBinding> _bindings = new();

        public IReadOnlyList<AbilityInputBinding> Bindings => _bindings;
    }
}

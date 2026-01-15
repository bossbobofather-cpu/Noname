using System;
using System.Collections;
using UnityEngine.InputSystem;

namespace Noname.GameAbilitySystem
{
    public sealed class AbilityTask_WaitInputPress : AbilityTask
    {
        private InputAction _action;

        public event Action<InputAction> Pressed;

        public static AbilityTask_WaitInputPress WaitInputPress(
            IAbilityTaskOwner owner,
            InputAction action)
        {
            var task = new AbilityTask_WaitInputPress
            {
                _action = action
            };

            task.Initialize(owner);
            task.Activate();
            return task;
        }

        protected override void OnActivate()
        {
            if (_action == null)
            {
                EndTask();
                return;
            }

            StartRoutine(WaitRoutine());
        }

        private IEnumerator WaitRoutine()
        {
            while (IsActive && _action != null && !_action.WasPressedThisFrame())
            {
                yield return null;
            }

            if (!IsActive || _action == null)
            {
                yield break;
            }

            Pressed?.Invoke(_action);
            EndTask();
        }
    }
}

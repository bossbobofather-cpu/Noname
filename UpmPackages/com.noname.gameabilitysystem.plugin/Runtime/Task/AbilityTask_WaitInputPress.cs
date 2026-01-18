using System;
using System.Collections;
using UnityEngine.InputSystem;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 입력 눌림을 기다리는 태스크입니다.
    /// </summary>
    public sealed class AbilityTask_WaitInputPress : AbilityTask
    {
        private InputAction _action;

        /// <summary>
        /// 입력이 눌리면 호출됩니다.
        /// </summary>
        public event Action<InputAction> Pressed;

        /// <summary>
        /// 입력 대기 태스크를 생성하고 활성화합니다.
        /// </summary>
        /// <param name="owner">태스크 소유자</param>
        /// <param name="action">대기할 입력</param>
        /// <returns>생성된 태스크</returns>
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
                // 입력이 없으면 종료한다.
                EndTask();
                return;
            }

            StartRoutine(WaitRoutine());
        }

        private IEnumerator WaitRoutine()
        {
            // 눌림 순간까지 대기한다.
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

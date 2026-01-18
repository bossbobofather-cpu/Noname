using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 실행 중인 어빌리티 인스턴스를 관리합니다.
    /// </summary>
    public sealed class GameplayAbilityInstance : IAbilityTaskOwner
    {
        private readonly List<AbilityTask> _tasks = new();
        private readonly GameplayAbility _ability;

        /// <summary>
        /// 어빌리티 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="asc">능력 시스템 컴포넌트</param>
        /// <param name="ability">능력 객체</param>
        /// <param name="context">실행 컨텍스트</param>
        public GameplayAbilityInstance(
            AbilitySystemComponent asc,
            GameplayAbility ability,
            AbilityContext context)
        {
            // 필요한 참조를 먼저 보관한다.
            ASC = asc;
            _ability = ability;
            Context = context;
            Handle = context.Handle;

            // 태스크 소유자를 바인딩한다.
            _ability?.BindTaskOwner(this);
        }

        /// <summary>
        /// 능력 시스템 컴포넌트입니다.
        /// </summary>
        public AbilitySystemComponent ASC { get; }

        /// <summary>
        /// 현재 실행 컨텍스트입니다.
        /// </summary>
        public AbilityContext Context { get; private set; }

        /// <summary>
        /// 실행 중인 능력 핸들입니다.
        /// </summary>
        public FGameplayAbilitySpecHandle Handle { get; }

        /// <summary>
        /// 어빌리티를 활성화합니다.
        /// </summary>
        public void Activate()
        {
            if (_ability == null)
            {
                // 능력이 없으면 아무것도 하지 않는다.
                return;
            }

            _ability.CallActivateAbility(Context);
        }

        /// <summary>
        /// 어빌리티를 종료합니다.
        /// </summary>
        public void End()
        {
            if (_ability == null)
            {
                // 능력이 없더라도 태스크는 정리한다.
                CancelTasks();
                return;
            }

            _ability.EndAbility(Handle);
            CancelTasks();
        }

        /// <summary>
        /// 어빌리티를 취소합니다.
        /// </summary>
        public void Cancel()
        {
            if (_ability != null)
            {
                _ability.CancelAbility(Handle);
            }

            CancelTasks();
        }

        /// <summary>
        /// 코루틴을 시작합니다.
        /// </summary>
        /// <param name="routine">실행할 루틴</param>
        /// <returns>코루틴 핸들</returns>
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (ASC == null || routine == null)
            {
                // 실행 조건이 맞지 않으면 중단한다.
                return null;
            }

            return ASC.StartCoroutine(routine);
        }

        /// <summary>
        /// 코루틴을 중단합니다.
        /// </summary>
        /// <param name="routine">중단할 코루틴</param>
        public void StopCoroutine(Coroutine routine)
        {
            if (ASC == null || routine == null)
            {
                // 잘못된 요청은 무시한다.
                return;
            }

            ASC.StopCoroutine(routine);
        }

        /// <summary>
        /// 태스크를 등록합니다.
        /// </summary>
        /// <param name="task">등록할 태스크</param>
        public void RegisterTask(AbilityTask task)
        {
            if (task == null)
            {
                // 비어 있는 태스크는 무시한다.
                return;
            }

            if (!_tasks.Contains(task))
            {
                _tasks.Add(task);
            }
        }

        /// <summary>
        /// 태스크 등록을 해제합니다.
        /// </summary>
        /// <param name="task">해제할 태스크</param>
        public void UnregisterTask(AbilityTask task)
        {
            if (task == null)
            {
                // 비어 있는 태스크는 무시한다.
                return;
            }

            _tasks.Remove(task);
        }

        /// <summary>
        /// 실행 컨텍스트를 갱신합니다.
        /// </summary>
        /// <param name="context">새 컨텍스트</param>
        public void UpdateContext(AbilityContext context)
        {
            // 실행 중인 컨텍스트를 교체한다.
            Context = context;
        }

        private void CancelTasks()
        {
            if (_tasks.Count == 0)
            {
                return;
            }

            // 순회 도중 변경을 피하려고 스냅샷을 만든다.
            var snapshot = _tasks.ToArray();
            _tasks.Clear();

            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i]?.Cancel();
            }
        }
    }
}

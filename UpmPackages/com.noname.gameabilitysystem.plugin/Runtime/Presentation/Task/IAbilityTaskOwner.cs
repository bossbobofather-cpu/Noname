using System.Collections;
using Noname.GameAbilitySystem.Domain;
using UnityEngine;

namespace Noname.GameAbilitySystem.Presentation
{
    /// <summary>
    /// 어빌리티 태스크 소유자 인터페이스입니다.
    /// </summary>
    public interface IAbilityTaskOwner
    {
        /// <summary>
        /// 능력 시스템 컴포넌트입니다.
        /// </summary>
        AbilitySystemComponentAdapter ASC { get; }

        /// <summary>
        /// 실행 컨텍스트입니다.
        /// </summary>
        AbilityContext Context { get; }

        /// <summary>
        /// 실행 중인 능력 핸들입니다.
        /// </summary>
        FGameplayAbilitySpecHandle Handle { get; }

        /// <summary>
        /// 코루틴을 시작합니다.
        /// </summary>
        /// <param name="routine">실행할 루틴</param>
        /// <returns>코루틴 핸들</returns>
        Coroutine StartCoroutine(IEnumerator routine);

        /// <summary>
        /// 코루틴을 중단합니다.
        /// </summary>
        /// <param name="routine">중단할 코루틴</param>
        void StopCoroutine(Coroutine routine);

        /// <summary>
        /// 태스크를 등록합니다.
        /// </summary>
        /// <param name="task">등록할 태스크</param>
        void RegisterTask(AbilityTask task);

        /// <summary>
        /// 태스크 등록을 해제합니다.
        /// </summary>
        /// <param name="task">해제할 태스크</param>
        void UnregisterTask(AbilityTask task);

        /// <summary>
        /// 실행 컨텍스트를 갱신합니다.
        /// </summary>
        /// <param name="context">새 컨텍스트</param>
        void UpdateContext(AbilityContext context);
    }
}

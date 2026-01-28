using System;
using System.Collections;
using Noname.GameAbilitySystem.Domain;
using UnityEngine;

namespace Noname.GameAbilitySystem.Presentation
{
    /// <summary>
    /// 어빌리티에서 사용하는 태스크의 기본 클래스입니다.
    /// </summary>
    public abstract class AbilityTask
    {
        private Coroutine _routine;
        private bool _initialized;

        /// <summary>
        /// 태스크 소유자입니다.
        /// </summary>
        public IAbilityTaskOwner Owner { get; private set; }

        /// <summary>
        /// 능력 시스템 컴포넌트입니다.
        /// </summary>
        public AbilitySystemComponentAdapter ASC => Owner?.ASC;

        /// <summary>
        /// 실행 컨텍스트입니다.
        /// </summary>
        public AbilityContext Context { get; private set; }

        /// <summary>
        /// 실행 중인 능력 핸들입니다.
        /// </summary>
        public FGameplayAbilitySpecHandle Handle { get; private set; }

        /// <summary>
        /// 활성화 상태 여부입니다.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// 종료 상태 여부입니다.
        /// </summary>
        public bool IsEnded { get; private set; }

        /// <summary>
        /// 태스크 완료 시 호출됩니다.
        /// </summary>
        public event Action Completed;

        /// <summary>
        /// 태스크 취소 시 호출됩니다.
        /// </summary>
        public event Action Cancelled;

        /// <summary>
        /// 태스크를 초기화합니다.
        /// </summary>
        /// <param name="owner">태스크 소유자</param>
        public void Initialize(IAbilityTaskOwner owner)
        {
            if (_initialized)
            {
                // 이미 초기화된 경우 재호출을 막는다.
                return;
            }

            if (owner == null)
            {
                Debug.LogWarning("AbilityTask requires an owner.");
                return;
            }

            _initialized = true;
            Owner = owner;
            Context = owner.Context;
            Handle = owner.Handle;
            Owner.RegisterTask(this);
        }

        /// <summary>
        /// 태스크를 활성화합니다.
        /// </summary>
        public void Activate()
        {
            if (!_initialized || IsActive || IsEnded)
            {
                // 초기화 전이거나 이미 활성/종료 상태면 중단한다.
                return;
            }

            IsActive = true;
            OnActivate();
        }

        /// <summary>
        /// 태스크를 종료합니다.
        /// </summary>
        public void EndTask()
        {
            if (IsEnded)
            {
                return;
            }

            IsActive = false;
            IsEnded = true;
            StopRoutine();
            OnEnd();
            Owner?.UnregisterTask(this);
            Completed?.Invoke();
        }

        /// <summary>
        /// 태스크를 취소합니다.
        /// </summary>
        public void Cancel()
        {
            if (IsEnded)
            {
                return;
            }

            IsActive = false;
            StopRoutine();
            OnCancel();
            IsEnded = true;
            Owner?.UnregisterTask(this);
            Cancelled?.Invoke();
        }

        /// <summary>
        /// 활성화 시점 훅입니다.
        /// </summary>
        protected virtual void OnActivate()
        {
            // 필요 시 파생 클래스에서 구현한다.
        }

        /// <summary>
        /// 종료 시점 훅입니다.
        /// </summary>
        protected virtual void OnEnd()
        {
            // 필요 시 파생 클래스에서 구현한다.
        }

        /// <summary>
        /// 취소 시점 훅입니다.
        /// </summary>
        protected virtual void OnCancel()
        {
            // 필요 시 파생 클래스에서 구현한다.
        }

        /// <summary>
        /// 코루틴을 시작합니다.
        /// </summary>
        /// <param name="routine">실행할 루틴</param>
        /// <returns>코루틴 핸들</returns>
        protected Coroutine StartRoutine(IEnumerator routine)
        {
            if (Owner == null || routine == null)
            {
                return null;
            }

            _routine = Owner.StartCoroutine(routine);
            return _routine;
        }

        /// <summary>
        /// 실행 중인 코루틴을 중단합니다.
        /// </summary>
        protected void StopRoutine()
        {
            if (_routine == null || Owner == null)
            {
                return;
            }

            Owner.StopCoroutine(_routine);
            _routine = null;
        }

        /// <summary>
        /// 컨텍스트의 타겟 데이터를 갱신합니다.
        /// </summary>
        /// <param name="targetData">새 타겟 데이터</param>
        protected void UpdateContextTargetData(AbilityTargetData targetData)
        {
            if (Owner == null || Handle == FGameplayAbilitySpecHandle.Invalid)
            {
                return;
            }

            // 컨텍스트의 타겟만 교체한다.
            Context = Context.WithTargetData(targetData);
            Owner.UpdateContext(Context);
        }
    }
}

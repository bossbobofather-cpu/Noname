using System;
using System.Collections;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public abstract class AbilityTask
    {
        private Coroutine _routine;
        private bool _initialized;

        public IAbilityTaskOwner Owner { get; private set; }
        public AbilitySystemComponent ASC => Owner?.ASC;
        public AbilityContext Context { get; private set; }
        public FGameplayAbilitySpecHandle Handle { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsEnded { get; private set; }

        public event Action Completed;
        public event Action Cancelled;

        public void Initialize(IAbilityTaskOwner owner)
        {
            if (_initialized)
            {
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

        public void Activate()
        {
            if (!_initialized || IsActive || IsEnded)
            {
                return;
            }

            IsActive = true;
            OnActivate();
        }

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

        protected virtual void OnActivate()
        {
        }

        protected virtual void OnEnd()
        {
        }

        protected virtual void OnCancel()
        {
        }

        protected Coroutine StartRoutine(IEnumerator routine)
        {
            if (Owner == null || routine == null)
            {
                return null;
            }

            _routine = Owner.StartCoroutine(routine);
            return _routine;
        }

        protected void StopRoutine()
        {
            if (_routine == null || Owner == null)
            {
                return;
            }

            Owner.StopCoroutine(_routine);
            _routine = null;
        }

        protected void UpdateContextTargetData(AbilityTargetData targetData)
        {
            if (Owner == null || Handle == FGameplayAbilitySpecHandle.Invalid)
            {
                return;
            }

            Context = Context.WithTargetData(targetData);
            Owner.UpdateContext(Context);
        }
    }
}

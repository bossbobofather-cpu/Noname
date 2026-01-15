using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public sealed class GameplayAbilityInstance : IAbilityTaskOwner
    {
        private readonly List<AbilityTask> _tasks = new();
        private readonly GameplayAbility _ability;

        public GameplayAbilityInstance(
            AbilitySystemComponent asc,
            GameplayAbility ability,
            AbilityContext context)
        {
            ASC = asc;
            _ability = ability;
            Context = context;
            Handle = context.Handle;

            _ability?.BindTaskOwner(this);
        }

        public AbilitySystemComponent ASC { get; }
        public AbilityContext Context { get; private set; }
        public FGameplayAbilitySpecHandle Handle { get; }

        public void Activate()
        {
            if (_ability == null)
            {
                return;
            }

            _ability.CallActivateAbility(Context);
        }

        public void End()
        {
            if (_ability == null)
            {
                CancelTasks();
                return;
            }

            _ability.EndAbility(Handle);
            CancelTasks();
        }

        public void Cancel()
        {
            if (_ability != null)
            {
                _ability.CancelAbility(Handle);
            }

            CancelTasks();
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (ASC == null || routine == null)
            {
                return null;
            }

            return ASC.StartCoroutine(routine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            if (ASC == null || routine == null)
            {
                return;
            }

            ASC.StopCoroutine(routine);
        }

        public void RegisterTask(AbilityTask task)
        {
            if (task == null)
            {
                return;
            }

            if (!_tasks.Contains(task))
            {
                _tasks.Add(task);
            }
        }

        public void UnregisterTask(AbilityTask task)
        {
            if (task == null)
            {
                return;
            }

            _tasks.Remove(task);
        }

        public void UpdateContext(AbilityContext context)
        {
            Context = context;
        }

        private void CancelTasks()
        {
            if (_tasks.Count == 0)
            {
                return;
            }

            var snapshot = _tasks.ToArray();
            _tasks.Clear();

            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i]?.Cancel();
            }
        }
    }
}

using System;

namespace Noname.GameAbilitySystem
{
    public sealed class AbilityTask_WaitGameplayEvent : AbilityTask
    {
        private FGameplayTag _triggerTag;
        private bool _triggerOnce;
        private bool _exactMatchOnly;

        public event Action<GameplayEventData> EventReceived;

        public static AbilityTask_WaitGameplayEvent WaitGameplayEvent(
            IAbilityTaskOwner owner,
            FGameplayTag triggerTag,
            bool triggerOnce = true,
            bool exactMatchOnly = false)
        {
            var task = new AbilityTask_WaitGameplayEvent
            {
                _triggerTag = triggerTag,
                _triggerOnce = triggerOnce,
                _exactMatchOnly = exactMatchOnly
            };

            task.Initialize(owner);
            task.Activate();
            return task;
        }

        protected override void OnActivate()
        {
            if (ASC == null || !_triggerTag.IsValid)
            {
                EndTask();
                return;
            }

            ASC.onGameplayEvent += HandleGameplayEvent;
        }

        protected override void OnEnd()
        {
            if (ASC != null)
            {
                ASC.onGameplayEvent -= HandleGameplayEvent;
            }
        }

        protected override void OnCancel()
        {
            if (ASC != null)
            {
                ASC.onGameplayEvent -= HandleGameplayEvent;
            }
        }

        private void HandleGameplayEvent(AbilitySystemComponent sender, GameplayEventData eventData)
        {
            if (!IsMatch(eventData.EventTag))
            {
                return;
            }

            EventReceived?.Invoke(eventData);

            if (_triggerOnce)
            {
                EndTask();
            }
        }

        private bool IsMatch(FGameplayTag eventTag)
        {
            if (!eventTag.IsValid || !_triggerTag.IsValid)
            {
                return false;
            }

            if (eventTag.Equals(_triggerTag))
            {
                return true;
            }

            if (_exactMatchOnly)
            {
                return false;
            }

            return GameplayTagUtility.IsDescendant(eventTag.Value, _triggerTag.Value);
        }
    }
}

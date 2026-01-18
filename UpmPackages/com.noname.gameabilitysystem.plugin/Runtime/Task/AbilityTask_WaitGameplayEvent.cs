using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 특정 태그의 이벤트를 기다리는 태스크입니다.
    /// </summary>
    public sealed class AbilityTask_WaitGameplayEvent : AbilityTask
    {
        private FGameplayTag _triggerTag;
        private bool _triggerOnce;
        private bool _exactMatchOnly;

        /// <summary>
        /// 이벤트 수신 시 호출됩니다.
        /// </summary>
        public event Action<GameplayEventData> EventReceived;

        /// <summary>
        /// 이벤트 대기 태스크를 생성하고 활성화합니다.
        /// </summary>
        /// <param name="owner">태스크 소유자</param>
        /// <param name="triggerTag">대기할 이벤트 태그</param>
        /// <param name="triggerOnce">한 번만 처리할지 여부</param>
        /// <param name="exactMatchOnly">정확히 일치만 허용할지 여부</param>
        /// <returns>생성된 태스크</returns>
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
                // 조건이 맞지 않으면 바로 종료한다.
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

            // 부모 태그 관계도 허용할지 확인한다.
            return GameplayTagUtility.IsDescendant(eventTag.Value, _triggerTag.Value);
        }
    }
}

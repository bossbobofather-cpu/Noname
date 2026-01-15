using System.Collections;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public interface IAbilityTaskOwner
    {
        AbilitySystemComponent ASC { get; }
        AbilityContext Context { get; }
        FGameplayAbilitySpecHandle Handle { get; }

        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);

        void RegisterTask(AbilityTask task);
        void UnregisterTask(AbilityTask task);
        void UpdateContext(AbilityContext context);
    }
}

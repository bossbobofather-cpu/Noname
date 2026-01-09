namespace Noname.GameAbilitySystem
{
    public readonly struct AbilityContext
    {
        public AbilityContext(
            FGameplayAbilitySpecHandle handle,
            GameplayEventData eventData)
        {
            Handle = handle;
            EventData = eventData;
        }

        public FGameplayAbilitySpecHandle Handle { get; }
        public GameplayEventData EventData { get; }
    }
}

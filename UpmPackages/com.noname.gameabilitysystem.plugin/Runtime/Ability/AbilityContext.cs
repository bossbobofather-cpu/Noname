namespace Noname.GameAbilitySystem
{
    public readonly struct AbilityContext
    {
        public AbilityContext(
            FGameplayAbilitySpecHandle handle,
            GameplayEventData eventData,
            AbilityTargetData targetData = null)
        {
            Handle = handle;
            EventData = eventData;
            TargetData = targetData;
        }

        public FGameplayAbilitySpecHandle Handle { get; }
        public GameplayEventData EventData { get; }
        public AbilityTargetData TargetData { get; }

        public AbilityContext WithTargetData(AbilityTargetData targetData)
        {
            return new AbilityContext(Handle, EventData, targetData);
        }
    }
}

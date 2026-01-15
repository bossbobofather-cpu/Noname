namespace Noname.GameAbilitySystem
{
    public readonly struct GameplayEffectContext
    {
        public GameplayEffectContext(
            AbilitySystemComponent source,
            AbilitySystemComponent target,
            AbilityContext abilityContext,
            object ruleContext = null)
        {
            Source = source;
            Target = target;
            AbilityContext = abilityContext;
            RuleContext = ruleContext;
        }

        public AbilitySystemComponent Source { get; }
        public AbilitySystemComponent Target { get; }
        public AbilityContext AbilityContext { get; }
        public object RuleContext { get; }
    }
}

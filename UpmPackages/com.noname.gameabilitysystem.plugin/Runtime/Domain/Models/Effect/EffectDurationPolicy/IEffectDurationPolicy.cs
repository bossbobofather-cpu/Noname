namespace Noname.GameAbilitySystem.Domain
{
    public interface IEffectDurationPolicy
    {
        public float CalculateDuration(AbilitySystemComponent asc, ref float baseDuration);
    }
}
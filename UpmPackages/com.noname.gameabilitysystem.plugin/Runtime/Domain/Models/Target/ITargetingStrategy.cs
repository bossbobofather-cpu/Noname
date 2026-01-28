namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 타겟 선정 전략 인터페이스입니다.
    /// </summary>
    public interface ITargetingStrategy
    {
        TargetData FindTargets(AbilitySystemComponent owner, TargetContext context);
    }
}

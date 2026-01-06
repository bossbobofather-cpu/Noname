namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력 시스템 컴포넌트 인터페이스 공급자
    /// </summary>
    public interface IAbilitySystemProvider
    {
        AbilitySystemComponent GetAbilitySystemComponent();
    }
}
namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// ASC(AbilitySystemComponent)에 대한 접근을 제공하는 인터페이스입니다.
    /// 구체적인 클래스에 의존하지 않고 ASC 기능을 사용하기 위해 도입되었습니다.
    /// </summary>
    public interface IAbilitySystemOwner
    {
        /// <summary>
        /// AbilitySystemComponent 인스턴스를 반환합니다.
        /// </summary>
        AbilitySystemComponent ASC { get; }
    }
}

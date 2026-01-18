namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력 시스템 컴포넌트를 제공하는 객체가 구현하는 인터페이스입니다.
    /// </summary>
    public interface IAbilitySystemProvider
    {
        /// <summary>
        /// 소유 중인 능력 시스템 컴포넌트를 반환합니다.
        /// </summary>
        /// <returns>능력 시스템 컴포넌트</returns>
        AbilitySystemComponent GetAbilitySystemComponent();
    }
}

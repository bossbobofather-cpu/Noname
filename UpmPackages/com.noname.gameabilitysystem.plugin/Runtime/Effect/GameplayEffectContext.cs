namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 효과 계산에 필요한 컨텍스트입니다.
    /// </summary>
    public readonly struct GameplayEffectContext
    {
        /// <summary>
        /// 효과 계산 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="source">효과를 주는 대상</param>
        /// <param name="target">효과를 받는 대상</param>
        /// <param name="abilityContext">어빌리티 컨텍스트</param>
        /// <param name="ruleContext">룰셋 관련 추가 정보</param>
        public GameplayEffectContext(
            AbilitySystemComponent source,
            AbilitySystemComponent target,
            AbilityContext abilityContext,
            object ruleContext = null)
        {
            // 전달받은 정보를 그대로 보관한다.
            Source = source;
            Target = target;
            AbilityContext = abilityContext;
            RuleContext = ruleContext;
        }

        /// <summary>
        /// 효과를 주는 대상입니다.
        /// </summary>
        public AbilitySystemComponent Source { get; }

        /// <summary>
        /// 효과를 받는 대상입니다.
        /// </summary>
        public AbilitySystemComponent Target { get; }

        /// <summary>
        /// 어빌리티 컨텍스트입니다.
        /// </summary>
        public AbilityContext AbilityContext { get; }

        /// <summary>
        /// 룰셋 관련 추가 정보입니다.
        /// </summary>
        public object RuleContext { get; }
    }
}

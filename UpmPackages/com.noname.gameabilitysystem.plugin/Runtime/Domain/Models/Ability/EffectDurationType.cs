namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 게임플레이 효과의 지속 타입입니다.
    /// </summary>
    public enum EffectDurationType
    {
        /// <summary>즉시 적용</summary>
        Instant,
        /// <summary>무한 지속</summary>
        Infinite,
        /// <summary>시간 기반 지속</summary>
        HasDuration
    }
}

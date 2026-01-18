namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 어빌리티 실행에 필요한 컨텍스트 묶음입니다.
    /// </summary>
    public readonly struct AbilityContext
    {
        /// <summary>
        /// 어빌리티 실행에 필요한 데이터를 묶어 생성합니다.
        /// </summary>
        /// <param name="handle">실행 중인 어빌리티 스펙 핸들</param>
        /// <param name="eventData">트리거를 일으킨 이벤트 데이터</param>
        /// <param name="targetData">타겟팅 결과 데이터(없으면 null)</param>
        public AbilityContext(
            FGameplayAbilitySpecHandle handle,
            GameplayEventData eventData,
            AbilityTargetData targetData = null)
        {
            // 전달받은 컨텍스트를 그대로 보관한다.
            Handle = handle;
            EventData = eventData;
            TargetData = targetData;
        }

        /// <summary>
        /// 실행 중인 어빌리티 스펙 핸들입니다.
        /// </summary>
        public FGameplayAbilitySpecHandle Handle { get; }

        /// <summary>
        /// 어빌리티를 유발한 이벤트 데이터입니다.
        /// </summary>
        public GameplayEventData EventData { get; }

        /// <summary>
        /// 타겟팅 결과 데이터입니다.
        /// </summary>
        public AbilityTargetData TargetData { get; }

        /// <summary>
        /// 타겟 데이터만 교체한 새 컨텍스트를 만듭니다.
        /// </summary>
        /// <param name="targetData">새 타겟 데이터</param>
        /// <returns>타겟 데이터가 교체된 컨텍스트</returns>
        public AbilityContext WithTargetData(AbilityTargetData targetData)
        {
            // 기존 정보는 유지하고 타겟만 바꾼다.
            return new AbilityContext(Handle, EventData, targetData);
        }
    }
}

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 게임플레이 이벤트 전달용 데이터입니다.
    /// </summary>
    public struct GameplayTagEventData
    {
        /// <summary>
        /// 이벤트 태그입니다.
        /// </summary>
        public FGameplayTag EventTag;
        /// <summary>
        /// 이벤트 데이터입니다.
        /// </summary>
        public object Payload;

        /// <summary>
        /// 이벤트 태그와 데이터로 생성합니다.
        /// </summary>
        public GameplayTagEventData(FGameplayTag eventTag, object payload = null)
        {
            // 전달 정보를 그대로 보관
            EventTag = eventTag;
            Payload = payload;
        }
    }
}
namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 런타임 속성 값 컨테이너입니다 (순수 C# 모델).
    /// Unity에 의존하지 않으며 Host 환경에서 사용 가능합니다.
    /// </summary>
    public sealed class AttributeValue
    {
        private float _currentValue;

        public AttributeValue(AttributeId attributeId, float baseValue = 0f, float minValue = 0f, float maxValue = 0f)
        {
            AttributeId = attributeId;
            BaseValue = baseValue;
            MinValue = minValue;
            MaxValue = maxValue;
            _currentValue = baseValue;
        }

        /// <summary>
        /// 속성 식별자입니다.
        /// </summary>
        public AttributeId AttributeId { get; }

        /// <summary>
        /// 베이스 값입니다.
        /// </summary>
        public float BaseValue { get; set; }

        /// <summary>
        /// 현재 값입니다.
        /// </summary>
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                // 범위 안으로 보정
                if (MaxValue > 0f && value > MaxValue)
                {
                    _currentValue = MaxValue;
                }
                else if (value < MinValue)
                {
                    _currentValue = MinValue;
                }
                else
                {
                    _currentValue = value;
                }
            }
        }

        /// <summary>
        /// 최소값입니다.
        /// </summary>
        public float MinValue { get; set; }

        /// <summary>
        /// 최대값입니다.
        /// </summary>
        public float MaxValue { get; set; }
    }
}

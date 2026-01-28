using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 속성 집합을 관리합니다 (순수 C# 모델).
    /// Unity에 의존하지 않으며 Host 환경에서 사용 가능합니다.
    /// </summary>
    public sealed class AttributeSet
    {
        private readonly Dictionary<AttributeId, AttributeValue> _values = new();

        /// <summary>
        /// 모든 속성 값을 반환합니다.
        /// </summary>
        public IReadOnlyCollection<AttributeValue> Values => _values.Values;

        /// <summary>
        /// 속성을 추가하거나 업데이트합니다.
        /// </summary>
        public void SetAttribute(AttributeId id, float baseValue, float minValue = 0f, float maxValue = 0f)
        {
            if (_values.TryGetValue(id, out var existing))
            {
                existing.BaseValue = baseValue;
                existing.MinValue = minValue;
                existing.MaxValue = maxValue;
                existing.CurrentValue = baseValue;
            }
            else
            {
                _values[id] = new AttributeValue(id, baseValue, minValue, maxValue);
            }
        }

        /// <summary>
        /// 식별자로 값을 조회합니다.
        /// </summary>
        public bool TryGet(AttributeId id, out AttributeValue value)
        {
            return _values.TryGetValue(id, out value);
        }

        /// <summary>
        /// 속성 값을 설정합니다.
        /// </summary>
        public void Set(AttributeId id, float value)
        {
            if (_values.TryGetValue(id, out var attr))
            {
                attr.CurrentValue = value;
            }
            else
            {
                _values[id] = new AttributeValue(id, value, 0f, 0f);
            }
        }

        /// <summary>
        /// 속성 값을 가져옵니다. 없으면 0을 반환합니다.
        /// </summary>
        public float Get(AttributeId id)
        {
            return _values.TryGetValue(id, out var attr) ? attr.CurrentValue : 0f;
        }

        /// <summary>
        /// 모든 속성을 제거합니다.
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }
    }
}

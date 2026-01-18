using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 기본 속성 식별자입니다.
    /// </summary>
    public enum AttributeId
    {
        MoveSpeed = 0,
        JumpSpeed = 1,
        Health = 2,
        AttackRange = 3,
        AttackSpeed = 4,
        AttackDamage = 5
    }

    /// <summary>
    /// 속성 정의용 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/Attribute")]
    public sealed class AttributeDefinition : ScriptableObject
    {
        [SerializeField] private AttributeId _id = AttributeId.MoveSpeed;
        [SerializeField] private float _defaultBaseValue = 0f;
        [SerializeField] private float _minValue = 0f;
        [SerializeField] private float _maxValue = 0f;

        /// <summary>
        /// 속성 식별자입니다.
        /// </summary>
        public AttributeId Id => _id;
        /// <summary>
        /// 기본 베이스 값입니다.
        /// </summary>
        public float DefaultBaseValue => _defaultBaseValue;
        /// <summary>
        /// 최소값입니다.
        /// </summary>
        public float MinValue => _minValue;
        /// <summary>
        /// 최대값입니다.
        /// </summary>
        public float MaxValue => _maxValue;
    }

    /// <summary>
    /// 런타임 속성 값 컨테이너입니다.
    /// </summary>
    public sealed class AttributeValue
    {
        private float _currentValue;

        /// <summary>
        /// 정의를 기준으로 기본 값을 초기화합니다.
        /// </summary>
        public AttributeValue(AttributeDefinition definition)
        {
            // 정의를 그대로 보관하고 기본값을 초기화
            Definition = definition;
            if (definition != null)
            {
                BaseValue = definition.DefaultBaseValue;
                _currentValue = BaseValue;
                MinValue = definition.MinValue;
                MaxValue = definition.MaxValue;
            }
        }

        /// <summary>
        /// 원본 정의입니다.
        /// </summary>
        public AttributeDefinition Definition { get; }
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
                if (value < MinValue)
                {
                    _currentValue = MinValue;
                }
                else if (value > MaxValue)
                {
                    _currentValue = MaxValue;
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

    /// <summary>
    /// 속성 집합을 관리합니다.
    /// </summary>
    public sealed class AttributeSet
    {
        private readonly Dictionary<AttributeDefinition, AttributeValue> _values =
            new();
        private readonly Dictionary<AttributeId, AttributeValue> _valuesById =
            new();

        /// <summary>
        /// 모든 속성 값을 반환합니다.
        /// </summary>
        public IReadOnlyCollection<AttributeValue> Values => _values.Values;

        /// <summary>
        /// 정의 목록으로 초기화합니다.
        /// </summary>
        public void Initialize(IEnumerable<AttributeDefinition> definitions)
        {
            // 기존 값 초기화
            _values.Clear();
            _valuesById.Clear();
            if (definitions == null)
            {
                return;
            }

            foreach (var definition in definitions)
            {
                if (definition == null || _values.ContainsKey(definition))
                {
                    continue;
                }

                // 정의별 AttributeValue 생성
                var value = new AttributeValue(definition);
                _values.Add(definition, value);
                if (!_valuesById.ContainsKey(definition.Id))
                {
                    _valuesById.Add(definition.Id, value);
                }
            }
        }

        /// <summary>
        /// 정의 객체로 값을 조회합니다.
        /// </summary>
        public bool TryGet(AttributeDefinition definition, out AttributeValue value)
        {
            return _values.TryGetValue(definition, out value);
        }

        /// <summary>
        /// 식별자로 값을 조회합니다.
        /// </summary>
        public bool TryGet(AttributeId id, out AttributeValue value)
        {
            return _valuesById.TryGetValue(id, out value);
        }
    }
}

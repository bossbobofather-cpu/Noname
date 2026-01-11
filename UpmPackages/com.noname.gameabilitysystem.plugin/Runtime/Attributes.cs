using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public enum AttributeId
    {
        MoveSpeed = 0,
        JumpSpeed = 1
    }

    [CreateAssetMenu(menuName = "GameAbilitySystem/Attribute")]
    public sealed class AttributeDefinition : ScriptableObject
    {
        [SerializeField] private AttributeId _id = AttributeId.MoveSpeed;
        [SerializeField] private float _defaultBaseValue = 0f;
        [SerializeField] private float _minValue = 0f;
        [SerializeField] private float _maxValue = 0f;

        public AttributeId Id => _id;
        public float DefaultBaseValue => _defaultBaseValue;
        public float MinValue => _minValue;
        public float MaxValue => _maxValue;
    }

    public sealed class AttributeValue
    {
        private float _currentValue;

        public AttributeValue(AttributeDefinition definition)
        {
            Definition = definition;
            if (definition != null)
            {
                BaseValue = definition.DefaultBaseValue;
                _currentValue = BaseValue;
                MinValue = definition.MinValue;
                MaxValue = definition.MaxValue;
            }
        }

        public AttributeDefinition Definition { get; }
        public float BaseValue { get; set; }
        public float CurrentValue 
        {
            get => _currentValue;
            set
            {
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
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
    }

    public sealed class AttributeSet
    {
        private readonly Dictionary<AttributeDefinition, AttributeValue> _values =
            new();

        public IReadOnlyCollection<AttributeValue> Values => _values.Values;

        public void Initialize(IEnumerable<AttributeDefinition> definitions)
        {
            _values.Clear();
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

                _values.Add(definition, new AttributeValue(definition));
            }
        }

        public bool TryGet(AttributeDefinition definition, out AttributeValue value)
        {
            return _values.TryGetValue(definition, out value);
        }
    }
}

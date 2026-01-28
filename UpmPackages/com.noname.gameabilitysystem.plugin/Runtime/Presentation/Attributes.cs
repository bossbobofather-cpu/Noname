using System.Collections.Generic;
using UnityEngine;
using Noname.GameAbilitySystem.Domain;

namespace Noname.GameAbilitySystem.Presentation
{
    // AttributeId는 Domain 레이어로 이동했습니다.
    // Domain.AttributeId를 사용하세요.

    /// <summary>
    /// 속성 정의용 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/Attribute")]
    public sealed class AttributeDefinition : ScriptableObject
    {
        [SerializeField] private string _attributeName = "Health";
        [SerializeField] private float _defaultBaseValue = 0f;
        [SerializeField] private float _minValue = 0f;
        [SerializeField] private float _maxValue = 0f;

        /// <summary>
        /// 속성 식별자입니다 (Domain 레이어).
        /// </summary>
        public AttributeId Id => new AttributeId(_attributeName);
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
}

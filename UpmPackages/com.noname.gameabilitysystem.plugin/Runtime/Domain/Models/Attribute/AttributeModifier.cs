using System;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 속성 수정자 정의입니다.
    /// </summary>
    [Serializable]
    public struct AttributeModifier
    {
        /// <summary>
        /// 대상 속성 Id입니다.
        /// </summary>
        public AttributeId AttributeId;

        /// <summary>
        /// 속성 값입니다.
        /// </summary>
        public AttributeValue AttributeValue;

        /// <summary>
        /// 값 계산 방식입니다.
        /// </summary>
        public AttributeModifierValueMode ValueMode;
        /// <summary>
        /// 적용 연산입니다.
        /// </summary>
        public AttributeModifierOperationType Operation;
        /// <summary>
        /// 정적 크기 값입니다.
        /// </summary>
        public float Magnitude;
    }
}

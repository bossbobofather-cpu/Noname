using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 속성 수정자입니다 (순수 C# 모델).
    /// </summary>
    [Serializable]
    public sealed class GameplayModifier
    {
        /// <summary>
        /// 대상 속성 이름입니다 (예: "Damage", "Defense").
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// 수정자 연산 타입입니다.
        /// </summary>
        public ModifierOperationType ModifierType { get; set; }

        /// <summary>
        /// 수정 값입니다.
        /// </summary>
        public float Value { get; set; }

        public GameplayModifier()
        {
        }

        public GameplayModifier(string attributeName, ModifierOperationType modifierType, float value)
        {
            AttributeName = attributeName;
            ModifierType = modifierType;
            Value = value;
        }
    }
}

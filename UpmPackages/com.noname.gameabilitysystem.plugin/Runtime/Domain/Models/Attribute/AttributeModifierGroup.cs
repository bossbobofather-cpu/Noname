using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 수정자 그룹입니다 (순수 C# 모델).
    /// 여러 수정자를 하나의 그룹으로 관리합니다.
    /// </summary>
    [Serializable]
    public sealed class AttributeModifierGroup
    {
        /// <summary>
        /// 그룹에 속한 수정자 목록입니다.
        /// </summary>
        public List<AttributeModifier> Modifiers { get; set; }

        public AttributeModifierGroup()
        {
            Modifiers = new List<AttributeModifier>();
        }
    }
}

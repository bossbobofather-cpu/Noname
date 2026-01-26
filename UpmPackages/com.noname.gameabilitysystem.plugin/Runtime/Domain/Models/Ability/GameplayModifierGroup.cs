using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 수정자 그룹입니다 (순수 C# 모델).
    /// 여러 수정자를 하나의 그룹으로 관리합니다.
    /// </summary>
    [Serializable]
    public sealed class GameplayModifierGroup
    {
        /// <summary>
        /// 그룹에 속한 수정자 목록입니다.
        /// </summary>
        public List<GameplayModifier> Modifiers { get; set; }

        public GameplayModifierGroup()
        {
            Modifiers = new List<GameplayModifier>();
        }
    }
}

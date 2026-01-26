namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 속성 수정자의 연산 타입입니다.
    /// </summary>
    public enum ModifierOperationType
    {
        /// <summary>더하기</summary>
        Add,
        /// <summary>퍼센트 더하기 (100 = +100%)</summary>
        AddPercent,
        /// <summary>곱하기</summary>
        Multiply,
        /// <summary>덮어쓰기</summary>
        Override
    }
}

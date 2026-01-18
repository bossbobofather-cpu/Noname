using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 효과 크기를 계산하는 계산기 기본 클래스입니다.
    /// </summary>
    public abstract class GameplayEffectCalculator : ScriptableObject
    {
        /// <summary>
        /// 효과 크기를 계산합니다.
        /// </summary>
        /// <param name="effectConfig">효과 설정</param>
        /// <param name="modifier">수정자 정보</param>
        /// <param name="context">계산 컨텍스트</param>
        /// <returns>계산된 값</returns>
        public abstract float EvaluateMagnitude(
            GameplayEffectConfig effectConfig,
            AttributeModifier modifier,
            GameplayEffectContext context);
    }
}

using System;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 속성 수정자 정의입니다.
    /// </summary>
    [Serializable]
    public struct AttributeModifier
    {
        /// <summary>
        /// 대상 속성입니다.
        /// </summary>
        public AttributeDefinition Attribute;
        /// <summary>
        /// 값 계산 방식입니다.
        /// </summary>
        public AttributeModifierValueMode ValueMode;
        /// <summary>
        /// 적용 연산입니다.
        /// </summary>
        public GameplayEffectModifierOperation Operation;
        /// <summary>
        /// 정적 크기 값입니다.
        /// </summary>
        public float Magnitude;
        /// <summary>
        /// 계산기 기반 보정입니다.
        /// </summary>
        public GameplayEffectCalculator Calculator;
    }

    /// <summary>
    /// 속성에 적용되는 연산 종류입니다.
    /// </summary>
    public enum GameplayEffectModifierOperation
    {
        Add,
        Multiply,
        Override
    }

    /// <summary>
    /// 수정자 값 계산 모드입니다.
    /// </summary>
    public enum AttributeModifierValueMode
    {
        Static,
        Calculated,
        StaticPlusCalculated
    }

    /// <summary>
    /// GameplayEffect 지속 방식입니다.
    /// </summary>
    public enum EGameplayEffectDurationType
    {
        // 즉시 적용
        Instant,
        // 무한 지속
        Infinite,
        // 시간 기반 지속
        HasDuration,
    }

    /// <summary>
    /// 게임플레이 이벤트 전달용 데이터입니다.
    /// </summary>
    public struct GameplayEventData
    {
        /// <summary>
        /// 이벤트 태그입니다.
        /// </summary>
        public FGameplayTag EventTag;
        /// <summary>
        /// 이벤트 데이터입니다.
        /// </summary>
        public object Payload;

        /// <summary>
        /// 이벤트 태그와 데이터로 생성합니다.
        /// </summary>
        public GameplayEventData(FGameplayTag eventTag, object payload = null)
        {
            // 전달 정보를 그대로 보관
            EventTag = eventTag;
            Payload = payload;
        }
    }
}

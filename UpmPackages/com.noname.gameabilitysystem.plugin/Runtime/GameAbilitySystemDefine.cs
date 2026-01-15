using System;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [Serializable]
    public struct AttributeModifier
    {
        public AttributeDefinition Attribute;
        public AttributeModifierValueMode ValueMode;
        public GameplayEffectModifierOperation Operation;
        public float Magnitude;
        public GameplayEffectCalculator Calculator;
    }

    public enum GameplayEffectModifierOperation
    {
        Add,
        Multiply,
        Override
    }

    public enum AttributeModifierValueMode
    {
        Static,
        Calculated,
        StaticPlusCalculated
    }


    public enum EGameplayEffectDurationType
    {
        Instant,            //즉시 적용
        Infinite,           //무한 지속
        HasDuration,        //지속 시간 보유
    }

    public struct GameplayEventData
    {
        public FGameplayTag EventTag;
        public object Payload; // 필요 시 커스텀 타입

        public GameplayEventData(FGameplayTag eventTag, object payload = null)
        {
            EventTag = eventTag;
            Payload = payload;
        }
    }
}

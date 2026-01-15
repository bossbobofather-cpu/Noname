using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public abstract class GameplayEffectCalculator : ScriptableObject
    {
        public abstract float EvaluateMagnitude(
            GameplayEffectConfig effectConfig,
            AttributeModifier modifier,
            GameplayEffectContext context);
    }
}

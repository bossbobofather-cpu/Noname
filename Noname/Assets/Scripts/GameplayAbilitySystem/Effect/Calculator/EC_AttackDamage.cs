using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.GameplayAbilitySystem.Effect
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Calculator/EC_AttackDamage")]
    public sealed class EC_AttackDamage : GameplayEffectCalculator
    {
        [SerializeField] private float _coefficient = 1f;

        public override float EvaluateMagnitude(
            GameplayEffectConfig effectConfig,
            AttributeModifier modifier,
            GameplayEffectContext context)
        {
            var source = context.Source;
            if (source == null || !source.Attributes.TryGet(AttributeId.AttackDamage, out var value) || value == null)
            {
                return 0;
            }

            return value.CurrentValue *_coefficient;
        }
    }
}

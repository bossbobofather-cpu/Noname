using Noname.GameAbilitySystem;
using UnityEngine;

namespace MyProject.GameplayAbilitySystem.Effect
{
    /// <summary>
    /// 공격 데미지를 계산하는 GameplayEffectCalculator입니다.
    /// 시전자의 AttackDamage 속성값을 기반으로 최종 데미지를 산출합니다.
    /// </summary>
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
            // 시전자의 공격력 속성이 존재하는지 확인
            if (source == null || !source.Attributes.TryGet(AttributeId.AttackDamage, out var value) || value == null)
            {
                return 0;
            }

            // 공격력 * 계수 반환
            return value.CurrentValue *_coefficient;
        }
    }
}

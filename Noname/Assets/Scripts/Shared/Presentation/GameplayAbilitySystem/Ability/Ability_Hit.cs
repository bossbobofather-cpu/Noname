using System.Collections.Generic;
using UnityEngine;
using Noname.GameAbilitySystem;

namespace MyProject.GameplayAbilitySystem.Ability
{
    /// <summary>
    /// 타겟에게 효과(GameplayEffect)를 적용하는 능력입니다.
    /// GameplayTargetConfig 설정에 따라 타겟을 탐지하고 효과를 부여합니다.
    /// </summary>
    public sealed class Ability_Hit : GameplayAbility
    {
        public override bool CanActivateAbility()
        {
            // 설정(Config)이 존재해야 발동 가능
            return TryGetConfig<GameplayTargetConfig>(out _);
        }

        protected override void ActivateAbility(AbilityContext context)
        {
            if (ASC == null)
            {
                return;
            }

            if (!TryGetConfig<GameplayTargetConfig>(out var config))
            {
                Debug.LogWarning("Ability_Hit activated, but GameplayTargetConfig is missing.");
                return;
            }

            if (config.Effects == null || config.Effects.Count == 0)
            {
                Debug.LogWarning("Ability_Hit activated, but GameplayTargetConfig has no effects.");
                return;
            }

            // 타겟 데이터를 비동기적으로 대기하는 태스크 생성
            var task = AbilityTask_WaitTargetData.Create(TaskOwner, config);
            task.TargetDataReady += targetData => ApplyEffects(targetData, config.Effects, ASC, context);
            task.Activate();
        }

        private static void ApplyEffects(
            AbilityTargetData targetData,
            IReadOnlyList<GameplayEffectConfig> effects,
            AbilitySystemComponent source,
            AbilityContext abilityContext)
        {
            if (targetData == null || effects == null || effects.Count == 0)
            {
                return;
            }

            // AbilitySystemComponent가 직접 타겟인 경우 우선 처리
            if (targetData.AbilitySystems.Count > 0)
            {
                for (var i = 0; i < targetData.AbilitySystems.Count; i++)
                {
                    ApplyEffectsToTarget(targetData.AbilitySystems[i], effects, source, abilityContext);
                }

                return;
            }

            // 일반 GameObject 타겟 처리
            for (var i = 0; i < targetData.Targets.Count; i++)
            {
                var target = targetData.Targets[i];
                if (target == null)
                {
                    continue;
                }

                var abilitySystem = target.GetComponentInParent<AbilitySystemComponent>();
                ApplyEffectsToTarget(abilitySystem, effects, source, abilityContext);
            }
        }

        private static void ApplyEffectsToTarget(
            AbilitySystemComponent target,
            IReadOnlyList<GameplayEffectConfig> effects,
            AbilitySystemComponent source,
            AbilityContext abilityContext)
        {
            if (target == null)
            {
                return;
            }

            // 효과 컨텍스트 생성 후 적용
            var context = new GameplayEffectContext(source, target, abilityContext);
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                target.ApplyGameplayEffect(effect, context);
            }
        }
    }
}

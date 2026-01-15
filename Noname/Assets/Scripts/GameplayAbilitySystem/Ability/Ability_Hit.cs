using System.Collections.Generic;
using UnityEngine;
using Noname.GameAbilitySystem;

namespace MyProject.GameplayAbilitySystem.Ability
{
    public sealed class Ability_Hit : GameplayAbility
    {
        public override bool CanActivateAbility()
        {
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

            if (targetData.AbilitySystems.Count > 0)
            {
                for (var i = 0; i < targetData.AbilitySystems.Count; i++)
                {
                    ApplyEffectsToTarget(targetData.AbilitySystems[i], effects, source, abilityContext);
                }

                return;
            }

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

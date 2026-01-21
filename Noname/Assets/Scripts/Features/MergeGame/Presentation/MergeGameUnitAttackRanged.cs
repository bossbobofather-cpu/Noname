using UnityEngine;
using Noname.GameAbilitySystem;
using MyProject.GameplayAbilitySystem.Define;
using MyProject.GameplayAbilitySystem.Target;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 단일 타겟 원거리 공격을 수행하는 유닛 공격 로직입니다.
    /// </summary>
    public sealed class MergeGameUnitAttackRanged : MergeGameUnitAttackBase
    {
        [SerializeField] private GameplayEffectConfig _damageEffect;
        [SerializeField] private TargetGroup _targetGroup = TargetGroup.Opponent;

        protected override bool CanAttack()
        {
            // 효과 설정이 없으면 공격 불가.
            return _damageEffect != null;
        }

        protected override void ExecuteAttack()
        {
            var source = SourceAbility;
            if (source == null || _damageEffect == null)
            {
                return;
            }

            if (!TargetRegistry.TryGet(out var registry))
            {
                return;
            }

            var targets = registry.GetTargets(_targetGroup);
            if (targets == null || targets.Count == 0)
            {
                return;
            }

            // 가장 가까운 타겟을 찾아 공격한다.
            var origin = transform.position;
            Targetable selected = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null)
                {
                    continue;
                }

                var targetTransform = target.GetTransform();
                if (targetTransform == null)
                {
                    continue;
                }

                var distance = (targetTransform.position - origin).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    selected = target;
                }
            }

            if (selected == null || selected.AbilitySystem == null)
            {
                return;
            }

            // GAS 효과를 타겟에 적용한다.
            var context = new GameplayEffectContext(source, selected.AbilitySystem, default);
            selected.AbilitySystem.ApplyGameplayEffect(_damageEffect, context);
        }
    }
}

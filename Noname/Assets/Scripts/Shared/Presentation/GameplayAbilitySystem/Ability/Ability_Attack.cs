using UnityEngine;
using Noname.GameAbilitySystem;
using MyProject.Common.Units;

namespace MyProject.GameplayAbilitySystem.Ability
{
    /// <summary>
    /// 공격 능력을 정의하는 클래스입니다.
    /// 애니메이터의 공격 트리거를 발동시킵니다.
    /// </summary>
    public class Ability_Attack : GameplayAbility
    {
        private const string AttackTriggerName = "Attack";

        protected override void ActivateAbility(AbilityContext context)
        {
            if (ASC == null)
            {
                return;
            }
            
            PlayAttackAnimation();
        }

        private void PlayAttackAnimation()
        {
            var animator = GetAnimator();
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(AttackTriggerName);
        }

        private Animator GetAnimator()
        {
            if (ASC == null)
            {
                return null;
            }

            // 부모 Unit 컴포넌트에서 Animator를 가져옴
            var unit = ASC.GetComponentInParent<Unit>();
            return unit != null ? unit.Animator : null;
        }
    }
}

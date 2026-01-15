using UnityEngine;
using Noname.GameAbilitySystem;
using MyProject.Common.Units;

namespace MyProject.GameplayAbilitySystem.Ability
{
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

            var unit = ASC.GetComponentInParent<Unit>();
            return unit != null ? unit.Animator : null;
        }
    }
}

using UnityEngine;
using Noname.GameAbilitySystem;

namespace MergeGame.Ability
{
    public class Ability_Attack : GameplayAbility
    {
        public override bool CanActivateAbility()
        {
            return TryGetConfig<GameplayTargettingConfig>(out var config);
        }

        protected override void PreActivate(AbilityContext context)
        {
            //Targetting Config이 없을 수 없어야 한다. CanActivateAbility에 의해 걸러졌어야 했기 때문
            if (!TryGetConfig<GameplayTargettingConfig>(out var config))
            {
                Debug.LogError($"Targetting Config를 찾을 수 없습니다.");
                return;
            }


        }
        protected override void ActivateAbility(AbilityContext context)
        {
            if (TryGetConfig<GameplayTargettingConfig>(out var config))
            {
                // 공격 능력 발동 로직 구현
                Debug.Log("Ability_Attack activated with targetting config.");
            }
            else
            {
                Debug.LogWarning("Ability_Attack activation failed: Missing GameplayTargettingConfig.");
            }
        }
    }
}

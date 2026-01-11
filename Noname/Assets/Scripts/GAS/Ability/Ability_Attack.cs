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
            //Targetting Config???�을 ???�어???�다. CanActivateAbility???�해 걸러졌어???�기 ?�문
            if (!TryGetConfig<GameplayTargettingConfig>(out var config))
            {
                UnityEngine.Debug.LogError($"Targetting Config�?찾을 ???�습?�다.");
                return;
            }


        }
        protected override void ActivateAbility(AbilityContext context)
        {
            if (TryGetConfig<GameplayTargettingConfig>(out var config))
            {
                // 공격 ?�력 발동 로직 구현
                UnityEngine.Debug.Log("Ability_Attack activated with targetting config.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("Ability_Attack activation failed: Missing GameplayTargettingConfig.");
            }
        }
    }
}


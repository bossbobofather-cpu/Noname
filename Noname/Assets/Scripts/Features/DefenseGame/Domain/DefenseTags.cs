using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 디펜스 게임에서 사용하는 태그 정의입니다.
    /// </summary>
    public static class DefenseTags
    {
        // 쿨다운 태그
        public static readonly FGameplayTag Cooldown_BasicAttack = new("Cooldown.BasicAttack");
        public static readonly FGameplayTag Cooldown_AreaAttack = new("Cooldown.AreaAttack");

        // 어빌리티 태그 (어빌리티 보유 여부)
        public static readonly FGameplayTag Ability_BasicAttack = new("Ability.BasicAttack");
        public static readonly FGameplayTag Ability_AreaAttack = new("Ability.AreaAttack");
        public static readonly FGameplayTag Ability_AttackSpeedUp = new("Ability.AttackSpeedUp");
        public static readonly FGameplayTag Ability_FullHealthRestore = new ("Ability.FullHealthRestore");
        public static readonly FGameplayTag Ability_AreaAttackTargetUp = new("Ability.AreaAttackTargetUp");

        // 상태 태그
        public static readonly FGameplayTag State_Invincible = new("State.Invincible");
    }
}

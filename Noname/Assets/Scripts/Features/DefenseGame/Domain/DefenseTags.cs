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
        public static readonly FGameplayTag Ability_LifeStealOnKill = new("Ability.LifeStealOnKill");

        // 버프 태그 (스택 가능)
        public static readonly FGameplayTag Buff_AttackSpeedUp = new("Buff.AttackSpeedUp");
        public static readonly FGameplayTag Buff_ExpGainUp = new("Buff.ExpGainUp");
        public static readonly FGameplayTag Buff_AreaAttackTargetUp = new("Buff.AreaAttackTargetUp");
        public static readonly FGameplayTag Buff_LifeStealAmountUp = new("Buff.LifeStealAmountUp");
    }
}

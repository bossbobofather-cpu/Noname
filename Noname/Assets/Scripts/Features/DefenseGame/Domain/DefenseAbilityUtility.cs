using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 디펜스 게임의 어빌리티 생성 유틸리티입니다.
    /// </summary>
    public static class DefenseAbilityUtility
    {
        // 쿨다운 태그
        public static readonly FGameplayTag CooldownBasicAttack = new("Cooldown.BasicAttack");

        // 어빌리티 태그
        public static readonly FGameplayTag AbilityBasicAttack = new("Ability.BasicAttack");

        /// <summary>
        /// 기본 공격 어빌리티를 생성합니다.
        /// </summary>
        /// <param name="cooldown">쿨다운 시간 (초)</param>
        /// <param name="damageAmount">데미지 양</param>
        public static GameplayAbility CreateBasicAttack(float cooldown = 1.0f, float damageAmount = 10f)
        {
            // 쿨다운 효과가 부여하는 태그
            var cooldownGrantedTags = new GameplayTagContainer();
            cooldownGrantedTags.AddTag(CooldownBasicAttack);

            // 쿨다운 효과 (자신에게 적용)
            var cooldownEffect = new GameplayEffect
            {
                EffectId = "Effect.Cooldown.BasicAttack",
                DisplayName = "기본 공격 쿨다운",
                DurationType = EffectDurationType.HasDuration,
                Duration = cooldown,
                GrantedTags = cooldownGrantedTags
            };

            // 데미지 효과 (적에게 적용)
            var damageEffect = new GameplayEffect
            {
                EffectId = "Effect.Damage.BasicAttack",
                DisplayName = "기본 공격 데미지",
                DurationType = EffectDurationType.Instant,
                ModifierGroups = new List<AttributeModifierGroup>
                {
                    new AttributeModifierGroup
                    {
                        Modifiers = new List<AttributeModifier>
                        {
                            new AttributeModifier
                            {
                                AttributeId = DefenseAttributeIds.Hp,
                                Operation = AttributeModifierOperationType.Add,
                                Magnitude = -damageAmount
                            }
                        }
                    }
                }
            };

            // 차단 태그 (쿨다운 중이면 발동 불가)
            var blockedTags = new GameplayTagContainer();
            blockedTags.AddTag(CooldownBasicAttack);

            return new GameplayAbility
            {
                AbilityId = "BasicAttack",
                AbilityTag = AbilityBasicAttack,
                DisplayName = "기본 공격",
                Description = "가장 가까운 적에게 데미지를 입힙니다.",
                CooldownEffect = cooldownEffect,
                AppliedEffects = new List<GameplayEffect> { damageEffect },
                ActivationBlockedTags = blockedTags,
                TargetingStrategy = new NearestEnemyTargetingStrategy()
            };
        }

        /// <summary>
        /// 범위 공격 어빌리티를 생성합니다.
        /// </summary>
        /// <param name="cooldown">쿨다운 시간 (초)</param>
        /// <param name="damageAmount">데미지 양</param>
        /// <param name="maxTargets">최대 타겟 수</param>
        public static GameplayAbility CreateAreaAttack(float cooldown = 2.0f, float damageAmount = 8f, int maxTargets = 3)
        {
            // 쿨다운 효과가 부여하는 태그
            var cooldownGrantedTags = new GameplayTagContainer();
            cooldownGrantedTags.AddTag(DefenseTags.Cooldown_AreaAttack);

            // 쿨다운 효과 (자신에게 적용)
            var cooldownEffect = new GameplayEffect
            {
                EffectId = "Effect.Cooldown.AreaAttack",
                DisplayName = "범위 공격 쿨다운",
                DurationType = EffectDurationType.HasDuration,
                Duration = cooldown,
                GrantedTags = cooldownGrantedTags
            };

            // 데미지 효과 (적에게 적용)
            var damageEffect = new GameplayEffect
            {
                EffectId = "Effect.Damage.AreaAttack",
                DisplayName = "범위 공격 데미지",
                DurationType = EffectDurationType.Instant,
                ModifierGroups = new List<AttributeModifierGroup>
                {
                    new AttributeModifierGroup
                    {
                        Modifiers = new List<AttributeModifier>
                        {
                            new AttributeModifier
                            {
                                AttributeId = DefenseAttributeIds.Hp,
                                Operation = AttributeModifierOperationType.Add,
                                Magnitude = -damageAmount
                            }
                        }
                    }
                }
            };

            // 차단 태그 (쿨다운 중이면 발동 불가)
            var blockedTags = new GameplayTagContainer();
            blockedTags.AddTag(DefenseTags.Cooldown_AreaAttack);

            return new GameplayAbility
            {
                AbilityId = "AreaAttack",
                AbilityTag = DefenseTags.Ability_AreaAttack,
                DisplayName = "범위 공격",
                Description = $"가장 가까운 적 {maxTargets}기에게 데미지를 입힙니다.",
                CooldownEffect = cooldownEffect,
                AppliedEffects = new List<GameplayEffect> { damageEffect },
                ActivationBlockedTags = blockedTags,
                TargetingStrategy = new NearestNEnemiesTargetingStrategy(maxTargets)
            };
        }

        public static GameplayAbility CreateAbility(int abilityId)
        {
            switch (abilityId)
            {
                case 1:
                    return CreateBasicAttack();
                case 2:
                    return CreateAreaAttack();
                default:
                    return null;
            }
        }

        public static List<GameplayAbility> CreateAbilities(IEnumerable<int> abilityIds)
        {
            var results = new List<GameplayAbility>();
            if (abilityIds == null)
            {
                return results;
            }

            foreach (var id in abilityIds)
            {
                var ability = CreateAbility(id);
                if (ability != null)
                {
                    results.Add(ability);
                }
            }

            return results;
        }
    }
}

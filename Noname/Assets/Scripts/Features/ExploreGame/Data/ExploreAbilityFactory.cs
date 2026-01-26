using System.Collections.Generic;
using Noname.GameAbilitySystem;
using MyProject.ExploreGame.Domain;

namespace MyProject.ExploreGame.Data
{
    /// <summary>
    /// 전투용 어빌리티를 생성하는 팩토리 클래스입니다.
    /// </summary>
    public static class ExploreAbilityFactory
    {
        /// <summary>
        /// 기본 어빌리티 세트를 생성합니다.
        /// </summary>
        public static List<ExploreAbilityState> CreateDefaultAbilities()
        {
            var abilities = new List<ExploreAbilityState>
            {
                CreateQuickStrike(),
                CreatePowerStrike(),
                CreateUltimateStrike()
            };

            return abilities;
        }

        /// <summary>
        /// 빠른 공격 어빌리티를 생성합니다.
        /// 충전 빠름, 데미지 보통 (공격력 * 1.5)
        /// </summary>
        private static ExploreAbilityState CreateQuickStrike()
        {
            var effect = new GameplayEffectModel
            {
                EffectId = "QuickStrike_Effect",
                DisplayName = "빠른 일격 효과",
                Description = "빠른 일격 데미지 증가 효과",
                DurationType = EffectDurationType.Instant,
                Duration = 0f,
                MaxStack = 1,
                ModifierGroups = new List<GameplayModifierGroup>
                {
                    new GameplayModifierGroup
                    {
                        Modifiers = new List<GameplayModifier>
                        {
                            new GameplayModifier
                            {
                                AttributeName = "Damage",
                                ModifierType = ModifierOperationType.AddPercent,
                                Value = 50f // +50% 데미지
                            }
                        }
                    }
                }
            };

            var ability = new GameplayAbilityModel
            {
                AbilityId = "QuickStrike",
                DisplayName = "빠른 일격",
                Description = "빠르게 충전되는 공격. 데미지 +50%",
                Cooldown = 0f,
                CostEffects = new List<GameplayEffectModel>(),
                AppliedEffects = new List<GameplayEffectModel> { effect }
            };

            return new ExploreAbilityState(
                "QuickStrike",
                ability,
                maxCharge: 100f,
                chargeRate: 15f // 약 6.7초에 충전 완료
            );
        }

        /// <summary>
        /// 강타 어빌리티를 생성합니다.
        /// 충전 보통, 데미지 높음 (공격력 * 3.0)
        /// </summary>
        private static ExploreAbilityState CreatePowerStrike()
        {
            var effect = new GameplayEffectModel
            {
                EffectId = "PowerStrike_Effect",
                DisplayName = "강타 효과",
                Description = "강타 데미지 증가 효과",
                DurationType = EffectDurationType.Instant,
                Duration = 0f,
                MaxStack = 1,
                ModifierGroups = new List<GameplayModifierGroup>
                {
                    new GameplayModifierGroup
                    {
                        Modifiers = new List<GameplayModifier>
                        {
                            new GameplayModifier
                            {
                                AttributeName = "Damage",
                                ModifierType = ModifierOperationType.AddPercent,
                                Value = 200f // +200% 데미지 (총 3배)
                            }
                        }
                    }
                }
            };

            var ability = new GameplayAbilityModel
            {
                AbilityId = "PowerStrike",
                DisplayName = "강타",
                Description = "강력한 일격. 데미지 3배",
                Cooldown = 0f,
                CostEffects = new List<GameplayEffectModel>(),
                AppliedEffects = new List<GameplayEffectModel> { effect }
            };

            return new ExploreAbilityState(
                "PowerStrike",
                ability,
                maxCharge: 100f,
                chargeRate: 8f // 약 12.5초에 충전 완료
            );
        }

        /// <summary>
        /// 필살기 어빌리티를 생성합니다.
        /// 충전 느림, 데미지 매우 높음 (공격력 * 5.0)
        /// </summary>
        private static ExploreAbilityState CreateUltimateStrike()
        {
            var effect = new GameplayEffectModel
            {
                EffectId = "UltimateStrike_Effect",
                DisplayName = "필살기 효과",
                Description = "필살기 데미지 증가 효과",
                DurationType = EffectDurationType.Instant,
                Duration = 0f,
                MaxStack = 1,
                ModifierGroups = new List<GameplayModifierGroup>
                {
                    new GameplayModifierGroup
                    {
                        Modifiers = new List<GameplayModifier>
                        {
                            new GameplayModifier
                            {
                                AttributeName = "Damage",
                                ModifierType = ModifierOperationType.AddPercent,
                                Value = 400f // +400% 데미지 (총 5배)
                            }
                        }
                    }
                }
            };

            var ability = new GameplayAbilityModel
            {
                AbilityId = "UltimateStrike",
                DisplayName = "필살기",
                Description = "강력한 필살기. 데미지 5배",
                Cooldown = 0f,
                CostEffects = new List<GameplayEffectModel>(),
                AppliedEffects = new List<GameplayEffectModel> { effect }
            };

            return new ExploreAbilityState(
                "UltimateStrike",
                ability,
                maxCharge: 100f,
                chargeRate: 5f // 20초에 충전 완료
            );
        }
    }
}

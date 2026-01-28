using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;

namespace Noname.GameAbilitySystem.Presentation
{
    /// <summary>
    /// Domain 레이어와 Presentation 레이어 간 변환을 위한 확장 메서드입니다.
    /// </summary>
    public static class DomainConversionExtensions
    {
        /// <summary>
        /// Unity GameplayTagContainer를 Domain GameplayTagContainerModel로 변환합니다.
        /// </summary>
        public static GameplayTagContainer ToDomain(this GameplayTagContainerView container)
        {
            var model = new GameplayTagContainer();
            if (container != null)
            {
                foreach (var tag in container.Tags)
                {
                    model.AddTag(tag);
                }
            }
            return model;
        }

        public static AttributeSet ToDomain(this AttributeDefinition definition)
        {
            if(definition == null) return null;

            var attributeSet = new AttributeSet();

            attributeSet.SetAttribute(
                definition.Id,
                definition.DefaultBaseValue,
                definition.MinValue,
                definition.MaxValue);

            return attributeSet;
        }

        /// <summary>
        /// List<AttributeDefinition>을 Domain AttributeSet 변환합니다.
        /// </summary>
        public static AttributeSet ToDomain(this List<AttributeDefinition> definitions)
        {
            if(definitions == null) return null;

            var attributeSet = new AttributeSet();

            foreach(var definition in definitions)
            {
                if(definition == null) continue;

                attributeSet.SetAttribute(
                    definition.Id,
                    definition.DefaultBaseValue,
                    definition.MinValue,
                    definition.MaxValue);
            }

            return attributeSet;
        }

        public static GameplayAbility ToDomain(this GameplayAbilityConfig config)
        {
            if(config == null) return null;

            var ability = new GameplayAbility
            {
                AbilityTag = config.AbilityTag,
                AbilityId = config.AbilityID,
                DisplayName = config.DisplayName,
                Description = config.Description,
                CostEffects = config.CostEffects.ToDomain(),
                AppliedEffects = config.AppliedEffects.ToDomain(),
                ActivationRequiredTags = config.ActivationRequiredTags.ToDomain(),
                ActivationBlockedTags = config.ActivationBlockedTags.ToDomain(),
            };

            return ability;
        }

        public static List<GameplayAbility> ToDomain(this List<GameplayAbilityConfig> config)
        {
            if(config == null) return null;

            var abilities = new List<GameplayAbility>();
            foreach(var ability in config)
            {
                if(ability == null) continue;

                abilities.Add(ability.ToDomain());
            }
        
            return abilities;
        }

        /// <summary>
        /// GameplayEffectConfig를 Domain GameplayEffect 변환합니다.
        /// </summary>
        public static GameplayEffect ToDomain(this GameplayEffectConfig config)
        {
            if (config == null) return null;

            var effect = new GameplayEffect
            {
                EffectId = config.name,
                DisplayName = config.name,
                Description = "",
                DurationType = config.DurationType,
                Duration = config.Duration,
                Period = 0f,
                MaxStack = 1,
                ModifierGroups = new List<AttributeModifierGroup>(),
                GrantedTags = config.GrantedTags.ToDomain(),
                RequiredTags = config.ActivationRequiredTags.ToDomain(),
                BlockedTags = config.ActivationBlockedTags.ToDomain(),
            };

            // 수정자 변환
            if (config.Modifiers != null)
            {
                var modifiers = new List<AttributeModifier>();
                foreach (var modifier in config.Modifiers)
                {
                    modifiers.Add(modifier);
                }

                if (modifiers.Count > 0)
                {
                    effect.ModifierGroups.Add(new AttributeModifierGroup { Modifiers = modifiers });
                }
            }

            return effect;
        }

        /// <summary>
        /// GameplayEffectConfig를 Domain GameplayEffect 변환합니다.
        /// </summary>
        public static List<GameplayEffect> ToDomain(this List<GameplayEffectConfig> config)
        {
            if (config == null) return null;

            var effects = new List<GameplayEffect>();
            foreach(var effect in config)
            {
                if(effect == null) continue;

                effects.Add(effect.ToDomain());
            }
        
            return effects;
        }
    }
}

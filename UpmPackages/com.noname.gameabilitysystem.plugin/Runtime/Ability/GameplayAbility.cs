using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// Base class for gameplay abilities.
    /// </summary>
    public abstract class GameplayAbility
    {
        private AbilitySystemComponent _asc;
        private IReadOnlyList<GameplayConfig> _configs;

        public AbilitySystemComponent ASC => _asc;

        public void InitializeAbility(AbilitySystemComponent asc, IReadOnlyList<GameplayConfig> configs)
        {
            _asc = asc;
            _configs = configs;
        }

        public bool TryGetConfigs<T>(out List<T> configs) where T : GameplayConfig
        {
            configs = new List<T>();
            for (var i = 0; i < _configs.Count; i++)
            {
                if (_configs[i] is T typed)
                {
                    configs.Add(typed);
                }
            }
            
            return configs.Count > 0;
        }

        public bool TryGetConfig<T>(out T config) where T : GameplayConfig
        {
            for (var i = 0; i < _configs.Count; i++)
            {
                if (_configs[i] is T typed)
                {
                    config = typed;
                    return true;
                }
            }

            config = null;
            return false;
        }

        public virtual bool CanActivateAbility(
            FGameplayAbilitySpecHandle handle,
            GameplayTagContainer sourceTags,
            GameplayTagContainer targetTags)
        {
            return true;
        }

        public virtual void CancelAbility(FGameplayAbilitySpecHandle handle)
        {
        }

        public virtual void EndAbility(FGameplayAbilitySpecHandle handle)
        {
        }

        public void CallActivateAbility(FGameplayAbilitySpecHandle handle, GameplayEventData eventData)
        {
            PreActivate(handle);
            ActivateAbility(handle, eventData);
        }

        protected virtual void PreActivate(FGameplayAbilitySpecHandle handle)
        {
        }

        protected virtual void ActivateAbility(FGameplayAbilitySpecHandle handle, GameplayEventData eventData)
        {
            
        }
    }
}

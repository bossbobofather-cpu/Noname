using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    public sealed class GameplayAbilitySpec
    {
        public Type AbilityType;
        public string AbilityName;
        public IReadOnlyList<GameplayConfig> Configs;
        public int Level;
        public int ActiveCount;
        public FGameplayAbilitySpecHandle Handle;

        public bool TryGetConfigs<T>(out List<T> configs) where T : GameplayConfig
        {
            configs = new List<T>();
            if (Configs == null)
            {
                return false;
            }

            for (var i = 0; i < Configs.Count; i++)
            {
                if (Configs[i] is T typed)
                {
                    configs.Add(typed);
                }
            }

            return configs.Count > 0;
        }

        public bool TryGetConfig<T>(out T config) where T : GameplayConfig
        {
            if (Configs != null)
            {
                for (var i = 0; i < Configs.Count; i++)
                {
                    if (Configs[i] is T typed)
                    {
                        config = typed;
                        return true;
                    }
                }
            }

            config = null;
            return false;
        }
    }

    public struct FGameplayAbilitySpecHandle : IEquatable<FGameplayAbilitySpecHandle>
    {
        public static readonly FGameplayAbilitySpecHandle Invalid = new FGameplayAbilitySpecHandle { Id = 0 };

        public int Id;

        public bool Equals(FGameplayAbilitySpecHandle other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is FGameplayAbilitySpecHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            return a.Id != b.Id;
        }
    }
}

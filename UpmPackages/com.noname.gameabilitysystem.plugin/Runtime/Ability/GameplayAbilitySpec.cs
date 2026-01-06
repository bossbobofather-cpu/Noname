
using System;

namespace Noname.GameAbilitySystem
{
    /// 게임 플레이 능력 사양 구조체
    public sealed class GameplayAbilitySpec
    {
        public GameplayAbility Ability;                 // 어떤 능력인지
        public int Level;                               // 능력의 레벨
        public int ActiveCount;                         // 능력이 활성화된 횟수
        public FGameplayAbilitySpecHandle Handle;       // 능력의 고유 핸들
    }

    /// <summary>
    /// 게임 플레이 능력 사양 핸들
    /// </summary>
    public struct FGameplayAbilitySpecHandle : IEquatable<FGameplayAbilitySpecHandle>   //박싱 방지 위한 Equatable 구현
    {
        public static readonly FGameplayAbilitySpecHandle Invalid = new FGameplayAbilitySpecHandle { Id = 0 };
        
        public int Id;                                  // 핸들의 고유 ID

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
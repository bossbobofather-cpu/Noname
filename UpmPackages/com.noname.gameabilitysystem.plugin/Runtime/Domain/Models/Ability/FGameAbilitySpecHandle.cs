using System;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 능력 사양을 식별하기 위한 핸들입니다.
    /// </summary>
    public struct FGameplayAbilitySpecHandle : IEquatable<FGameplayAbilitySpecHandle>
    {
        /// <summary>
        /// 유효하지 않은 핸들 값입니다.
        /// </summary>
        public static readonly FGameplayAbilitySpecHandle Invalid = new FGameplayAbilitySpecHandle { Id = 0 };

        /// <summary>
        /// 식별자 값입니다.
        /// </summary>
        public int Id;

        /// <summary>
        /// 다른 핸들과 동일한지 비교합니다.
        /// </summary>
        /// <param name="other">비교 대상</param>
        /// <returns>동일 여부</returns>
        public bool Equals(FGameplayAbilitySpecHandle other)
        {
            // 식별자 값으로 비교한다.
            return Id == other.Id;
        }

        /// <summary>
        /// 객체 동일 여부를 비교합니다.
        /// </summary>
        /// <param name="obj">비교 대상</param>
        /// <returns>동일 여부</returns>
        public override bool Equals(object obj)
        {
            // 같은 타입인지 확인한 뒤 비교한다.
            return obj is FGameplayAbilitySpecHandle other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        /// <returns>해시 값</returns>
        public override int GetHashCode()
        {
            // 식별자 값을 그대로 사용한다.
            return Id;
        }

        /// <summary>
        /// 동일 연산자입니다.
        /// </summary>
        public static bool operator ==(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // 식별자 값으로 비교한다.
            return a.Id == b.Id;
        }

        /// <summary>
        /// 다름 연산자입니다.
        /// </summary>
        public static bool operator !=(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // 식별자 값으로 비교한다.
            return a.Id != b.Id;
        }
    }
}
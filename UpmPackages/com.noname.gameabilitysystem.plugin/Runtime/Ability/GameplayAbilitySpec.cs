using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력 사양 정보를 담는 구조입니다.
    /// </summary>
    public sealed class GameplayAbilitySpec
    {
        /// <summary>
        /// 능력 클래스 타입입니다.
        /// </summary>
        public Type AbilityType;

        /// <summary>
        /// 에디터 표시용 이름입니다.
        /// </summary>
        public string AbilityName;

        /// <summary>
        /// 연결된 구성 목록입니다.
        /// </summary>
        public IReadOnlyList<GameplayConfig> Configs;

        /// <summary>
        /// 능력 레벨입니다.
        /// </summary>
        public int Level;

        /// <summary>
        /// 활성화 중인 횟수입니다.
        /// </summary>
        public int ActiveCount;

        /// <summary>
        /// 능력 핸들입니다.
        /// </summary>
        public FGameplayAbilitySpecHandle Handle;

        /// <summary>
        /// 특정 타입의 구성들을 가져옵니다.
        /// </summary>
        /// <typeparam name="T">요청할 구성 타입</typeparam>
        /// <param name="configs">찾아낸 구성 목록</param>
        /// <returns>하나 이상 찾았는지 여부</returns>
        public bool TryGetConfigs<T>(out List<T> configs) where T : GameplayConfig
        {
            // 결과 목록을 새로 만든다.
            configs = new List<T>();
            if (Configs == null)
            {
                // 구성 자체가 없으면 실패로 처리한다.
                return false;
            }

            for (var i = 0; i < Configs.Count; i++)
            {
                if (Configs[i] is T typed)
                {
                    // 요청한 타입만 수집한다.
                    configs.Add(typed);
                }
            }

            return configs.Count > 0;
        }

        /// <summary>
        /// 특정 타입의 구성을 하나 가져옵니다.
        /// </summary>
        /// <typeparam name="T">요청할 구성 타입</typeparam>
        /// <param name="config">찾아낸 구성</param>
        /// <returns>찾았는지 여부</returns>
        public bool TryGetConfig<T>(out T config) where T : GameplayConfig
        {
            if (Configs != null)
            {
                for (var i = 0; i < Configs.Count; i++)
                {
                    if (Configs[i] is T typed)
                    {
                        // 첫 번째로 발견한 항목을 반환한다.
                        config = typed;
                        return true;
                    }
                }
            }

            // 찾지 못하면 기본값을 넘긴다.
            config = null;
            return false;
        }
    }

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

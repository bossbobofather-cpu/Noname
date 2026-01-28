using System;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 게임플레이 태그 구조체입니다 (순수 C# 모델).
    /// Unity에 의존하지 않으며 Host 환경에서 사용 가능합니다.
    /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        private string _value;
        private int _hash;

        /// <summary>
        /// 문자열 값을 기반으로 태그를 생성합니다.
        /// </summary>
        /// <param name="value">태그 문자열</param>
        public FGameplayTag(string value)
        {
            _value = value;
            _hash = 0;
            if (!string.IsNullOrEmpty(value))
            {
                _hash = GameplayTagUtility.Fnv1a32(value);
            }
        }

        /// <summary>
        /// 태그 문자열입니다.
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// 태그 해시 값입니다.
        /// </summary>
        public int Hash
        {
            get
            {
                if (_hash == 0 && !string.IsNullOrEmpty(_value))
                {
                    // 필요할 때 해시를 계산한다.
                    _hash = GameplayTagUtility.Fnv1a32(_value);
                }
                return _hash;
            }
        }

        /// <summary>
        /// 태그 문자열이 유효한지 여부입니다.
        /// </summary>
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        /// <summary>
        /// 다른 태그와 동일한지 비교합니다.
        /// </summary>
        /// <param name="other">비교 대상</param>
        /// <returns>동일 여부</returns>
        public bool Equals(FGameplayTag other)
        {
            return Hash == other.Hash;
        }

        /// <summary>
        /// 객체 동일 여부를 비교합니다.
        /// </summary>
        /// <param name="obj">비교 대상</param>
        /// <returns>동일 여부</returns>
        public override bool Equals(object obj)
        {
            return obj is FGameplayTag other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        /// <returns>해시 값</returns>
        public override int GetHashCode()
        {
            return Hash;
        }

        /// <summary>
        /// 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>태그 문자열</returns>
        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}

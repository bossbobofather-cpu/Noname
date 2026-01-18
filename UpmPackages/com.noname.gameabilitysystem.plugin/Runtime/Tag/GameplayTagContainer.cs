using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 게임플레이 태그를 관리하는 컨테이너입니다.
    /// </summary>
    [Serializable]
    public sealed class GameplayTagContainer : ISerializationCallbackReceiver
    {
        [SerializeField] private List<FGameplayTag> _tags = new();
        [NonSerialized] private HashSet<int> _explicitTags;
        [NonSerialized] private HashSet<int> _expandedTags;

        /// <summary>
        /// 보관 중인 태그 목록입니다.
        /// </summary>
        public IReadOnlyList<FGameplayTag> Tags => _tags;

        /// <summary>
        /// 부모 태그까지 포함하여 보유 여부를 확인합니다.
        /// </summary>
        /// <param name="tag">확인할 태그</param>
        /// <returns>보유 여부</returns>
        public bool HasTag(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: true);
        }

        /// <summary>
        /// 정확히 일치하는 태그만 확인합니다.
        /// </summary>
        /// <param name="tag">확인할 태그</param>
        /// <returns>보유 여부</returns>
        public bool HasTagExact(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: false);
        }

        /// <summary>
        /// 다른 컨테이너의 태그 중 하나라도 포함하는지 확인합니다.
        /// </summary>
        /// <param name="other">비교 대상</param>
        /// <returns>포함 여부</returns>
        public bool HasAny(GameplayTagContainer other)
        {
            if (other == null)
            {
                return false;
            }

            for (var i = 0; i < other._tags.Count; i++)
            {
                if (HasTag(other._tags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 다른 컨테이너의 태그를 모두 포함하는지 확인합니다.
        /// </summary>
        /// <param name="other">비교 대상</param>
        /// <returns>포함 여부</returns>
        public bool HasAll(GameplayTagContainer other)
        {
            if (other == null)
            {
                return true;
            }

            for (var i = 0; i < other._tags.Count; i++)
            {
                if (!HasTag(other._tags[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 정확히 일치하는 태그 중 하나라도 포함하는지 확인합니다.
        /// </summary>
        /// <param name="other">비교 대상</param>
        /// <returns>포함 여부</returns>
        public bool HasAnyExact(GameplayTagContainer other)
        {
            if (other == null)
            {
                return false;
            }

            for (var i = 0; i < other._tags.Count; i++)
            {
                if (HasTagExact(other._tags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 정확히 일치하는 태그를 모두 포함하는지 확인합니다.
        /// </summary>
        /// <param name="other">비교 대상</param>
        /// <returns>포함 여부</returns>
        public bool HasAllExact(GameplayTagContainer other)
        {
            if (other == null)
            {
                return true;
            }

            for (var i = 0; i < other._tags.Count; i++)
            {
                if (!HasTagExact(other._tags[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 태그를 추가합니다.
        /// </summary>
        /// <param name="tag">추가할 태그</param>
        public void AddTag(FGameplayTag tag)
        {
            var value = tag.Value;
            if (string.IsNullOrEmpty(value))
            {
                // 빈 태그는 추가하지 않는다.
                return;
            }

            EnsureCache();
            if (_explicitTags.Contains(tag.Hash))
            {
                // 이미 있는 태그는 무시한다.
                return;
            }

            _tags.Add(tag);
            AddToCache(tag);
        }

        /// <summary>
        /// 태그를 제거합니다.
        /// </summary>
        /// <param name="tag">제거할 태그</param>
        public void RemoveTag(FGameplayTag tag)
        {
            for (var i = _tags.Count - 1; i >= 0; i--)
            {
                if (_tags[i].Equals(tag))
                {
                    _tags.RemoveAt(i);
                }
            }

            // 캐시는 다시 구성한다.
            RebuildCache();
        }

        /// <summary>
        /// 태그를 모두 제거합니다.
        /// </summary>
        public void Clear()
        {
            _tags.Clear();
            RebuildCache();
        }

        /// <summary>
        /// 직렬화 전 처리입니다.
        /// </summary>
        public void OnBeforeSerialize()
        {
            // 현재는 별도 처리 없음.
        }

        /// <summary>
        /// 역직렬화 후 처리입니다.
        /// </summary>
        public void OnAfterDeserialize()
        {
            // 캐시를 다시 만든다.
            RebuildCache();
        }

        private bool HasTagInternal(FGameplayTag tag, bool includeParents)
        {
            if (string.IsNullOrEmpty(tag.Value))
            {
                return false;
            }

            EnsureCache();
            return includeParents ? _expandedTags.Contains(tag.Hash) : _explicitTags.Contains(tag.Hash);
        }

        private void EnsureCache()
        {
            if (_explicitTags == null || _expandedTags == null)
            {
                RebuildCache();
            }
        }

        private void RebuildCache()
        {
            if (_explicitTags == null)
                _explicitTags = new HashSet<int>();
            else
                _explicitTags.Clear();

            if (_expandedTags == null)
                _expandedTags = new HashSet<int>();
            else
                _expandedTags.Clear();

            for (var i = 0; i < _tags.Count; i++)
            {
                var tag = _tags[i];
                if (string.IsNullOrEmpty(tag.Value))
                {
                    continue;
                }

                if (_explicitTags.Add(tag.Hash))
                {
                    AddParentsToCache(tag.Value);
                }
            }
        }

        private void AddToCache(FGameplayTag tag)
        {
            if (_explicitTags.Add(tag.Hash))
            {
                AddParentsToCache(tag.Value);
            }
        }

        private void AddParentsToCache(string value)
        {
            _expandedTags.Add(Animator.StringToHash(value));
            foreach (var parent in GameplayTagUtility.EnumerateParents(value))
            {
                _expandedTags.Add(Animator.StringToHash(parent));
            }
        }
    }

    /// <summary>
    /// 게임플레이 태그 구조체입니다.
    /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        [SerializeField] private string _value;
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
                _hash = Animator.StringToHash(value);
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
                    _hash = Animator.StringToHash(_value);
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

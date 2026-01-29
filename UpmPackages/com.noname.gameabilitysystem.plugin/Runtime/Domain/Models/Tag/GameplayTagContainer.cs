using System.Collections.Generic;
using System.Linq;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 게임플레이 태그를 관리하는 컨테이너입니다 (순수 C# 모델).
    /// Unity에 의존하지 않으며 Host 환경에서 사용 가능합니다.
    /// </summary>
    public sealed class GameplayTagContainer
    {
        private readonly List<FGameplayTag> _tags = new();
        private readonly HashSet<int> _explicitTags = new();
        private readonly HashSet<int> _expandedTags = new();

        /// <summary>
        /// 보관 중인 태그 목록입니다.
        /// </summary>
        public IReadOnlyList<FGameplayTag> Tags => _tags;

        public GameplayTagContainer()
        {

        }

        /// <summary>
        /// 부모 태그까지 포함하여 보유 여부를 확인합니다.
        /// </summary>
        public bool HasTag(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: true);
        }

        /// <summary>
        /// 정확히 일치하는 태그만 확인합니다.
        /// </summary>
        public bool HasTagExact(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: false);
        }

        /// <summary>
        /// 다른 컨테이너의 태그 중 하나라도 포함하는지 확인합니다.
        /// </summary>
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
        /// 태그를 추가합니다.
        /// </summary>
        public bool AddTag(FGameplayTag tag)
        {
            if (string.IsNullOrEmpty(tag.Value))
            {
                return false;
            }

            if (_explicitTags.Contains(tag.Hash))
            {
                return false;
            }

            _tags.Add(tag);
            AddToCache(tag);
            return true;
        }

        /// <summary>
        /// 태그를 제거합니다.
        /// </summary>
        public void RemoveTag(FGameplayTag tag)
        {
            for (var i = _tags.Count - 1; i >= 0; i--)
            {
                if (_tags[i].Equals(tag))
                {
                    _tags.RemoveAt(i);
                }
            }

            RebuildCache();
        }

        /// <summary>
        /// 태그를 모두 제거합니다.
        /// </summary>
        public void Clear()
        {
            _tags.Clear();
            _explicitTags.Clear();
            _expandedTags.Clear();
        }

        private bool HasTagInternal(FGameplayTag tag, bool includeParents)
        {
            if (string.IsNullOrEmpty(tag.Value))
            {
                return false;
            }

            return includeParents ? _expandedTags.Contains(tag.Hash) : _explicitTags.Contains(tag.Hash);
        }

        private void RebuildCache()
        {
            _explicitTags.Clear();
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
                    AddParentsToCache(tag);
                }
            }
        }

        private void AddToCache(FGameplayTag tag)
        {
            if (_explicitTags.Add(tag.Hash))
            {
                AddParentsToCache(tag);
            }
        }

        private void AddParentsToCache(FGameplayTag tag)
        {
            _expandedTags.Add(tag.Hash);

            // 부모 태그 추가 (예: "Ability.Attack.Fire" -> "Ability.Attack", "Ability")
            var lastDotIndex = tag.Value.LastIndexOf('.');
            while (lastDotIndex > 0)
            {
                var parent = tag.Value.Substring(0, lastDotIndex);
                _expandedTags.Add(parent.GetHashCode());
                lastDotIndex = parent.LastIndexOf('.');
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 게임 플레이 태그 레지스트리. 사용할 태그를 추가/제거할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/TagRegistry")]
    public sealed class GameplayTagRegistry : ScriptableObject
    {
        [SerializeField] private List<string> _tags = new();
        [SerializeField] private GameplayTagContainer _systemMessageIgnoreTags = new();

        private static GameplayTagRegistry _runtimeRegistry;

        public IReadOnlyList<string> Tags => _tags;
        public GameplayTagContainer SystemMessageIgnoreTags => _systemMessageIgnoreTags;
        public static GameplayTagRegistry RuntimeRegistry => _runtimeRegistry;

        public static void SetRuntimeRegistry(GameplayTagRegistry registry)
        {
            _runtimeRegistry = registry;
        }

public bool IsSystemMessageIgnored(FGameplayTag tag)
{
    if (_systemMessageIgnoreTags == null || !tag.IsValid)
    {
        return false;
    }

    foreach (var candidate in GameplayTagUtility.EnumerateTagAndParents(tag.Value))
    {
        if (_systemMessageIgnoreTags.HasTagExact(new FGameplayTag(candidate)))
        {
            return true;
        }
    }

    return false;
}

        public List<string> GetAllTags(bool includeParents = true)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < _tags.Count; i++)
            {
                var tag = _tags[i];
                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                if (!GameplayTagUtility.IsValidTagString(tag))
                {
                    continue;
                }

                set.Add(tag);
                if (includeParents)
                {
                    foreach (var parent in GameplayTagUtility.EnumerateParents(tag))
                    {
                        set.Add(parent);
                    }
                }
            }

            var list = new List<string>(set);
            list.Sort(StringComparer.Ordinal);
            return list;
        }

        /// <summary>
        /// 태그가 정의되었는지 확인합니다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="includeParents"></param>
        /// <returns></returns>
        public bool IsTagDefined(string value, bool includeParents = true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!GameplayTagUtility.IsValidTagString(value))
            {
                return false;
            }

            if (includeParents)
            {
                for (var i = 0; i < _tags.Count; i++)
                {
                    var tag = _tags[i];
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        continue;
                    }

                    if (!GameplayTagUtility.IsValidTagString(tag))
                    {
                        continue;
                    }

                    if (string.Equals(tag, value, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    foreach (var parent in GameplayTagUtility.EnumerateParents(tag))
                    {
                        if (string.Equals(parent, value, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            for (var i = 0; i < _tags.Count; i++)
            {
                if (string.Equals(_tags[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

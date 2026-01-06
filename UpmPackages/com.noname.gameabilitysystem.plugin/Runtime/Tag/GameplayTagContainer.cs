using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [Serializable]
    public sealed class GameplayTagContainer : ISerializationCallbackReceiver
    {
        [SerializeField] private List<FGameplayTag> _tags = new();
        [NonSerialized] private HashSet<string> _explicitTags;
        [NonSerialized] private HashSet<string> _expandedTags;

        public IReadOnlyList<FGameplayTag> Tags => _tags;

        public bool HasTag(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: true);
        }

        public bool HasTagExact(FGameplayTag tag)
        {
            return HasTagInternal(tag, includeParents: false);
        }

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

        public void AddTag(FGameplayTag tag)
        {
            var value = tag.Value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            EnsureCache();
            if (_explicitTags.Contains(value))
            {
                return;
            }

            _tags.Add(tag);
            AddToCache(value);
        }

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

        public void Clear()
        {
            _tags.Clear();
            RebuildCache();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            RebuildCache();
        }

        private bool HasTagInternal(FGameplayTag tag, bool includeParents)
        {
            var value = tag.Value;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            EnsureCache();
            return includeParents ? _expandedTags.Contains(value) : _explicitTags.Contains(value);
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
            _explicitTags = new HashSet<string>(StringComparer.Ordinal);
            _expandedTags = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < _tags.Count; i++)
            {
                var value = _tags[i].Value;
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (_explicitTags.Add(value))
                {
                    AddParentsToCache(value);
                }
            }
        }

        private void AddToCache(string value)
        {
            if (_explicitTags.Add(value))
            {
                AddParentsToCache(value);
            }
        }

        private void AddParentsToCache(string value)
        {
            _expandedTags.Add(value);
            foreach (var parent in GameplayTagUtility.EnumerateParents(value))
            {
                _expandedTags.Add(parent);
            }
        }
    }

    /// <summary>
    /// 게임 플레이 태그 구조체
    /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        [SerializeField] private string _value;

        public FGameplayTag(string value)
        {
            _value = value;
        }

        public string Value => _value;
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        public bool Equals(FGameplayTag other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is FGameplayTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}

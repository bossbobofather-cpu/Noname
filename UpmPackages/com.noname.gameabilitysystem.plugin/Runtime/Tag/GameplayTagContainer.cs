using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [Serializable]
    public sealed class GameplayTagContainer : ISerializationCallbackReceiver
    {
        [SerializeField] private List<FGameplayTag> _tags = new();
        [NonSerialized] private HashSet<int> _explicitTags;
        [NonSerialized] private HashSet<int> _expandedTags;

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
            if (_explicitTags.Contains(tag.Hash))
            {
                return;
            }

            _tags.Add(tag);
            AddToCache(tag);
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
    /// 게임 플레이 태그 구조체
    /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        [SerializeField] private string _value;
        private int _hash;

        public FGameplayTag(string value)
        {
            _value = value;
            _hash = 0;
            if (!string.IsNullOrEmpty(value))
            {
                _hash = Animator.StringToHash(value);
            }
        }

        public string Value => _value;
        public int Hash
        {
            get
            {
                if (_hash == 0 && !string.IsNullOrEmpty(_value))
                {
                    _hash = Animator.StringToHash(_value);
                }
                return _hash;
            }
        }
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        public bool Equals(FGameplayTag other)
        {
            return Hash == other.Hash;
        }

        public override bool Equals(object obj)
        {
            return obj is FGameplayTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Hash;
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}

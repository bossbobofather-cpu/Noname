using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// AbilitySystem의 상태 모델입니다.
    /// Host 환경에서 스레드 안전하게 동작합니다.
    /// </summary>
    public sealed class AbilitySystemModel
    {
        public struct ActiveGameplayEffect
        {
            public long EffectUid;
            public GameplayEffectConfig Config;
            public float EndTime;
        }

        private readonly object _modelLock = new();
        private readonly AttributeSet _attributes;
        private readonly GameplayTagContainer _ownedTags;
        /// <summary>
        /// AttributeSet에 정의되지 않은 속성의 fallback 값을 저장합니다.
        /// 런타임에 동적으로 추가된 속성이나 임시 속성 값 보관용입니다.
        /// </summary>
        private readonly Dictionary<AttributeId, float> _fallbackValues = new();
        private readonly List<string> _skills = new();
        private readonly Dictionary<int, int> _effectTagCounts = new();
        private readonly Dictionary<int, int> _looseTagCounts = new();
        private readonly List<ActiveGameplayEffect> _activeEffects = new();
        private long _nextEffectUid = 1;

        public AbilitySystemModel()
        {
            _attributes = new AttributeSet();
            _ownedTags = new GameplayTagContainer();
        }

        public AbilitySystemModel(AttributeSet attributes, GameplayTagContainer ownedTags)
        {
            _attributes = attributes ?? new AttributeSet();
            _ownedTags = ownedTags ?? new GameplayTagContainer();
        }

        /// <summary>
        /// 속성 컨테이너입니다. (스레드 안전하지 않음 - 직접 수정 금지)
        /// </summary>
        public AttributeSet Attributes => _attributes;

        /// <summary>
        /// 소유 태그 컨테이너입니다. (스레드 안전하지 않음 - 직접 수정 금지)
        /// </summary>
        public GameplayTagContainer OwnedTags => _ownedTags;

        /// <summary>
        /// 활성 효과 개수입니다.
        /// </summary>
        public int ActiveEffectCount
        {
            get
            {
                lock (_modelLock)
                {
                    return _activeEffects.Count;
                }
            }
        }

        /// <summary>
        /// 기본 속성을 초기화합니다.
        /// </summary>
        public void InitializeAttributes(IEnumerable<AttributeDefinition> definitions)
        {
            lock (_modelLock)
            {
                _attributes.Initialize(definitions);
            }
        }

        /// <summary>
        /// 속성 값을 조회합니다.
        /// </summary>
        public float Get(AttributeId id)
        {
            lock (_modelLock)
            {
                if (_attributes.TryGet(id, out var value) && value != null)
                {
                    return value.CurrentValue;
                }

                return _fallbackValues.TryGetValue(id, out var fallback) ? fallback : 0f;
            }
        }

        /// <summary>
        /// 속성 값을 설정합니다.
        /// </summary>
        public void Set(AttributeId id, float value)
        {
            lock (_modelLock)
            {
                if (_attributes.TryGet(id, out var attr) && attr != null)
                {
                    attr.CurrentValue = value;
                    return;
                }

                _fallbackValues[id] = value;
            }
        }

        /// <summary>
        /// 속성 값을 증감합니다.
        /// </summary>
        public void Add(AttributeId id, float delta)
        {
            lock (_modelLock)
            {
                var current = GetUnsafe(id);
                SetUnsafe(id, current + delta);
            }
        }

        /// <summary>
        /// 속성 값을 퍼센트로 증감합니다.
        /// 양수 퍼센트는 증가, 음수 퍼센트는 감소를 의미합니다.
        /// </summary>
        public void AddPercent(AttributeId id, float percent)
        {
            if (percent == 0f)
            {
                return;
            }

            lock (_modelLock)
            {
                var current = GetUnsafe(id);
                var bonus = (float)System.Math.Round(current * percent);

                // 최소 변화량 보장 (버프/디버프가 너무 작지 않도록)
                if (bonus > 0f && bonus < 1f)
                {
                    bonus = 1f;
                }
                else if (bonus < 0f && bonus > -1f)
                {
                    bonus = -1f;
                }

                SetUnsafe(id, current + bonus);
            }
        }

        /// <summary>
        /// lock 없이 속성 값을 조회합니다. (내부 사용 전용)
        /// </summary>
        private float GetUnsafe(AttributeId id)
        {
            if (_attributes.TryGet(id, out var value) && value != null)
            {
                return value.CurrentValue;
            }

            return _fallbackValues.TryGetValue(id, out var fallback) ? fallback : 0f;
        }

        /// <summary>
        /// lock 없이 속성 값을 설정합니다. (내부 사용 전용)
        /// </summary>
        private void SetUnsafe(AttributeId id, float value)
        {
            if (_attributes.TryGet(id, out var attr) && attr != null)
            {
                attr.CurrentValue = value;
                return;
            }

            _fallbackValues[id] = value;
        }

        /// <summary>
        /// 스킬을 추가합니다.
        /// </summary>
        public void AddSkill(string skillId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return;
            }

            lock (_modelLock)
            {
                _skills.Add(skillId);
            }
        }

        /// <summary>
        /// 스킬 목록을 복사하여 반환합니다. (스레드 안전)
        /// </summary>
        public List<string> GetSkills()
        {
            lock (_modelLock)
            {
                return new List<string>(_skills);
            }
        }

        /// <summary>
        /// 내부 스킬 목록에 안전하지 않은 접근을 제공합니다. (lock 내부에서만 호출)
        /// </summary>
        internal IReadOnlyList<string> GetSkillsUnsafe()
        {
            return _skills;
        }

        /// <summary>
        /// 루즈 태그를 추가합니다.
        /// </summary>
        public bool AddLooseTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return AddTagInternal(_looseTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 루즈 태그를 제거합니다.
        /// </summary>
        public bool RemoveLooseTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return RemoveTagInternal(_looseTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 효과 태그를 추가합니다.
        /// </summary>
        public bool AddEffectTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return AddTagInternal(_effectTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 효과 태그를 제거합니다.
        /// </summary>
        public bool RemoveEffectTag(FGameplayTag tag, out int totalCount)
        {
            lock (_modelLock)
            {
                return RemoveTagInternal(_effectTagCounts, tag, out totalCount);
            }
        }

        /// <summary>
        /// 태그의 총 개수를 조회합니다.
        /// </summary>
        public int GetTotalTagCount(FGameplayTag tag)
        {
            if (!tag.IsValid)
            {
                return 0;
            }

            lock (_modelLock)
            {
                return GetTotalTagCount(tag.Hash);
            }
        }

        /// <summary>
        /// 활성 효과를 추가하고 생성된 UID를 반환합니다.
        /// </summary>
        public long AddActiveEffect(GameplayEffectConfig config, float endTime)
        {
            lock (_modelLock)
            {
                var uid = _nextEffectUid++;
                _activeEffects.Add(new ActiveGameplayEffect
                {
                    EffectUid = uid,
                    Config = config,
                    EndTime = endTime
                });
                return uid;
            }
        }

        /// <summary>
        /// UID로 활성 효과를 제거합니다.
        /// </summary>
        public bool RemoveActiveEffectByUid(long effectUid)
        {
            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    if (_activeEffects[i].EffectUid != effectUid)
                    {
                        continue;
                    }

                    var lastIndex = _activeEffects.Count - 1;
                    if (i < lastIndex)
                    {
                        _activeEffects[i] = _activeEffects[lastIndex];
                    }
                    _activeEffects.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Config로 활성 효과를 제거합니다. (같은 Config의 첫 번째 효과만 제거)
        /// </summary>
        public bool RemoveActiveEffect(GameplayEffectConfig config)
        {
            if (config == null)
            {
                return false;
            }

            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    if (_activeEffects[i].Config != config)
                    {
                        continue;
                    }

                    var lastIndex = _activeEffects.Count - 1;
                    if (i < lastIndex)
                    {
                        _activeEffects[i] = _activeEffects[lastIndex];
                    }
                    _activeEffects.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 만료된 효과를 수집하고 제거합니다.
        /// </summary>
        public void CollectExpiredEffects(float now, List<GameplayEffectConfig> expired)
        {
            if (expired == null)
            {
                return;
            }

            lock (_modelLock)
            {
                for (var i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    var active = _activeEffects[i];
                    // 버그 수정: now < active.EndTime -> now >= active.EndTime
                    if (active.Config == null || now < active.EndTime)
                    {
                        continue;
                    }

                    expired.Add(active.Config);

                    var lastIndex = _activeEffects.Count - 1;
                    if (i < lastIndex)
                    {
                        _activeEffects[i] = _activeEffects[lastIndex];
                    }
                    _activeEffects.RemoveAt(lastIndex);
                }
            }
        }

        /// <summary>
        /// 활성 효과 목록을 복사하여 반환합니다. (스레드 안전)
        /// </summary>
        public List<GameplayEffectConfig> GetActiveEffects()
        {
            lock (_modelLock)
            {
                var results = new List<GameplayEffectConfig>(_activeEffects.Count);
                for (var i = 0; i < _activeEffects.Count; i++)
                {
                    var config = _activeEffects[i].Config;
                    if (config != null)
                    {
                        results.Add(config);
                    }
                }
                return results;
            }
        }

        /// <summary>
        /// 활성 효과 목록을 제공된 리스트에 추가합니다. (스레드 안전)
        /// </summary>
        public void GetActiveEffects(List<GameplayEffectConfig> results)
        {
            if (results == null)
            {
                return;
            }

            lock (_modelLock)
            {
                for (var i = 0; i < _activeEffects.Count; i++)
                {
                    var config = _activeEffects[i].Config;
                    if (config != null)
                    {
                        results.Add(config);
                    }
                }
            }
        }

        /// <summary>
        /// 내부 활성 효과 목록에 안전하지 않은 접근을 제공합니다. (lock 내부에서만 호출)
        /// </summary>
        internal IReadOnlyList<ActiveGameplayEffect> GetActiveEffectsUnsafe()
        {
            return _activeEffects;
        }

        /// <summary>
        /// 태그를 추가하는 내부 메서드입니다. (lock 내부에서만 호출)
        /// </summary>
        private bool AddTagInternal(Dictionary<int, int> counts, FGameplayTag tag, out int totalCount)
        {
            totalCount = 0;
            if (!tag.IsValid)
            {
                return false;
            }

            var hash = tag.Hash;
            var beforeTotal = GetTotalTagCount(hash);
            counts.TryGetValue(hash, out var count);
            count++;
            counts[hash] = count;

            totalCount = GetTotalTagCount(hash);
            if (beforeTotal == 0)
            {
                _ownedTags.AddTag(tag);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 태그를 제거하는 내부 메서드입니다. (lock 내부에서만 호출)
        /// </summary>
        private bool RemoveTagInternal(Dictionary<int, int> counts, FGameplayTag tag, out int totalCount)
        {
            totalCount = 0;
            if (!tag.IsValid)
            {
                return false;
            }

            var hash = tag.Hash;
            if (!counts.TryGetValue(hash, out var count))
            {
                return false;
            }

            count--;
            if (count <= 0)
            {
                counts.Remove(hash);
            }
            else
            {
                counts[hash] = count;
            }

            totalCount = GetTotalTagCount(hash);
            if (totalCount == 0)
            {
                _ownedTags.RemoveTag(tag);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 태그의 총 개수를 조회하는 내부 메서드입니다. (lock 내부에서만 호출)
        /// </summary>
        private int GetTotalTagCount(int hash)
        {
            _effectTagCounts.TryGetValue(hash, out var effectCount);
            _looseTagCounts.TryGetValue(hash, out var looseCount);
            return effectCount + looseCount;
        }

        /// <summary>
        /// 현재 상태의 불변 스냅샷을 생성합니다. (스레드 안전)
        /// Host 환경에서 클라이언트로 상태를 전송할 때 사용됩니다.
        /// </summary>
        public AbilitySystemSnapshot BuildSnapshot()
        {
            lock (_modelLock)
            {
                // 속성 값 복사
                var attributeDict = new Dictionary<AttributeId, float>();
                foreach (var attr in _attributes.Values)
                {
                    if (attr != null && attr.Definition != null)
                    {
                        attributeDict[attr.Definition.Id] = attr.CurrentValue;
                    }
                }

                // Fallback 값 복사
                foreach (var kvp in _fallbackValues)
                {
                    if (!attributeDict.ContainsKey(kvp.Key))
                    {
                        attributeDict[kvp.Key] = kvp.Value;
                    }
                }

                // 태그 복사
                var tagList = new List<FGameplayTag>(_ownedTags.Tags);

                // 스킬 복사
                var skillList = new List<string>(_skills);

                // 활성 효과 복사
                var effectList = new List<ActiveGameplayEffectSnapshot>(_activeEffects.Count);
                for (var i = 0; i < _activeEffects.Count; i++)
                {
                    var active = _activeEffects[i];
                    effectList.Add(new ActiveGameplayEffectSnapshot(
                        active.EffectUid,
                        active.Config,
                        active.EndTime
                    ));
                }

                return new AbilitySystemSnapshot(attributeDict, tagList, skillList, effectList);
            }
        }
    }
}

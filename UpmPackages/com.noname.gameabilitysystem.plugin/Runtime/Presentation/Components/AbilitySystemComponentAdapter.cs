using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;
using UnityEngine;

namespace Noname.GameAbilitySystem.Presentation
{
    /// <summary>
    /// 능력 시스템을 관리하는 Unity 어댑터입니다.
    /// Domain의 AbilitySystemComponent를 래핑하고 Unity 환경과 연동합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AbilitySystemComponentAdapter : MonoBehaviour
    {
        [Header("초기화 설정 (Inspector)")]
        [SerializeField] private List<AttributeDefinition> _attributeDefaults = new();
        [SerializeField] private List<GameplayAbilityConfig> _startupAbilityConfigs = new();
        [SerializeField] private GameplayTagContainerView _ownedTags;

        [Header("디버그")]
        [SerializeField] private bool _emitSystemMessages = true;

        private AbilitySystemComponent _model;
        private readonly List<GameplayEffect> _expiredEffects = new();
        private bool _initialized;

        private event Action<AbilitySystemComponentAdapter, FGameplayTag> _onAddedTag;
        private event Action<AbilitySystemComponentAdapter, FGameplayTag> _onRemovedTag;

        /// <summary>
        /// Domain 모델입니다.
        /// </summary>
        public AbilitySystemComponent Model => _model;

        /// <summary>
        /// 소유한 태그 컨테이너입니다.
        /// </summary>
        public GameplayTagContainer OwnedTags => _model?.OwnedTags;

        /// <summary>
        /// 속성 집합입니다.
        /// </summary>
        public AttributeSet Attributes => _model?.Attributes;

        /// <summary>
        /// 초기화 여부입니다.
        /// </summary>
        public bool IsInitialized => _initialized;

        private void Update()
        {
            if (!_initialized || _model == null) return;
            if (_model.ActiveEffectCount == 0) return;

            // 만료된 효과 처리
            _expiredEffects.Clear();
            _model.CollectExpiredEffects(Time.time, _expiredEffects);

            for (var i = 0; i < _expiredEffects.Count; i++)
            {
                RemoveEffectTags(_expiredEffects[i]);
            }
        }

        /// <summary>
        /// Inspector 필드를 기반으로 초기화합니다.
        /// </summary>
        public void InitializeFromDefaults()
        {
            if (_initialized) return;

            var attributes = _attributeDefaults.ToDomain();
            var abilities = _startupAbilityConfigs.ToDomain();
            var tags = _ownedTags != null ? _ownedTags.ToDomain() : new GameplayTagContainer();

            Initialize(attributes, abilities, tags);
        }

        /// <summary>
        /// 외부에서 정보를 전달받아 초기화합니다.
        /// </summary>
        public void Initialize(AttributeSet attributes, List<GameplayAbility> abilities, GameplayTagContainer tags)
        {
            if (_initialized) return;

            _model = new AbilitySystemComponent(attributes, abilities, tags);
            _initialized = true;

            PublishSystemMessage("AbilitySystemComponent 초기화 완료");
        }

        /// <summary>
        /// 이미 생성된 Domain 모델로 초기화합니다.
        /// </summary>
        public void Initialize(AbilitySystemComponent model)
        {
            if (_initialized) return;
            if (model == null) return;

            _model = model;
            _initialized = true;

            PublishSystemMessage("AbilitySystemComponent 초기화 완료 (외부 모델)");
        }

        #region 능력 관리 (Domain 위임)

        /// <summary>
        /// 능력을 부여합니다.
        /// </summary>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbility ability)
        {
            if (!EnsureInitialized()) return FGameplayAbilitySpecHandle.Invalid;
            if (ability == null) return FGameplayAbilitySpecHandle.Invalid;

            var handle = _model.GiveAbility(ability);
            PublishSystemMessage($"능력 부여: {ability.DisplayName}");
            return handle;
        }

        /// <summary>
        /// Config에서 능력을 부여합니다.
        /// </summary>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbilityConfig config)
        {
            if (!EnsureInitialized()) return FGameplayAbilitySpecHandle.Invalid;
            if (config == null) return FGameplayAbilitySpecHandle.Invalid;

            return GiveAbility(config.ToDomain());
        }

        /// <summary>
        /// 핸들로 능력을 제거합니다.
        /// </summary>
        public bool RemoveAbility(FGameplayAbilitySpecHandle handle)
        {
            if (!EnsureInitialized()) return false;

            var result = _model.RemoveAbility(handle);
            if (result)
            {
                PublishSystemMessage($"능력 제거: Handle {handle.Id}");
            }
            return result;
        }

        /// <summary>
        /// 능력을 제거합니다.
        /// </summary>
        public bool RemoveAbility(GameplayAbility ability)
        {
            if (!EnsureInitialized()) return false;
            if (ability == null) return false;

            var result = _model.RemoveAbility(ability);
            if (result)
            {
                PublishSystemMessage($"능력 제거: {ability.DisplayName}");
            }
            return result;
        }

        #endregion

        #region 태그 관리 (Domain 위임)

        /// <summary>
        /// 루즈 태그를 추가합니다.
        /// </summary>
        public void AddLooseTag(FGameplayTag tag)
        {
            if (!EnsureInitialized()) return;
            if (!tag.IsValid) return;

            var addedToOwned = _model.AddLooseTag(tag, out var totalCount);
            if (addedToOwned)
            {
                _onAddedTag?.Invoke(this, tag);
            }

            PublishSystemMessage($"태그 추가: {tag.Value} (총 {totalCount})");
        }

        /// <summary>
        /// 루즈 태그를 제거합니다.
        /// </summary>
        public void RemoveLooseTag(FGameplayTag tag)
        {
            if (!EnsureInitialized()) return;
            if (!tag.IsValid) return;

            var beforeTotal = _model.GetTotalTagCount(tag);
            var removedFromOwned = _model.RemoveLooseTag(tag, out var totalCount);

            if (totalCount == beforeTotal) return;

            if (removedFromOwned)
            {
                _onRemovedTag?.Invoke(this, tag);
            }

            PublishSystemMessage($"태그 제거: {tag.Value} (총 {totalCount})");
        }

        #endregion

        #region 효과 관리

        /// <summary>
        /// 게임플레이 효과를 적용합니다.
        /// </summary>
        public long ApplyGameplayEffect(GameplayEffect effect, float duration = 0f)
        {
            if (!EnsureInitialized()) return -1;
            if (effect == null) return -1;

            // 적용 조건 확인
            if (!CanApplyEffect(effect)) return -1;

            // 즉시 효과는 속성만 변경
            if (effect.DurationType == EffectDurationType.Instant)
            {
                ApplyModifiers(effect);
                PublishSystemMessage($"즉시 효과 적용: {effect.DisplayName}");
                return 0;
            }

            // 지속 효과는 태그 추가 및 활성 효과 등록
            AddEffectTags(effect);

            float endTime;
            if (effect.DurationType == EffectDurationType.HasDuration)
            {
                endTime = Time.time + (duration > 0 ? duration : effect.Duration);
            }
            else // Infinite
            {
                endTime = float.PositiveInfinity;
            }

            var uid = _model.AddActiveEffect(effect, endTime);
            PublishSystemMessage($"지속 효과 적용: {effect.DisplayName} (UID: {uid})");
            return uid;
        }

        /// <summary>
        /// Config에서 효과를 적용합니다.
        /// </summary>
        public long ApplyGameplayEffect(GameplayEffectConfig config)
        {
            if (config == null) return -1;
            return ApplyGameplayEffect(config.ToDomain(), config.Duration);
        }

        /// <summary>
        /// UID로 효과를 제거합니다.
        /// </summary>
        public bool RemoveGameplayEffect(long effectUid)
        {
            if (!EnsureInitialized()) return false;

            var result = _model.RemoveActiveEffectByUid(effectUid);
            if (result)
            {
                PublishSystemMessage($"효과 제거: UID {effectUid}");
            }
            return result;
        }

        /// <summary>
        /// EffectId로 효과를 제거합니다.
        /// </summary>
        public bool RemoveGameplayEffect(string effectId)
        {
            if (!EnsureInitialized()) return false;

            var result = _model.RemoveActiveEffect(effectId);
            if (result)
            {
                PublishSystemMessage($"효과 제거: {effectId}");
            }
            return result;
        }

        private bool CanApplyEffect(GameplayEffect effect)
        {
            if (effect == null) return false;

            // 필수 태그 확인
            if (effect.RequiredTags != null && !OwnedTags.HasAll(effect.RequiredTags))
            {
                return false;
            }

            // 차단 태그 확인
            if (effect.BlockedTags != null && OwnedTags.HasAny(effect.BlockedTags))
            {
                return false;
            }

            return true;
        }

        private void ApplyModifiers(GameplayEffect effect)
        {
            if (effect?.ModifierGroups == null) return;

            foreach (var group in effect.ModifierGroups)
            {
                if (group?.Modifiers == null) continue;

                foreach (var modifier in group.Modifiers)
                {
                    ApplyModifier(modifier);
                }
            }
        }

        private void ApplyModifier(AttributeModifier modifier)
        {
            var attrId = modifier.AttributeId;
            if (!_model.Attributes.TryGet(attrId, out var attrValue)) return;

            var prevValue = attrValue.CurrentValue;

            switch (modifier.Operation)
            {
                case AttributeModifierOperationType.Add:
                    _model.Add(attrId, modifier.Magnitude);
                    break;
                case AttributeModifierOperationType.AddPercent:
                    _model.AddPercent(attrId, modifier.Magnitude / 100f);
                    break;
                case AttributeModifierOperationType.Multiply:
                    _model.Set(attrId, attrValue.CurrentValue * modifier.Magnitude);
                    break;
                case AttributeModifierOperationType.Override:
                    _model.Set(attrId, modifier.Magnitude);
                    break;
            }

            var newValue = _model.Get(attrId);
            if (!Mathf.Approximately(prevValue, newValue))
            {
                PublishSystemMessage($"속성 변경: {attrId.Name} {prevValue} → {newValue}");
            }
        }

        private void AddEffectTags(GameplayEffect effect)
        {
            if (effect?.GrantedTags == null) return;

            foreach (var tag in effect.GrantedTags.Tags)
            {
                var addedToOwned = _model.AddEffectTag(tag, out _);
                if (addedToOwned)
                {
                    _onAddedTag?.Invoke(this, tag);
                }
            }
        }

        private void RemoveEffectTags(GameplayEffect effect)
        {
            if (effect?.GrantedTags == null) return;

            foreach (var tag in effect.GrantedTags.Tags)
            {
                var removedFromOwned = _model.RemoveEffectTag(tag, out _);
                if (removedFromOwned)
                {
                    _onRemovedTag?.Invoke(this, tag);
                }
            }
        }

        #endregion

        #region 속성 관리 (Domain 위임)

        /// <summary>
        /// 속성 값을 조회합니다.
        /// </summary>
        public float GetAttribute(AttributeId id)
        {
            if (!EnsureInitialized()) return 0f;
            return _model.Get(id);
        }

        /// <summary>
        /// 속성 값을 설정합니다.
        /// </summary>
        public void SetAttribute(AttributeId id, float value)
        {
            if (!EnsureInitialized()) return;
            _model.Set(id, value);
        }

        /// <summary>
        /// 속성 값을 증감합니다.
        /// </summary>
        public void AddAttribute(AttributeId id, float delta)
        {
            if (!EnsureInitialized()) return;
            _model.Add(id, delta);
        }

        #endregion

        #region 이벤트

        /// <summary>
        /// 태그가 추가되었을 때 호출됩니다.
        /// </summary>
        public event Action<AbilitySystemComponentAdapter, FGameplayTag> OnAddedTag
        {
            add => _onAddedTag += value;
            remove => _onAddedTag -= value;
        }

        /// <summary>
        /// 태그가 제거되었을 때 호출됩니다.
        /// </summary>
        public event Action<AbilitySystemComponentAdapter, FGameplayTag> OnRemovedTag
        {
            add => _onRemovedTag += value;
            remove => _onRemovedTag -= value;
        }

        #endregion

        #region 유틸리티

        private bool EnsureInitialized()
        {
            if (_initialized && _model != null) return true;

            Debug.LogWarning("AbilitySystemComponentAdapter가 초기화되지 않았습니다.");
            return false;
        }

        private void PublishSystemMessage(string message)
        {
            if (!_emitSystemMessages) return;
            SystemMessageBus.Publish(message);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력 시스템 컴포넌트
    /// </summary>
    public sealed class AbilitySystemComponent : MonoBehaviour
    {
        /// <summary>
        /// 소유한 태그 컨테이너
        /// </summary>
        public GameplayTagContainer OwnedTags => _ownedTags;

        /// <summary>
        /// 소유한 능력들
        /// </summary>
        public IReadOnlyList<GameplayAbilitySpec> Abilities => _abilities;

        /// <summary>
        /// 소유자 컴포넌트
        /// </summary>
        public Component Owner => _owner;

        [SerializeField, ReadOnly] private GameplayTagContainer _ownedTags;

        [SerializeField] private List<GameplayAbilityDefinition> _startupAbilityDefinitions = new();

        private readonly Dictionary<FGameplayAbilitySpecHandle, GameplayAbilitySpec> _activatableAbilities = new();

        private List<GameplayAbilitySpec> _abilities = new();


        private readonly Dictionary<string, int> _effectTagCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        private readonly Dictionary<string, int> _looseTagCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        private readonly List<ActiveGameplayEffect> _activeEffects = new();

        private int _nextAbilityHandleId = 1;

        private Component _owner;

        private struct ActiveGameplayEffect
        {
            public GameplayEffectConfig Config;
            public float EndTime;
        }

        private void Awake()
        {
            var found = GetComponentsInChildren<IAbilitySystemProvider>();
            if (found.Length == 0)
            {
                Debug.LogError($"AbilitySystemComponent requires an IAbilitySystemInterface on {gameObject.name}.");
                return;
            }

            if (found.Length > 1)
            {
                Debug.LogWarning($"Multiple IAbilitySystemInterface components found on {gameObject.name}. Using the first one.");
            }

            _owner = found[0] as Component;
            ApplyStartupAbilities();
        }

        private void Update()
        {
            if (_activeEffects.Count == 0)
            {
                return;
            }

            var now = Time.time;
            for (var i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var active = _activeEffects[i];
                if (active.Config == null || now < active.EndTime)
                {
                    continue;
                }

                RemoveEffectTags(active.Config);
                _activeEffects.RemoveAt(i);
            }
        }

        /// <summary>
        /// 능력을 부여합니다.
        /// </summary>
        /// <param name="abilityDefinition"></param>
        /// <returns></returns>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                Debug.LogWarning("Ability definition is null.");
                return FGameplayAbilitySpecHandle.Invalid;
            }

            var abilityType = Type.GetType(abilityDefinition.AbilityTypeName);
            if (abilityType == null)
            {
                return FGameplayAbilitySpecHandle.Invalid;
            }

            if (!typeof(GameplayAbility).IsAssignableFrom(abilityType))
            {
                return FGameplayAbilitySpecHandle.Invalid;
            }

            try
            {
                var abilityInstance = (GameplayAbility)Activator.CreateInstance(abilityType);
                abilityInstance.InitializeAbility(this, abilityDefinition.Configs);
                return GiveAbility(abilityInstance);
            }
            catch
            {
                Debug.LogError($"Failed to create ability instance: {abilityDefinition.AbilityTypeName}");
                return FGameplayAbilitySpecHandle.Invalid;
            }
        }

        /// <summary>
        /// 능력을 부여합니다.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbility ability)
        {
            if (ability == null)
            {
                Debug.LogWarning("Ability is null.");
                return FGameplayAbilitySpecHandle.Invalid;
            }

            var spec = new GameplayAbilitySpec
            {
                Ability = ability,
                Level = 1,
                ActiveCount = 0,
                Handle = new FGameplayAbilitySpecHandle { Id = _nextAbilityHandleId++ }
            };

            _abilities.Add(spec);
            return spec.Handle;
        }

        /// <summary>
        /// 특정 타입의 능력을 활성화 시도합니다.
        /// </summary>
        /// <param name="abilityType"></param>
        /// <returns></returns>
        public bool TryActivateAbilityByType(Type abilityType)
        {
            if (abilityType == null)
            {
                Debug.LogWarning("Ability type is null.");
                return false;
            }

            foreach (var spec in _abilities)
            {
                if (spec == null) continue;
                if (spec.Ability == null) continue;
                if (spec.Ability.GetType() != abilityType) continue;

                return TryActivateAbility(spec);
            }

            return false;
        }

        /// <summary>
        /// 특정 태그를 가진 능력을 활성화 시도합니다.(복수 가능)
        /// </summary>
        /// <param name="abilityTag"></param>
        /// <returns></returns>
        public bool TryActivateAbilityByTag(FGameplayTag abilityTag)
        {
            var bSuccess = false;
            foreach (var spec in _abilities)
            {
                if (spec == null) continue;
                if (spec.Ability == null) continue;

                if (!spec.Ability.TryGetConfig<GameplayTagConfig>(out var tagConfig)) continue;
                if (tagConfig == null) continue;
                if (!tagConfig.AbilityTags.HasTag(abilityTag)) continue;

                bSuccess |= TryActivateAbility(spec);
            }

            return bSuccess;
        }

        /// <summary>
        /// 핸들을 통해 능력을 활성화 시도합니다.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool TryActivateAbility(FGameplayAbilitySpecHandle handle)
        {
            if (handle == FGameplayAbilitySpecHandle.Invalid)
            {
                Debug.LogWarning("Ability handle is invalid.");
                return false;
            }

            if (!FindAbilitySpec(handle, out var spec))
            {
                Debug.LogWarning($"Ability spec not found. Handle ID: {handle.Id}");
                return false;
            }

            return TryActivateAbility(spec);
        }

        /// <summary>
        /// 능력사양을 통해 능력을 활성화 시도합니다.
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec)
        {
            return TryActivateAbility(spec, default);
        }

        /// <summary>
        /// 최종적으로 해당 함수를 통해 능력 활성화를 시도하도록 합니다.
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        private bool TryActivateAbility(GameplayAbilitySpec spec, GameplayEventData eventData)
        {
            if (spec == null)
            {
                Debug.LogWarning("능력 사양이 null입니다.");
                return false;
            }

            var ability = spec.Ability;
            if (ability == null)
            {
                Debug.LogWarning($"능력 이 null입니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            if (!ability.TryGetConfig<GameplayTagConfig>(out var tagConfig))
            {
                Debug.LogWarning($"게임 태그 구성을 찾을 수 없습니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            if (!_ownedTags.HasAll(tagConfig.ActivationRequiredTags))
            {
                Debug.LogWarning($"필수 활성화 태그가 누락되었습니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            if (_ownedTags.HasAny(tagConfig.ActivationBlockedTags))
            {
                Debug.LogWarning($"차단 태그로 인해 활성화할 수 없습니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            if (!ability.CanActivateAbility(spec.Handle, _ownedTags, _ownedTags))
            {
                Debug.LogWarning($"능력을 활성화할 수 없습니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            ability.CallActivateAbility(spec.Handle, eventData);

            //능력 발휘 시 효과 적용
            //효과는 여러개 일 수 있을 듯
            if (ability.TryGetConfigs<GameplayEffectConfig>(out var effectConfigs))
            {
                ApplyGameplayEffect(effectConfigs);
            }

            return true;
        }

        /// <summary>
        /// 게임플레이 이벤트를 처리합니다. 이벤트 태그로 트리거되는 능력을 활성화합니다.
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public bool HandleGameplayEvent(GameplayEventData eventData)
        {
            if (!eventData.EventTag.IsValid)
            {
                Debug.LogWarning("이벤트 태그가 유효하지 않습니다.");
                return false;
            }

            var eventTag = eventData.EventTag;
            var bSuccess = false;
            foreach (var spec in _abilities)
            {
                if (spec == null) continue;
                if (spec.Ability == null) continue;

                if (!spec.Ability.TryGetConfig<GameplayEventTriggerConfig>(out var triggerConfig))
                {
                    continue;
                }

                if (triggerConfig == null || !triggerConfig.ActivateOnEvent)
                {
                    continue;
                }

                if (!IsEventTagMatch(eventTag, triggerConfig.TriggerTag))
                {
                    continue;
                }

                bSuccess |= TryActivateAbility(spec, eventData);
            }

            return bSuccess;
        }

        /// <summary>
        /// 느슨한 태그를 추가합니다.
        /// </summary>
        /// <param name="tag"></param>
        public void AddLooseTag(FGameplayTag tag)
        {
            AddTagInternal(_looseTagCounts, tag);
        }

        /// <summary>
        /// 느슨한 태그를 제거합니다.
        /// </summary>
        /// <param name="tag"></param>
        public void RemoveLooseTag(FGameplayTag tag)
        {
            RemoveTagInternal(_looseTagCounts, tag);
        }

        /// <summary>
        /// 핸들을 통해 능력 사양을 찾습니다.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="outSpec"></param>
        /// <returns></returns>
        private bool FindAbilitySpec(FGameplayAbilitySpecHandle handle, out GameplayAbilitySpec outSpec)
        {
            outSpec = null;
            foreach (var spec in _abilities)
            {
                if (spec == null) continue;
                if (handle != spec.Handle) continue;

                outSpec = spec;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 시작 능력을 적용합니다.
        /// </summary>
        private void ApplyStartupAbilities()
        {
            foreach (var elem in _startupAbilityDefinitions)
            {
                if (elem == null) continue;
                GiveAbility(elem);
            }
        }

        /// <summary>
        /// 태그를 내부적으로 추가합니다.
        /// </summary>
        /// <param name="counts"></param>
        /// <param name="tag"></param>
        private void AddTagInternal(Dictionary<string, int> counts, FGameplayTag tag)
        {
            var value = tag.Value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var beforeTotal = GetTotalTagCount(value);
            counts.TryGetValue(value, out var count);
            count++;
            counts[value] = count;

            if (beforeTotal == 0)
            {
                _ownedTags.AddTag(tag);
            }
        }

        /// <summary>
        /// 태그를 내부적으로 제거합니다.
        /// </summary>
        /// <param name="counts"></param>
        /// <param name="tag"></param>
        private void RemoveTagInternal(Dictionary<string, int> counts, FGameplayTag tag)
        {
            var value = tag.Value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (!counts.TryGetValue(value, out var count))
            {
                return;
            }

            count--;
            if (count <= 0)
            {
                counts.Remove(value);
            }
            else
            {
                counts[value] = count;
            }

            if (GetTotalTagCount(value) == 0)
            {
                _ownedTags.RemoveTag(new FGameplayTag(value));
            }
        }

        /// <summary>
        /// 태그의 총 개수를 가져옵니다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int GetTotalTagCount(string value)
        {
            _effectTagCounts.TryGetValue(value, out var effectCount);
            _looseTagCounts.TryGetValue(value, out var looseCount);
            return effectCount + looseCount;
        }

        /// <summary>
        /// 이벤트 태그가 트리거 태그와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="eventTag"></param>
        /// <param name="triggerTag"></param>
        /// <returns></returns>
        private static bool IsEventTagMatch(FGameplayTag eventTag, FGameplayTag triggerTag)
        {
            if (!eventTag.IsValid || !triggerTag.IsValid)
            {
                return false;
            }

            if (eventTag.Equals(triggerTag))
            {
                return true;
            }

            foreach (var parent in global::GameplayTagUtility.EnumerateParents(eventTag.Value))
            {
                if (string.Equals(parent, triggerTag.Value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 게임플레이 효과를 적용합니다.
        /// </summary>
        /// <param name="effectConfigs"></param>
        private void ApplyGameplayEffect(IReadOnlyList<GameplayEffectConfig> effectConfigs)
        {
            for (var i = 0; i < effectConfigs.Count; i++)
            {
                ApplyGameplayEffect(effectConfigs[i]);
            }
        }

        /// <summary>
        /// 게임플레이 효과를 적용합니다.
        /// </summary>
        /// <param name="effectConfig"></param>
        private void ApplyGameplayEffect(GameplayEffectConfig effectConfig)
        {
            if (effectConfig == null)
            {
                return;
            }

            //Attribute 변경 수행

            if (effectConfig.DurationType == EGameplayEffectDurationType.Instant)
            {
                //즉시 효과는 Attribute 변경만 수행하고 태그는 적용하지 않음
                return;
            }

            AddEffectTags(effectConfig);

            if (effectConfig.DurationType == EGameplayEffectDurationType.HasDuration)
            {
                var duration = Mathf.Max(0f, effectConfig.Duration);
                if (duration <= 0f)
                {
                    RemoveEffectTags(effectConfig);
                    return;
                }

                _activeEffects.Add(new ActiveGameplayEffect
                {
                    Config = effectConfig,
                    EndTime = Time.time + duration
                });
            }
        }

        private void AddEffectTags(GameplayEffectConfig effectConfig)
        {
            foreach (var tag in effectConfig.GrantedTags.Tags)
            {
                AddTagInternal(_effectTagCounts, tag);
            }
        }

        private void RemoveEffectTags(GameplayEffectConfig effectConfig)
        {
            foreach (var tag in effectConfig.GrantedTags.Tags)
            {
                RemoveTagInternal(_effectTagCounts, tag);
            }
        }
    }
}

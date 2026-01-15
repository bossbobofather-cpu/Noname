using System;
using System.Collections.Generic;
using noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력 시스템 컴포넌트
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AbilitySystemComponent : MonoBehaviour
    {
        [SerializeField] private List<AttributeDefinition> _attributeDefaults = new List<AttributeDefinition>();
        [SerializeField, ReadOnly] private GameplayTagContainer _ownedTags;

        [SerializeField] private List<GameplayAbilityDefinition> _startupAbilityDefinitions = new();
        [SerializeField] private bool _emitSystemMessages = true;

        private readonly Dictionary<FGameplayAbilitySpecHandle, GameplayAbilitySpec> _activatableAbilities = new();

        private List<GameplayAbilitySpec> _abilities = new();
        private readonly Dictionary<FGameplayAbilitySpecHandle, List<GameplayAbilityInstance>> _activeInstances = new();


        private readonly Dictionary<int, int> _effectTagCounts = new Dictionary<int, int>();

        private readonly Dictionary<int, int> _looseTagCounts = new Dictionary<int, int>();

        private readonly List<ActiveGameplayEffect> _activeEffects = new();
        private readonly AttributeSet _attributes = new AttributeSet();

        private int _nextAbilityHandleId = 1;

        private Component _owner;
        private event Action<AbilitySystemComponent, AttributeModifier, AttributeValue, AttributeValue> _onChangedAttributeModifier;
        private event Action<AbilitySystemComponent, FGameplayTag> _onAddedTag;
        private event Action<AbilitySystemComponent, FGameplayTag> _onRemovedTag;
        private event Action<AbilitySystemComponent, GameplayEventData> _onGameplayEvent;

        private struct ActiveGameplayEffect
        {
            public GameplayEffectConfig Config;
            public float EndTime;
        }

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

        /// <summary>
        /// 속성 집합
        /// </summary>
        public AttributeSet Attributes => _attributes;

        private void Awake()
        {
            _attributes.Initialize(_attributeDefaults);

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

                // Swap and Pop 최적화: 순서가 중요하지 않으므로 마지막 요소와 교체 후 제거
                var lastIndex = _activeEffects.Count - 1;
                if (i < lastIndex)
                {
                    _activeEffects[i] = _activeEffects[lastIndex];
                }
                _activeEffects.RemoveAt(lastIndex);
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

            return GiveAbilityInternal(abilityType, abilityDefinition.Configs, abilityDefinition.name);
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

            var abilityType = ability.GetType();
            var abilityName = abilityType != null ? abilityType.Name : string.Empty;
            return GiveAbilityInternal(abilityType, ability.Configs, abilityName);
        }

        public bool RemoveAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                return false;
            }

            var abilityType = Type.GetType(abilityDefinition.AbilityTypeName);
            return RemoveAbilityByTypeInternal(abilityType, abilityDefinition.name);
        }

        public bool RemoveAbility(FGameplayAbilitySpecHandle handle)
        {
            if (handle == FGameplayAbilitySpecHandle.Invalid)
            {
                return false;
            }

            for (var i = _abilities.Count - 1; i >= 0; i--)
            {
                var spec = _abilities[i];
                if (spec == null || spec.Handle != handle)
                {
                    continue;
                }

                EndAbilityInstances(handle);
                PublishSystemMessage($"능력 해제: {ResolveAbilityLabel(spec)}");
                _abilities.RemoveAt(i);
                return true;
            }

            return false;
        }

        public bool RemoveAbilityByType(Type abilityType)
        {
            return RemoveAbilityByTypeInternal(abilityType, string.Empty);
        }

        public bool EndAbility(FGameplayAbilitySpecHandle handle)
        {
            if (handle == FGameplayAbilitySpecHandle.Invalid)
            {
                return false;
            }

            if (!FindAbilitySpec(handle, out var spec))
            {
                return false;
            }

            return EndAbilityInstances(handle);
        }

        public bool EndAbilityByType(Type abilityType)
        {
            if (abilityType == null)
            {
                return false;
            }

            for (var i = 0; i < _abilities.Count; i++)
            {
                var spec = _abilities[i];
                if (spec == null || spec.AbilityType == null)
                {
                    continue;
                }

                if (spec.AbilityType != abilityType)
                {
                    continue;
                }

                return EndAbilityInstances(spec.Handle);
            }

            return false;
        }

        public bool EndAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                return false;
            }

            var abilityType = Type.GetType(abilityDefinition.AbilityTypeName);
            if (!TryGetAbilitySpec(abilityType, abilityDefinition.name, out var spec))
            {
                return false;
            }

            return EndAbilityInstances(spec.Handle);
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
                if (spec.AbilityType == null) continue;
                if (spec.AbilityType != abilityType) continue;

                return TryActivateAbility(spec);
            }

            return false;
        }

        public bool TryActivateAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                return false;
            }

            var abilityType = Type.GetType(abilityDefinition.AbilityTypeName);
            if (!TryGetAbilitySpec(abilityType, abilityDefinition.name, out var spec))
            {
                return false;
            }

            return TryActivateAbility(spec);
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
                if (spec.AbilityType == null) continue;

                if (!spec.TryGetConfig<GameplayTagConfig>(out var tagConfig)) continue;
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

            if (spec.AbilityType == null)
            {
                Debug.LogWarning($"능력 타입이 null입니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            if (!spec.TryGetConfig<GameplayTagConfig>(out var tagConfig))
            {
                Debug.LogWarning($"게임 태그 구성을 찾을 수 없습니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            if (!_ownedTags.HasAll(tagConfig.ActivationRequiredTags))
            {
                Debug.LogWarning($"필수 활성화 태그가 누락되었습니다. 핸들 ID: {spec.Handle.Id}");

                SystemMessageBus.Publish($"능력 활성화 차단됨: {ResolveAbilityLabel(spec)} (필수 태그 누락)");
                return false;
            }

            if (_ownedTags.HasAny(tagConfig.ActivationBlockedTags))
            {
                Debug.LogWarning($"차단 태그로 인해 활성화할 수 없습니다. 핸들 ID: {spec.Handle.Id}");

                SystemMessageBus.Publish($"능력 활성화 차단됨: {ResolveAbilityLabel(spec)} (차단 태그 존재)");

                return false;
            }

            var ability = CreateAbilityInstance(spec);
            if (ability == null)
            {
                Debug.LogWarning($"능력 인스턴스를 생성할 수 없습니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            if (!ability.CanActivateAbility())
            {
                Debug.LogWarning($"능력을 활성화할 수 없습니다. 핸들 ID: {spec.Handle.Id}");
                return false;
            }

            var context = new AbilityContext(spec.Handle, eventData);
            var instance = new GameplayAbilityInstance(this, ability, context);
            RegisterAbilityInstance(spec, instance);
            instance.Activate();

            //능력 발휘 시 효과 적용
            //효과는 여러개 일 수 있을 듯
            if (spec.TryGetConfigs<GameplayEffectConfig>(out var effectConfigs))
            {
                ApplyGameplayEffect(effectConfigs);
            }

            return true;
        }

        private GameplayAbility CreateAbilityInstance(GameplayAbilitySpec spec)
        {
            if (spec == null || spec.AbilityType == null)
            {
                return null;
            }

            if (!typeof(GameplayAbility).IsAssignableFrom(spec.AbilityType))
            {
                return null;
            }

            try
            {
                var ability = (GameplayAbility)Activator.CreateInstance(spec.AbilityType);
                ability.InitializeAbility(this, spec.Configs);
                return ability;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create ability instance: {spec.AbilityType}. {ex.Message}");
                return null;
            }
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
            _onGameplayEvent?.Invoke(this, eventData);
            var bSuccess = false;
            foreach (var spec in _abilities)
            {
                if (spec == null) continue;
                if (spec.AbilityType == null) continue;

                if (!spec.TryGetConfigs<GameplayEventTriggerConfig>(out var triggerConfigs))
                {
                    continue;
                }

                var matched = false;
                for (var i = 0; i < triggerConfigs.Count; i++)
                {
                    var triggerConfig = triggerConfigs[i];
                    if (triggerConfig == null || !triggerConfig.ActivateOnEvent)
                    {
                        continue;
                    }

                    if (!IsEventTagMatch(eventTag, triggerConfig.TriggerTag))
                    {
                        continue;
                    }

                    matched = true;
                    break;
                }

                if (matched)
                {
                    bSuccess |= TryActivateAbility(spec, eventData);
                }
            }

            return bSuccess;
        }

        /// <summary>
        /// 느슨한 태그를 추가합니다.
        /// </summary>
        /// <param name="tag"></param>
        public void AddLooseTag(FGameplayTag tag)
        {
            AddTagInternal(_looseTagCounts, tag, isLooseTag: true);
        }

        /// <summary>
        /// 느슨한 태그를 제거합니다.
        /// </summary>
        /// <param name="tag"></param>
        public void RemoveLooseTag(FGameplayTag tag)
        {
            RemoveTagInternal(_looseTagCounts, tag, isLooseTag: true);
        }

        private void RegisterAbilityInstance(GameplayAbilitySpec spec, GameplayAbilityInstance instance)
        {
            if (spec == null || instance == null)
            {
                return;
            }

            if (!_activeInstances.TryGetValue(spec.Handle, out var list))
            {
                list = new List<GameplayAbilityInstance>();
                _activeInstances[spec.Handle] = list;
            }

            list.Add(instance);
            spec.ActiveCount = list.Count;
        }

        private bool EndAbilityInstances(FGameplayAbilitySpecHandle handle)
        {
            if (!_activeInstances.TryGetValue(handle, out var list))
            {
                return false;
            }

            var snapshot = list.ToArray();
            list.Clear();
            _activeInstances.Remove(handle);

            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i]?.End();
            }

            if (FindAbilitySpec(handle, out var spec))
            {
                spec.ActiveCount = 0;
            }

            return snapshot.Length > 0;
        }

        public event Action<AbilitySystemComponent, AttributeModifier, AttributeValue, AttributeValue> onChangedAttributeModifier
        {
            add => _onChangedAttributeModifier += value;
            remove => _onChangedAttributeModifier -= value;
        }

        public event Action<AbilitySystemComponent, FGameplayTag> onAddedTag
        {
            add => _onAddedTag += value;
            remove => _onAddedTag -= value;
        }

        public event Action<AbilitySystemComponent, FGameplayTag> onRemovedTag
        {
            add => _onRemovedTag += value;
            remove => _onRemovedTag -= value;
        }

        public event Action<AbilitySystemComponent, GameplayEventData> onGameplayEvent
        {
            add => _onGameplayEvent += value;
            remove => _onGameplayEvent -= value;
        }

        public void GetActiveEffects(List<GameplayEffectConfig> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            for (var i = 0; i < _activeEffects.Count; i++)
            {
                var config = _activeEffects[i].Config;
                if (config != null)
                {
                    results.Add(config);
                }
            }
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

        private bool TryGetAbilitySpec(Type abilityType, string abilityName, out GameplayAbilitySpec outSpec)
        {
            outSpec = null;
            if (abilityType == null)
            {
                return false;
            }

            foreach (var spec in _abilities)
            {
                if (spec == null || spec.AbilityType == null)
                {
                    continue;
                }

                if (spec.AbilityType != abilityType)
                {
                    continue;
                }

                if (!IsAbilityNameMatch(spec, abilityName))
                {
                    continue;
                }

                outSpec = spec;
                return true;
            }

            return false;
        }

        private FGameplayAbilitySpecHandle GiveAbilityInternal(Type abilityType, IReadOnlyList<GameplayConfig> configs, string abilityName)
        {
            if (abilityType == null)
            {
                Debug.LogWarning("Ability type is null.");
                return FGameplayAbilitySpecHandle.Invalid;
            }

            var spec = new GameplayAbilitySpec
            {
                AbilityType = abilityType,
                AbilityName = abilityName,
                Configs = configs ?? Array.Empty<GameplayConfig>(),
                Level = 1,
                ActiveCount = 0,
                Handle = new FGameplayAbilitySpecHandle { Id = _nextAbilityHandleId++ }
            };

            _abilities.Add(spec);
            PublishSystemMessage($"능력 부착: {ResolveAbilityLabel(spec)}");
            return spec.Handle;
        }

        private bool RemoveAbilityByTypeInternal(Type abilityType, string abilityName)
        {
            if (abilityType == null)
            {
                return false;
            }

            for (var i = _abilities.Count - 1; i >= 0; i--)
            {
                var spec = _abilities[i];
                if (spec == null || spec.AbilityType == null)
                {
                    continue;
                }

                if (spec.AbilityType != abilityType)
                {
                    continue;
                }

                if (!IsAbilityNameMatch(spec, abilityName))
                {
                    continue;
                }

                EndAbilityInstances(spec.Handle);
                _abilities.RemoveAt(i);

                PublishSystemMessage($"능력 해제: {ResolveAbilityLabel(spec, abilityName)}");
                return true;
            }

            return false;
        }

        private static bool IsAbilityNameMatch(GameplayAbilitySpec spec, string abilityName)
        {
            if (string.IsNullOrWhiteSpace(abilityName))
            {
                return true;
            }

            if (spec == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(spec.AbilityName))
            {
                return string.Equals(spec.AbilityName, abilityName, StringComparison.Ordinal);
            }

            var typeName = spec.AbilityType != null ? spec.AbilityType.Name : string.Empty;
            return string.Equals(typeName, abilityName, StringComparison.Ordinal);
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

        private void PublishSystemMessage(string message)
        {
            if (!_emitSystemMessages)
            {
                return;
            }

            SystemMessageBus.Publish(message);
        }

        private static string ResolveAbilityLabel(GameplayAbilitySpec spec, string fallback = null)
        {
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }

            if (spec != null && !string.IsNullOrWhiteSpace(spec.AbilityName))
            {
                return spec.AbilityName;
            }

            if (spec?.AbilityType != null)
            {
                return spec.AbilityType.Name;
            }

            return "Ability";
        }

        private static bool IsTagMessageIgnored(FGameplayTag tag)
        {
            var registry = GameplayTagRegistry.RuntimeRegistry;
            return registry != null && registry.IsSystemMessageIgnored(tag);
        }

        /// <summary>
        /// 태그를 내부적으로 추가합니다.
        /// </summary>
        /// <param name="counts"></param>
        /// <param name="tag"></param>
        /// <param name="isLooseTag">느슨한 태그 여부 (로그용)</param>
        private void AddTagInternal(Dictionary<int, int> counts, FGameplayTag tag, bool isLooseTag = false)
        {
            if (!tag.IsValid)
            {
                return;
            }

            var hash = tag.Hash;
            var beforeTotal = GetTotalTagCount(hash);
            counts.TryGetValue(hash, out var count);
            count++;
            counts[hash] = count;

            if (beforeTotal == 0)
            {
                _ownedTags.AddTag(tag);

                _onAddedTag?.Invoke(this, tag);
            }

            // 로그 메시지가 켜져 있고, 시스템 메시지 무시 태그가 아닐 때만 로그 발생 (느슨한 태그일 때만 주로 로그를 남기거나 필요에 따라 조정)
            if (isLooseTag && !IsTagMessageIgnored(tag))
            {
                PublishSystemMessage($"태그 추가: {tag.Value} (총 {GetTotalTagCount(hash)})");
            }
        }

        /// <summary>
        /// 태그를 내부적으로 제거합니다.
        /// </summary>
        /// <param name="counts"></param>
        /// <param name="tag"></param>
        /// <param name="isLooseTag">느슨한 태그 여부 (로그용)</param>
        private void RemoveTagInternal(Dictionary<int, int> counts, FGameplayTag tag, bool isLooseTag = false)
        {
            if (!tag.IsValid)
            {
                return;
            }

            var hash = tag.Hash;
            if (!counts.TryGetValue(hash, out var count))
            {
                return;
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

            if (GetTotalTagCount(hash) == 0)
            {
                _ownedTags.RemoveTag(tag);

                _onRemovedTag?.Invoke(this, tag);
            }

            if (isLooseTag && !IsTagMessageIgnored(tag))
            {
                PublishSystemMessage($"태그 제거: {tag.Value} (총 {GetTotalTagCount(hash)})");
            }
        }

        /// <summary>
        /// 태그의 총 개수를 가져옵니다.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private int GetTotalTagCount(int hash)
        {
            _effectTagCounts.TryGetValue(hash, out var effectCount);
            _looseTagCounts.TryGetValue(hash, out var looseCount);
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

            // 최적화: 부모 태그를 열거(Substring 할당)하고 해시하는 대신, 문자열 포함 여부만 확인
            // FGameplayTag는 내부적으로 문자열을 가지고 있으므로 이를 활용하여 Zero Allocation 구현
            return GameplayTagUtility.IsDescendant(eventTag.Value, triggerTag.Value);
        }

        /// <summary>
        /// 게임플레이 효과를 적용합니다.
        /// </summary>
        /// <param name="effectConfigs"></param>
        private void ApplyGameplayEffect(IReadOnlyList<GameplayEffectConfig> effectConfigs)
        {
            var context = new GameplayEffectContext(this, this, default);
            ApplyGameplayEffect(effectConfigs, context);
        }

        /// <summary>
        /// 게임플레이 효과를 적용합니다.
        /// </summary>
        /// <param name="effectConfig"></param>
        public void ApplyGameplayEffect(GameplayEffectConfig effectConfig)
        {
            ApplyGameplayEffect(effectConfig, new GameplayEffectContext(this, this, default));
        }

        public void ApplyGameplayEffect(GameplayEffectConfig effectConfig, GameplayEffectContext context)
        {
            if (effectConfig == null)
            {
                return;
            }

            //Attribute 변경 수행
            ApplyModifiers(effectConfig, context);

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
            else if (effectConfig.DurationType == EGameplayEffectDurationType.Infinite)
            {
                _activeEffects.Add(new ActiveGameplayEffect
                {
                    Config = effectConfig,
                    EndTime = float.PositiveInfinity
                });
            }
        }

        private void ApplyGameplayEffect(IReadOnlyList<GameplayEffectConfig> effectConfigs, GameplayEffectContext context)
        {
            if (effectConfigs == null)
            {
                return;
            }

            for (var i = 0; i < effectConfigs.Count; i++)
            {
                ApplyGameplayEffect(effectConfigs[i], context);
            }
        }

        public bool RemoveGameplayEffect(GameplayEffectConfig effectConfig)
        {
            if (effectConfig == null)
            {
                return false;
            }

            for (var i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var active = _activeEffects[i];
                if (active.Config != effectConfig)
                {
                    continue;
                }

                RemoveEffectTags(effectConfig);
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

        /// <summary>
        /// 게임플레이 효과 태그를 추가합니다.
        /// </summary>
        /// <param name="effectConfig"></param>
        private void AddEffectTags(GameplayEffectConfig effectConfig)
        {
            foreach (var tag in effectConfig.GrantedTags.Tags)
            {
                AddTagInternal(_effectTagCounts, tag, isLooseTag: false);
            }
        }

        /// <summary>
        /// 게임플레이 효과 태그를 제거합니다.
        /// </summary>
        /// </summary>
        /// <param name="effectConfig"></param>
        private void RemoveEffectTags(GameplayEffectConfig effectConfig)
        {
            foreach (var tag in effectConfig.GrantedTags.Tags)
            {
                RemoveTagInternal(_effectTagCounts, tag, isLooseTag: false);
            }
        }

        /// <summary>
        /// 속성 수정자를 적용합니다.
        /// </summary>
        /// <param name="effectConfig"></param>
        /// <param name="context"></param>
        private void ApplyModifiers(GameplayEffectConfig effectConfig, GameplayEffectContext context)
        {
            var modifiers = effectConfig.Modifiers;
            if (modifiers == null)
            {
                return;
            }

            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier.Attribute == null)
                {
                    continue;
                }

                if (!_attributes.TryGet(modifier.Attribute, out var value))
                {
                    continue;
                }

                var prevValue = value;
                var magnitude = ResolveModifierMagnitude(modifier, effectConfig, context);
                switch (modifier.Operation)
                {
                    case GameplayEffectModifierOperation.Add:
                        value.CurrentValue += magnitude;
                        break;
                    case GameplayEffectModifierOperation.Multiply:
                        value.CurrentValue *= magnitude;
                        break;
                    case GameplayEffectModifierOperation.Override:
                        value.CurrentValue = magnitude;
                        break;
                }

                if (prevValue == value)
                {
                    continue;
                }

                OnChangedAttributeModifier(modifier, prevValue, value);

                // 로그 메시지 생성 비용(문자열 할당)을 줄이기 위해 플래그 먼저 확인
                if (_emitSystemMessages)
                {
                    SystemMessageBus.Publish(
                        $"속성 수정: {modifier.Attribute.name} {prevValue.CurrentValue} 에서 {value.CurrentValue} 로 (연산: {modifier.Operation}, 크기: {magnitude})");
                }
            }
        }

        private static float ResolveModifierMagnitude(
            AttributeModifier modifier,
            GameplayEffectConfig effectConfig,
            GameplayEffectContext context)
        {
            switch (modifier.ValueMode)
            {
                case AttributeModifierValueMode.Calculated:
                    return EvaluateModifierCalculator(modifier, effectConfig, context);
                case AttributeModifierValueMode.StaticPlusCalculated:
                    return modifier.Magnitude + EvaluateModifierCalculator(modifier, effectConfig, context);
                default:
                    return modifier.Magnitude;
            }
        }

        private static float EvaluateModifierCalculator(
            AttributeModifier modifier,
            GameplayEffectConfig effectConfig,
            GameplayEffectContext context)
        {
            if (modifier.Calculator == null)
            {
                return 0f;
            }

            return modifier.Calculator.EvaluateMagnitude(effectConfig, modifier, context);
        }

        private void OnChangedAttributeModifier(AttributeModifier modifier, AttributeValue prevValue, AttributeValue value)
        {
            _onChangedAttributeModifier?.Invoke(this, modifier, prevValue, value);
        }
    }
}

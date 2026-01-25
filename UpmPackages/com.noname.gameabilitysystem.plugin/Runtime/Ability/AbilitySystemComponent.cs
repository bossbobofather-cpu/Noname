using System;
using System.Collections.Generic;
using noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력 시스템을 관리하는 컴포넌트입니다.
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


        private readonly AttributeSet _attributes = new AttributeSet();
        private readonly List<GameplayEffectConfig> _expiredEffects = new();
        private AbilitySystemModel _model;

        private int _nextAbilityHandleId = 1;

        private Component _owner;
        private event Action<AbilitySystemComponent, AttributeModifier, AttributeValue, AttributeValue> _onChangedAttributeModifier;
        private event Action<AbilitySystemComponent, FGameplayTag> _onAddedTag;
        private event Action<AbilitySystemComponent, FGameplayTag> _onRemovedTag;
        private event Action<AbilitySystemComponent, GameplayEventData> _onGameplayEvent;

        /// <summary>
        /// 소유한 태그 컨테이너
        /// </summary>
        public GameplayTagContainer OwnedTags => EnsureModel().OwnedTags;

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
        public AttributeSet Attributes => EnsureModel().Attributes;

        /// <summary>
        /// 순수 데이터 모델입니다.
        /// </summary>
        public AbilitySystemModel Model => EnsureModel();

        private void Awake()
        {
            EnsureModel();

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

            // 소유자 참조를 저장하고 시작 능력을 적용한다.
            _owner = found[0] as Component;
            ApplyStartupAbilities();
        }

        private AbilitySystemModel EnsureModel()
        {
            if (_model != null)
            {
                return _model;
            }

            // 기본 속성 값을 초기화한다.
            _attributes.Initialize(_attributeDefaults);
            if (_ownedTags == null)
            {
                _ownedTags = new GameplayTagContainer();
            }

            _model = new AbilitySystemModel(_attributes, _ownedTags);
            return _model;
        }

        private void Update()
        {
            var model = EnsureModel();
            if (model.ActiveEffectCount == 0)
            {
                return;
            }

            _expiredEffects.Clear();
            model.CollectExpiredEffects(Time.time, _expiredEffects);
            if (_expiredEffects.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _expiredEffects.Count; i++)
            {
                RemoveEffectTags(_expiredEffects[i]);
            }
        }

        /// <summary>
        /// 능력 정의 에셋을 기반으로 능력을 부여합니다.
        /// </summary>
        /// <param name="abilityDefinition">능력 정의 에셋</param>
        /// <returns>부여된 능력의 핸들</returns>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                Debug.LogWarning("Ability definition is null.");
                return FGameplayAbilitySpecHandle.Invalid;
            }

            // 정의에 있는 타입 정보를 사용해 부여한다.
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
        /// 능력 인스턴스를 기반으로 능력을 부여합니다.
        /// </summary>
        /// <param name="ability">능력 인스턴스</param>
        /// <returns>부여된 능력의 핸들</returns>
        public FGameplayAbilitySpecHandle GiveAbility(GameplayAbility ability)
        {
            if (ability == null)
            {
                Debug.LogWarning("Ability is null.");
                return FGameplayAbilitySpecHandle.Invalid;
            }

            // 인스턴스 타입과 구성을 사용해 부여한다.
            var abilityType = ability.GetType();
            var abilityName = abilityType != null ? abilityType.Name : string.Empty;
            return GiveAbilityInternal(abilityType, ability.Configs, abilityName);
        }

        /// <summary>
        /// 능력 정의 에셋을 기준으로 능력을 제거합니다.
        /// </summary>
        /// <param name="abilityDefinition">능력 정의 에셋</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                return false;
            }

            // 정의에서 타입을 찾아 제거한다.
            var abilityType = Type.GetType(abilityDefinition.AbilityTypeName);
            return RemoveAbilityByTypeInternal(abilityType, abilityDefinition.name);
        }

        /// <summary>
        /// 핸들로 능력을 제거합니다.
        /// </summary>
        /// <param name="handle">능력 핸들</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveAbility(FGameplayAbilitySpecHandle handle)
        {
            if (handle == FGameplayAbilitySpecHandle.Invalid)
            {
                return false;
            }

            // 핸들로 목록에서 제거한다.
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

        /// <summary>
        /// 타입으로 능력을 제거합니다.
        /// </summary>
        /// <param name="abilityType">능력 타입</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveAbilityByType(Type abilityType)
        {
            // 타입 기준 제거를 위임한다.
            return RemoveAbilityByTypeInternal(abilityType, string.Empty);
        }

        /// <summary>
        /// 핸들로 능력을 종료합니다.
        /// </summary>
        /// <param name="handle">능력 핸들</param>
        /// <returns>종료 성공 여부</returns>
        public bool EndAbility(FGameplayAbilitySpecHandle handle)
        {
            if (handle == FGameplayAbilitySpecHandle.Invalid)
            {
                return false;
            }

            // 핸들로 스펙을 찾아 종료한다.
            if (!FindAbilitySpec(handle, out var spec))
            {
                return false;
            }

            return EndAbilityInstances(handle);
        }

        /// <summary>
        /// 타입으로 능력을 종료합니다.
        /// </summary>
        /// <param name="abilityType">능력 타입</param>
        /// <returns>종료 성공 여부</returns>
        public bool EndAbilityByType(Type abilityType)
        {
            if (abilityType == null)
            {
                return false;
            }

            // 타입이 일치하는 스펙을 찾아 종료한다.
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

        /// <summary>
        /// 능력 정의 에셋을 기준으로 능력을 종료합니다.
        /// </summary>
        /// <param name="abilityDefinition">능력 정의 에셋</param>
        /// <returns>종료 성공 여부</returns>
        public bool EndAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                return false;
            }

            // 정의 이름까지 고려해 스펙을 찾는다.
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
        /// <param name="abilityType">능력 타입</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbilityByType(Type abilityType)
        {
            if (abilityType == null)
            {
                Debug.LogWarning("Ability type is null.");
                return false;
            }

            // 타입이 일치하는 스펙을 찾아 활성화한다.
            foreach (var spec in _abilities)
            {
                if (spec == null) continue;
                if (spec.AbilityType == null) continue;
                if (spec.AbilityType != abilityType) continue;

                return TryActivateAbility(spec);
            }

            return false;
        }

        /// <summary>
        /// 능력 정의 에셋으로 활성화를 시도합니다.
        /// </summary>
        /// <param name="abilityDefinition">능력 정의 에셋</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbility(GameplayAbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null)
            {
                return false;
            }

            // 정의에서 타입을 얻어 활성화를 시도한다.
            var abilityType = Type.GetType(abilityDefinition.AbilityTypeName);
            if (!TryGetAbilitySpec(abilityType, abilityDefinition.name, out var spec))
            {
                return false;
            }

            return TryActivateAbility(spec);
        }

        /// <summary>
        /// 특정 태그를 가진 능력을 활성화 시도합니다. 여러 개일 수 있습니다.
        /// </summary>
        /// <param name="abilityTag">능력 태그</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbilityByTag(FGameplayTag abilityTag)
        {
            // 태그가 포함된 스펙을 모두 시도한다.
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
        /// 핸들로 능력을 활성화 시도합니다.
        /// </summary>
        /// <param name="handle">능력 핸들</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbility(FGameplayAbilitySpecHandle handle)
        {
            if (handle == FGameplayAbilitySpecHandle.Invalid)
            {
                Debug.LogWarning("Ability handle is invalid.");
                return false;
            }

            // 핸들로 스펙을 찾아 활성화한다.
            if (!FindAbilitySpec(handle, out var spec))
            {
                Debug.LogWarning($"Ability spec not found. Handle ID: {handle.Id}");
                return false;
            }

            return TryActivateAbility(spec);
        }

        /// <summary>
        /// 능력 사양으로 능력을 활성화 시도합니다.
        /// </summary>
        /// <param name="spec">능력 사양</param>
        /// <returns>활성화 성공 여부</returns>
        public bool TryActivateAbility(GameplayAbilitySpec spec)
        {
            // 이벤트 없이 활성화를 시도한다.
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

            // 타입과 태그 조건을 먼저 확인한다.
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

            if (!OwnedTags.HasAll(tagConfig.ActivationRequiredTags))
            {
                Debug.LogWarning($"필수 활성화 태그가 누락되었습니다. 핸들 ID: {spec.Handle.Id}");

                SystemMessageBus.Publish($"능력 활성화 차단됨: {ResolveAbilityLabel(spec)} (필수 태그 누락)");
                return false;
            }

            if (OwnedTags.HasAny(tagConfig.ActivationBlockedTags))
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

            // 타입이 올바른지 확인하고 인스턴스를 만든다.
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
        /// <param name="eventData">이벤트 데이터</param>
        /// <returns>활성화 성공 여부</returns>
        public bool HandleGameplayEvent(GameplayEventData eventData)
        {
            if (!eventData.EventTag.IsValid)
            {
                Debug.LogWarning("이벤트 태그가 유효하지 않습니다.");
                return false;
            }

            // 이벤트를 브로드캐스트하고 조건에 맞는 능력을 찾는다.
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
        /// <param name="tag">추가할 태그</param>
        public void AddLooseTag(FGameplayTag tag)
        {
            var model = EnsureModel();
            if (!tag.IsValid)
            {
                return;
            }

            // 느슨한 태그 카운트를 갱신한다.
            var addedToOwned = model.AddLooseTag(tag, out var totalCount);
            if (addedToOwned)
            {
                _onAddedTag?.Invoke(this, tag);
            }

            if (!IsTagMessageIgnored(tag))
            {
                PublishSystemMessage($"태그 추가: {tag.Value} (총 {totalCount})");
            }
        }

        /// <summary>
        /// 느슨한 태그를 제거합니다.
        /// </summary>
        /// <param name="tag">제거할 태그</param>
        public void RemoveLooseTag(FGameplayTag tag)
        {
            var model = EnsureModel();
            if (!tag.IsValid)
            {
                return;
            }

            // 느슨한 태그 카운트를 감소시킨다.
            var beforeTotal = model.GetTotalTagCount(tag);
            var removedFromOwned = model.RemoveLooseTag(tag, out var totalCount);
            if (totalCount == beforeTotal)
            {
                return;
            }
            if (removedFromOwned)
            {
                _onRemovedTag?.Invoke(this, tag);
            }

            if (!IsTagMessageIgnored(tag))
            {
                PublishSystemMessage($"태그 제거: {tag.Value} (총 {totalCount})");
            }
        }

        private void RegisterAbilityInstance(GameplayAbilitySpec spec, GameplayAbilityInstance instance)
        {
            if (spec == null || instance == null)
            {
                return;
            }

            // 핸들 기준으로 실행 인스턴스를 보관한다.
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

            // 등록된 인스턴스를 모두 종료한다.
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

        /// <summary>
        /// 속성 수정이 발생했을 때 호출됩니다.
        /// </summary>
        public event Action<AbilitySystemComponent, AttributeModifier, AttributeValue, AttributeValue> onChangedAttributeModifier
        {
            add => _onChangedAttributeModifier += value;
            remove => _onChangedAttributeModifier -= value;
        }

        /// <summary>
        /// 태그가 추가되었을 때 호출됩니다.
        /// </summary>
        public event Action<AbilitySystemComponent, FGameplayTag> onAddedTag
        {
            add => _onAddedTag += value;
            remove => _onAddedTag -= value;
        }

        /// <summary>
        /// 태그가 제거되었을 때 호출됩니다.
        /// </summary>
        public event Action<AbilitySystemComponent, FGameplayTag> onRemovedTag
        {
            add => _onRemovedTag += value;
            remove => _onRemovedTag -= value;
        }

        /// <summary>
        /// 게임플레이 이벤트가 발생했을 때 호출됩니다.
        /// </summary>
        public event Action<AbilitySystemComponent, GameplayEventData> onGameplayEvent
        {
            add => _onGameplayEvent += value;
            remove => _onGameplayEvent -= value;
        }

        /// <summary>
        /// 현재 적용 중인 효과 목록을 반환합니다.
        /// </summary>
        /// <param name="results">결과를 받을 리스트</param>
        public void GetActiveEffects(List<GameplayEffectConfig> results)
        {
            if (results == null)
            {
                return;
            }

            // 현재 적용 중인 효과만 모아 반환한다.
            results.Clear();
            EnsureModel().GetActiveEffects(results);
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
            // 핸들로 보유 목록을 순회한다.
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

            // 타입과 이름 조건으로 스펙을 찾는다.
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

            // 내부 스펙을 구성하고 목록에 추가한다.
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

            // 타입과 이름이 맞는 스펙을 제거한다.
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

            // 이름이 비어 있으면 타입 이름으로 비교한다.
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
            // 시작 시점에 지정된 능력을 부여한다.
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

            // 시스템 메시지를 전파한다.
            SystemMessageBus.Publish(message);
        }

        private static string ResolveAbilityLabel(GameplayAbilitySpec spec, string fallback = null)
        {
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }

            // 이름이 없으면 타입 이름을 사용한다.
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
            // 런타임 레지스트리 기준으로 무시 여부를 확인한다.
            var registry = GameplayTagRegistry.RuntimeRegistry;
            return registry != null && registry.IsSystemMessageIgnored(tag);
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
            // 기본 컨텍스트로 효과를 적용한다.
            var context = new GameplayEffectContext(this, this, default);
            ApplyGameplayEffect(effectConfigs, context);
        }

        /// <summary>
        /// 게임플레이 효과를 적용합니다.
        /// </summary>
        /// <param name="effectConfig">적용할 효과 설정</param>
        public void ApplyGameplayEffect(GameplayEffectConfig effectConfig)
        {
            // 기본 컨텍스트로 단일 효과를 적용한다.
            ApplyGameplayEffect(effectConfig, new GameplayEffectContext(this, this, default));
        }

        /// <summary>
        /// 컨텍스트를 지정하여 게임플레이 효과를 적용합니다.
        /// </summary>
        /// <param name="effectConfig">적용할 효과 설정</param>
        /// <param name="context">계산 컨텍스트</param>
        public void ApplyGameplayEffect(GameplayEffectConfig effectConfig, GameplayEffectContext context)
        {
            if (effectConfig == null)
            {
                return;
            }

            // 적용 조건을 먼저 확인한다.
            if (!CanApplyEffect(effectConfig))
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

                EnsureModel().AddActiveEffect(effectConfig, Time.time + duration);
            }
            else if (effectConfig.DurationType == EGameplayEffectDurationType.Infinite)
            {
                EnsureModel().AddActiveEffect(effectConfig, float.PositiveInfinity);
            }
        }

        private bool CanApplyEffect(GameplayEffectConfig effectConfig)
        {
            if (effectConfig == null)
            {
                return false;
            }

            // 필수/차단 태그 조건을 확인한다.
            if (!OwnedTags.HasAll(effectConfig.ActivationRequiredTags))
            {
                return false;
            }

            if (OwnedTags.HasAny(effectConfig.ActivationBlockedTags))
            {
                return false;
            }

            return true;
        }

        private void ApplyGameplayEffect(IReadOnlyList<GameplayEffectConfig> effectConfigs, GameplayEffectContext context)
        {
            if (effectConfigs == null)
            {
                return;
            }

            // 리스트에 포함된 효과를 순서대로 적용한다.
            for (var i = 0; i < effectConfigs.Count; i++)
            {
                ApplyGameplayEffect(effectConfigs[i], context);
            }
        }

        /// <summary>
        /// 적용 중인 효과를 제거합니다.
        /// </summary>
        /// <param name="effectConfig">제거할 효과 설정</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveGameplayEffect(GameplayEffectConfig effectConfig)
        {
            if (effectConfig == null)
            {
                return false;
            }

            // 동일한 설정을 찾아 제거한다.
            if (!EnsureModel().RemoveActiveEffect(effectConfig))
            {
                return false;
            }

            RemoveEffectTags(effectConfig);
            return true;
        }

        /// <summary>
        /// 게임플레이 효과 태그를 추가합니다.
        /// </summary>
        /// <param name="effectConfig"></param>
        private void AddEffectTags(GameplayEffectConfig effectConfig)
        {
            // 효과가 부여하는 태그를 추가한다.
            var model = EnsureModel();
            foreach (var tag in effectConfig.GrantedTags.Tags)
            {
                var addedToOwned = model.AddEffectTag(tag, out _);
                if (addedToOwned)
                {
                    _onAddedTag?.Invoke(this, tag);
                }
            }
        }

        /// <summary>
        /// 게임플레이 효과 태그를 제거합니다.
        /// </summary>
        /// <param name="effectConfig"></param>
        private void RemoveEffectTags(GameplayEffectConfig effectConfig)
        {
            // 효과가 부여한 태그를 제거한다.
            var model = EnsureModel();
            foreach (var tag in effectConfig.GrantedTags.Tags)
            {
                var removedFromOwned = model.RemoveEffectTag(tag, out _);
                if (removedFromOwned)
                {
                    _onRemovedTag?.Invoke(this, tag);
                }
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

            // 각 수정자를 순회하며 값을 갱신한다.
            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier.Attribute == null)
                {
                    continue;
                }

                if (!Attributes.TryGet(modifier.Attribute, out var value))
                {
                    continue;
                }

                var prevValue = CreateAttributeSnapshot(value);
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

                if (Mathf.Approximately(prevValue.CurrentValue, value.CurrentValue))
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
            // 수정자 값 모드에 따라 값을 계산한다.
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

            // 계산기를 통해 값을 평가한다.
            return modifier.Calculator.EvaluateMagnitude(effectConfig, modifier, context);
        }

        private static AttributeValue CreateAttributeSnapshot(AttributeValue source)
        {
            if (source == null || source.Definition == null)
            {
                return source;
            }

            // 원본 값을 복제해 이전 상태를 만든다.
            var snapshot = new AttributeValue(source.Definition)
            {
                BaseValue = source.BaseValue,
                MinValue = source.MinValue,
                MaxValue = source.MaxValue
            };
            snapshot.CurrentValue = source.CurrentValue;
            return snapshot;
        }

        private void OnChangedAttributeModifier(AttributeModifier modifier, AttributeValue prevValue, AttributeValue value)
        {
            // 변경 이벤트를 전달한다.
            _onChangedAttributeModifier?.Invoke(this, modifier, prevValue, value);
        }
    }
}

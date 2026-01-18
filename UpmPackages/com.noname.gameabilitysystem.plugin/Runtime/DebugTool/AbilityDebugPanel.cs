using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 능력 디버그 패널을 구성하는 컴포넌트입니다.
    /// </summary>
    public sealed class AbilityDebugPanel : MonoBehaviour
    {
        private sealed class AbilityItem
        {
            public GameplayAbilityDefinition Definition;
            public Type AbilityType;
            public Button ToggleButton;
            public Button ActivateButton;
            public Button EndButton;
            public Image Background;
        }

        private sealed class SimpleRow
        {
            public GameObject Root;
            public Text Label;
            public Button Button;
        }

        private const float MinRefreshInterval = 0.05f;

        private readonly List<AbilityItem> _abilityItems = new();
        private readonly List<GameplayEffectConfig> _activeEffectsBuffer = new();
        private readonly HashSet<string> _equippedKeys = new();
        private readonly List<SimpleRow> _activeEffectRows = new();
        private readonly List<SimpleRow> _tagRows = new();
        private readonly List<SimpleRow> _attributeRows = new();

        private AbilitySystemComponent _target;
        private IReadOnlyList<GameplayAbilityDefinition> _allAbilities;
        private IReadOnlyList<GameplayEffectConfig> _allEffects;
        private float _refreshInterval = 0.25f;
        private float _nextRefreshTime;
        private bool _initialized;

        [SerializeField] private Text _titleText;
        [SerializeField] private RectTransform _abilityContent;
        [SerializeField] private RectTransform _effectContent;
        [SerializeField] private RectTransform _activeEffectContent;
        [SerializeField] private RectTransform _tagContent;
        [SerializeField] private RectTransform _attributeContent;
        [SerializeField] private Font _font;
        [SerializeField] private AbilityDebugTooltipConfig _tooltipConfig;
        [SerializeField] private float _tooltipDelay = 1f;

        private readonly Color _rowColor = new Color(0.18f, 0.18f, 0.18f, 0.9f);
        private readonly Color _equippedRowColor = new Color(0.12f, 0.28f, 0.12f, 0.9f);
        private readonly Color _textColor = Color.white;
        private readonly Color _buttonColor = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        private readonly Color _buttonDisabledColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
        private readonly Color _tooltipColor = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        private AbilityDebugTooltip _tooltip;

        private void Awake()
        {
            // 폰트와 툴팁 기본 구성을 준비한다.
            EnsureFont();
            if (_tooltipConfig != null)
            {
                EnsureTooltip();
            }
        }

        /// <summary>
        /// 패널을 초기화합니다.
        /// </summary>
        public void Initialize(string title, IReadOnlyList<GameplayAbilityDefinition> allAbilities, IReadOnlyList<GameplayEffectConfig> allEffects, float refreshInterval)
        {
            // 폰트를 먼저 확보한다.
            EnsureFont();
            var abilities = allAbilities ?? Array.Empty<GameplayAbilityDefinition>();
            var effects = allEffects ?? Array.Empty<GameplayEffectConfig>();
            _refreshInterval = Mathf.Max(MinRefreshInterval, refreshInterval);

            var shouldRebuild = !_initialized
                || !ReferenceEquals(_allAbilities, abilities)
                || !ReferenceEquals(_allEffects, effects);
            _allAbilities = abilities;
            _allEffects = effects;

            if (shouldRebuild)
            {
                BuildStaticLists();
            }

            _initialized = true;
        }

        /// <summary>
        /// 패널 대상과 표시 이름을 설정합니다.
        /// </summary>
        public void SetTarget(AbilitySystemComponent target, string displayName)
        {
            // 대상과 타이틀을 설정한다.
            _target = target;
            if (_titleText != null)
            {
                _titleText.text = displayName;
            }

            Refresh();
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            // 일정 주기마다만 갱신한다.
            if (Time.unscaledTime < _nextRefreshTime)
            {
                return;
            }

            _nextRefreshTime = Time.unscaledTime + _refreshInterval;
            Refresh();
        }

        private void BuildStaticLists()
        {
            // 정적 리스트를 다시 만든다.
            BuildAbilityList();
            BuildEffectList();
        }

        private void BuildAbilityList()
        {
            // 능력 목록 UI를 다시 만든다.
            if (_abilityContent == null)
            {
                return;
            }

            ClearChildren(_abilityContent);
            _abilityItems.Clear();

            for (var i = 0; i < _allAbilities.Count; i++)
            {
                var definition = _allAbilities[i];
                if (definition == null)
                {
                    continue;
                }

                var row = CreateRow(_abilityContent, out var background, horizontal: true);
                var rowRect = row.GetComponent<RectTransform>();
                var nameButton = CreateButton("Toggle", row.transform, definition.name, flexibleWidth: true, dragTarget: rowRect);
                var activateButton = CreateButton("Activate", row.transform, "Act", width: 40f, dragTarget: rowRect);
                var endButton = CreateButton("End", row.transform, "End", width: 40f, dragTarget: rowRect);

                var abilityType = Type.GetType(definition.AbilityTypeName);
                var item = new AbilityItem
                {
                    Definition = definition,
                    AbilityType = abilityType,
                    ToggleButton = nameButton,
                    ActivateButton = activateButton,
                    EndButton = endButton,
                    Background = background
                };

                nameButton.onClick.AddListener(() => ToggleAbility(item));
                activateButton.onClick.AddListener(() => ActivateAbility(item));
                endButton.onClick.AddListener(() => EndAbility(item));

                if (TryGetAbilityTooltip(definition, out var title, out var description))
                {
                    TryAttachTooltip(nameButton, title, description);
                }
                else
                {
                    RemoveTooltip(nameButton);
                }

                _abilityItems.Add(item);
            }
        }

        private void BuildEffectList()
        {
            // 효과 목록 UI를 다시 만든다.
            if (_effectContent == null)
            {
                return;
            }

            ClearChildren(_effectContent);

            for (var i = 0; i < _allEffects.Count; i++)
            {
                var effect = _allEffects[i];
                if (effect == null)
                {
                    continue;
                }

                var row = CreateRow(_effectContent, out _);
                var rowRect = row.GetComponent<RectTransform>();
                var button = CreateButton("Effect", row.transform, effect.name, flexibleWidth: true, dragTarget: rowRect);
                button.onClick.AddListener(() => ApplyEffect(effect));

                if (TryGetEffectTooltip(effect, out var title, out var description))
                {
                    TryAttachTooltip(button, title, description);
                }
                else
                {
                    RemoveTooltip(button);
                }
            }
        }

        private void Refresh()
        {
            if (_target == null)
            {
                return;
            }

            // 상태별 목록을 갱신한다.
            RefreshAbilityStates();
            RefreshActiveEffects();
            RefreshTags();
            RefreshAttributes();
        }

        private void RefreshAbilityStates()
        {
            // 장착 상태를 계산한다.
            _equippedKeys.Clear();
            var specs = _target.Abilities;
            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                if (spec == null || spec.AbilityType == null)
                {
                    continue;
                }

                _equippedKeys.Add(BuildAbilityKey(spec.AbilityType, spec.AbilityName));
            }

            for (var i = 0; i < _abilityItems.Count; i++)
            {
                var item = _abilityItems[i];
                var abilityName = item.Definition != null ? item.Definition.name : string.Empty;
                var equipped = item.AbilityType != null && _equippedKeys.Contains(BuildAbilityKey(item.AbilityType, abilityName));
                if (item.Background != null)
                {
                    item.Background.color = equipped ? _equippedRowColor : _rowColor;
                }

                if (item.ActivateButton != null)
                {
                    item.ActivateButton.interactable = equipped;
                }

                if (item.EndButton != null)
                {
                    item.EndButton.interactable = equipped;
                }
            }
        }

        private void RefreshActiveEffects()
        {
            // 활성 효과 목록을 갱신한다.
            _target.GetActiveEffects(_activeEffectsBuffer);
            if (_activeEffectContent == null)
            {
                return;
            }

            var rowIndex = 0;
            for (var i = 0; i < _activeEffectsBuffer.Count; i++)
            {
                var effect = _activeEffectsBuffer[i];
                if (effect == null)
                {
                    continue;
                }

                var row = GetOrCreateButtonRow(_activeEffectRows, _activeEffectContent, rowIndex);
                row.Root.SetActive(true);
                if (row.Label != null)
                {
                    row.Label.text = effect.name;
                }

                if (row.Button != null)
                {
                    row.Button.onClick.RemoveAllListeners();
                    row.Button.onClick.AddListener(() => RemoveEffect(effect));
                    if (TryGetEffectTooltip(effect, out var title, out var description))
                    {
                        TryAttachTooltip(row.Button, title, description);
                    }
                    else
                    {
                        RemoveTooltip(row.Button);
                    }
                }

                rowIndex++;
            }

            DeactivateUnusedRows(_activeEffectRows, rowIndex);
        }

        private void RefreshTags()
        {
            // 태그 목록을 갱신한다.
            if (_tagContent == null)
            {
                return;
            }

            var tags = _target.OwnedTags;
            if (tags == null)
            {
                DeactivateUnusedRows(_tagRows, 0);
                return;
            }

            var list = tags.Tags;
            var rowIndex = 0;
            for (var i = 0; i < list.Count; i++)
            {
                var tag = list[i];
                var value = tag.Value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var row = GetOrCreateLabelRow(_tagRows, _tagContent, rowIndex);
                row.Root.SetActive(true);
                if (row.Label != null)
                {
                    row.Label.text = tag.IsValid ? value : $"(Invalid) {value}";
                }

                rowIndex++;
            }

            DeactivateUnusedRows(_tagRows, rowIndex);
        }

        private void RefreshAttributes()
        {
            // 속성 목록을 갱신한다.
            if (_attributeContent == null)
            {
                return;
            }

            var rowIndex = 0;
            var values = _target.Attributes.Values;
            foreach (var value in values)
            {
                if (value == null || value.Definition == null)
                {
                    continue;
                }

                var label = $"{value.Definition.Id}: {value.CurrentValue:0.##} (Base {value.BaseValue:0.##})";
                var row = GetOrCreateLabelRow(_attributeRows, _attributeContent, rowIndex);
                row.Root.SetActive(true);
                if (row.Label != null)
                {
                    row.Label.text = label;
                }

                rowIndex++;
            }

            DeactivateUnusedRows(_attributeRows, rowIndex);
        }

        private static string BuildAbilityKey(Type abilityType, string abilityName)
        {
            // 타입과 이름을 결합해 키를 만든다.
            var typeName = abilityType != null ? abilityType.FullName : string.Empty;
            var name = string.IsNullOrWhiteSpace(abilityName) ? string.Empty : abilityName;
            return $"{typeName}|{name}";
        }

        private void ToggleAbility(AbilityItem item)
        {
            // 장착 여부에 따라 부여/해제를 토글한다.
            if (_target == null || item == null)
            {
                return;
            }

            var abilityName = item.Definition != null ? item.Definition.name : string.Empty;
            var equipped = item.AbilityType != null && _equippedKeys.Contains(BuildAbilityKey(item.AbilityType, abilityName));
            if (!equipped)
            {
                _target.GiveAbility(item.Definition);
            }
            else
            {
                _target.RemoveAbility(item.Definition);
            }

            RefreshAbilityStates();
        }

        private void ActivateAbility(AbilityItem item)
        {
            // 능력을 즉시 실행한다.
            if (_target == null || item == null || item.Definition == null)
            {
                return;
            }

            _target.TryActivateAbility(item.Definition);
        }

        private void EndAbility(AbilityItem item)
        {
            // 능력을 종료한다.
            if (_target == null || item == null || item.Definition == null)
            {
                return;
            }

            _target.EndAbility(item.Definition);
        }

        private void ApplyEffect(GameplayEffectConfig effect)
        {
            // 효과를 적용한다.
            if (_target == null || effect == null)
            {
                return;
            }

            _target.ApplyGameplayEffect(effect);
        }

        private void RemoveEffect(GameplayEffectConfig effect)
        {
            // 효과를 제거하고 목록을 갱신한다.
            if (_target == null || effect == null)
            {
                return;
            }

            _target.RemoveGameplayEffect(effect);
            RefreshActiveEffects();
        }

        private RectTransform CreateUIRect(string name, Transform parent)
        {
            // 기본 RectTransform을 생성한다.
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            return rect;
        }

        private Text CreateText(string name, Transform parent, string text, int fontSize, FontStyle style, TextAnchor alignment, bool stretch)
        {
            // 텍스트 오브젝트를 생성한다.
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            if (stretch)
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            var textComponent = obj.AddComponent<Text>();
            textComponent.font = _font;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = style;
            textComponent.color = _textColor;
            textComponent.alignment = alignment;
            textComponent.raycastTarget = false;
            textComponent.text = text;
            return textComponent;
        }

        private GameObject CreateRow(RectTransform parent, out Image background, bool horizontal = true)
        {
            // 행 컨테이너를 만든다.
            var rect = CreateUIRect("Row", parent);
            background = rect.gameObject.AddComponent<Image>();
            background.color = _rowColor;

            var element = rect.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 24f;

            if (horizontal)
            {
                var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(4, 4, 2, 2);
                layout.spacing = 4f;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = true;
                layout.childForceExpandWidth = false;
            }

            return rect.gameObject;
        }

        private Button CreateButton(string name, Transform parent, string label, float width = 0f, bool flexibleWidth = false, RectTransform dragTarget = null)
        {
            // 버튼 요소를 만든다.
            var rect = CreateUIRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = _buttonColor;

            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = _buttonColor;
            colors.disabledColor = _buttonDisabledColor;
            button.colors = colors;

            var element = rect.gameObject.AddComponent<LayoutElement>();
            if (flexibleWidth)
            {
                element.flexibleWidth = 1f;
            }
            else if (width > 0f)
            {
                element.preferredWidth = width;
            }

            element.preferredHeight = 20f;

            CreateLabel("Label", rect.transform, label, TextAnchor.MiddleCenter);
            var draggable = rect.gameObject.AddComponent<UIDraggableItem>();
            if (dragTarget != null)
            {
                draggable.SetDragTarget(dragTarget);
            }
            return button;
        }

        private Text CreateLabel(string name, Transform parent, string label, TextAnchor alignment)
        {
            // 라벨 텍스트를 만든다.
            var text = CreateText(name, parent, label, 12, FontStyle.Normal, alignment, stretch: true);
            var element = text.gameObject.AddComponent<LayoutElement>();
            element.flexibleWidth = 1f;
            return text;
        }

        private void AddDraggable(GameObject row)
        {
            // 드래그 컴포넌트를 보장한다.
            if (row.GetComponent<UIDraggableItem>() == null)
            {
                row.AddComponent<UIDraggableItem>();
            }
        }

        private SimpleRow GetOrCreateButtonRow(List<SimpleRow> rows, RectTransform parent, int index)
        {
            // 필요한 버튼 행을 가져오거나 만든다.
            if (index < rows.Count)
            {
                return rows[index];
            }

            var row = CreateRow(parent, out _);
            var rowRect = row.GetComponent<RectTransform>();
            var button = CreateButton("Button", row.transform, string.Empty, flexibleWidth: true, dragTarget: rowRect);
            var label = button.GetComponentInChildren<Text>();

            var entry = new SimpleRow
            {
                Root = row,
                Label = label,
                Button = button
            };

            rows.Add(entry);
            return entry;
        }

        private SimpleRow GetOrCreateLabelRow(List<SimpleRow> rows, RectTransform parent, int index)
        {
            // 필요한 라벨 행을 가져오거나 만든다.
            if (index < rows.Count)
            {
                return rows[index];
            }

            var row = CreateRow(parent, out _, horizontal: false);
            var label = CreateLabel("Label", row.transform, string.Empty, TextAnchor.MiddleLeft);
            AddDraggable(row);

            var entry = new SimpleRow
            {
                Root = row,
                Label = label
            };

            rows.Add(entry);
            return entry;
        }

        private void DeactivateUnusedRows(List<SimpleRow> rows, int usedCount)
        {
            // 사용하지 않는 행은 비활성화한다.
            for (var i = usedCount; i < rows.Count; i++)
            {
                if (rows[i]?.Root != null)
                {
                    rows[i].Root.SetActive(false);
                }
            }
        }

        private void EnsureFont()
        {
            // 사용할 폰트를 순서대로 찾는다.
            if (_font != null)
            {
                return;
            }

            if (_titleText != null && _titleText.font != null)
            {
                _font = _titleText.font;
                return;
            }

            var text = GetComponentInChildren<Text>(true);
            if (text != null && text.font != null)
            {
                _font = text.font;
                return;
            }

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void EnsureTooltip()
        {
            // 툴팁 인스턴스를 준비한다.
            if (_tooltip != null)
            {
                return;
            }

            _tooltip = GetComponentInChildren<AbilityDebugTooltip>(true);
            if (_tooltip == null)
            {
                var obj = new GameObject("AbilityDebugTooltip");
                obj.transform.SetParent(transform, false);
                _tooltip = obj.AddComponent<AbilityDebugTooltip>();
            }

            _tooltip.EnsureBuilt(_font, _textColor, _tooltipColor);
        }

        private bool TryGetAbilityTooltip(GameplayAbilityDefinition definition, out string title, out string description)
        {
            title = string.Empty;
            description = string.Empty;

            // 설정에서 능력 툴팁을 조회한다.
            if (_tooltipConfig == null || definition == null)
            {
                return false;
            }

            if (!_tooltipConfig.TryGetAbilityTooltip(definition, out title, out description))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = definition.name;
            }

            return true;
        }

        private bool TryGetEffectTooltip(GameplayEffectConfig effect, out string title, out string description)
        {
            title = string.Empty;
            description = string.Empty;

            // 설정에서 효과 툴팁을 조회한다.
            if (_tooltipConfig == null || effect == null)
            {
                return false;
            }

            if (!_tooltipConfig.TryGetEffectTooltip(effect, out title, out description))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = effect.name;
            }

            return true;
        }

        private void TryAttachTooltip(Button button, string title, string description)
        {
            // 버튼에 툴팁 트리거를 연결한다.
            if (button == null)
            {
                return;
            }

            EnsureTooltip();

            var trigger = button.GetComponent<AbilityDebugTooltipTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<AbilityDebugTooltipTrigger>();
            }

            trigger.Setup(_tooltip, title, description, _tooltipDelay);
        }

        private void RemoveTooltip(Button button)
        {
            // 버튼에서 툴팁 트리거를 제거한다.
            if (button == null)
            {
                return;
            }

            var trigger = button.GetComponent<AbilityDebugTooltipTrigger>();
            if (trigger != null)
            {
                Destroy(trigger);
            }
        }

        private void ClearChildren(RectTransform parent)
        {
            // 자식 오브젝트를 모두 제거한다.
            if (parent == null)
            {
                return;
            }

            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}

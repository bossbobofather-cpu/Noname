using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 능력 디버그 패널을 구성하는 루트 컴포넌트입니다.
    /// </summary>
    public sealed class AbilityDebugUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private AbilityDebugCatalog _catalog;
        [SerializeField] private List<GameplayAbilityDefinition> _abilityFallback = new();
        [SerializeField] private List<GameplayEffectConfig> _effectFallback = new();

        [Header("Target")]
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private Transform _followTarget;

        [Header("UI")]
        [SerializeField] private AbilityDebugPanel _panel;
        [SerializeField] private AbilityDebugPanel _panelPrefab;
        [SerializeField] private RectTransform _panelParent;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private bool _autoCreateCanvas = true;

        [Header("Follow")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);

        [Header("Behavior")]
        [SerializeField] private float _refreshInterval = 0.25f;
        [SerializeField] private bool _visible = true;

        [Header("Toggle")]
        [SerializeField] private bool _toggleWithKey = true;
        [SerializeField] private Key _toggleKey = Key.F1;

        private IReadOnlyList<GameplayAbilityDefinition> AbilityDefinitions =>
            _catalog != null ? _catalog.Abilities : _abilityFallback;

        private IReadOnlyList<GameplayEffectConfig> EffectConfigs =>
            _catalog != null ? _catalog.Effects : _effectFallback;

        private void Awake()
        {
            // 필수 구성 요소를 준비한다.
            EnsureEventSystem();
            EnsureCanvas();
            ResolveTarget();
            EnsurePanel();
            ApplyPanel();
            ApplyVisibility();
        }

        private void Update()
        {
            if (_toggleWithKey && Keyboard.current != null && Keyboard.current[_toggleKey].wasPressedThisFrame)
            {
                // 토글 입력이 들어오면 표시를 전환한다.
                Toggle();
            }
        }

        /// <summary>
        /// 패널 표시 여부를 설정합니다.
        /// </summary>
        /// <param name="visible">표시 여부</param>
        public void SetVisible(bool visible)
        {
            // 표시 상태를 반영한다.
            _visible = visible;
            ApplyVisibility();
        }

        /// <summary>
        /// 패널 표시 상태를 토글합니다.
        /// </summary>
        public void Toggle()
        {
            // 현재 상태를 반전한다.
            SetVisible(!_visible);
        }

        private void ResolveTarget()
        {
            if (_abilitySystem == null)
            {
                // 상위에서 능력 시스템을 찾는다.
                _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
            }

            if (_followTarget == null)
            {
                _followTarget = transform;
            }

            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
        }

        private void EnsurePanel()
        {
            if (_panel == null)
            {
                _panel = GetComponentInChildren<AbilityDebugPanel>(true);
            }

            if (_panel == null && _panelPrefab != null)
            {
                var parent = _panelParent != null
                    ? _panelParent
                    : _canvas != null ? _canvas.transform : transform;
                _panel = Instantiate(_panelPrefab, parent);
            }
        }

        private void ApplyPanel()
        {
            if (_panel == null)
            {
                return;
            }

            if (_abilitySystem == null)
            {
                _panel.gameObject.SetActive(false);
                return;
            }

            // 패널 데이터를 갱신하고 타겟을 지정한다.
            _panel.Initialize(_abilitySystem.gameObject.name, AbilityDefinitions, EffectConfigs, _refreshInterval);
            _panel.SetTarget(_abilitySystem, _abilitySystem.gameObject.name);

            var follower = _panel.GetComponent<AbilityDebugWorldFollower>();
            if (follower == null)
            {
                follower = _panel.gameObject.AddComponent<AbilityDebugWorldFollower>();
            }

            follower.Setup(_followTarget, _worldCamera, _canvas, _worldOffset);
        }

        private void ApplyVisibility()
        {
            if (_panel != null)
            {
                _panel.gameObject.SetActive(_visible);
            }
        }

        private void EnsureCanvas()
        {
            if (_panelParent != null)
            {
                return;
            }

            if (_canvas == null)
            {
                if (_autoCreateCanvas)
                {
                    // 디버그용 캔버스를 자동 생성한다.
                    var obj = new GameObject("AbilityDebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    _canvas = obj.GetComponent<Canvas>();
                    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    var scaler = obj.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }

            if (_panelParent == null && _canvas != null)
            {
                _panelParent = _canvas.transform as RectTransform;
            }
        }

        private static void EnsureEventSystem()
        {
            var current = EventSystem.current;
            if (current == null)
            {
                // 이벤트 시스템이 없으면 생성한다.
                var obj = new GameObject("EventSystem", typeof(EventSystem));
                current = obj.GetComponent<EventSystem>();
                DontDestroyOnLoad(obj);
            }

            var standalone = current.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                Destroy(standalone);
            }

            if (current.GetComponent<InputSystemUIInputModule>() == null)
            {
                current.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }
    }
}

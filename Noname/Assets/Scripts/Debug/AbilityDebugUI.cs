using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MergeGame.Debug
{
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
                Toggle();
            }
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            ApplyVisibility();
        }

        public void Toggle()
        {
            SetVisible(!_visible);
        }

        private void ResolveTarget()
        {
            if (_abilitySystem == null)
            {
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
                _canvas = FindFirstObjectByType<Canvas>();
            }

            if (_canvas == null && _autoCreateCanvas)
            {
                var obj = new GameObject("AbilityDebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                _canvas = obj.GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = obj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
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

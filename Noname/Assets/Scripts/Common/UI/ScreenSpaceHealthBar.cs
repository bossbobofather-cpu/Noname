using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Noname.GameAbilitySystem;

namespace MyProject.Common.UI
{
    [DisallowMultipleComponent]
    public sealed class ScreenSpaceHealthBar : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private AttributeId _healthAttribute = AttributeId.Health;
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _valueLabel;
        [SerializeField] private bool _showValue = true;
        [SerializeField] private float _refreshInterval = 0.1f;
        [SerializeField] private float _smoothSpeed = 0f;
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("Follow")]
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);

        private RectTransform _rectTransform;
        private RectTransform _canvasRect;
        private Canvas _canvas;
        private Camera _camera;
        private AbilitySystemComponent _abilitySystem;
        private Transform _followTarget;
        private float _nextRefreshTime;
        private float _targetFill = 1f;
        private bool _initialized;

        private float Now => _useUnscaledTime ? Time.unscaledTime : Time.time;
        private float Delta => _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (!_initialized)
            {
                return;
            }

            UpdatePosition();

            if (Now >= _nextRefreshTime)
            {
                _nextRefreshTime = Now + Mathf.Max(0.02f, _refreshInterval);
                UpdateHealth(false);
            }

            UpdateFill();
        }

        public void Bind(
            AbilitySystemComponent abilitySystem,
            Transform followTarget,
            Canvas canvas,
            Camera camera,
            Vector3 worldOffset,
            AttributeId healthAttribute)
        {
            _abilitySystem = abilitySystem;
            _followTarget = followTarget;
            _canvas = canvas;
            _camera = camera;
            _worldOffset = worldOffset;
            _healthAttribute = healthAttribute;
            _rectTransform ??= GetComponent<RectTransform>();
            _canvasRect = _canvas != null ? _canvas.transform as RectTransform : null;
            _nextRefreshTime = 0f;
            _targetFill = 1f;
            _initialized = true;

            UpdateHealth(true);
            UpdatePosition();
            UpdateFill();
        }

        public void Unbind()
        {
            _abilitySystem = null;
            _followTarget = null;
            _canvas = null;
            _camera = null;
            _canvasRect = null;
            _initialized = false;
        }

        private void UpdatePosition()
        {
            if (_followTarget == null || _canvasRect == null || _rectTransform == null)
            {
                return;
            }

            var worldCamera = _camera != null ? _camera : Camera.main;
            if (worldCamera == null && _canvas != null)
            {
                worldCamera = _canvas.worldCamera;
            }

            if (worldCamera == null)
            {
                return;
            }

            var uiCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? (_canvas.worldCamera != null ? _canvas.worldCamera : worldCamera)
                : null;

            var worldPos = _followTarget.position + _worldOffset;
            var screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPos);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, uiCamera, out var localPos))
            {
                return;
            }

            _rectTransform.anchoredPosition = localPos;
        }

        private void UpdateHealth(bool force)
        {
            if (_abilitySystem == null || _fillImage == null)
            {
                return;
            }

            if (!_abilitySystem.Attributes.TryGet(_healthAttribute, out var health) || health == null)
            {
                if (force)
                {
                    _fillImage.fillAmount = 0f;
                }
                return;
            }

            var min = health.MinValue;
            var max = health.MaxValue;
            if (max <= min)
            {
                max = Mathf.Max(health.BaseValue, health.CurrentValue, 1f);
                min = 0f;
            }

            var current = Mathf.Clamp(health.CurrentValue, min, max);
            _targetFill = Mathf.InverseLerp(min, max, current);

            if (_valueLabel != null)
            {
                _valueLabel.gameObject.SetActive(_showValue);
                if (_showValue)
                {
                    _valueLabel.text = $"{current:0}/{max:0}";
                }
            }
        }

        private void UpdateFill()
        {
            if (_fillImage == null)
            {
                return;
            }

            if (_smoothSpeed > 0f)
            {
                _fillImage.fillAmount = Mathf.MoveTowards(
                    _fillImage.fillAmount,
                    _targetFill,
                    _smoothSpeed * Delta);
            }
            else
            {
                _fillImage.fillAmount = _targetFill;
            }
        }
    }
}

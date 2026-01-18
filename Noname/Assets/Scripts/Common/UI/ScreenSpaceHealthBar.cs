using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Noname.GameAbilitySystem;

namespace MyProject.Common.UI
{
    /// <summary>
    /// 화면 공간(Screen Space)에서 대상을 따라다니는 체력바 UI입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScreenSpaceHealthBar : MonoBehaviour
    {
        [Header("Health")]
        /// <summary>
        /// 표시할 체력 속성의 ID입니다.
        /// </summary>
        [SerializeField] private AttributeId _healthAttribute = AttributeId.Health;

        /// <summary>
        /// 체력 상태를 표시할 이미지(Fill)입니다.
        /// </summary>
        [SerializeField] private Image _fillImage;

        /// <summary>
        /// 체력 수치를 표시할 텍스트입니다.
        /// </summary>
        [SerializeField] private TMP_Text _valueLabel;

        /// <summary>
        /// 체력 수치 텍스트 표시 여부입니다.
        /// </summary>
        [SerializeField] private bool _showValue = true;

        /// <summary>
        /// 체력 정보 갱신 간격(초)입니다.
        /// </summary>
        [SerializeField] private float _refreshInterval = 0.1f;

        /// <summary>
        /// 게이지 변화 시 부드러운 이동 속도입니다. 0이면 즉시 반영합니다.
        /// </summary>
        [SerializeField] private float _smoothSpeed = 0f;

        /// <summary>
        /// UnscaledTime 사용 여부입니다.
        /// </summary>
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("Follow")]
        /// <summary>
        /// 대상 위치로부터 적용할 월드 오프셋입니다.
        /// </summary>
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

        /// <summary>
        /// 체력바를 특정 대상 및 데이터와 연결하고 초기화합니다.
        /// </summary>
        /// <param name="abilitySystem">체력 정보를 가진 시스템입니다.</param>
        /// <param name="followTarget">따라다닐 월드 대상입니다.</param>
        /// <param name="canvas">UI가 위치할 캔버스입니다.</param>
        /// <param name="camera">월드 좌표 변환에 사용할 카메라입니다.</param>
        /// <param name="worldOffset">대상 위치 오프셋입니다.</param>
        /// <param name="healthAttribute">체력 속성 ID입니다.</param>
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

        /// <summary>
        /// 연결 정보를 해제합니다.
        /// </summary>
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

            // 캔버스 렌더 모드에 따라 UI 카메라 결정
            var uiCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? (_canvas.worldCamera != null ? _canvas.worldCamera : worldCamera)
                : null;

            var worldPos = _followTarget.position + _worldOffset;
            var screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPos);
            
            // 스크린 좌표를 캔버스 내 로컬 좌표로 변환
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

            // 최소/최대값 보정
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

            // 스무딩 옵션에 따라 게이지 갱신
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

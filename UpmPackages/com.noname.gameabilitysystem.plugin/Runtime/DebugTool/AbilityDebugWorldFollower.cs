using UnityEngine;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 월드 위치를 따라가는 UI 보조 컴포넌트입니다.
    /// </summary>
    public sealed class AbilityDebugWorldFollower : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private bool _hideWhenOffscreen = true;
        [SerializeField] private Vector2 _screenPadding = new Vector2(8f, 8f);

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            // 필요한 컴포넌트를 캐싱한다.
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
            }

            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
        }

        /// <summary>
        /// 대상과 카메라 정보를 설정합니다.
        /// </summary>
        public void Setup(Transform target, Camera worldCamera, Canvas canvas, Vector3 worldOffset)
        {
            // 외부에서 전달된 값을 적용한다.
            _target = target;
            if (worldCamera != null)
            {
                _worldCamera = worldCamera;
            }

            if (canvas != null)
            {
                _canvas = canvas;
            }

            _worldOffset = worldOffset;
        }

        private void LateUpdate()
        {
            if (_target == null || _rectTransform == null)
            {
                return;
            }

            if (_canvas != null && _canvas.renderMode == RenderMode.WorldSpace)
            {
                // 월드 스페이스 캔버스는 위치만 맞춘다.
                _rectTransform.position = _target.position + _worldOffset;
                return;
            }

            var camera = _worldCamera != null ? _worldCamera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var screen = camera.WorldToScreenPoint(_target.position + _worldOffset);
            var visible = screen.z > 0f
                && screen.x >= 0f
                && screen.x <= Screen.width
                && screen.y >= 0f
                && screen.y <= Screen.height;

            if (_hideWhenOffscreen && _canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = visible;
                _canvasGroup.interactable = visible;
            }

            if (!visible)
            {
                return;
            }

            var parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null)
            {
                _rectTransform.position = screen;
                return;
            }

            var canvas = _canvas != null ? _canvas : parentRect.GetComponentInParent<Canvas>();
            var camForConversion = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? camera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screen, camForConversion, out var localPoint);
            _rectTransform.anchoredPosition = ClampToPadding(localPoint, parentRect);
        }

        private Vector2 ClampToPadding(Vector2 localPoint, RectTransform parentRect)
        {
            if (_screenPadding == Vector2.zero)
            {
                return localPoint;
            }

            // 가장자리 여백을 넘어가지 않도록 보정한다.
            var half = parentRect.rect.size * 0.5f;
            var min = new Vector2(-half.x + _screenPadding.x, -half.y + _screenPadding.y);
            var max = new Vector2(half.x - _screenPadding.x, half.y - _screenPadding.y);
            return new Vector2(Mathf.Clamp(localPoint.x, min.x, max.x), Mathf.Clamp(localPoint.y, min.y, max.y));
        }
    }
}
